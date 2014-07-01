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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Digillect.Collections
{
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public sealed class XCombinedCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IList<IXList<T>> _baseCollections;

		private int _size = -1;
		private int _version;

		#region Constructor/Disposer
		[ContractVerification(false)]
		public XCombinedCollection(params IXList<T>[] collections)
			: this((IEnumerable<IXList<T>>) collections)
		{
			Contract.Requires(collections != null);
			Contract.Requires(Contract.ForAll(collections, item => item != null));
		}

		public XCombinedCollection(IEnumerable<IXList<T>> collections)
		{
			if ( collections == null )
			{
				throw new ArgumentNullException("collections");
			}

			if ( !Contract.ForAll(collections, item => item != null) )
			{
				throw new ArgumentException("Null element found.", "collections");
			}

			Contract.EndContractBlock();

			_baseCollections = collections.ToList();

			foreach ( var item in _baseCollections )
			{
				item.CollectionChanged += BaseCollection_CollectionChanged;
			}
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				foreach ( var item in _baseCollections )
				{
					item.CollectionChanged -= BaseCollection_CollectionChanged;
				}
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Properties
		/// <inheritdoc/>
		public override int Count
		{
			get
			{
				if ( _size == -1 )
				{
					Interlocked.Exchange(ref _size, _baseCollections.Sum(x => x.Count));
				}

				return _size;
			}
		}

		/// <inheritdoc/>
		public override T this[int index]
		{
			get
			{
				foreach ( var c in _baseCollections )
				{
					if ( index >= c.Count )
					{
						index -= c.Count;
					}
					else
					{
						return c[index];
					}
				}

				throw new ArgumentOutOfRangeException("index", Errors.ArgumentOutOfRange_Index);
			}
		}
		#endregion

		#region Methods
		/// <inheritdoc/>
		public override XBasedCollection<T> Clone(bool deep)
		{
			IEnumerable<IXList<T>> collections = deep ? _baseCollections.Select(x => (IXList<T>) x.Clone(deep)) : _baseCollections;

			Contract.Assume(Contract.ForAll(collections, item => item != null));

			return new XCombinedCollection<T>(collections);
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _baseCollections.Any(x => x.Contains(item));
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			return _baseCollections.Any(x => x.ContainsKey(key));
		}

		/// <inheritdoc/>
		public override void CopyTo(T[] array, int arrayIndex)
		{
			foreach ( var c in _baseCollections )
			{
				c.CopyTo(array, arrayIndex);

				arrayIndex += c.Count;
			}
		}

		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator()
		{
			int version = _version;

			foreach ( var item in _baseCollections.SelectMany(x => x) )
			{
				if ( version != _version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				yield return item;
			}
		}

		/// <inheritdoc/>
		public override IEnumerable<XKey> GetKeys()
		{
			return _baseCollections.SelectMany(x => x.GetKeys());
		}

		/// <inheritdoc/>
		public override int IndexOf(XKey key)
		{
			int count = 0;

			foreach ( var c in _baseCollections )
			{
				int index = c.IndexOf(key);

				if ( index >= 0 )
				{
					return count + index;
				}

				count += c.Count;
			}

			return -1;
		}

		/// <inheritdoc/>
		public override int IndexOf(T item)
		{
			int count = 0;

			foreach ( var c in _baseCollections )
			{
				int index = c.IndexOf(item);

				if ( index >= 0 )
				{
					return count + index;
				}

				count += c.Count;
			}

			return -1;
		}
		#endregion

		#region Collections Manipulations
		public void AddCollection(IXList<T> item)
		{
			Contract.Requires(item != null);

			InsertCollection(_baseCollections.Count, item);
		}

		public void InsertCollection(int index, IXList<T> item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			Contract.Requires(index >= 0);
			//Contract.Requires(index <= _collections.Count);

			_baseCollections.Insert(index, item);

			_version++;

			item.CollectionChanged += BaseCollection_CollectionChanged;

			if ( !this.IsInUpdate && item.Count != 0 )
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				// WPF is known to not support range operations so we can not issue Add with all of the items at a time
				OnCollectionReset();
			}
		}

		public bool RemoveCollection(IXList<T> item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			Contract.EndContractBlock();

			if ( !_baseCollections.Remove(item) )
			{
				return false;
			}

			_version++;

			item.CollectionChanged -= BaseCollection_CollectionChanged;

			if ( !this.IsInUpdate && item.Count != 0 )
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				// WPF is known to not support range operations so we can not issue Remove with all of the items at a time
				OnCollectionReset();
			}

			return true;
		}
		#endregion

		#region CalculateCombinedIndex
		private int CalculateCombinedIndex(IXList<T> collection, int index)
		{
			Contract.Requires(collection != null);
			Contract.Requires(index >= 0);
			Contract.Requires(index < collection.Count);

			int count = 0;

			foreach ( var c in _baseCollections )
			{
				if ( !Object.ReferenceEquals(c, collection) )
				{
					count += c.Count;
				}
				else
				{
					return count + index;
				}
			}

			throw new ArgumentException("The collection is not a member of this collection.", "collection");
		}
		#endregion

		#region Event Handlers
		private void BaseCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_version++;

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				_size = -1;

				OnPropertyChanged(CountString);
			}

			if ( this.IsInUpdate )
			{
				return;
			}

			OnPropertyChanged(IndexerName);

			IXList<T> collection = (IXList<T>) sender;

			Contract.Assume(collection != null);

			switch ( e.Action )
			{
#if NET40 && SILVERLIGHT
				case NotifyCollectionChangedAction.Add:
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalculateCombinedIndex(collection, e.NewStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Remove:
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalculateCombinedIndex(collection, e.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalculateCombinedIndex(collection, e.NewStartingIndex)));
					break;
#else
				case NotifyCollectionChangedAction.Add:
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, CalculateCombinedIndex(collection, e.NewStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Remove:
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, CalculateCombinedIndex(collection, e.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Move:
					// e.NewItems and e.OldItems have the same content
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, CalculateCombinedIndex(collection, e.NewStartingIndex), CalculateCombinedIndex(collection, e.OldStartingIndex)));
					break;
				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, CalculateCombinedIndex(collection, e.NewStartingIndex)));
					break;
#endif
				case NotifyCollectionChangedAction.Reset:
					OnCollectionReset();
					break;
				default:
					throw new ArgumentException(e.Action.ToString(), "e");
			}
		}
		#endregion
	}
}
