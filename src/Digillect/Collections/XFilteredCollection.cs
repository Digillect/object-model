using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
#if DEBUG || CONTRACTS_FULL
	[ContractClass(typeof(XFilteredCollectionContract<>))]
#endif
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public abstract class XFilteredCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IXList<T> _originalCollection;

		private int count = -1;
		private int version;

		#region Constructor/Disposer
		protected XFilteredCollection(IXList<T> originalCollection)
		{
			if ( originalCollection == null )
			{
				throw new ArgumentNullException("originalCollection");
			}

			Contract.EndContractBlock();

			this._originalCollection = originalCollection;

			this._originalCollection.CollectionChanged += OriginalCollection_CollectionChanged;
			_originalCollection.Updated += OriginalCollection_Updated;
		}

		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				this._originalCollection.CollectionChanged -= OriginalCollection_CollectionChanged;
				_originalCollection.Updated -= OriginalCollection_Updated;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region OriginalCollection
		public IXList<T> OriginalCollection
		{
			get { return this._originalCollection; }
		}
		#endregion

		#region IXList`1 Members
		public override int IndexOf(XKey key)
		{
			return CalcFilteredIndex(this._originalCollection.IndexOf(key));
		}
		#endregion

		#region IXCollection`1 Members
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		public override bool ContainsKey(XKey key)
		{
			return IndexOf(key) != -1;
		}

		public override IEnumerable<XKey> GetKeys()
		{
			return this.Select(x => x.GetKey());
		}

		public override XBasedCollection<T> Clone(bool deep)
		{
			Contract.Ensures(Contract.Result<IXCollection<T>>() != null);

			IXList<T> collection = deep ? (IXList<T>) this._originalCollection.Clone(true) : this._originalCollection;

			return CreateInstanceOfSameType( collection );
		}
		#endregion

		#region IXUpdatable`1 Members
		public override event EventHandler Updated;

		public override void BeginUpdate()
		{
			_originalCollection.BeginUpdate();
		}

		public override void EndUpdate()
		{
			_originalCollection.EndUpdate();
		}
		#endregion

		#region IList`1 Members
		public override T this[int index]
		{
			get
			{
				var originalIndex = CalcOriginalIndex( index );

				return originalIndex >= 0 ? this._originalCollection[originalIndex] : default( T );
			}
		}

		public override int IndexOf(T item)
		{
			Contract.Ensures( Contract.Result<int>() >= -1 );

			return CalcFilteredIndex( this._originalCollection.IndexOf( item ) );
		}
		#endregion

		#region ICollection`1 Members
		public override int Count
		{
#if WINDOWS_PHONE && CODE_ANALYSIS
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
			get
			{
				if ( this.count == -1 )
				{
					lock ( this._originalCollection )
					{
						if ( this.count == -1 )
						{
							this.count = this._originalCollection.Count(Filter);
						}
					}
				}

				Contract.Assume(this.count >= 0);

				return this.count;
			}
		}

#if WINDOWS_PHONE && CODE_ANALYSIS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
		public override bool Contains(T item)
		{
			bool contains = this._originalCollection.Contains(item) && Filter(item);

			if ( contains )
			{
				Contract.Assume(this.Count > 0);
			}

			return contains;
			//return this.originalCollection.Contains(item) && Filter(item, this.originalCollection.IndexOf(item));
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			if ( array == null )
			{
				throw new ArgumentNullException("array");
			}

			foreach ( T obj in this )
			{
				array[arrayIndex++] = obj;
			}
		}
		#endregion

		#region IEnumerable`1 Members
		public override IEnumerator<T> GetEnumerator()
		{
			int version = this.version;

			for ( int i = 0; i < _originalCollection.Count; i++ )
			{
				if ( this.version != version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				T obj = _originalCollection[i];

				if ( Filter(obj) )
				{
					yield return obj;
				}
			}
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// This method supports the <see cref="Clone"/> infrastructure.
		/// </summary>
		/// <param name="originalCollection">The original collection used to construct a filtered one.</param>
		/// <returns>New collection identical to the current one.</returns>
		[Pure]
		protected abstract XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> originalCollection);

		/// <summary>
		/// Determines whether an item satisfies a filtering criteria.
		/// </summary>
		/// <param name="obj">An item to check.</param>
		/// <returns><see langword="true"/> if the item passes the filter; otherwise, <see langword="false"/>.</returns>
		[Pure]
		protected abstract bool Filter(T obj);

		protected int CalcFilteredIndex(int originalIndex)
		{
			Contract.Ensures(Contract.Result<int>() >= -1);

			if ( originalIndex < 0 )
			{
				return -1;
			}

			int filteredIndex = -1;

			for ( int i = 0; i <= originalIndex; i++ )
			{
				if ( Filter(this._originalCollection[i]) )
				{
					filteredIndex++;
				}
			}

			return filteredIndex;
		}

		protected int CalcOriginalIndex(int filteredIndex)
		{
			if ( filteredIndex < 0 )
			{
				return -1;
			}

			int originalIndex = -1;

			for ( int i = 0, counter = 0; i < this._originalCollection.Count && counter <= filteredIndex; i++ )
			{
				if ( Filter(this._originalCollection[i]) )
				{
					originalIndex = i;
					counter++;
				}
			}

			return originalIndex;
		}
		#endregion

		#region Event Handlers
#if WINDOWS_PHONE && CODE_ANALYSIS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
		private void OriginalCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			version++;

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				count = -1;
			}

			if ( CollectionChanged != null )
			{
				NotifyCollectionChangedEventArgs args;

				switch ( e.Action )
				{
#if SILVERLIGHT
					case NotifyCollectionChangedAction.Add:
						Contract.Assume(e.NewItems.Count > 0);
						if ( !Filter((T) e.NewItems[0]) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalcFilteredIndex(e.NewStartingIndex));
						break;
					case NotifyCollectionChangedAction.Remove:
						Contract.Assume(e.OldItems.Count > 0);
						if ( !Filter((T) e.OldItems[0]) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalcFilteredIndex(e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						Contract.Assume(e.NewItems.Count > 0);
						Contract.Assume(e.OldItems.Count > 0);
						if ( !Filter((T) e.NewItems[0]) && !Filter((T) e.OldItems[0]) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalcFilteredIndex(e.NewStartingIndex));
						break;
#else
					case NotifyCollectionChangedAction.Add:
						Contract.Assume( e.NewItems != null );

						var newItems = e.NewItems.Cast<T>().Where( Filter ).ToArray();

						if( newItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, newItems, CalcFilteredIndex( e.NewStartingIndex ) );

						break;

					case NotifyCollectionChangedAction.Remove:
						Contract.Assume( e.OldItems != null );

						var oldItems = e.OldItems.Cast<T>().Where( Filter ).ToArray();

						if( oldItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, oldItems, CalcFilteredIndex( e.OldStartingIndex ) );
						break;

					case NotifyCollectionChangedAction.Move:
						// e.NewItems and e.OldItems have the same content
						Contract.Assume( e.NewItems != null );

						newItems = e.NewItems.Cast<T>().Where( Filter ).ToArray();

						if( newItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, newItems, CalcFilteredIndex( e.NewStartingIndex ), CalcFilteredIndex( e.OldStartingIndex ) );
						break;

					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						Contract.Assume( e.NewItems != null );
						Contract.Assume( e.OldItems != null );

						newItems = e.NewItems.Cast<T>().Where( Filter ).ToArray();
						oldItems = e.OldItems.Cast<T>().Where( Filter ).ToArray();

						if( newItems.Length == 0 && oldItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, newItems, oldItems, CalcFilteredIndex( e.NewStartingIndex ) );
						break;
#endif
					case NotifyCollectionChangedAction.Reset:
						args = new NotifyCollectionChangedEventArgs(e.Action);
						break;
					default:
						throw new ArgumentException(e.Action.ToString(), "e");
				}

				CollectionChanged(this, args);
			}
		}

		private void OriginalCollection_Updated(object sender, EventArgs e)
		{
			version++;
			count = -1;

			if ( Updated != null )
			{
				Updated(this, EventArgs.Empty);
			}
		}
		#endregion
	}

	#region XFilteredCollection`1 contract binding
#if DEBUG || CONTRACTS_FULL
	[ContractClassFor(typeof(XFilteredCollection<>))]
	abstract class XFilteredCollectionContract<T> : XFilteredCollection<T>
		where T : XObject
	{
		protected XFilteredCollectionContract()
			: base(null)
		{
		}

		protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> originalCollection)
		{
			Contract.Ensures(Contract.Result<XFilteredCollection<T>>() != null);

			return null;
		}
	}
#endif
	#endregion
}
