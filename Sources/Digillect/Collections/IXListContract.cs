using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
#if DEBUG || CONTRACTS_FULL
	[ContractClassFor(typeof(IXList<>))]
	abstract class IXListContract<T> : IXList<T>
	{
		protected IXListContract()
		{
		}

		public int IndexOf(XKey key)
		{
			Contract.Requires<ArgumentNullException>(key != null, "key");
			Contract.Ensures(Contract.Result<int>() >= -1);
			Contract.Ensures(Contract.Result<int>() < this.Count);
			Contract.Ensures(Contract.Result<int>() == -1 || this.Count > 0);

			return -1;
		}

		#region IXCollection<T> Members
		bool IXCollection<T>.ContainsKey(XKey key)
		{
			return false;
		}

		T IXCollection<T>.Find(XKey key)
		{
			return default(T);
		}

		bool IXCollection<T>.Remove(XKey key)
		{
			return false;
		}

		IEnumerable<XKey> IXCollection<T>.GetKeys()
		{
			return null;
		}

		IXCollection<T> IXCollection<T>.Clone(bool deep)
		{
			return null;
		}
		#endregion

		#region IList<T> Members
		int IList<T>.IndexOf(T item)
		{
			return -1;
		}

		void IList<T>.Insert(int index, T item)
		{
		}

		void IList<T>.RemoveAt(int index)
		{
		}

		T IList<T>.this[int index]
		{
			get { return default(T); }
			set { }
		}
		#endregion

		#region ICollection<T> Members
		void ICollection<T>.Add(T item)
		{
		}

		void ICollection<T>.Clear()
		{
		}

		bool ICollection<T>.Contains(T item)
		{
			return false;
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
		}

		public int Count
		{
			get { return 0; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<T>.Remove(T item)
		{
			return false;
		}
		#endregion

		#region IEnumerable<T> Members
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return null;
		}
		#endregion

		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return null;
		}
		#endregion

		#region IXUpdatable<IXCollection<T>> Members
		event EventHandler IXUpdatable<IXCollection<T>>.Updated
		{
			add { }
			remove { }
		}

		void IXUpdatable<IXCollection<T>>.BeginUpdate()
		{
		}

		void IXUpdatable<IXCollection<T>>.EndUpdate()
		{
		}

		bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
		{
			return false;
		}

		void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source)
		{
		}
		#endregion

		#region IEquatable<IXList<T>> Members
		bool IEquatable<IXList<T>>.Equals(IXList<T> other)
		{
			return false;
		}
		#endregion

		#region IEquatable<IXCollection<T>> Members
		bool IEquatable<IXCollection<T>>.Equals(IXCollection<T> other)
		{
			return false;
		}
		#endregion

		#region INotifyCollectionChanged Members
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { }
			remove { }
		}
		#endregion
	}
#endif
}
