using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

using Digillect.Properties;

namespace Digillect.Collections
{
	/// <summary>
	/// A collection of unique non-null objects.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if !(SILVERLIGHT || NETFX_CORE)
	[Serializable]
#endif
	public class XUniqueCollection<T> : XCollection<T>
		where T : XObject
	{
		#region Constructor
		/// <summary>
		/// Initializes new instance of the <see cref="XUniqueCollection&lt;T&gt;"/> class.
		/// </summary>
		public XUniqueCollection()
		{
		}

		/// <summary>
		/// Initializes new instance of the <see cref="XUniqueCollection&lt;T&gt;"/> class using elements of the provided enumeration as the source for this list.
		/// </summary>
		/// <param name="collection">The enumeration which elements are used to construct a new list to use as the parameter for the <see cref="Collection&lt;T&gt;(IList&lt;T&gt;)"/> constructor.</param>
		public XUniqueCollection(IEnumerable<T> collection)
			: base(collection)
		{
			Contract.Requires(collection != null);
		}
		#endregion

		#region XCollection`1 Overrides
		protected override void OnInsert(int index, T item)
		{
			base.OnInsert(index, item);

			XKey key = item.GetKey();

			if ( key == null )
			{
				throw new ArgumentException(Resources.XObjectNullKeyException, "item");
			}

			if ( ContainsKey(key) )
			{
				throw new ArgumentException(Resources.XCollectionItemDuplicateException, "item");
			}
		}

		protected override void OnSet(int index, T oldItem, T newItem)
		{
			base.OnSet(index, oldItem, newItem);

			XKey key = newItem.GetKey();

			if ( key == null )
			{
				throw new ArgumentException(Resources.XObjectNullKeyException, "newItem");
			}

			if ( !key.Equals(oldItem.GetKey()) && ContainsKey(key) )
			{
				throw new ArgumentException(Resources.XCollectionItemDuplicateException, "newItem");
			}
		}
		#endregion
	}
}
