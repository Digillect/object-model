using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Digillect.Collections
{
	/// <summary>
	/// Represents a collection of objects which can be accessed by a key.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
	public interface IXCollection<T> : ICollection<T>, IXUpdatable<IXCollection<T>>, IEquatable<IXCollection<T>>, INotifyCollectionChanged
	{
		/// <summary>
		/// Determines whether the <b>collection</b> contains an item with the specific key.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <see cref="IXCollection&lt;T&gt;"/>.</param>
		/// <returns><see langword="true"/> if item is found in the <see cref="IXCollection&lt;T&gt;"/>; otherwise, <see langword="false"/>.</returns>
		bool Contains(XKey key);

		/// <summary>
		/// Gets an item with the specific key.
		/// </summary>
		/// <param name="key">The key of the item to find.</param>
		/// <returns>An item with the specified key if the item exists in the <b>collection</b>; otherwise, <see langword="null"/>.</returns>
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
		IEnumerable<XKey> GetKeys();

#if false
		/// <summary>
		/// Begins the mass-update operation.
		/// </summary>
		/// <remarks>
		/// This method can be called multiple times.
		/// Until the last corresponding <see cref="EndUpdate"/> method is called none of the <see cref="INotifyCollectionChanged.CollectionChanged"/> events are raised.
		/// </remarks>
		void BeginUpdate();

		/// <summary>
		/// Ends the mass-update operation.
		/// </summary>
		/// <remarks>
		/// Do not forget to call this method for each corresponding <see cref="BeginUpdate"/> method you called.
		/// Until the last corresponding <see cref="EndUpdate()"/> method is called none of the <see cref="INotifyCollectionChanged.CollectionChanged"/> events are raised.
		/// </remarks>
		void EndUpdate();

		/// <summary>
		/// Determines whether the update operation is needed.
		/// </summary>
		/// <param name="source">Source <b>collection</b> to compare with.</param>
		/// <param name="options">Update options.</param>
		/// <returns><see langword="false"/> if two collections are the same (equal by reference), otherwise, <see langword="true"/>.</returns>
		bool NeedUpdate(IEnumerable<T> source, CollectionUpdateOptions options);

		/// <summary>
		/// Обновляет текущую коллекцию на основе другой коллекции.
		/// </summary>
		/// <param name="source">Источник изменений.</param>
		/// <returns>The <see cref="CollectionUpdateResults">results</see> of the operation.</returns>
		/// <remarks>
		/// Вызов данного метода эквивалентен вызову метода <see cref="Update(IEnumerable&lt;T&gt;,CollectionUpdateOptions)"/> со вторым параметром, равным <see cref="CollectionUpdateOptions.All"/>.
		/// </remarks>
		CollectionUpdateResults Update(IEnumerable<T> source);

		/// <summary>
		/// Обновляет текущую коллекцию на основе другой коллекции.
		/// </summary>
		/// <param name="source">Источник изменений.</param>
		/// <param name="options">Операции, которые надо произвести с объектами, находящимися в данной коллекции.</param>
		/// <returns>The <see cref="CollectionUpdateResults">results</see> of the operation.</returns>
		CollectionUpdateResults Update(IEnumerable<T> source, CollectionUpdateOptions options);
#endif
	}
}
