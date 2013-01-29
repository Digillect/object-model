using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

using Digillect.Collections;

namespace Digillect
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	public class XCache<T> : IEnumerable<T>
		where T : XObject
	{
		private readonly IDictionary<XKey, WeakReference> _objectCache = new Dictionary<XKey, WeakReference>();
		private readonly IDictionary<XQuery<T>, XCachedQuery> _queryCache = new Dictionary<XQuery<T>, XCachedQuery>();

		#region Constructors/Disposer
		public XCache()
		{
			QueriesConversionEnabled = true;
		}
		#endregion

		#region Public Properties
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

		public IEnumerable<T> Cache( IEnumerable<T> collection )
		{
			if( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}

			Contract.Ensures( Contract.Result<IEnumerable<T>>() != null );

			return CacheCollectionEx( collection );
		}

		public bool RemoveFromCache( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			foreach ( XCachedQuery cq in _queryCache.Values )
			{
				cq.Items.Remove(key);
			}

			return UncacheObjectEx( key );
		}

		public bool ContainsKey( XKey key )
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

			return cq == null ? null : cq.ReadonlyItems;
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
			else if ( !this.QueriesConversionEnabled )
			{
				return null;
			}
			else
			{
				cq = _queryCache.Values.FirstOrDefault(x => x.Query.CanConvertTo(query));

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

			return cq.ReadonlyItems;
		}

		/// <summary>
		/// Кеширует переданный список и (если <paramref name="cookie"/> не равен <see langword="null"/>) создает
		/// <see cref="XCachedQuery"/> или добавляет <paramref name="cookie"/> к уже существующему.
		/// </summary>
		/// <returns>Возвращает список объектов.</returns>
		public IEnumerable<T> Cache(XQuery<T> query, IEnumerable<T> list, object cookie)
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			if( list == null )
			{
				throw new ArgumentNullException( "list" );
			}

			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			var cached = CacheCollectionEx( list );

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

			return cq.ReadonlyItems;
		}

		public bool RemoveFromCache( XQuery<T> query, object cookie )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			if ( !_queryCache.ContainsKey(query) )
			{
				return false;
			}

			XCachedQuery cq = _queryCache[query];

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

		public bool ContainsQuery( XQuery<T> query )
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			Contract.EndContractBlock();

			return ContainsQuery( query, false );
		}

		public bool ContainsQuery( XQuery<T> query, bool ignoreConversion )
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
		public event EventHandler<XCacheObjectEventArgs<T>> ObjectAddedToCache;
		public event EventHandler<XCacheObjectEventArgs<T>> ObjectRemovedFromCache;
		public event EventHandler<XCacheMatchEventArgs<T>> Match;

		protected virtual void OnObjectAddedToCache( XCacheObjectEventArgs<T> e )
		{
			if( ObjectAddedToCache != null )
			{
				ObjectAddedToCache( this, e );
			}
		}

		protected virtual void OnObjectRemovedFromCache( XCacheObjectEventArgs<T> e )
		{
			if( ObjectRemovedFromCache != null )
			{
				ObjectRemovedFromCache( this, e );
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

			OnObjectRemovedFromCache( new XCacheObjectEventArgs<T>( key, null ) );

			return null;
		}

		private T CacheObjectEx( T o )
		{
			XKey key = o.GetKey();

			lock( _objectCache )
			{
				_objectCache[key] = new WeakReference( o );
			}

			OnObjectAddedToCache( new XCacheObjectEventArgs<T>( key, o ) );

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

			OnObjectRemovedFromCache( new XCacheObjectEventArgs<T>( key, r.IsAlive ? (T) r.Target : null ) );

			return true;
		}

		private XCollection<T> CacheCollectionEx( IEnumerable<T> collection )
		{
			XCollection<T> cached = new XCollection<T>();

			foreach( T item in collection )
			{
				if( item != null )
				{
					cached.Add( Cache( item ) );
				}
			}

			return cached;
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
		private void ProcessCleanup()
		{
			List<XQuery<T>> queriesToRemove = new List<XQuery<T>>();
			List<XKey> objectsToRemove = new List<XKey>();

			lock( _objectCache )
			{
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
						OnObjectRemovedFromCache( new XCacheObjectEventArgs<T>( key, null ) );
						_objectCache.Remove( key );
					}
				}
			}
		}
		#endregion

		#region class XCachedQuery
		private sealed class XCachedQuery
		{
			private readonly XQuery<T> _query;
			private readonly IXList<T> _items;
			private readonly IXList<T> _readonlyItems;
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
				_items = collection;
				_readonlyItems = XCollectionsUtil.UnmodifiableList( collection );

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

			public IXList<T> ReadonlyItems
			{
				get { return _readonlyItems; }
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
