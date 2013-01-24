using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

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

		private int _count = -1;
#if CUSTOM_ENUMERATOR
		private int _version;
#endif

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

		/// <inheritdoc/>
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
		/// <inheritdoc/>
		public override int IndexOf(XKey key)
		{
			return CalculateFilteredIndex(this._originalCollection.IndexOf(key));
		}
		#endregion

		#region IXCollection`1 Members
		/// <inheritdoc/>
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <inheritdoc/>
		public override XBasedCollection<T> Clone(bool deep)
		{
			IXList<T> collection = deep ? (IXList<T>) this._originalCollection.Clone(true) : this._originalCollection;

			return CreateInstanceOfSameType( collection );
		}
		#endregion

		#region IXUpdatable`1 Members
		/// <inheritdoc/>
		public override void BeginUpdate()
		{
			_originalCollection.BeginUpdate();
		}

		/// <inheritdoc/>
		public override void EndUpdate()
		{
			_originalCollection.EndUpdate();
		}
		#endregion

		#region IList`1 Members
		/// <inheritdoc/>
		public override T this[int index]
		{
			get
			{
				var originalIndex = CalculateOriginalIndex( index );

				return originalIndex >= 0 ? this._originalCollection[originalIndex] : default( T );
			}
		}

		/// <inheritdoc/>
		public override int IndexOf(T item)
		{
			return CalculateFilteredIndex( this._originalCollection.IndexOf( item ) );
		}
		#endregion

		#region ICollection`1 Members
		/// <inheritdoc/>
		public override int Count
		{
			get
			{
				if ( _count == -1 )
				{
					Interlocked.Exchange(ref _count, _originalCollection.Count(Filter));
				}

				//Contract.Assume(_count >= 0);

				return _count;
			}
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return this._originalCollection.Contains(item) && Filter(item);
		}
		#endregion

		#region IEnumerable`1 Members
		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator()
		{
#if CUSTOM_ENUMERATOR
			int version = _version;

			for ( int i = 0; i < _originalCollection.Count; i++ )
			{
				if ( version != _version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				T obj = _originalCollection[i];

				if ( Filter(obj) )
				{
					yield return obj;
				}
			}
#else
			return _originalCollection.Where(Filter).GetEnumerator();
#endif
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

		[Pure]
		protected int CalculateFilteredIndex(int originalIndex)
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

		[Pure]
		protected int CalculateOriginalIndex(int filteredIndex)
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
		private void OriginalCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
#if CUSTOM_ENUMERATOR
			_version++;
#endif

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				_count = -1;
			}

			switch ( e.Action )
			{
#if NET40 && SILVERLIGHT
				case NotifyCollectionChangedAction.Add:
					if ( !Filter((T) e.NewItems[0]) )
					{
						return;
					}

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalculateFilteredIndex(e.NewStartingIndex)));
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					if ( !Filter((T) e.OldItems[0]) )
					{
						return;
					}

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalculateFilteredIndex(e.OldStartingIndex)));
					}

					break;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					if ( !Filter((T) e.NewItems[0]) && !Filter((T) e.OldItems[0]) )
					{
						return;
					}

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalculateFilteredIndex(e.NewStartingIndex)));
					}

					break;
#else
				case NotifyCollectionChangedAction.Add:
					T[] newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();

					if ( newItems.Length == 0 )
						return;

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, newItems, CalculateFilteredIndex(e.NewStartingIndex)));
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					T[] oldItems = e.OldItems.Cast<T>().Where(Filter).ToArray();

					if ( oldItems.Length == 0 )
						return;

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, oldItems, CalculateFilteredIndex(e.OldStartingIndex)));
					}

					break;

				case NotifyCollectionChangedAction.Move:
					// e.NewItems and e.OldItems have the same content
					newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();

					if ( newItems.Length == 0 )
						return;

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, newItems, CalculateFilteredIndex(e.NewStartingIndex), CalculateFilteredIndex(e.OldStartingIndex)));
					}

					break;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();
					oldItems = e.OldItems.Cast<T>().Where(Filter).ToArray();

					if ( newItems.Length == 0 && oldItems.Length == 0 )
						return;

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, newItems, oldItems, CalculateFilteredIndex(e.NewStartingIndex)));
					}

					break;
#endif
				case NotifyCollectionChangedAction.Reset:
					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action));
					}

					break;

				default:
					throw new ArgumentException(e.Action.ToString(), "e");
			}
		}

		private void OriginalCollection_Updated(object sender, EventArgs e)
		{
#if CUSTOM_ENUMERATOR
			_version++;
#endif
			_count = -1;

			OnPropertyChanged(CountString);
			OnPropertyChanged(IndexerName);
			OnUpdated(EventArgs.Empty);
		}
		#endregion

#if DEBUG || CONTRACTS_FULL
		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_count >= -1);
		}
		#endregion
#endif

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
