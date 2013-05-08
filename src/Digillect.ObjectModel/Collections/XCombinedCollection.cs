﻿#region Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
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
	public class XCombinedCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IList<IXList<T>> _collections;

		private int _count = -1;
		private ushort _updateCount;
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

			_collections = new List<IXList<T>>(collections);

			foreach ( var item in _collections )
			{
				item.CollectionChanged += UnderlyingCollection_CollectionChanged;
			}
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				foreach ( var item in _collections )
				{
					item.CollectionChanged -= UnderlyingCollection_CollectionChanged;

					for ( uint i = _updateCount; i != 0; i-- )
					{
						item.EndUpdate();
					}
				}

				_collections.Clear();
				_updateCount = 0;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Events
		/// <inheritdoc/>
#if !(SILVERLIGHT || WINDOWS8)
		[field: NonSerialized]
#endif
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Raises the <see cref="CollectionChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if ( _updateCount == 0 && CollectionChanged != null )
			{
				CollectionChanged(this, e);
			}
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if ( _updateCount == 0 )
			{
				base.OnPropertyChanged(e);
			}
		}
		#endregion

		#region Properties
		/// <inheritdoc/>
		public override int Count
		{
			get
			{
				if ( _count == -1 )
				{
					Interlocked.Exchange(ref _count, _collections.Sum(x => x.Count));
				}

				return _count;
			}
		}

		/// <inheritdoc/>
		public override T this[int index]
		{
			get
			{
				foreach ( var c in _collections )
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
		public override void BeginUpdate()
		{
			_updateCount++;

			foreach ( var c in _collections )
			{
				c.BeginUpdate();
			}
		}

		/// <inheritdoc/>
		public override XBasedCollection<T> Clone(bool deep)
		{
			IList<IXList<T>> collections;

			if ( deep )
			{
				collections = new List<IXList<T>>();

				for ( int i = 0; i < _collections.Count; i++ )
				{
					collections.Add((IXList<T>) _collections[i].Clone(deep));
				}
			}
			else
			{
				collections = _collections;
			}

			return (XBasedCollection<T>) Activator.CreateInstance(GetType(), collections);
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _collections.Any(x => x.Contains(item));
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			return _collections.Any(x => x.ContainsKey(key));
		}

		/// <inheritdoc/>
		public override void CopyTo(T[] array, int arrayIndex)
		{
			foreach ( var c in _collections )
			{
				c.CopyTo(array, arrayIndex);

				arrayIndex += c.Count;
			}
		}

		/// <inheritdoc/>
		public override void EndUpdate()
		{
			if ( _updateCount == 0 )
			{
				return;
			}

			foreach ( var c in _collections )
			{
				c.EndUpdate();
			}

			if ( --_updateCount == 0 )
			{
				OnPropertyChanged((string) null);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator()
		{
			int version = _version;

			foreach ( var item in _collections.SelectMany(x => x) )
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
			return _collections.SelectMany(x => x.GetKeys());
		}

		/// <inheritdoc/>
		public override int IndexOf(XKey key)
		{
			int count = 0;

			foreach ( var c in _collections )
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

			foreach ( var c in _collections )
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

			InsertCollection(_collections.Count, item);
		}

		public void InsertCollection(int index, IXList<T> item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			Contract.Requires(index >= 0);

			_collections.Insert(index, item);

			item.CollectionChanged += UnderlyingCollection_CollectionChanged;

			if ( _updateCount != 0 )
			{
				for ( uint i = 0; i < _updateCount; i++ )
				{
					item.BeginUpdate();
				}
			}
			else if ( item.Count != 0 )
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				// WPF is known to not support range operations so we can not issue Add with all of the items at a time
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public bool RemoveCollection(IXList<T> item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			Contract.EndContractBlock();

			if ( !_collections.Remove(item) )
			{
				return false;
			}

			item.CollectionChanged -= UnderlyingCollection_CollectionChanged;

			if ( _updateCount != 0 )
			{
				for ( uint i = _updateCount; i != 0; i-- )
				{
					item.EndUpdate();
				}
			}
			else if ( item.Count != 0 )
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				// WPF is known to not support range operations so we can not issue Remove with all of the items at a time
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			return true;
		}
		#endregion

		#region Protected Methods
		protected int CalculateCombinedIndex(IXList<T> collection, int index)
		{
			Contract.Requires(collection != null);
			Contract.Requires(index >= 0);
			Contract.Requires(index < collection.Count);

			int count = 0;

			foreach ( var c in _collections )
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

			throw new ArgumentException("The collection is not an underlying member of this collection.", "collection");
		}
		#endregion

		#region Event Handlers
		private void UnderlyingCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_version++;

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				_count = -1;

				OnPropertyChanged(CountString);
			}

			if ( _updateCount != 0 )
			{
				return;
			}

			OnPropertyChanged(IndexerName);

			if ( CollectionChanged != null )
			{
				Contract.Assume(sender != null);

				IXList<T> collection = (IXList<T>) sender;
				NotifyCollectionChangedEventArgs args;

				switch ( e.Action )
				{
#if NET40 && SILVERLIGHT
					case NotifyCollectionChangedAction.Add:
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalculateCombinedIndex(collection, e.NewStartingIndex));
						break;
					case NotifyCollectionChangedAction.Remove:
						args = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalculateCombinedIndex(collection, e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalculateCombinedIndex(collection, e.NewStartingIndex));
						break;
#else
					case NotifyCollectionChangedAction.Add:
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, CalculateCombinedIndex(collection, e.NewStartingIndex));
						break;
					case NotifyCollectionChangedAction.Remove:
						args = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, CalculateCombinedIndex(collection, e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Move:
						// e.NewItems and e.OldItems have the same content
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, CalculateCombinedIndex(collection, e.NewStartingIndex), CalculateCombinedIndex(collection, e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, CalculateCombinedIndex(collection, e.NewStartingIndex));
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
		#endregion
	}
}
