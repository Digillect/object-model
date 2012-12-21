using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

#if WINDOWS8
using Windows.System.Threading;
#else
using System.Threading;
#endif

using Digillect.Collections;

namespace Digillect
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	public class XCache<T> : IDisposable, IEnumerable<T>
		where T : XObject
	{
		private readonly IDictionary<XKey, WeakReference> _objectCache = new Dictionary<XKey, WeakReference>();
		private readonly IDictionary<XQuery<T>, XCachedQuery> _queryCache = new Dictionary<XQuery<T>, XCachedQuery>();
#if WINDOWS8
		private ThreadPoolTimer _cleanupTimer;
#else
		private readonly Timer _cleanupTimer;
#endif

		private int _cleanupInterval = 10 * 60 * 1000;
		private bool _cleanupInProgress;

		#region Constructors/Disposer
		public XCache()
		{
			QueriesConversionEnabled = true;
			CleanupEnabled = true;

#if WINDOWS8
			_cleanupTimer = ThreadPoolTimer.CreatePeriodicTimer( timer => ProcessCleanup(), TimeSpan.FromMilliseconds( _cleanupInterval ) );
#else
			_cleanupTimer = new Timer( state => ProcessCleanup(), this, 10 * 60 * 1000, 10 * 60 * 1000 );
#endif
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		protected virtual void Dispose( bool disposing )
		{
			if( disposing )
			{
#if WINDOWS8
#else
				_cleanupTimer.Dispose();
#endif
			}
		}
		#endregion

		#region Public Properties
		public int CleanupInterval
		{
			get { return _cleanupInterval; }
			set
			{
				if( _cleanupInterval != value )
				{
					_cleanupInterval = value;
					UpdateCleanupTimerInterval();
				}
			}
		}

		public bool CleanupEnabled { get; set; }
		public bool QueriesConversionEnabled { get; set; }
		#endregion

		#region Public Methods : Objects
		/// <summary>
		/// Returns cached object, if any.
		/// </summary>
		public T Get( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			return GetCachedObjectEx( key );
		}

		/// <summary>
		/// Stores object in cache or updates existing object.
		/// </summary>
		/// <param name="obj">Object to cache.</param>
		/// <returns>Cached object.</returns>
		public T Cache( T obj )
		{
			if( obj == null )
			{
				throw new ArgumentNullException( "obj" );
			}

			Contract.Ensures( Contract.Result<T>() != null );

			XKey key = obj.GetKey();
			T cached = GetCachedObjectEx( key );

			if( cached == null )
			{
				cached = CacheObjectEx( obj );
			}
			else
			{
				cached.Update( obj );
			}

			foreach( XCachedQuery cq in new List<XCachedQuery>( _queryCache.Values ) )
			{
				var eventArgs = new XCacheMatchEventArgs<T>( cq.Query, cached );
				bool match;

				OnMatch( eventArgs );

				if( eventArgs.Processed )
				{
					match = eventArgs.Matched;
				}
				else
				{
					if( !cq.Query.SupportsMatch )
					{
						continue;
					}

					match = cq.Query.Match( cached );
				}

				if( match )
				{
					if( !cq.Items.ContainsKey( key ) )
					{
						cq.Items.Add( cached );
					}
				}
				else
				{
					if( cq.Items.ContainsKey( key ) )
					{
						cq.Items.Remove( key );
					}
				}
			}

			return cached;
		}

		public IXList<T> Cache( IEnumerable<T> collection )
		{
			if( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			Contract.Ensures( Contract.Result<IXList<T>>() != null );

			XCollection<T> cached = new XCollection<T>();

			foreach( T item in collection )
			{
				cached.Add( Cache( item ) );
			}

			return cached;
		}

		public bool Uncache( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "parameter" );
			}

			Contract.EndContractBlock();

			foreach( XCachedQuery cq in new List<XCachedQuery>( _queryCache.Values ) )
			{
				var index = cq.Items.IndexOf( key );

				if( index >= 0 )
				{
					cq.Items.RemoveAt( index );
				}
			}

			return UncacheObjectEx( key );
		}

		public bool Contains( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			return GetCachedObjectEx( key ) != null;
		}
		#endregion
		#region Public Methods : Queries
		/// <summary>
		/// Returns cached query result, if any.
		/// </summary>
		public IXList<T> Get( XQuery<T> query )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			XCachedQuery cq = _queryCache.ContainsKey( query ) ? _queryCache[query] : null;

			return cq == null ? null : cq.Items;
		}

		public IXList<T> Get( XQuery<T> query, object cookie )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			XCachedQuery cq = _queryCache.ContainsKey( query ) ? _queryCache[query] : null;

			if( cq != null )
			{
				if( cookie != null )
				{
					cq.AddCookie( cookie );
				}
			}
			else
			{
				if( !QueriesConversionEnabled )
				{
					return null;
				}

				foreach( XCachedQuery ccq in _queryCache.Values )
				{
					if( ccq.Query.CanConvertTo( query ) )
					{
						cq = ccq;
						break;
					}
				}

				if( cq == null )
				{
					return null;
				}

				var converted = ConvertQueryResults( cq.Items, query );

				if( cookie == null )
				{
					return converted;
				}

				cq = new XCachedQuery( query, converted, cookie );

				lock( _queryCache )
				{
					_queryCache.Add( cq.Query, cq );
				}
			}

			return cq.Items;
		}

		/// <summary>
		/// Кеширует переданный список и (если <paramref name="cookie"/> не равен <see langword="null"/>) создает
		/// <see cref="XCachedQuery"/> или добавляет <paramref name="cookie"/> к уже существующему.
		/// </summary>
		/// <returns>Возвращает список объектов.</returns>
		public IXList<T> Cache( XQuery<T> query, IEnumerable<T> list, object cookie )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			if( list == null )
			{
				throw new ArgumentNullException( "list" );
			}

			Contract.Ensures( Contract.Result<IXList<T>>() != null );

			var cached = Cache( list );

			XCachedQuery cq = _queryCache.ContainsKey( query ) ? _queryCache[query] : null;

			if( cq != null )
			{
				if( cookie != null )
				{
					cq.AddCookie( cookie );
				}
			}
			else
			{
				if( cookie == null )
				{
					return cached;
				}

				cq = new XCachedQuery( query, cached, cookie );

				lock( _queryCache )
				{
					_queryCache.Add( cq.Query, cq );
				}
			}

			return cq.Items;
		}

		public bool Uncache( XQuery<T> query, object cookie )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			XCachedQuery cq = _queryCache.ContainsKey( query ) ? _queryCache[query] : null;

			if( cq == null )
			{
				return false;
			}

			cq.RemoveCookie( cookie );

			if( cq.CookiesCount == 0 )
			{
				lock( _queryCache )
				{
					_queryCache.Remove( query );
				}

				return true;
			}

			return false;
		}

		public bool Contains( XQuery<T> query )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			return Contains( query, false );
		}

		public bool Contains( XQuery<T> query, bool ignoreConversion )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			if( _queryCache.ContainsKey( query ) )
			{
				return true;
			}

			if( !QueriesConversionEnabled || ignoreConversion )
			{
				return false;
			}

			lock( _queryCache )
			{
				foreach( XCachedQuery cq in _queryCache.Values )
				{
					if( cq.Query.CanConvertTo( query ) )
					{
						return true;
					}
				}
			}

			return false;
		}
		#endregion
		#region Public Methods : Cleanup
		public void Cleanup()
		{
			ProcessCleanup();
		}
		#endregion

		#region Events and Event Handlers
		public event EventHandler<XCacheObjectEventArgs<T>> ObjectCached;
		public event EventHandler<XCacheObjectEventArgs<T>> ObjectUncached;
		public event EventHandler<XCacheMatchEventArgs<T>> Match;

		protected virtual void OnObjectCached( XCacheObjectEventArgs<T> e )
		{
			if( ObjectCached != null )
			{
				ObjectCached( this, e );
			}
		}

		protected virtual void OnObjectUncached( XCacheObjectEventArgs<T> e )
		{
			if( ObjectUncached != null )
			{
				ObjectUncached( this, e );
			}
		}

		protected virtual void OnMatch( XCacheMatchEventArgs<T> e )
		{
			if( Match != null )
			{
				Match( this, e );
			}
		}
		#endregion

		#region Object Cache Private Methods
		private T GetCachedObjectEx( XKey key )
		{
			if( !_objectCache.ContainsKey( key ) )
			{
				return null;
			}

			WeakReference r = _objectCache[key];

			if( r.IsAlive )
			{
				return (T) r.Target;
			}

			lock( _objectCache )
			{
				_objectCache.Remove( key );
			}

			OnObjectUncached( new XCacheObjectEventArgs<T>( key, null ) );

			return null;
		}

		private T CacheObjectEx( T o )
		{
			XKey key = o.GetKey();

			lock( _objectCache )
			{
				_objectCache[key] = new WeakReference( o );
			}

			OnObjectCached( new XCacheObjectEventArgs<T>( key, o ) );

			return o;
		}

		private bool UncacheObjectEx( XKey key )
		{
			WeakReference r;

			lock( _objectCache )
			{
				if( !_objectCache.ContainsKey( key ) )
				{
					return false;
				}

				r = _objectCache[key];

				if( !_objectCache.Remove( key ) )
				{
					return false;
				}
			}

			OnObjectUncached( new XCacheObjectEventArgs<T>( key, r.IsAlive ? (T) r.Target : null ) );

			return true;
		}
		#endregion
		#region Queries Private Methods
		private static XCollection<T> ConvertQueryResults( IEnumerable<T> original, XQuery<T> query )
		{
			XCollection<T> result = new XCollection<T>();

			foreach( T o in original )
			{
				if( query.Match( o ) )
				{
					result.Add( o );
				}
			}

			return result;
		}
		#endregion
		#region Cleanup Private Methods
		private void UpdateCleanupTimerInterval()
		{
#if WINDOWS8
			_cleanupTimer = ThreadPoolTimer.CreatePeriodicTimer( timer => ProcessCleanup(), TimeSpan.FromMilliseconds( _cleanupInterval ) );
#else
			_cleanupTimer.Change( _cleanupInterval, _cleanupInterval );
#endif
		}

		private void ProcessCleanup()
		{
			if( _cleanupInProgress )
			{
				return;
			}

			List<XQuery<T>> queriesToRemove = new List<XQuery<T>>();
			List<XKey> objectsToRemove = new List<XKey>();

			lock( _objectCache )
			{
				_cleanupInProgress = true;

				lock( _queryCache )
				{
					foreach( var entry in _queryCache )
					{
						if( entry.Value.CookiesCount == 0 )
						{
							queriesToRemove.Add( entry.Key );
						}
					}

					foreach( var entry in _objectCache )
					{
						if( !entry.Value.IsAlive )
						{
							objectsToRemove.Add( entry.Key );
						}
					}

					foreach( var key in queriesToRemove )
					{
						_queryCache.Remove( key );
					}

					foreach( var key in objectsToRemove )
					{
						OnObjectUncached( new XCacheObjectEventArgs<T>( key, null ) );
						_objectCache.Remove( key );
					}
				}

				_cleanupInProgress = false;
			}
		}
		#endregion

		#region class XCachedQuery
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible" )]
		private sealed class XCachedQuery
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
			private readonly XQuery<T> _query;
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
			private readonly IXList<T> _items;
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
			private readonly IList<WeakReference> _cookies = new List<WeakReference>();

			#region Constructors/Disposer
			public XCachedQuery( XQuery<T> query, IXList<T> collection, object cookie )
			{
				if( query == null )
				{
					throw new ArgumentNullException( "query" );
				}

				if( collection == null )
				{
					throw new ArgumentNullException( "collection" );
				}

				Contract.EndContractBlock();

				_query = query.Clone();
				_items = XCollectionsUtil.UnmodifiableList( collection );

				AddCookie( cookie );
			}
			#endregion

			#region Public Properties
			public XQuery<T> Query
			{
				get { return _query; }
			}

			public IXList<T> Items
			{
				get { return _items; }
			}

			public int CookiesCount
			{
				get
				{
					CleanCookies();

					return _cookies.Count;
				}
			}
			#endregion

			#region Public Methods
			public bool AddCookie( object cookie )
			{
				if( cookie == null )
				{
					return false;
				}

				if( FindCookieCleaningList( cookie ) != null )
				{
					return false;
				}

				lock( _cookies )
				{
					_cookies.Add( new WeakReference( cookie ) );
				}

				return true;
			}

			public bool RemoveCookie( object cookie )
			{
				if( cookie == null )
				{
					return false;
				}

				WeakReference r = FindCookieCleaningList( cookie );

				if( r == null )
				{
					return false;
				}

				lock( _cookies )
				{
					_cookies.Remove( r );
				}

				return true;
			}
			#endregion

			#region FindCookieCleaningList
			private WeakReference FindCookieCleaningList( object cookie )
			{
				IList<WeakReference> toRemove = null;

				lock( _cookies )
				{
					try
					{
						foreach( WeakReference r in _cookies )
						{
							if( !r.IsAlive )
							{
								if( toRemove == null )
								{
									toRemove = new List<WeakReference>();
								}

								toRemove.Add( r );
							}
							else
							{
								if( r.Target == cookie )
								{
									return r;
								}
							}
						}
					}
					finally
					{
						if( toRemove != null )
						{
							foreach( WeakReference r in toRemove )
							{
								_cookies.Remove( r );
							}
						}
					}
				}

				return null;
			}
			#endregion
			#region CleanCookies()
			private void CleanCookies()
			{
				IList<WeakReference> toRemove = null;

				lock( _cookies )
				{
					foreach( WeakReference r in _cookies )
					{
						if( !r.IsAlive )
						{
							if( toRemove == null )
							{
								toRemove = new List<WeakReference>();
							}

							toRemove.Add( r );
						}
					}

					if( toRemove != null )
					{
						foreach( WeakReference r in toRemove )
						{
							_cookies.Remove( r );
						}
					}
				}
			}
			#endregion
		}
		#endregion

		#region IEnumerable<TObject> Members
		public IEnumerator<T> GetEnumerator()
		{
			foreach( WeakReference r in _objectCache.Values )
			{
				if( r.IsAlive )
				{
					yield return r.Target as T;
				}
			}
		}
		#endregion
		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}

	#region class XCacheObjectEventArgs
	public class XCacheObjectEventArgs<TObject> : EventArgs
		where TObject : XObject
	{
		#region Constructors/Disposer
		internal XCacheObjectEventArgs( XKey key, TObject @object )
		{
			Key = key;
			Object = @object;
		}
		#endregion

		#region Public Properties
		public XKey Key { get; private set; }
		public TObject Object { get; private set; }
		#endregion
	}
	#endregion
	#region class XCacheMatchEventArgs
	public class XCacheMatchEventArgs<TObject> : EventArgs
		where TObject : XObject
	{
		#region Constructors/Disposer
		public XCacheMatchEventArgs( XQuery<TObject> query, TObject @object )
		{
			Query = query;
			Object = @object;
		}
		#endregion

		#region Public Properties
		public XQuery<TObject> Query { get; private set; }
		public TObject Object { get; private set; }
		public bool Processed { get; set; }
		public bool Matched { get; set; }
		#endregion
	}
	#endregion
}
