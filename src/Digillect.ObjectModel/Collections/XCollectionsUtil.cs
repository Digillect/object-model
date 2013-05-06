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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
	/// <summary>
	/// Helpers and extensions for collections.
	/// </summary>
	public static class XCollectionsUtil
	{
		#region IsNullOrEmpty
		/// <summary>
		/// Indicates whether the specified collection is <c>null</c> or has no items.
		/// </summary>
		/// <param name="value">The collection to test.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="value"/> is null or has no items; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <c>IsNullOrEmpty</c> is a convenience method that enables you to simultaneously test whether a collection is <c>null</c> or has no items.
		/// It is equivalent to the following code:
		/// <code>
		/// result = <paramref name="value"/> == null || <paramref name="value"/>.Count == 0;
		/// </code>
		/// </remarks>
		[Pure]
		public static bool IsNullOrEmpty(ICollection value)
		{
			return value == null || value.Count == 0;
		}
		#endregion

		#region IsNullOrEmpty`1
		/// <summary>
		/// Indicates whether the specified collection is <c>null</c> or has no items.
		/// </summary>
		/// <typeparam name="T"><see cref="Type"/> of collection members.</typeparam>
		/// <param name="value">The collection to test.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="value"/> is null or has no items; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <c>IsNullOrEmpty</c> is a convenience method that enables you to simultaneously test whether a collection is <c>null</c> or has no items.
		/// It is equivalent to the following code:
		/// <code>
		/// result = <paramref name="value"/> == null || <paramref name="value"/>.Count == 0;
		/// </code>
		/// </remarks>
		[Pure]
		public static bool IsNullOrEmpty<T>(ICollection<T> value)
		{
			return value == null || value.Count == 0;
		}

#if NET45
		/// <summary>
		/// Indicates whether the specified collection is <c>null</c> or has no items.
		/// </summary>
		/// <typeparam name="T"><see cref="Type"/> of collection members.</typeparam>
		/// <param name="value">The collection to test.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="value"/> is null or has no items; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <c>IsNullOrEmpty</c> is a convenience method that enables you to simultaneously test whether a collection is <c>null</c> or has no items.
		/// It is equivalent to the following code:
		/// <code>
		/// result = <paramref name="value"/> == null || <paramref name="value"/>.Count == 0;
		/// </code>
		/// </remarks>
		[Pure]
		public static bool IsNullOrEmpty<T>(IReadOnlyCollection<T> value)
		{
			return value == null || value.Count == 0;
		}
#endif
		#endregion

		#region ToXCollection`1 Extension
		/// <summary>
		/// Creates a <see cref="XCollection{T}"/> from an <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <param name="source">The <see cref="IEnumerable{T}"/> to create a <see cref="XCollection{T}"/> from.</param>
		/// <returns>A <see cref="XCollection{T}"/> that contains elements from the input sequence.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		public static XCollection<T> ToXCollection<T>(this IEnumerable<T> source)
			where T : XObject
		{
			Contract.Requires(source != null);
			Contract.Requires(Contract.ForAll(source, item => item != null));

			return new XCollection<T>(source);
		}
		#endregion

		#region RemoveAll`1 Extension
		/// <summary>
		/// Removes all items from the <paramref name="source"/> collection which match the <paramref name="predicate"/>.
		/// </summary>
		/// <typeparam name="T">The type of the collection's members.</typeparam>
		/// <param name="source">The source collection.</param>
		/// <param name="predicate">The predicate to apply to items in the collection.</param>
		/// <returns><c>true</c> if any items were removed; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> parameter cannot be null.</exception>
		public static bool RemoveAll<T>(this ICollection<T> source, Func<T, bool> predicate)
		{
			Contract.Requires(source != null);
			Contract.Requires(predicate != null);
			Contract.Ensures(!Contract.Result<bool>() || source.Count < Contract.OldValue(source.Count));

			return source.RemoveAll(source.Where(predicate).ToArray());
		}

		/// <summary>
		/// Removes all items from the <paramref name="source"/> collection which are contained in the provided <paramref name="collection"/>.
		/// </summary>
		/// <typeparam name="T">The type of the collection's members.</typeparam>
		/// <param name="source">The source collection.</param>
		/// <param name="collection">The collection which contains the items to remove. The value can be <c>null</c> in which case no operation is performed and the returned value will be <c>false</c>.</param>
		/// <returns><c>true</c> if any items were removed; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> parameter cannot be null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Ensures")]
		public static bool RemoveAll<T>(this ICollection<T> source, IEnumerable<T> collection)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.Ensures(!Contract.Result<bool>() || source.Count < Contract.OldValue(source.Count));

			bool modified = false;

			if ( collection != null )
			{
				foreach ( T item in collection )
				{
					while ( source.Remove(item) )
					{
						modified = true;
					}
				}
			}

			return modified;
		}

		/// <summary>
		/// Removes all items from the <paramref name="source"/> collection which have keys contained in the provided <paramref name="collection"/>.
		/// </summary>
		/// <typeparam name="T">The type of the collection's members.</typeparam>
		/// <param name="source">The source collection.</param>
		/// <param name="collection">The collection which contains the keys to remove. The value can be <c>null</c> in which case no operation is performed and the returned value will be <c>false</c>.</param>
		/// <returns><c>true</c> if any items were removed; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="source"/> parameter cannot be null.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Ensures")]
		public static bool RemoveAll<T>(this IXCollection<T> source, IEnumerable<XKey> collection)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.Ensures(!Contract.Result<bool>() || source.Count < Contract.OldValue(source.Count));

			bool modified = false;

			if ( collection != null )
			{
				foreach ( XKey item in collection )
				{
					while ( source.Remove(item) )
					{
						modified = true;
					}
				}
			}

			return modified;
		}
		#endregion

		#region RetainAll`1 Extension
#if false // Not fully implemented yet
		public static bool RetainAll<T>(this ICollection<T> source, IEnumerable<T> collection)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.EndContractBlock();

			if ( collection == null )
			{
				if ( source.Count == 0 )
				{
					return false;
				}
				else
				{
					source.Clear();

					return true;
				}
			}

			// TODO: Implement this!
			throw new NotImplementedException();

			//bool modified = false;

			//return modified;
		}
#endif
		#endregion

		#region Merge`1 Extension
		/// <summary>
		/// Merges the specified collection into the current one.
		/// </summary>
		/// <typeparam name="T">Type of the collections' members.</typeparam>
		/// <param name="source">The <see cref="IList&lt;T&gt;"/> to merge into.</param>
		/// <param name="collection">The <see cref="IEnumerable&lt;T&gt;"/> containing the source of changes.</param>
		/// <param name="options">The operations to perform on objects within the collections.</param>
		/// <returns>The <see cref="CollectionMergeResults">results</see> of the operation.</returns>
		/// <remarks>
		/// Be careful to not use this method on objects implementing the <see cref="INotifyCollectionChanged"/> interface unless you don't care about events being raised.
		/// Specifically, in cases of <see cref="IXCollection&lt;T&gt;"/> or <see cref="XCollection&lt;T&gt;"/> use theirs <b>Update</b> methods.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "God knows how to make it simplier.")]
		public static CollectionMergeResults Merge<T>(this IList<T> source, IEnumerable<T> collection, CollectionMergeOptions options)
			where T : XObject
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			Contract.Ensures(Contract.Result<CollectionMergeResults>() != null);

			if ( source.IsReadOnly )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			if ( Object.ReferenceEquals(source, collection) || options == CollectionMergeOptions.None )
			{
				return CollectionMergeResults.Empty;
			}

			int added = 0;
			int updated = 0;
			int removed = 0;

			bool optAddNew = (options & CollectionMergeOptions.AddNew) == CollectionMergeOptions.AddNew;
			bool optUpdateExisting = (options & CollectionMergeOptions.UpdateExisting) == CollectionMergeOptions.UpdateExisting;
			bool optRemoveOld = (options & CollectionMergeOptions.RemoveOld) == CollectionMergeOptions.RemoveOld;
			bool optPreserveSourceOrder = (options & CollectionMergeOptions.PreserveSourceOrder) == CollectionMergeOptions.PreserveSourceOrder;

			var updateCandidates = new Dictionary<XKey, List<MergeItem<T>>>();

			int index = 0;

			while ( index < source.Count )
			{
				T item = source[index];

				if ( item == null && optRemoveOld )
				{
					source.RemoveAt(index);
					removed++;
				}
				else
				{
					XKey key = item == null ? XKey.Empty : item.GetKey();
					List<MergeItem<T>> items;

					if ( updateCandidates.ContainsKey(key) )
					{
						items = updateCandidates[key];
					}
					else
					{
						items = new List<MergeItem<T>>();
						updateCandidates.Add(key, items);
					}

					items.Add(new MergeItem<T>() { Item = item, Index = index });

					index++;
				}
			}

			index = 0;

			foreach ( T item in collection )
			{
				XKey key;

				if ( item != null && updateCandidates.ContainsKey(key = item.GetKey()) )
				{
					var existing = updateCandidates[key];

					Contract.Assume(existing != null);
					Contract.Assume(existing.Count > 0);

					// Take first item to update
					var existing0 = existing[0];

					if ( existing0.Item.IsObjectCompatible(item) )
					{
						if ( optUpdateExisting )
						{
							MergeUpdateExistingItem(existing0, item, source, optPreserveSourceOrder, index, updateCandidates.Values);

							updated++;
						}

						// Remove the updated item from the candidates collection
						existing.RemoveAt(0);

						if ( existing.Count == 0 )
						{
							// No more candidates exist for the given key
							updateCandidates.Remove(key);
						}
					}
					else
					{
						MergeAddNewItem(item, source, optPreserveSourceOrder, index, updateCandidates.Values);

						added++;
					}
				}
				else if ( optAddNew )
				{
					MergeAddNewItem(item, source, optPreserveSourceOrder, index, updateCandidates.Values);

					added++;
				}

				index++;
			}

			if ( optRemoveOld )
			{
				// Тут нужна сортировка по индексу в порядке убывания, чтобы не вылететь за границы коллекции
				var indexes = from mi in updateCandidates.SelectMany(x => x.Value)
							  orderby mi.Index descending
							  select mi.Index;

				foreach ( var i in indexes )
				{
					// И еще один safeguard, для порядку
					//Contract.Assert(idx < this.Items.Count);
					Contract.Assume(i >= 0);
					source.RemoveAt(i);
					removed++;
				}
			}

			return CollectionMergeResults.Empty.With(added, removed, updated);
		}

		private static void MergeAddNewItem<T>(T item, IList<T> collection, bool preserveOrder, int index, IEnumerable<List<MergeItem<T>>> updateCandidates)
		{
			if ( preserveOrder )
			{
				Contract.Assume(index >= 0);

				collection.Insert(index, item);

				// Recalculate original indexes upon insertion
				foreach ( var items in updateCandidates )
				{
					for ( int i = 0; i < items.Count; i++ )
					{
						var x = items[i];

						if ( x.Index >= index )
						{
							x.Index++;
						}
					}
				}
			}
			else
			{
				collection.Add(item);
			}
		}

		private static void MergeUpdateExistingItem<T>(MergeItem<T> exisitingItem, T otherItem, IList<T> collection, bool preserveOrder, int index, IEnumerable<List<MergeItem<T>>> updateCandidates)
			where T : XObject
		{
			Contract.Assume(otherItem != null);

			exisitingItem.Item.Update(otherItem);

			if ( preserveOrder )
			{
				Contract.Assert(index <= exisitingItem.Index);
				Contract.Assume(exisitingItem.Index < collection.Count);

				if ( index < exisitingItem.Index )
				{
					// Reorder the updated item in the source collection to match the current position in the other collection
					// HACK: instead of remove-then-insert we will change indexes of all affected items

					// Move range [index..existing0.Index) one element up
					for ( int i = exisitingItem.Index; i > index; i-- )
					{
						collection[i] = collection[i - 1];
					}

					// Set updated item at the correct (source) position
					collection[index] = exisitingItem.Item;

					// Recalculate original indexes upon moving
					foreach ( var items in updateCandidates )
					{
						for ( int i = 0; i < items.Count; i++ )
						{
							var x = items[i];

							if ( index <= x.Index && x.Index < exisitingItem.Index )
							{
								x.Index++;
							}
						}
					}
				}
			}
		}
		#endregion

		#region Difference`2 Extension
		/// <summary>
		/// Calculates differences between two collections.
		/// </summary>
		/// <example>
		/// <code>
		/// var result = source.Difference(target);
		/// 
		/// foreach ( var o in result.Added )
		/// {
		///		// Process added object
		///	}
		///	
		///	foreach ( var o in result.Changed )
		///	{
		///		// Process changed object
		///	}
		///	
		///	foreach ( var o in result.Deleted )
		///	{
		///		// Process deleted object
		///	}
		/// </code>
		/// </example>
		[Pure]
		public static XCollectionDifference<T> Difference<T>(this IXCollection<T> source, IEnumerable<T> target)
			where T : XObject
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			if ( target == null )
			{
				throw new ArgumentNullException("target");
			}

			Contract.EndContractBlock();
			Contract.Assume(Contract.ForAll(source, item => item != null));

			IXCollection<T> added = new XCollection<T>();
			IXCollection<T> changed = new XCollection<T>();
			IXCollection<T> deleted = new XCollection<T>(source);
			IXCollection<T> modified = new XCollection<T>();
			IXCollection<T> nonModified = new XCollection<T>();

			foreach ( T targetObject in target )
			{
				// IXCollection does not allow null items - ignore such items
				if ( targetObject != null )
				{
					XKey targetKey;
					T sourceObject;

					if ( (targetKey = targetObject.GetKey()) != null && (sourceObject = source.FirstOrDefault(x => x.GetKey() == targetKey)) != null )
					{
						deleted.Remove(sourceObject);

						if ( !sourceObject.Equals(targetObject) )
						{
							changed.Add(targetObject);
							modified.Add(targetObject);
						}
						else
						{
							nonModified.Add(sourceObject);
						}
					}
					else
					{
						added.Add(targetObject);
						modified.Add(targetObject);
					}
				}
			}

			return new XCollectionDifference<T>(UnmodifiableCollection(added), UnmodifiableCollection(changed), UnmodifiableCollection(deleted), UnmodifiableCollection(modified), UnmodifiableCollection(nonModified));
		}
		#endregion

		#region Unmodifiable Wrappers
		/// <summary>
		/// Returns an unmodifiable wrapper around the specified collection.
		/// </summary>
		/// <typeparam name="T"><see cref="Type"/> of the collection members.</typeparam>
		/// <param name="collection">The collection to wrap.</param>
		/// <returns>
		/// Unmodifiable (<see cref="ICollection&lt;T&gt;.IsReadOnly"/> == <c>true</c>) wrapper around the original collection.
		/// </returns>
		[Pure]
		public static IXCollection<T> UnmodifiableCollection<T>(IXCollection<T> collection)
		{
			Contract.Requires( collection != null );

			return new ReadOnlyXCollection<T>(collection);
		}

		/// <summary>
		/// Returns an unmodifiable wrapper around the specified collection.
		/// </summary>
		/// <typeparam name="T"><see cref="Type"/> of the collection members.</typeparam>
		/// <param name="collection">The collection to wrap.</param>
		/// <returns>
		/// Unmodifiable (<see cref="ICollection&lt;T&gt;.IsReadOnly"/> == <c>true</c>) wrapper around the original collection.
		/// </returns>
		[Pure]
		public static IXList<T> UnmodifiableList<T>(IXList<T> collection)
		{
			Contract.Requires(collection != null);

			return new ReadOnlyXList<T>(collection);
		}

#if false
		public static IXCollection<T> AsReadOnly<T>(this IXCollection<T> collection)
		{
			return new ReadOnlyXCollection<T>(collection);
		}

		public static IXList<T> AsReadOnly<T>(this IXList<T> collection)
		{
			return new ReadOnlyXList<T>(collection);
		}
#endif
		#endregion

		#region FilteredList
		/// <summary>
		/// Creates a <see cref="FilteredList&lt;T&gt;"/> instance using the <paramref name="filter"/> as a filter.
		/// </summary>
		/// <typeparam name="T">Type of the collection's members.</typeparam>
		/// <param name="collection">Source collection.</param>
		/// <param name="filter">A function which performs actual filtering.</param>
		/// <returns></returns>
		[Pure]
		public static XFilteredCollection<T> FilteredList<T>(IXList<T> collection, Func<T, bool> filter)
			where T : XObject
		{
			Contract.Requires(collection != null);
			Contract.Requires(filter != null);

			return new FuncFilteredCollection<T>(collection, filter);
		}
		#endregion

		[ContractArgumentValidator]
		[Pure]
		internal static void ValidateCollection<T>([ValidatedNotNull] IEnumerable<T> collection)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			if ( !Contract.ForAll(collection, item => item != null) )
			{
				throw new ArgumentException("Null element found.", "collection");
			}

			Contract.EndContractBlock();
		}

		#region Compatibility Extensions
#if NET40 && SILVERLIGHT
		internal static int FindIndex<T>(this List<T> collection, Predicate<T> match)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			if ( match == null )
			{
				throw new ArgumentNullException("match");
			}

			Contract.EndContractBlock();

			for ( int i = 0; i < collection.Count; i++ )
			{
				if ( match(collection[i]) )
				{
					return i;
				}
			}

			return -1;
		}
#endif

#if WINDOWS8
		internal static void ForEach<T>(this List<T> collection, Action<T> action)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			if ( action == null )
			{
				throw new ArgumentNullException("action");
			}

			Contract.EndContractBlock();

			foreach ( var item in collection )
			{
				action(item);
			}
		}
#endif
		#endregion

		#region class ReadOnlyXCollection`1
#if !(SILVERLIGHT || WINDOWS8)
		[Serializable]
#endif
		private class ReadOnlyXCollection<T> : IXCollection<T>
#if !(SILVERLIGHT || WINDOWS8)
			, ICloneable
#endif
		{
			private readonly IXCollection<T> collection;

			#region Constructor
			public ReadOnlyXCollection(IXCollection<T> collection)
			{
				if ( collection == null )
				{
					throw new ArgumentNullException("collection");
				}

				Contract.EndContractBlock();

				this.collection = collection;
			}
			#endregion

			#region IXCollection`1 Members
			bool IXCollection<T>.ContainsKey(XKey key)
			{
				return this.collection.ContainsKey(key);
			}

			bool IXCollection<T>.Remove(XKey key)
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			IEnumerable<XKey> IXCollection<T>.GetKeys()
			{
				return this.collection.GetKeys();
			}

			public IXCollection<T> Clone(bool deep)
			{
				return new ReadOnlyXCollection<T>(deep ? this.collection.Clone(deep) : this.collection);
			}
			#endregion

			#region IXUpdatable`1 Members
			void IXUpdatable<IXCollection<T>>.BeginUpdate()
			{
				collection.BeginUpdate();
			}

			void IXUpdatable<IXCollection<T>>.EndUpdate()
			{
				collection.EndUpdate();
			}

			bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
			{
				return false;
			}

			void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}
			#endregion

			#region ICollection`1 Members
			void ICollection<T>.Add(T item)
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			void ICollection<T>.Clear()
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			bool ICollection<T>.Contains(T item)
			{
				return this.collection.Contains( item );
			}

			void ICollection<T>.CopyTo(T[] array, int arrayIndex)
			{
				this.collection.CopyTo( array, arrayIndex );
			}

			public int Count
			{
				get { return this.collection.Count; }
			}

			bool ICollection<T>.IsReadOnly
			{
				get { return true; }
			}

			bool ICollection<T>.Remove(T item)
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}
			#endregion

			#region IEnumerable`1 Members
			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				return this.collection.GetEnumerator();
			}
			#endregion

			#region IEnumerable Members
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.collection.GetEnumerator();
			}
			#endregion

			#region IEquatable`1 Members
			bool IEquatable<IXCollection<T>>.Equals(IXCollection<T> other)
			{
				return this.collection.Equals( other );
			}
			#endregion

			#region INotifyCollectionChanged Members
			event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
			{
				add { this.collection.CollectionChanged += value; }
				remove { this.collection.CollectionChanged -= value; }
			}
			#endregion

			#region INotifyPropertyChanged Members
			event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
			{
				add { collection.PropertyChanged += value; }
				remove { collection.PropertyChanged -= value; }
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

			#region Object Overrides
			public override bool Equals(object obj)
			{
				return this.collection.Equals( obj );
			}

			public override int GetHashCode()
			{
				return this.collection.GetHashCode();
			}

			public override string ToString()
			{
				return this.collection.ToString();
			}
			#endregion

			#region ObjectInvariant
			[ContractInvariantMethod]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
			private void ObjectInvariant()
			{
				Contract.Invariant(this.Count == this.collection.Count);
			}
			#endregion
		}
		#endregion

		#region class ReadOnlyXList`1
#if !(SILVERLIGHT || WINDOWS8)
		[Serializable]
#endif
		private class ReadOnlyXList<T> : ReadOnlyXCollection<T>, IXList<T>
		{
			private readonly IXList<T> collection;

			#region Constructor
			public ReadOnlyXList(IXList<T> collection)
				: base(collection)
			{
				Contract.Requires(collection != null, "collection");

				this.collection = collection;
			}
			#endregion

			#region IXList`1 Members
			int IXList<T>.IndexOf(XKey key)
			{
				return this.collection.IndexOf(key);
			}
			#endregion

			#region IList`1 Members
			int IList<T>.IndexOf(T item)
			{
				return this.collection.IndexOf(item);
			}

			void IList<T>.Insert(int index, T item)
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			void IList<T>.RemoveAt(int index)
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			T IList<T>.this[int index]
			{
				get { return this.collection[index]; }
				set { throw new NotSupportedException(Errors.XCollectionReadOnlyException); }
			}
			#endregion

			#region IEquatable`1 Members
			bool IEquatable<IXList<T>>.Equals(IXList<T> other)
			{
				return this.collection.Equals(other);
			}
			#endregion

			#region ObjectInvariant
			[ContractInvariantMethod]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
			private void ObjectInvariant()
			{
				Contract.Invariant(this.Count == this.collection.Count);
			}
			#endregion
		}
		#endregion

		#region class FuncFilteredCollection`1
		private class FuncFilteredCollection<T> : XFilteredCollection<T>
			where T : XObject
		{
			private readonly Func<T, bool> _filter;

			public FuncFilteredCollection(IXList<T> originalCollection, Func<T, bool> filter)
				: base(originalCollection)
			{
				if ( filter == null )
				{
					throw new ArgumentNullException("filter");
				}

				Contract.Requires(originalCollection != null);

				this._filter = filter;
			}

			protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> originalCollection)
			{
#if !(SILVERLIGHT || WINDOWS8)
				Func<T, bool> filter = (Func<T, bool>) this._filter.Clone();
#else
				Func<T, bool> filter = this._filter;
#endif

				return new FuncFilteredCollection<T>(originalCollection, filter);
			}

			protected override bool Filter(T obj)
			{
				return this._filter(obj);
			}
		} 
		#endregion

		#region class MergeItem`1
		private sealed class MergeItem<T>
		{
			public T Item;
			public int Index;
		}
		#endregion
	}

	#region class XCollectionDifference`1
	/// <summary>
	/// Represents the results of the <see cref="XCollectionsUtil.Difference"/> method.
	/// </summary>
	/// <typeparam name="T">Type of the collections' members.</typeparam>
	public sealed class XCollectionDifference<T>
	{
		private readonly IXCollection<T> _added;
		private readonly IXCollection<T> _changed;
		private readonly IXCollection<T> _deleted;
		private readonly IXCollection<T> _modified;
		private readonly IXCollection<T> _nonModified;

		internal XCollectionDifference(IXCollection<T> added, IXCollection<T> changed, IXCollection<T> deleted, IXCollection<T> modified, IXCollection<T> nonModified)
		{
			this._added = added;
			this._changed = changed;
			this._deleted = deleted;
			this._modified = modified;
			this._nonModified = nonModified;
		}

		/// <summary>
		/// Gets the collection of objects presented in a <b>target</b> collection but not found in a <b>source</b> one.
		/// </summary>
		public IXCollection<T> Added
		{
			get { return this._added; }
		}

		/// <summary>
		/// Gets the collection of objects presented in both <b>source</b> and <b>target</b> collections and which are not the same.
		/// </summary>
		public IXCollection<T> Changed
		{
			get { return this._changed; }
		}

		/// <summary>
		/// Gets the collection of objects presented in a <b>source</b> collection, but not found in a <b>target</b> one.
		/// </summary>
		public IXCollection<T> Deleted
		{
			get { return this._deleted; }
		}

		/// <summary>
		/// Gets the collection of objects which are <see cref="Added">added</see> or <see cref="Changed">changed</see>.
		/// </summary>
		public IXCollection<T> Modified
		{
			get { return this._modified; }
		}

		/// <summary>
		/// Gets the collection of objects which are the same in both <b>source</b> and <b>target</b> collections.
		/// </summary>
		public IXCollection<T> NonModified
		{
			get { return this._nonModified; }
		}
	}
	#endregion

	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	internal sealed class ValidatedNotNullAttribute : Attribute
	{
	}
}
