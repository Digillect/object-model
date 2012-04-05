using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
	public static class XCollectionsUtil
	{
		#region IsNullOrEmpty`1
		public static bool IsNullOrEmpty<T>(T[] value)
		{
			return value == null || value.Length == 0;
		}

		public static bool IsNullOrEmpty<T>(ICollection<T> value)
		{
			return value == null || value.Count == 0;
		}
		#endregion

		#region RemoveAll`1 Extension
		public static bool RemoveAll<T>(this ICollection<T> source, IEnumerable<T> collection)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.EndContractBlock();

			bool modified = false;

			if ( collection != null )
			{
				foreach ( T item in collection )
				{
					if ( source.Remove(item) )
					{
						modified = true;
					}
				}
			}

			return modified;
		}

		public static bool RemoveAll<T>(this IXCollection<T> source, IEnumerable<XKey> collection)
			where T : XObject
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.EndContractBlock();

			bool modified = false;

			if ( collection != null )
			{
				foreach ( XKey item in collection )
				{
					if ( source.Remove(item) )
					{
						modified = true;
					}
				}
			}

			return modified;
		}

		public static bool RemoveAll<T>(this IXCollection<T> source, Func<T, bool> predicate)
			where T : XObject
		{
			Contract.Requires(source != null);
			Contract.Requires(predicate != null);

			return source.RemoveAll(source.Where(predicate).ToArray());
		}

		public static bool RemoveAll<T>(this IXCollection<T> source, Predicate<T> predicate)
			where T : XObject
		{
			Contract.Requires(source != null);
			Contract.Requires(predicate != null);

			return source.RemoveAll(predicate.ToFunction());
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
#if WINDOWS_PHONE && CODE_ANALYSIS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
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

			if ( Object.ReferenceEquals(source, collection) )
			{
				return CollectionMergeResults.Empty;
			}

			int added = 0;
			int updated = 0;
			int removed = 0;

			int index = 0;

			while ( index < source.Count )
			{
				if ( source[index] == null )
				{
					source.RemoveAt(index);
					removed++;
				}
				else
				{
					index++;
				}
			}

			IDictionary<XKey, List<XMergeItem<T>>> updateCandidates = new Dictionary<XKey, List<XMergeItem<T>>>();

			for ( int i = 0; i < source.Count; i++ )
			{
				T item = source[i];

				Contract.Assume(item != null); // Since we've removed all nulls in the previous step.

				XKey key = item.GetKey();
				List<XMergeItem<T>> items;

				if ( updateCandidates.ContainsKey(key) )
				{
					items = updateCandidates[key];
				}
				else
				{
					items = new List<XMergeItem<T>>();
					updateCandidates.Add(key, items);
				}

				items.Add(new XMergeItem<T>() { Item = item, Index = i });
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

					if ( (options & CollectionMergeOptions.UpdateExisting) == CollectionMergeOptions.UpdateExisting )
					{
						// Take first item to update
						var existing0 = existing[0];

						existing0.Item.Update(item);
						updated++;

						Contract.Assert(index <= existing0.Index);
						Contract.Assume(existing0.Index < source.Count);

						// Move the updated item to the current (source) position
						// HACK: instead of remove-then-insert we will change indexes of all affected items

						// Move range [index..existing0.Index) one element up
						for ( int i = existing0.Index; i > index; i-- )
						{
							source[i] = source[i - 1];
						}

						// Set updated item at the corect (source) position
						source[index] = existing0.Item;

						// Recalculate original indexes upon moving
						foreach ( var items in updateCandidates.Values )
						{
#if !NETFX_CORE // List<T>.ForEach is not supported in NETFX_CORE (WinRT)
							items.ForEach(x =>
#else
							foreach ( var x in items )
#endif
							{
								if ( index <= x.Index && x.Index < existing0.Index )
								{
									x.Index++;
								}
							}
#if !NETFX_CORE
							);
#endif
						}
					}

					// Remove updated item form candidates
					existing.RemoveAt(0);

					if ( existing.Count == 0 )
					{
						// No more candidates exist for the given key
						updateCandidates.Remove(key);
					}
				}
				else if ( (options & CollectionMergeOptions.AddNew) == CollectionMergeOptions.AddNew )
				{
					source.Insert(index, item);
					added++;

					// Recalculate original indexes upon insertion
					foreach ( var items in updateCandidates.Values )
					{
#if !NETFX_CORE // List<T>.ForEach is not supported in NETFX_CORE (WinRT)
						items.ForEach(x =>
#else
						foreach ( var x in items )
#endif
						{
							if ( x.Index >= index )
							{
								x.Index++;
							}
						}
#if !NETFX_CORE
						);
#endif
					}
				}

				index++;
			}

			if ( (options & CollectionMergeOptions.RemoveOld) == CollectionMergeOptions.RemoveOld )
			{
				foreach ( var items in updateCandidates.Values )
				{
					// Тут нужна сортировка по индексу в порядке убывания, чтобы не вылететь за границы коллекции
					// Т.к. items в дальнейшем нам больше не нужен, можем смело его "испортить"
					// Изначально гарантировано, что его элементы идут в правильном порядке, т.е. по возрастанию индексов оригинальной коллекции
					items.Reverse();

#if !NETFX_CORE // List<T>.ForEach is not supported in NETFX_CORE (WinRT)
					items.ForEach(x =>
#else
					foreach ( var x in items )
#endif
					{
						// И еще один safeguard, для порядку
						//Contract.Assert(x.Index < this.Items.Count);
						Contract.Assume(x.Index >= 0);
						source.RemoveAt(x.Index);
						removed++;
					}
#if !NETFX_CORE
					);
#endif
				}
			}

			return new CollectionMergeResults(added, updated, removed);
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
		/// foreach ( T o in result.Added )
		/// {
		///		// Process added object
		///	}
		///	
		///	foreach ( T o in result.Changed )
		///	{
		///		// Process changed object
		///	}
		///	
		///	foreach ( T o in result.Deleted )
		///	{
		///		// Process deleted object
		///	}
		/// </code>
		/// </example>
#if WINDOWS_PHONE && CODE_ANALYSIS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
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

			IXCollection<T> added = new XCollection<T>();
			IXCollection<T> changed = new XCollection<T>();
			Contract.Assume(Contract.ForAll(source, x => x != null));
			IXCollection<T> deleted = new XCollection<T>(source);
			IXCollection<T> modified = new XCollection<T>();
			IXCollection<T> nonModified = new XCollection<T>();

#if WINDOWS_PHONE // God knows why it can't be proven by the static checker - old (from Silverlight3) Microsoft.Contracts library?
			Contract.Assume(source != null);
#endif

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
		public static IXCollection<T> UnmodifiableCollection<T>(IXCollection<T> collection)
		{
			Contract.Requires( collection != null );

			return new ReadOnlyXCollection<T>(collection);
		}

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
		public static XFilteredCollection<T> FilteredList<T>(IXList<T> collection, Func<T, bool> filter)
			where T : XObject
		{
			Contract.Requires(collection != null);
			Contract.Requires(filter != null);

			return new FuncFilteredCollection<T>(collection, filter);
		}

		public static XFilteredCollection<T> FilteredList<T>(IXList<T> collection, Predicate<T> filter)
			where T : XObject
		{
			Contract.Requires(collection != null);
			Contract.Requires(filter != null);

			return FilteredList(collection, filter.ToFunction());
		}
		#endregion

		#region Predicate<T> -> Func<T, bool>
		internal static Func<T, bool> ToFunction<T>(this Predicate<T> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Ensures(Contract.Result<Func<T, bool>>() != null);

			return new Func<T, bool>(new PredicateToFunctionConverter<T>(predicate).Function);
		}

		private sealed class PredicateToFunctionConverter<T>
		{
			private readonly Predicate<T> predicate;

			public PredicateToFunctionConverter(Predicate<T> predicate)
			{
				if ( predicate == null )
				{
					throw new ArgumentNullException("predicate");
				}

				Contract.EndContractBlock();

				this.predicate = predicate;
			}

			public bool Function(T arg)
			{
				return this.predicate(arg);
			}
		}
		#endregion

		#region class ReadOnlyXCollection`1
#if !(SILVERLIGHT || NETFX_CORE)
		[Serializable]
#endif
		private class ReadOnlyXCollection<T> : IXCollection<T>
#if !(SILVERLIGHT || NETFX_CORE)
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
			event EventHandler IXUpdatable<IXCollection<T>>.Updated
			{
				add { this.collection.Updated += value; }
				remove { this.collection.Updated -= value; }
			}

			void IXUpdatable<IXCollection<T>>.BeginUpdate()
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			void IXUpdatable<IXCollection<T>>.EndUpdate()
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
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

#if !(SILVERLIGHT || NETFX_CORE)
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
#if !(SILVERLIGHT || NETFX_CORE)
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

			public FuncFilteredCollection(IXList<T> collection, Func<T, bool> filter)
				: base(collection)
			{
				if ( filter == null )
				{
					throw new ArgumentNullException("filter");
				}

				Contract.Requires(collection != null);

				this._filter = filter;
			}

#if WINDOWS_PHONE && CODE_ANALYSIS
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
			protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> originalCollection)
			{
#if !(SILVERLIGHT || NETFX_CORE)
				Func<T, bool> filter = (Func<T, bool>) this._filter.Clone();
#else
				Contract.Assume(this._filter != null);
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

		#region class XMergeItem`1
		private sealed class XMergeItem<T>
			where T : XObject
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
}
