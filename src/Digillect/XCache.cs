using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	public class XCache<TId, TObject, TCollection> : IDisposable, IEnumerable<TObject>
		where TId : IEquatable<TId>, IComparable<TId>
		where TObject : XObject<TId>
		where TCollection : IXCollection<TObject>, new()
	{
		private readonly IDictionary<TId, WeakReference> _objectCache = new Dictionary<TId, WeakReference>();
		private readonly IDictionary<XQuery<TObject>, XCachedQuery> _queryCache = new Dictionary<XQuery<TObject>, XCachedQuery>();
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
		/// Возвращает существующий объект из кеша.
		/// </summary>
		public TObject Get( TId id )
		{
			return GetCachedObjectEx( id );
		}

		public TObject CacheObject( TObject obj )
		{
			if( obj == null )
			{
				throw new ArgumentNullException( "obj" );
			}

			TObject cached = GetCachedObjectEx( obj.Id );

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
				var eventArgs = new XCacheMatchEventArgs<TObject>( cq.Query, cached );
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
					if( !cq.List.Any( o => cached.Id.Equals( o.Id ) ) )
					{
						cq.List.Add( cached );
					}
				}
				else
				{
					if( cq.List.Any( o => cached.Id.Equals( o.Id ) ) )
					{
						cq.List.Remove( cached.GetKey() );
					}
				}
			}

			return cached;
		}

		public TCollection CacheCollection( IEnumerable<TObject> collection )
		{
			if( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			TCollection cached = new TCollection();

			foreach( TObject o in collection )
			{
				cached.Add( CacheObject( o ) );
			}

			return cached;
		}

		public bool UncacheObject( TId id )
		{
			foreach( XCachedQuery cq in new List<XCachedQuery>( _queryCache.Values ) )
			{
				var cached = cq.List.FirstOrDefault( o => id.Equals( o.Id ) );

				if( cached != default( TObject ) )
				{
					cq.List.Remove( cached );
				}
			}

			return UncacheObjectEx( id );
		}

		public bool Contains( TId id )
		{
			return GetCachedObjectEx( id ) != null;
		}
		#endregion
		#region Public Methods : Queries
		/// <summary>
		/// Возвращает результаты запроса, если запрос был прокеширован, иначе <see langword="null"/>.
		/// </summary>
		public TCollection Get( XQuery<TObject> query )
		{
			XCachedQuery cq = GetCachedQuery( query );

			return cq == null ? default( TCollection ) : cq.List;
		}

		/// <summary>
		/// Возвращает результаты запроса, если они есть в кеше. Если <paramref name="cookie"/> не равен <see langword="null"/>,
		/// то cookie добавляется к соответствующему <see cref="XCachedQuery"/>.
		/// </summary>
		public TCollection Get( XQuery<TObject> query, object cookie )
		{
			XCachedQuery cq = GetCachedQuery( query );

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
					return default( TCollection );
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
					return default( TCollection );
				}

				TCollection converted = ConvertQueryResults( cq.List, query );

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

			return cq.List;
		}

		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public XCachedQuery GetCachedQuery( XQuery<TObject> query )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			return _queryCache.ContainsKey( query ) ? _queryCache[query] : null;
		}

		[EditorBrowsable( EditorBrowsableState.Advanced )]
		public XCachedQuery AddCachedQuery( XQuery<TObject> query, TCollection list, object cookie )
		{
			XCachedQuery cq = GetCachedQuery( query );

			if( cq != null )
			{
				cq.AddCookie( cookie );
			}
			else
			{
				if( Equals( list, default( TCollection ) ) )
				{
					list = new TCollection();
				}

				cq = new XCachedQuery( query, list, cookie );

				lock( _queryCache )
				{
					_queryCache.Add( cq.Query, cq );
				}
			}

			return cq;
		}

		/// <summary>
		/// Кеширует переданный список и (если <paramref name="cookie"/> не равен <see langword="null"/>) создает
		/// <see cref="XCachedQuery"/> или добавляет <paramref name="cookie"/> к уже существующему.
		/// </summary>
		/// <returns>Возвращает список объектов.</returns>
		public TCollection CacheQuery( XQuery<TObject> query, IEnumerable<TObject> list, object cookie )
		{
			TCollection cached = CacheCollection( list );

			XCachedQuery cq = GetCachedQuery( query );

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

			return cq.List;
		}

		public bool ReleaseCachedQuery( XQuery<TObject> query, object cookie )
		{
			XCachedQuery cq = GetCachedQuery( query );

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

		public bool Contains( XQuery<TObject> query )
		{
			return Contains( query, false );
		}

		public bool Contains( XQuery<TObject> query, bool ignoreConversion )
		{
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
		public event EventHandler<XCacheObjectEventArgs<TObject>> ObjectCached;
		//public event EventHandler<XCacheObjectEventArgs<TObject>> ObjectUncached;
		public event EventHandler<XCacheMatchEventArgs<TObject>> Match;

		protected virtual void OnObjectCached( XCacheObjectEventArgs<TObject> e )
		{
			if( ObjectCached != null )
			{
				ObjectCached( this, e );
			}
		}

#if false
		protected virtual void OnObjectUncached( XCacheObjectEventArgs<TObject> e )
		{
			if( ObjectUncached != null )
			{
				ObjectUncached( this, e );
			}
		}
#endif

		protected virtual void OnMatch( XCacheMatchEventArgs<TObject> e )
		{
			if( Match != null )
			{
				Match( this, e );
			}
		}
		#endregion

		#region Object Cache Private Methods
		private TObject GetCachedObjectEx( TId id )
		{
			if( !_objectCache.ContainsKey( id ) )
			{
				return null;
			}

			WeakReference r = _objectCache[id];

			if( r.IsAlive )
			{
				return (TObject) r.Target;
			}

			lock( _objectCache )
			{
				_objectCache.Remove( id );
			}

			return null;
		}

		private TObject CacheObjectEx( TObject o )
		{
			lock( _objectCache )
			{
				_objectCache[o.Id] = new WeakReference( o );
			}

			OnObjectCached( new XCacheObjectEventArgs<TObject>( o ) );

			return o;
		}

		private bool UncacheObjectEx( TId id )
		{
			WeakReference r;

			lock( _objectCache )
			{
				if( !_objectCache.ContainsKey( id ) )
				{
					return false;
				}

				r = _objectCache[id];

				if( !_objectCache.Remove( id ) )
				{
					return false;
				}
			}

			//OnObjectUncached( new XCacheObjectEventArgs<TId, TObject>( id, r.IsAlive ? (TObject) r.Target : null ) );

			return true;
		}
		#endregion
		#region Queries Private Methods
		private static TCollection ConvertQueryResults( IEnumerable<TObject> original, XQuery<TObject> query )
		{
			TCollection result = new TCollection();

			foreach( TObject o in original )
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

			List<XQuery<TObject>> queriesToRemove = new List<XQuery<TObject>>();
			List<TId> objectsToRemove = new List<TId>();

			lock( _objectCache )
			{
				_cleanupInProgress = true;

				lock( _queryCache )
				{
					foreach( KeyValuePair<XQuery<TObject>, XCachedQuery> entry in _queryCache )
					{
						if( entry.Value.CookiesCount == 0 )
						{
							queriesToRemove.Add( entry.Key );
						}
					}

					foreach( KeyValuePair<TId, WeakReference> entry in _objectCache )
					{
						if( !entry.Value.IsAlive )
						{
							objectsToRemove.Add( entry.Key );
						}
					}

					foreach( XQuery<TObject> key in queriesToRemove )
					{
						_queryCache.Remove( key );
					}

					foreach( TId key in objectsToRemove )
					{
						//OnObjectUncached( new XCacheObjectEventArgs<TId, TObject>( key, null ) );
						_objectCache.Remove( key );
					}
				}

				_cleanupInProgress = false;
			}
		}
		#endregion

		#region class XCachedQuery
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible" )]
		public sealed class XCachedQuery
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
			private readonly XQuery<TObject> _query;
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
			private readonly TCollection _list;
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields" )]
			private readonly IList<WeakReference> _cookies = new List<WeakReference>();

			#region Constructors/Disposer
			public XCachedQuery( XQuery<TObject> query, TCollection list, object cookie )
			{
				if( query == null )
				{
					throw new ArgumentNullException( "query" );
				}

				if( list == null )
				{
					throw new ArgumentNullException( "list" );
				}

				_query = query.Clone();
				_list = list;

				AddCookie( cookie );
			}
			#endregion

			#region Public Properties
			public XQuery<TObject> Query
			{
				get { return _query; }
			}

			public TCollection List
			{
				get { return _list; }
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
		public IEnumerator<TObject> GetEnumerator()
		{
			foreach( WeakReference r in _objectCache.Values )
			{
				if( r.IsAlive )
				{
					yield return r.Target as TObject;
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
		internal XCacheObjectEventArgs( TObject @object )
		{
			Object = @object;
		}
		#endregion

		#region Public Properties
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

	public class XCache<TId, TObject> : XCache<TId, TObject, XCollection<TObject>>
		where TId : IEquatable<TId>, IComparable<TId>
		where TObject : XObject<TId>
	{
	}
}
