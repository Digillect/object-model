using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

using Digillect.Properties;

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

		#region RemoveAll`2 Extension
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
		#endregion

		#region RetainAll`2 Extension
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

					if ( (targetKey = targetObject.GetKey()) != null && (sourceObject = source.Find(targetKey)) != null )
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
		public static Func<T, bool> ToFunction<T>(this Predicate<T> predicate)
		{
			Contract.Requires(predicate != null);

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
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed by underlying collection")]
			bool IXCollection<T>.ContainsKey(XKey key)
			{
				return this.collection.ContainsKey(key);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed by underlying collection")]
			T IXCollection<T>.Find(XKey key)
			{
				return this.collection.Find( key );
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation not needed since argument not used")]
			bool IXCollection<T>.Remove(XKey key)
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
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
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
			}

			void IXUpdatable<IXCollection<T>>.EndUpdate()
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation not needed since argument not used")]
			bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
			{
				return false;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation not needed since argument not used")]
			void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source )
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
			}
			#endregion

			#region ICollection`1 Members
			void ICollection<T>.Add(T item)
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
			}

			void ICollection<T>.Clear()
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
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
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
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
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed by underlying collection")]
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
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
			}

			void IList<T>.RemoveAt(int index)
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
			}

			T IList<T>.this[int index]
			{
				get { return this.collection[index]; }
				set { throw new NotSupportedException( Resources.XCollectionReadOnlyException ); }
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
			private readonly Func<T, bool> filter;

			public FuncFilteredCollection(IXList<T> collection, Func<T, bool> filter)
				: base(collection)
			{
				if ( filter == null )
				{
					throw new ArgumentNullException("filter");
				}

				Contract.Requires(collection != null);

				this.filter = filter;
			}

			protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> originalCollection)
			{
#if !(SILVERLIGHT || NETFX_CORE)
				Func<T, bool> filter = (Func<T, bool>) this.filter.Clone();
#else
				Contract.Assume(this.filter != null);
				Func<T, bool> filter = this.filter;
#endif

				return new FuncFilteredCollection<T>(originalCollection, filter);
			}

			protected override bool Filter(T obj)
			{
				return this.filter(obj);
			}
		} 
		#endregion
	}

	#region class XCollectionDifference`1
	public sealed class XCollectionDifference<T>
	{
		/// <summary>
		/// Gets the collection of objects presented in a <b>target</b> collection but not found in a <b>source</b> one.
		/// </summary>
		public IXCollection<T> Added { get; private set; }

		/// <summary>
		/// Gets the collection of objects presented in both <b>source</b> and <b>target</b> collections and which are not the same.
		/// </summary>
		public IXCollection<T> Changed { get; private set; }

		/// <summary>
		/// Gets the collection of objects presented in a <b>source</b> collection, but not found in a <b>target</b> one.
		/// </summary>
		public IXCollection<T> Deleted { get; private set; }

		/// <summary>
		/// Gets the collection of objects which are <see cref="Added">added</see> or <see cref="Changed">changed</see>.
		/// </summary>
		public IXCollection<T> Modified { get; private set; }

		/// <summary>
		/// Gets the collection of objects which are the same in both <b>source</b> and <b>target</b> collections.
		/// </summary>
		public IXCollection<T> NonModified { get; private set; }

		internal XCollectionDifference(IXCollection<T> added, IXCollection<T> changed, IXCollection<T> deleted, IXCollection<T> modified, IXCollection<T> nonModified)
		{
			this.Added = added;
			this.Changed = changed;
			this.Deleted = deleted;
			this.Modified = modified;
			this.NonModified = nonModified;
		}
	}
	#endregion
}
