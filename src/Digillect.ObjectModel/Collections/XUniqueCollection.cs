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
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
	/// <summary>
	/// A collection of unique non-null objects.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XUniqueCollection<T> : XCollection<T>
		where T : XObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XUniqueCollection&lt;T&gt;"/> class.
		/// </summary>
		public XUniqueCollection()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XUniqueCollection&lt;T&gt;"/> class that contains elements copied from the specified collection.
		/// </summary>
		/// <param name="collection">The collection from which the elements are copied.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> parameter cannot be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> parameter cannot contain <c>null</c> members.</exception>
		public XUniqueCollection(IEnumerable<T> collection)
			: base(collection)
		{
			Contract.Requires(collection != null);
			Contract.Requires(Contract.ForAll(collection, item => item != null));
		}

#if !WINDOWS8
		/// <summary>
		/// Initializes a new instance of the <see cref="XUniqueCollection&lt;T&gt;"/> class that contains elements copied from the specified list.
		/// </summary>
		/// <param name="list">The list from which the elements are copied.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="list"/> parameter cannot be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="list"/> parameter cannot contain <c>null</c> members.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Base type uses it")]
		public XUniqueCollection(List<T> list)
			: base(list)
		{
			Contract.Requires(list != null);
			Contract.Requires(Contract.ForAll(list, item => item != null));
		}
#endif
		#endregion

		#region XCollection`1 Overrides
		/// <inheritdoc/>
		protected override void InsertItem(int index, T item)
		{
			if ( ContainsKey(item.GetKey()) )
			{
				throw new ArgumentException(Errors.XCollectionItemDuplicateException, "item");
			}

			base.InsertItem(index, item);
		}

		/// <inheritdoc/>
		protected override void SetItem(int index, T item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			XKey key = item.GetKey();

			if ( !key.Equals(this.Items[index].GetKey()) && ContainsKey(key) )
			{
				throw new ArgumentException(Errors.XCollectionItemDuplicateException, "item");
			}

			base.SetItem(index, item);
		}
		#endregion
	}
}
