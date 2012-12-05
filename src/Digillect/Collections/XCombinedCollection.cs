using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XCombinedCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IXList<T>[] _collections;

		private int _count = -1;
		private int _version;

		#region Constructor/Disposer
		public XCombinedCollection(params IXList<T>[] collections)
		{
			if ( collections == null )
			{
				throw new ArgumentNullException("collections");
			}

			if ( collections.Length == 0 )
			{
				throw new ArgumentException("No collections specified.", "collections");
			}

			if ( !Contract.ForAll(collections, item => item != null) )
			{
				throw new ArgumentException("Null element found.", "collections");
			}

			Contract.EndContractBlock();

			_collections = (IXList<T>[]) collections.Clone();

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
				}
			}

			base.Dispose(disposing);
		}
		#endregion

		#region IXList`1 Members
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
		#endregion

		#region IXCollection`1 Members
		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			return _collections.Any(x => x.ContainsKey(key));
		}

		public override IEnumerable<XKey> GetKeys()
		{
			return _collections.SelectMany(x => x.GetKeys());
		}

		public override XBasedCollection<T> Clone(bool deep)
		{
			IXList<T>[] collections;

			if ( deep )
			{
				collections = new IXList<T>[_collections.Length];

				for ( int i = 0; i < _collections.Length; i++ )
				{
					collections[i] = (IXList<T>) _collections[i].Clone(deep);
				}
			}
			else
			{
				collections = _collections;
			}

			return (XBasedCollection<T>) Activator.CreateInstance(GetType(), collections);
		}
		#endregion

		#region IXUpdatable`1 Members
		public override event EventHandler Updated;

		public override void BeginUpdate()
		{
			foreach ( var c in _collections )
			{
				c.BeginUpdate();
			}
		}

		public override void EndUpdate()
		{
			foreach ( var c in _collections )
			{
				c.EndUpdate();
			}
		}
		#endregion

		#region IList`1 Members
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

		#region ICollection`1 Members
		public override int Count
		{
			get
			{
				if ( _count == -1 )
				{
					_count = _collections.Sum(x => x.Count);
				}

				return _count;
			}
		}

		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _collections.Any(x => x.Contains(item));
		}

		public override void CopyTo(T[] array, int arrayIndex)
		{
			foreach ( var c in _collections )
			{
				c.CopyTo(array, arrayIndex);

				arrayIndex += c.Count;
			}
		}
		#endregion

		#region IEnumerable`1 Members
		public override IEnumerator<T> GetEnumerator()
		{
			int version = _version;

			foreach ( var item in _collections.SelectMany(x => x) )
			{
				if ( _version != version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				yield return item;
			}
		}
		#endregion

		#region INotifyCollectionChanged Members
		public override event NotifyCollectionChangedEventHandler CollectionChanged;
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
			}

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

			if ( Updated != null )
			{
				Updated(this, EventArgs.Empty);
			}
		}
		#endregion
	}
}
