using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Digillect.Collections
{
	/// <summary>
	/// Represents an indexed collection of objects which can also be accessed by a key.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
	public interface IXList<T> : IXCollection<T>, IList<T>, IEquatable<IXList<T>>
	{
		/// <summary>
		/// Determines the index of an item with the specific key within the collection.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <b>collection</b>.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		int IndexOf(XKey key);

		/// <summary>
		/// Returns a collection of all objects' keys.
		/// </summary>
		/// <returns>A collection with objects' keys.</returns>
		new IList<XKey> GetKeys();
	}
}
