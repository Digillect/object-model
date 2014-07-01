#region Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
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
using System.Threading;

namespace Digillect.Collections
{
	[ContractClass(typeof(XFilteredCollectionContract<>))]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public abstract class XFilteredCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IXList<T> _baseCollection;

		private int _size = -1;
#if CUSTOM_ENUMERATOR
		private int _version;
#endif

		#region Constructor/Disposer
		protected XFilteredCollection(IXList<T> collection)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			Contract.EndContractBlock();

			_baseCollection = collection;

			_baseCollection.CollectionChanged += BaseCollection_CollectionChanged;
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				_baseCollection.CollectionChanged -= BaseCollection_CollectionChanged;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region BaseCollection
		public IXList<T> BaseCollection
		{
			get { return _baseCollection; }
		}
		#endregion

		#region IXList`1 Members
		/// <inheritdoc/>
		public override int IndexOf(XKey key)
		{
			return CalculateFilteredIndex(_baseCollection.IndexOf(key));
		}
		#endregion

		#region IXUpdatable`1 Members
		/// <inheritdoc/>
		public override XBasedCollection<T> Clone(bool deep)
		{
			IXList<T> collection = deep ? (IXList<T>) _baseCollection.Clone(true) : _baseCollection;

			return CreateInstanceOfSameType( collection );
		}
		#endregion

		#region IList`1 Members
		/// <inheritdoc/>
		public override T this[int index]
		{
			get
			{
				var originalIndex = CalculateOriginalIndex( index );

				return originalIndex >= 0 ? _baseCollection[originalIndex] : default(T);
			}
		}

		/// <inheritdoc/>
		public override int IndexOf(T item)
		{
			return CalculateFilteredIndex(_baseCollection.IndexOf(item));
		}
		#endregion

		#region ICollection`1 Members
		/// <inheritdoc/>
		public override int Count
		{
			get
			{
				if ( _size == -1 )
				{
					Interlocked.Exchange(ref _size, _baseCollection.Count(Filter));
				}

				return _size;
			}
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _baseCollection.Contains(item) && Filter(item);
		}
		#endregion

		#region IEnumerable`1 Members
		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator()
		{
#if CUSTOM_ENUMERATOR
			int version = _version;

			for ( int i = 0; i < _baseCollection.Count; i++ )
			{
				if ( version != _version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				T obj = _baseCollection[i];

				if ( Filter(obj) )
				{
					yield return obj;
				}
			}
#else
			return _baseCollection.Where(Filter).GetEnumerator();
#endif
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// This method supports the <see cref="Clone"/> infrastructure.
		/// </summary>
		/// <param name="collection">The original collection used to construct a filtered one.</param>
		/// <returns>New collection identical to the current one.</returns>
		[Pure]
		protected abstract XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> collection);

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
				if ( Filter(_baseCollection[i]) )
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

			for ( int i = 0, counter = 0; i < _baseCollection.Count && counter <= filteredIndex; i++ )
			{
				if ( Filter(_baseCollection[i]) )
				{
					originalIndex = i;
					counter++;
				}
			}

			return originalIndex;
		}
		#endregion

		#region Event Handlers
		private void BaseCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
#if CUSTOM_ENUMERATOR
			_version++;
#endif

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				_size = -1;
			}

			if ( this.IsInUpdate )
			{
				return;
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
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalculateFilteredIndex(e.NewStartingIndex)));

					break;

				case NotifyCollectionChangedAction.Remove:
					if ( !Filter((T) e.OldItems[0]) )
					{
						return;
					}

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalculateFilteredIndex(e.OldStartingIndex)));

					break;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					if ( !Filter((T) e.NewItems[0]) && !Filter((T) e.OldItems[0]) )
					{
						return;
					}

					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalculateFilteredIndex(e.NewStartingIndex)));

					break;
#else
				case NotifyCollectionChangedAction.Add:
					T[] newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();

					if ( newItems.Length == 0 )
						return;

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, newItems, CalculateFilteredIndex(e.NewStartingIndex)));

					break;

				case NotifyCollectionChangedAction.Remove:
					T[] oldItems = e.OldItems.Cast<T>().Where(Filter).ToArray();

					if ( oldItems.Length == 0 )
						return;

					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, oldItems, CalculateFilteredIndex(e.OldStartingIndex)));

					break;

				case NotifyCollectionChangedAction.Move:
					// e.NewItems and e.OldItems have the same content
					newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();

					if ( newItems.Length == 0 )
						return;

					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, newItems, CalculateFilteredIndex(e.NewStartingIndex), CalculateFilteredIndex(e.OldStartingIndex)));

					break;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();
					oldItems = e.OldItems.Cast<T>().Where(Filter).ToArray();

					if ( newItems.Length == 0 && oldItems.Length == 0 )
						return;

					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, newItems, oldItems, CalculateFilteredIndex(e.NewStartingIndex)));

					break;
#endif
				case NotifyCollectionChangedAction.Reset:
					OnPropertyChanged(CountString);
					OnPropertyChanged(IndexerName);
					OnCollectionReset();

					break;

				default:
					throw new ArgumentException(e.Action.ToString(), "e");
			}
		}
		#endregion

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_size >= -1);
		}
		#endregion
	}

	#region XFilteredCollection`1 contract binding
	[ContractClassFor(typeof(XFilteredCollection<>))]
	abstract class XFilteredCollectionContract<T> : XFilteredCollection<T>
		where T : XObject
	{
		protected XFilteredCollectionContract()
			: base(null)
		{
		}

		protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> collection)
		{
			Contract.Ensures(Contract.Result<XFilteredCollection<T>>() != null);

			return null;
		}
	}
	#endregion
}
