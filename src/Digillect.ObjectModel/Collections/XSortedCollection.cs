#region Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XSortedCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private IXList<T> _originalCollection;
		private readonly Comparison<T> _itemsComparison;

		private readonly List<T> _sortedItems;

		#region Constructor/Disposer
		public XSortedCollection(Comparison<T> itemsComparison)
		{
			if ( itemsComparison == null )
			{
				throw new ArgumentNullException("itemsComparison");
			}

			Contract.EndContractBlock();

			_itemsComparison = itemsComparison;
		}

		public XSortedCollection(IXList<T> originalCollection, Comparison<T> itemsComparison)
			: this(itemsComparison)
		{
			if ( originalCollection == null )
			{
				throw new ArgumentNullException("originalCollection");
			}

			Contract.Requires(itemsComparison != null);

			_originalCollection = originalCollection;

			_sortedItems = new List<T>(_originalCollection.Count);

			InitSortedItems();

			_originalCollection.CollectionChanged += OriginalCollection_CollectionChanged;
		}

		public XSortedCollection(IComparer<T> itemsComparer)
			: this(itemsComparer == null ? null : new Comparison<T>(itemsComparer.Compare))
		{
		}

		public XSortedCollection(IXList<T> originalCollection, IComparer<T> itemsComparer)
			: this(originalCollection, itemsComparer == null ? null : new Comparison<T>(itemsComparer.Compare))
		{
			Contract.Requires(originalCollection != null);
			Contract.Requires(itemsComparer != null);
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				if ( _originalCollection != null )
				{
					_originalCollection.CollectionChanged -= OriginalCollection_CollectionChanged;

					CleanSortedItems();
				}
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Public Properties
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IXList<T> OriginalCollection
		{
			get { return _originalCollection; }
			set
			{
				if ( value == null )
				{
					throw new ArgumentNullException("value");
				}

				Contract.EndContractBlock();

				if ( _originalCollection != value )
				{
					if ( _originalCollection != null )
					{
						_originalCollection.CollectionChanged -= OriginalCollection_CollectionChanged;

						CleanSortedItems();
					}

					_originalCollection = value;

					InitSortedItems();

					_originalCollection.CollectionChanged += OriginalCollection_CollectionChanged;

					OnPropertyChanged((string) null);
					OnCollectionReset();
				}
			}
		}
		#endregion

		#region XBasedCollection`1 Overrides
		/// <inheritdoc/>
		public override int Count
		{
			get { return _originalCollection.Count; }
		}

		/// <inheritdoc/>
		public override T this[int index]
		{
			get { return _sortedItems[index]; }
		}

		/// <inheritdoc/>
		public override void BeginUpdate()
		{
			_originalCollection.BeginUpdate();
		}

		/// <inheritdoc/>
		public override XBasedCollection<T> Clone(bool deep)
		{
			return new XSortedCollection<T>(deep? (IXList<T>) _originalCollection.Clone(true) : _originalCollection, _itemsComparison);
		}

		/// <inheritdoc/>
		public override bool Contains(T item)
		{
			return _originalCollection.Contains(item);
		}

		/// <inheritdoc/>
		public override bool ContainsKey(XKey key)
		{
			return _originalCollection.ContainsKey(key);
		}

		/// <inheritdoc/>
		public override void EndUpdate()
		{
			_originalCollection.EndUpdate();
		}

		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator()
		{
			return _sortedItems.GetEnumerator();
		}

		/// <inheritdoc/>
		public override int IndexOf(T item)
		{
			return _sortedItems.IndexOf(item);
		}

		/// <inheritdoc/>
		public override int IndexOf(XKey key)
		{
			return _sortedItems.FindIndex(x => x.GetKey() == key);
		}
		#endregion

		#region Protected Methods
		protected void InitSortedItems()
		{
			_sortedItems.AddRange(_originalCollection);
			_sortedItems.Sort(_itemsComparison);
			_sortedItems.ForEach(x => x.PropertyChanged += Item_PropertyChanged);
		}

		protected void CleanSortedItems()
		{
			_sortedItems.ForEach(x => x.PropertyChanged -= Item_PropertyChanged);
			_sortedItems.Clear();
		}

		protected int InsertNewItemsSorted(IList newItems)
		{
			if ( newItems == null )
			{
				throw new ArgumentNullException("newItems");
			}

			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() <= Contract.OldValue(_sortedItems.Count));

			int minIndex = Int32.MaxValue;

			foreach ( T item in newItems )
			{
				int index;

				for ( index = 0; index < _sortedItems.Count; index++ )
				{
					if ( _itemsComparison(item, _sortedItems[index]) < 0 )
					{
						break;
					}
				}

				if ( index < minIndex )
				{
					minIndex = index;
				}

				_sortedItems.Insert(index, item);

				item.PropertyChanged += Item_PropertyChanged;
			}

			return minIndex;
		}

		protected int RemoveOldItems(IList oldItems)
		{
			if ( oldItems == null )
			{
				throw new ArgumentNullException("oldItems");
			}

			Contract.Ensures(Contract.Result<int>() >= 0);
			Contract.Ensures(Contract.Result<int>() < Contract.OldValue(_sortedItems.Count));

			int minIndex = Int32.MaxValue;

			foreach ( T item in oldItems )
			{
				item.PropertyChanged -= Item_PropertyChanged;

				int index = _sortedItems.IndexOf(item);

				if ( index < minIndex )
				{
					minIndex = index;
				}

				_sortedItems.RemoveAt(index);
			}

			return minIndex;
		}
		#endregion

		#region Event Handlers
		private void OriginalCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					int newStartingIndex = InsertNewItemsSorted(e.NewItems);

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( e.NewItems.Count == 1 )
					{
						OnCollectionChanged(() => {
							return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems[0], newStartingIndex);
						});
					}
					else
					{
						OnCollectionReset();
					}

					break;

#if !(NET40 && SILVERLIGHT)
				case NotifyCollectionChangedAction.Move:
					// Any moves within the original collection do not affect our sorted collection
					break;
#endif

				case NotifyCollectionChangedAction.Remove:
					int oldStartingIndex = RemoveOldItems(e.OldItems);

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);

					if ( e.OldItems.Count == 1 )
					{
						OnCollectionChanged(() => {
							return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems[0], oldStartingIndex);
						});
					}
					else
					{
						OnCollectionReset();
					}

					break;

				case NotifyCollectionChangedAction.Replace:
					oldStartingIndex = RemoveOldItems(e.OldItems);

					if ( e.OldItems.Count == 1 )
					{
						OnCollectionChanged(() => {
							return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems[0], oldStartingIndex);
						});
					}

					newStartingIndex = InsertNewItemsSorted(e.NewItems);

					OnPropertyChanged(IndexerName);

					if ( e.NewItems.Count == 1 )
					{
						OnCollectionChanged(() => {
							return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems[0], newStartingIndex);
						});
					}
					else
					{
						OnCollectionReset();
					}

					break;

				case NotifyCollectionChangedAction.Reset:
					CleanSortedItems();
					InitSortedItems();

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);
					OnCollectionReset();

					break;
			}
		}

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			T item = _sortedItems[0];

			foreach ( var next in _sortedItems.Skip(1) )
			{
				if ( _itemsComparison(item, next) > 0 )
				{
					_sortedItems.Sort(_itemsComparison);

					OnPropertyChanged(IndexerName);
					OnCollectionReset();

					break;
				}
			}
		}
		#endregion

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.Count == _originalCollection.Count);
			Contract.Invariant(_sortedItems.Count == _originalCollection.Count);
		}
		#endregion
	}
}
