using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
#if WINDOWS8
using System.Reflection;
#endif

namespace Digillect.Collections
{
	[ContractClass(typeof(XBasedCollectionContract<>))]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public abstract class XBasedCollection<T> : IXList<T>, IList, INotifyPropertyChanged, IDisposable
#if !(SILVERLIGHT || WINDOWS8)
		, ICloneable
#endif
		where T : XObject
	{
		/// <summary>
		/// The name of the <see cref="ICollection&lt;T&gt;.Count"/> property for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// Contains the "Count" string.
		/// </summary>
		protected const string CountString = "Count";

		/// <summary>
		/// The name of the <see cref="IList&lt;T&gt;.this"/> property for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// Contains the "Item[]" string.
		/// </summary>
		protected const string IndexerName = "Item[]";

		#region Constructor/Disposer
		/// <summary>
		/// Default constructor.
		/// </summary>
		protected XBasedCollection()
		{
		}

		/// <inheritdoc/>
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
		/// <inheritdoc/>
		public abstract int IndexOf(XKey key);
		#endregion

		#region IXCollection`1 Members
		/// <inheritdoc/>
		public virtual bool ContainsKey(XKey key)
		{
			return IndexOf(key) != -1;
		}

		bool IXCollection<T>.Remove(XKey key)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		/// <inheritdoc/>
		public virtual IEnumerable<XKey> GetKeys()
		{
			return this.Select(x => x.GetKey());
		}
		#endregion

		#region IXUpdatable`1 Members
		/// <inheritdoc/>
		public event EventHandler Updated;

		/// <summary>
		/// Raises the <see cref="Updated" /> event.
		/// </summary>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		protected virtual void OnUpdated(EventArgs e)
		{
			if ( Updated != null )
			{
				Updated(this, e);
			}
		}

		IXCollection<T> IXUpdatable<IXCollection<T>>.Clone(bool deep)
		{
			return Clone(deep);
		}

		/// <inheritdoc cref="IXUpdatable&lt;T&gt;.Clone(bool)"/>
		public abstract XBasedCollection<T> Clone(bool deep);

		/// <inheritdoc/>
		public abstract void BeginUpdate();

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public virtual bool Contains(T item)
		{
			return IndexOf(item) != -1;
		}

		/// <inheritdoc/>
		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			if ( array == null )
			{
				throw new ArgumentNullException("array");
			}

			if ( arrayIndex < 0 )
			{
				throw new ArgumentOutOfRangeException("arrayIndex", Errors.ArgumentOutOfRange_NeedNonNegNum);
			}

			if ( array.Length - arrayIndex < this.Count )
			{
				throw new ArgumentException(Errors.Arg_ArrayPlusOffTooSmall);
			}

			foreach ( T item in this )
			{
				array[arrayIndex++] = item;
			}
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}
		#endregion

		#region IEnumerable`1 Members
		/// <inheritdoc/>
		public abstract IEnumerator<T> GetEnumerator();
		#endregion

		#region IList Members
		object IList.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(Errors.XCollectionReadOnlyException); }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		void IList.Clear()
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		bool IList.Contains(object value)
		{
			return IsCompatibleObject(value) && Contains((T) value);
		}

		int IList.IndexOf(object value)
		{
			return IsCompatibleObject(value) ? IndexOf((T) value) : -1;
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException(Errors.XCollectionReadOnlyException);
		}

		private static bool IsCompatibleObject(object value)
		{
			return value is T || value == null && default(T) == null;
		}
		#endregion

		#region ICollection Members
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if ( array == null )
			{
				throw new ArgumentNullException("array");
			}

			if ( array.Rank != 1 || array.GetLowerBound(0) != 0 )
			{
				throw new ArgumentException("Incompatible array.", "array");
			}

			if ( index < 0 )
			{
				throw new ArgumentOutOfRangeException("index", Errors.ArgumentOutOfRange_NeedNonNegNum);
			}

			if ( array.Length - index < this.Count )
			{
				throw new ArgumentException("ArrayPlusOffTooSmall");
			}

			T[] localArray = array as T[];

			if ( localArray != null )
			{
				CopyTo(localArray, index);
			}
			else
			{
#if WINDOWS8
				TypeInfo elementType = array.GetType().GetElementType().GetTypeInfo();
				TypeInfo c = typeof(T).GetTypeInfo();
#else
				Type elementType = array.GetType().GetElementType();
				Type c = typeof(T);
#endif

				if ( !elementType.IsAssignableFrom(c) && !c.IsAssignableFrom(elementType) )
				{
					throw new ArgumentException("InvalidArrayType");
				}

				object[] objArray = array as object[];

				if ( objArray == null )
				{
					throw new ArgumentException("InvalidArrayType");
				}

				int count = this.Count;

				try
				{
					for ( int i = 0; i < count; i++ )
					{
						objArray[index++] = this[i];
					}
				}
				catch ( ArrayTypeMismatchException )
				{
					throw new ArgumentException("InvalidArrayType");
				}
			}
		}
		#endregion

		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region IEquatable`1 Members
		/// <inheritdoc/>
		public virtual bool Equals(IXList<T> other)
		{
			if ( other == null || this.Count != other.Count )
			{
				return false;
			}

			return this.SequenceEqual(other);
		}

		/// <inheritdoc/>
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
		/// <inheritdoc/>
		public abstract event NotifyCollectionChangedEventHandler CollectionChanged;
		#endregion

		#region INotifyPropertyChanged Members
		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged" /> event.
		/// </summary>
		/// <param name="propertyName">The name of a property being changed.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged(this, e);
			}
		}
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
	[ContractClassFor(typeof(XBasedCollection<>))]
	abstract class XBasedCollectionContract<T> : XBasedCollection<T>
		where T : XObject
	{
		public override T this[int index]
		{
			get
			{
				Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(index < this.Count);

				return null;
			}
		}

		public override XBasedCollection<T> Clone(bool deep)
		{
			Contract.Ensures(Contract.Result<XBasedCollection<T>>() != null);

			return null;
		}
	}
	#endregion
}
