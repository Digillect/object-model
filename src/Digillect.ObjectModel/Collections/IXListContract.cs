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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
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

		bool IXCollection<T>.Remove(XKey key)
		{
			return false;
		}

		IEnumerable<XKey> IXCollection<T>.GetKeys()
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

		#region INotifyPropertyChanged Members
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { }
			remove { }
		}
		#endregion
	}
}
