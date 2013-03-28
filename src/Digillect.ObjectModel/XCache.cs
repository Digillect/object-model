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
#if NET45
		private readonly IDictionary<XKey, WeakReference<T>> _objectCache = new Dictionary<XKey, WeakReference<T>>();
#else
		private readonly IDictionary<XKey, WeakReference> _objectCache = new Dictionary<XKey, WeakReference>();
#endif
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

		#region Protected Properties
		[EditorBrowsable(EditorBrowsableState.Never)]
#if NET45
		protected IDictionary<XKey, WeakReference<T>> ObjectCache
#else
		protected IDictionary<XKey, WeakReference> ObjectCache
#endif
		{
			get { return _objectCache; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected IDictionary<XQuery<T>, XCachedQuery> QueryCache
		{
			get { return _queryCache; }
		}
		#endregion

		#region IEnumerable`1 Members
		public IEnumerator<T> GetEnumerator()
		{
			foreach ( var r in _objectCache.Values )
			{
#if NET45
				T target;

				if ( r.TryGetTarget(out target) )
				{
					yield return target;
				}
#else
				if ( r.IsAlive )
				{
					yield return r.Target as T;
				}
#endif
			}
		}
		#endregion

		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Public Methods : Get
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

			return GetCachedObject(key);
		}

		/// <summary>
		/// Returns cached query result, if any.
		/// </summary>
		public IXList<T> Get(XQuery<T> query)
		{
			Contract.Requires(query != null);

			return Get(query, null, true);
		}

		public IXList<T> Get(XQuery<T> query, object cookie)
		{
			Contract.Requires(query != null);

			return Get(query, cookie, false);
		}

		public IXList<T> Get(XQuery<T> query, object cookie, bool suppressConversion)
		{
			if ( query == null )
			{
				throw new ArgumentNullException("query");
			}

			Contract.EndContractBlock();

			XCachedQuery cq;

			if ( _queryCache.TryGetValue(query, out cq) )
			{
				if ( cookie != null )
				{
					cq.AddCookie(cookie);
				}

				return cq.Items;
			}

			if ( !suppressConversion && this.QueriesConversionEnabled )
			{
				XCachedQuery candidate = _queryCache.Values.FirstOrDefault(x => x.Query.CanConvertTo(query));

				if ( candidate != null )
				{
					var converted = candidate.Items.Where(x => query.Match(x));

#if NET45
					Contract.Assume(converted != null);
#endif
					Contract.Assume(Contract.ForAll(converted, item => item != null));

					var items = new XCollection<T>(converted);

					if ( cookie == null )
					{
						return items;
					}

					cq = new XCachedQuery(query, items, cookie);

					lock ( _queryCache )
					{
						_queryCache.Add(cq.Query, cq);
					}

					return cq.Items;
				}
			}

			return null;
		}
		#endregion

		#region Public Methods : Cache
		/// <summary>
		/// Stores object in the cache or updates an existing cached object.
		/// </summary>
		/// <param name="object">Object to cache.</param>
		/// <returns>Cached object.</returns>
		public T Cache(T @object)
		{
			if ( @object == null )
			{
				throw new ArgumentNullException("object");
			}

			Contract.Ensures( Contract.Result<T>() != null );

			XKey key = @object.GetKey();
			T cached = GetCachedObject(key);

			if ( cached == null )
			{
				lock ( _objectCache )
				{
#if NET45
					_objectCache[@object.GetKey()] = new WeakReference<T>(@object);
#else
					_objectCache[@object.GetKey()] = new WeakReference(@object);
#endif
				}

				OnObjectAddedToCache(new XCacheObjectEventArgs<T>(@object));

				cached = @object;
			}
			else
			{
				cached.Update(@object);
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
				else if ( cq.Query.SupportsMatch )
				{
					match = cq.Query.Match(cached);
				}
				else
				{
					continue;
				}

				if( match )
				{
					cq.AddItem(cached);
				}
				else
				{
					cq.RemoveItem(key);
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

			return CacheCollectionCore(collection);
		}

		/// <summary>
		/// �������� ���������� ������ � (���� <paramref name="cookie"/> �� ����� <see langword="null"/>) �������
		/// <see cref="XCachedQuery"/> ��� ��������� <paramref name="cookie"/> � ��� �������������.
		/// </summary>
		/// <returns>���������� ������ ��������.</returns>
		public IEnumerable<T> Cache(IEnumerable<T> collection, XQuery<T> query, object cookie)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			if ( query == null )
			{
				throw new ArgumentNullException("query");
			}

			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			var cached = CacheCollectionCore(collection);

			XCachedQuery cq;

			if ( _queryCache.TryGetValue(query, out cq) )
			{
				if ( cookie != null )
				{
					cq.AddCookie(cookie);
				}

				return cq.Items;
			}

			if ( cookie != null )
			{
				cq = new XCachedQuery(query, new XCollection<T>(cached), cookie);

				lock ( _queryCache )
				{
					_queryCache.Add(cq.Query, cq);
				}

				return cq.Items;
			}

			return cached;
		}
		#endregion

		#region Public Methods : Contains
		public bool ContainsKey( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			return GetCachedObject(key) != null;
		}

		public bool ContainsQuery( XQuery<T> query )
		{
			Contract.Requires(query != null);

			return ContainsQuery( query, false );
		}

		public bool ContainsQuery(XQuery<T> query, bool suppressConversion)
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

			if ( !suppressConversion && this.QueriesConversionEnabled )
			{
				lock ( _queryCache )
				{
					return _queryCache.Values.Any(x => x.Query.CanConvertTo(query));
				}
			}

			return false;
		}
		#endregion

		#region Public Methods : Uncache
		public bool RemoveFromCache( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			foreach ( XCachedQuery cq in _queryCache.Values )
			{
				cq.RemoveItem(key);
			}

			//return UncacheObjectCore( key );
#if NET45
			WeakReference<T> r;
#else
			WeakReference r;
#endif

			lock ( _objectCache )
			{
				if ( !_objectCache.TryGetValue(key, out r) )
				{
					return false;
				}

				if ( !_objectCache.Remove(key) )
				{
					return false;
				}
			}

#if NET45
			T target;

			r.TryGetTarget(out target);
#else
			T target = r.IsAlive ? (T) r.Target : null;
#endif

			OnObjectRemovedFromCache(target != null ? new XCacheObjectEventArgs<T>(target) : new XCacheObjectEventArgs<T>(key));

			return true;
		}

		/// <summary>
		/// Releases the previously cached query by removing the locking <paramref name="cookie"/>.
		/// </summary>
		/// <param name="query">The query to release.</param>
		/// <param name="cookie">An object which locks the query in the cache.</param>
		/// <returns><c>true</c> if the query was finally removed from the cache (of didn't exist there at all); <c>false</c> if only the <paramref name="cookie"/> was removed.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="query"/> parameter or <paramref name="cookie"/> parameter is null.</exception>
		/// <seealso cref="Cache(IEnumerable{T},XQuery{T},object)"/>
		public bool ReleaseCachedQuery(XQuery<T> query, object cookie)
		{
			if( query == null )
			{
				throw new ArgumentNullException( "query" );
			}

			if ( cookie == null )
			{
				throw new ArgumentNullException("cookie");
			}

			Contract.EndContractBlock();

			XCachedQuery cq;

			if ( _queryCache.TryGetValue(query, out cq) )
			{
				cq.RemoveCookie(cookie);

				if ( cq.CookiesCount == 0 )
				{
					lock ( _queryCache )
					{
						return _queryCache.Remove(query);
					}
				}
			}

			return true;
		}
		#endregion

		#region Cache Cleanup Methods
		public void Cleanup()
		{
			ProcessCleanup();
		}

		private void ProcessCleanup()
		{
			lock ( _objectCache )
			{
				lock ( _queryCache )
				{
					_queryCache.RemoveAll(x => x.Value.CookiesCount == 0);

#if NET45
					T target;

					var objectsToRemove = _objectCache.Where(x => !x.Value.TryGetTarget(out target)).Select(x => x.Key).ToArray();
#else
					var objectsToRemove = _objectCache.Where(x => !x.Value.IsAlive).Select(x => x.Key).ToArray();
#endif

					Contract.Assume(Contract.ForAll(objectsToRemove, item => item != null));

					foreach ( var key in objectsToRemove )
					{
						OnObjectRemovedFromCache(new XCacheObjectEventArgs<T>(key));
						_objectCache.Remove(key);
					}
				}
			}
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

		#region Private Infrastructure Methods
		private T GetCachedObject(XKey key)
		{
			Contract.Requires(key != null);

#if NET45
			WeakReference<T> r;
#else
			WeakReference r;
#endif

			if ( _objectCache.TryGetValue(key, out r) )
			{
#if NET45
				T target;

				if ( r.TryGetTarget(out target) )
				{
					return target;
				}
#else
				if ( r.IsAlive )
				{
					return (T) r.Target;
				}
#endif

				lock ( _objectCache )
				{
					_objectCache.Remove(key);
				}

				OnObjectRemovedFromCache(new XCacheObjectEventArgs<T>(key));
			}

			return null;
		}

		private IEnumerable<T> CacheCollectionCore(IEnumerable<T> collection)
		{
			Contract.Requires(collection != null);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
			Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<T>>(), item => item != null));

			return collection.Where(x => x != null).Select(x => Cache(x)).ToArray();
		}
		#endregion

		#region class XCachedQuery
		protected sealed class XCachedQuery
		{
			private readonly XQuery<T> _query;
			private readonly IXList<T> _items;
			private readonly IXList<T> _readonlyItems;
			private readonly IList<WeakReference> _cookies = new List<WeakReference>();

			#region Constructors/Disposer
			public XCachedQuery( XQuery<T> query, IXList<T> items, object cookie )
			{
				if( query == null )
				{
					throw new ArgumentNullException( "query" );
				}

				if ( items == null )
				{
					throw new ArgumentNullException("items");
				}

				if ( cookie == null )
				{
					throw new ArgumentNullException("cookie");
				}

				Contract.EndContractBlock();

				_query = query.Clone();
				_items = items;
				_readonlyItems = XCollectionsUtil.UnmodifiableList( items );

				AddCookie( cookie );
			}
			#endregion

			#region Public Properties
			public XQuery<T> Query
			{
				get
				{
					Contract.Ensures(Contract.Result<XQuery<T>>() != null);

					return _query;
				}
			}

			public IXList<T> Items
			{
				get
				{
					Contract.Ensures(Contract.Result<IXList<T>>() != null);

					return _readonlyItems;
				}
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
			public bool AddItem(T item)
			{
				if ( !_items.Contains(item) )
				{
					_items.Add(item);

					return true;
				}

				return false;
			}

			public bool RemoveItem(XKey key)
			{
				Contract.Requires(key != null);

				return _items.Remove(key);
			}

			public bool AddCookie( object cookie )
			{
				if( cookie == null )
				{
					throw new ArgumentNullException("cookie");
				}

				Contract.EndContractBlock();

				if ( FindCookieWithCleanup(cookie) != null )
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
					throw new ArgumentNullException("cookie");
				}

				Contract.EndContractBlock();

				WeakReference r = FindCookieWithCleanup( cookie );

				if( r == null )
				{
					return false;
				}

				lock( _cookies )
				{
					return _cookies.Remove(r);
				}
			}
			#endregion

			#region Private Methods
			private WeakReference FindCookieWithCleanup(object cookie)
			{
				ICollection<WeakReference> toRemove = null;

				lock( _cookies )
				{
					try
					{
						foreach( WeakReference r in _cookies )
						{
							if( !r.IsAlive )
							{
								(toRemove ?? (toRemove = new List<WeakReference>())).Add(r);
							}
							else if ( r.Target == cookie )
							{
								return r;
							}
						}
					}
					finally
					{
						_cookies.RemoveAll(toRemove);
					}
				}

				return null;
			}

			private void CleanCookies()
			{
				lock( _cookies )
				{
					_cookies.RemoveAll(r => !r.IsAlive);
				}
			}
			#endregion
		}
		#endregion
	}

	#region class XCacheObjectEventArgs
	public sealed class XCacheObjectEventArgs<T> : EventArgs
		where T : XObject
	{
		#region Constructors/Disposer
		internal XCacheObjectEventArgs(XKey key)
		{
			if ( key == null )
			{
				throw new ArgumentNullException("key");
			}

			Contract.EndContractBlock();

			this.Key = key;
		}

		internal XCacheObjectEventArgs(T @object)
		{
			if ( @object == null )
			{
				throw new ArgumentNullException("object");
			}

			Contract.EndContractBlock();

			this.Key = @object.GetKey();
			this.Object = @object;
		}
		#endregion

		#region Public Properties
		public XKey Key
		{
			get;
			private set;
		}

		public T Object
		{
			get;
			private set;
		}
		#endregion

		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.Key != null);
			//Contract.Invariant(this.Object == null || this.Key == this.Object.GetKey());
		}
	}
	#endregion

	#region class XCacheMatchEventArgs
	public class XCacheMatchEventArgs<T> : EventArgs
		where T : XObject
	{
		#region Constructors/Disposer
		public XCacheMatchEventArgs(XQuery<T> query, T @object)
		{
			if ( query == null )
			{
				throw new ArgumentNullException("query");
			}

			if ( @object == null )
			{
				throw new ArgumentNullException("object");
			}

			Contract.EndContractBlock();

			this.Query = query;
			this.Object = @object;
		}
		#endregion

		#region Public Properties
		public XQuery<T> Query
		{
			get;
			private set;
		}

		public T Object
		{
			get;
			private set;
		}

		public bool Processed
		{
			get;
			set;
		}

		public bool Matched
		{
			get;
			set;
		}
		#endregion

		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.Query != null);
			Contract.Invariant(this.Object != null);
		}
	}
	#endregion
}
