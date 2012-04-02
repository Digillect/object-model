using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
	/// <summary>
	/// Represents a collection of objects which can be accessed by a key.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if DEBUG || CONTRACTS_FULL
	[ContractClass(typeof(IXCollectionContract<>))]
#endif
	public interface IXCollection<T> : ICollection<T>, IXUpdatable<IXCollection<T>>, IEquatable<IXCollection<T>>, INotifyCollectionChanged
	{
		/// <summary>
		/// Determines whether the <b>collection</b> contains an item with the specific key.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <see cref="IXCollection&lt;T&gt;"/>.</param>
		/// <returns><see langword="true"/> if item is found in the <see cref="IXCollection&lt;T&gt;"/>; otherwise, <see langword="false"/>.</returns>
		[Pure]
		bool ContainsKey(XKey key);

		/// <summary>
		/// Gets an item with the specific key.
		/// </summary>
		/// <param name="key">The key of the item to find.</param>
		/// <returns>An item with the specified key if the item exists in the <b>collection</b>; otherwise, <see langword="null"/>.</returns>
		[Pure]
		T Find(XKey key);

		/// <summary>
		/// Removes the first occurrence of an item with the specific key from the <b>collection</b>.
		/// </summary>
		/// <param name="key">The key of an item to remove from the <b>collection</b>.</param>
		/// <returns>
		/// <see langword="true"/> if item was successfully removed from the <b>collection</b>; otherwise, <see langword="false"/>.
		/// This method also returns <see langword="false"/> if an item was not found in the <b>collection</b>.
		/// </returns>
		bool Remove(XKey key);

		/// <summary>
		/// Returns an enumeration of all objects' keys.
		/// </summary>
		/// <returns>A collection with objects' keys.</returns>
		[Pure]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		IEnumerable<XKey> GetKeys();

		/// <summary>
		/// Creates a copy of this collection.
		/// </summary>
		/// <param name="deep"><see langword="true"/> to deep-clone inner collections (including their members), <see langword="false"/> to clone only inner collections but not their members.</param>
		/// <returns>Cloned copy of the collection.</returns>
		[Pure]
		IXCollection<T> Clone( bool deep );
	}
}
