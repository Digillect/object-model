using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
		private uint _updateCount;
		private int _version;

		#region Constructor/Disposer
		[ContractVerification(false)]
		public XCombinedCollection(params IXList<T>[] collections)
			: this((IEnumerable<IXList<T>>) collections)
		{
			Contract.Requires(collections != null);
			Contract.Requires(Contract.ForAll(collections, item => item != null));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
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
				item.Updated += UnderlyingCollection_Updated;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				foreach ( var item in _collections )
				{
					item.CollectionChanged -= UnderlyingCollection_CollectionChanged;
					item.Updated -= UnderlyingCollection_Updated;

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
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if ( _updateCount == 0 && CollectionChanged != null )
			{
				CollectionChanged(this, e);
			}
		}

		protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
		{
			if ( _updateCount == 0 )
			{
				base.OnPropertyChanged(e);
			}
		}

		protected override void OnUpdated(EventArgs e)
		{
			if ( _updateCount == 0 )
			{
				base.OnUpdated(e);
			}
		}
		#endregion

		#region Properties
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
		public override void BeginUpdate()
		{
			_updateCount++;

			foreach ( var c in _collections )
			{
				c.BeginUpdate();
			}
		}

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

		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _collections.Any(x => x.Contains(item));
		}

		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			return _collections.Any(x => x.ContainsKey(key));
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			foreach ( var c in _collections )
			{
				c.CopyTo(array, arrayIndex);

				arrayIndex += c.Count;
			}
		}

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
				OnUpdated(EventArgs.Empty);
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

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

		public override IEnumerable<XKey> GetKeys()
		{
			return _collections.SelectMany(x => x.GetKeys());
		}

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
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			Contract.EndContractBlock();

			_collections.Add(item);

			item.CollectionChanged += UnderlyingCollection_CollectionChanged;
			item.Updated += UnderlyingCollection_Updated;

			if ( _updateCount != 0 )
			{
				for ( uint i = 0; i < _updateCount; i++ )
				{
					item.BeginUpdate();
				}
			}
			else if ( item.Count != 0 )
			{
				OnUpdated(EventArgs.Empty);
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				// WPF is known to not support range operations so we can not issue Add with all of the items at a time
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
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
			item.Updated += UnderlyingCollection_Updated;

			if ( _updateCount != 0 )
			{
				for ( uint i = 0; i < _updateCount; i++ )
				{
					item.BeginUpdate();
				}
			}
			else if ( item.Count != 0 )
			{
				OnUpdated(EventArgs.Empty);
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
			item.Updated -= UnderlyingCollection_Updated;

			if ( _updateCount != 0 )
			{
				for ( uint i = _updateCount; i != 0; i-- )
				{
					item.EndUpdate();
				}
			}
			else if ( item.Count != 0 )
			{
				OnUpdated(EventArgs.Empty);
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

				if ( _updateCount == 0 )
				{
					OnPropertyChanged(CountString);
				}
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
#if SILVERLIGHT
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

		private void UnderlyingCollection_Updated(object sender, EventArgs e)
		{
			_version++;
			_count = -1;

			if ( _updateCount == 0 )
			{
				OnUpdated(EventArgs.Empty);
			}
		}
		#endregion
	}
}
