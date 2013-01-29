using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
	/// <summary>
	/// Represents an indexed collection of objects which can also be accessed by a key.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if DEBUG || CONTRACTS_FULL
	[ContractClass(typeof(IXListContract<>))]
#endif
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public interface IXList<T> : IXCollection<T>, IList<T>, IEquatable<IXList<T>>
	{
		/// <summary>
		/// Determines the index of an item with the specific key within the collection.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <b>collection</b>.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		[Pure]
		int IndexOf(XKey key);
	}
}
