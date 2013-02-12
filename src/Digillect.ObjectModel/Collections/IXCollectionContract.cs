using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
#if DEBUG || CONTRACTS_FULL
	[ContractClassFor(typeof(IXCollection<>))]
	abstract class IXCollectionContract<T> : IXCollection<T>
	{
		protected IXCollectionContract()
		{
		}

		public bool ContainsKey(XKey key)
		{
			Contract.Requires<ArgumentNullException>(key != null, "key");
			Contract.Ensures(!Contract.Result<bool>() || this.Count > 0);

			return false;
		}

		public bool Remove(XKey key)
		{
			Contract.Requires<ArgumentNullException>(key != null, "key");
			Contract.Ensures(!Contract.Result<bool>() || this.Count == Contract.OldValue(this.Count) - 1);

			return false;
		}

		public IEnumerable<XKey> GetKeys()
		{
			Contract.Ensures(Contract.Result<IEnumerable<XKey>>() != null);
			Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<XKey>>(), item => item != null));

			return null;
		}

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

		IXCollection<T> IXUpdatable<IXCollection<T>>.Clone(bool deep)
		{
			return null;
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

		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(Contract.ForAll(this, item => item != null));
		}
	}
#endif
}