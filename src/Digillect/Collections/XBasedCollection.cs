﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
#if DEBUG || CONTRACTS_FULL
	[ContractClass(typeof(XBasedCollectionContract<>))]
#endif
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public abstract class XBasedCollection<T> : IXList<T>, IDisposable
#if !(SILVERLIGHT || WINDOWS8)
		, ICloneable
#endif
		where T : XObject
	{
		#region Constructor/Disposer
		protected XBasedCollection()
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
		#endregion

		#region IXList`1 Members
		public abstract int IndexOf(XKey key);
		#endregion

		#region IXCollection`1 Members
		public abstract bool ContainsKey(XKey key);

		bool IXCollection<T>.Remove(XKey key)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		public abstract IEnumerable<XKey> GetKeys();

		IXCollection<T> IXCollection<T>.Clone(bool deep)
		{
			return Clone(deep);
		}

		public abstract XBasedCollection<T> Clone(bool deep);
		#endregion

		#region IXUpdatable`1 Members
		public abstract event EventHandler Updated;

		public abstract void BeginUpdate();

		public abstract void EndUpdate();

		bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
		{
			return false;
		}

		void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}
		#endregion

		#region IList`1 Members
		public abstract T this[int index]
		{
			get;
		}

		T IList<T>.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(Errors.XCollectionReadOnlyException); }
		}

		public abstract int IndexOf(T item);

		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}
		#endregion

		#region ICollection`1 Members
		public abstract int Count
		{
			get;
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		void ICollection<T>.Clear()
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		public virtual bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public abstract void CopyTo(T[] array, int arrayIndex);

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}
		#endregion

		#region IEnumerable`1 Members
		public abstract IEnumerator<T> GetEnumerator();
		#endregion

		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region IEquatable`1 Members
		public virtual bool Equals(IXList<T> other)
		{
			if ( other == null || this.Count != other.Count )
			{
				return false;
			}

			return this.SequenceEqual(other);
		}

		public virtual bool Equals(IXCollection<T> other)
		{
			if ( other == null || this.Count != other.Count )
			{
				return false;
			}

			foreach ( T item in this )
			{
				XKey key = item.GetKey();

				if ( !item.Equals(other.FirstOrDefault(x => x.GetKey() == key)) )
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region INotifyCollectionChanged Members
		public abstract event NotifyCollectionChangedEventHandler CollectionChanged;
		#endregion

#if !(SILVERLIGHT || WINDOWS8)
		#region ICloneable Members
		object ICloneable.Clone()
		{
			return Clone(true);
		}
		#endregion
#endif
	}

	#region XBasedCollection`1 contract binding
#if DEBUG || CONTRACTS_FULL
	[ContractClassFor(typeof(XBasedCollection<>))]
	abstract class XBasedCollectionContract<T> : XBasedCollection<T>
		where T : XObject
	{
		public override T this[int index]
		{
			get
			{
				Contract.Requires(index >= 0);
				Contract.Requires(index < this.Count);

				return null;
			}
		}
	}
#endif
	#endregion
}