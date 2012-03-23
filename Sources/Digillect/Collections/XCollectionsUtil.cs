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
			Contract.Requires(source != null, "source");

			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

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
			Contract.Requires(source != null, "source");

			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

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
			Contract.Requires(source != null, "source");

			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

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
			Contract.Requires(source != null, "source");
			Contract.Requires(target != null, "target");

			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			if ( target == null )
			{
				throw new ArgumentNullException("target");
			}

			IXCollection<T> added = new XCollection<T>();
			IXCollection<T> changed = new XCollection<T>();
			IXCollection<T> deleted = new XCollection<T>(source);
			IXCollection<T> modified = new XCollection<T>();
			IXCollection<T> nonModified = new XCollection<T>();

			foreach ( T targetObject in target )
			{
				T sourceObject;

				if ( targetObject != null && (sourceObject = source.Find(targetObject.GetKey())) != null )
				{
					deleted.Remove(sourceObject.GetKey());

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
		public static XFilteredCollection<T> FilteredList<T>(IXList<T> collection, Predicate<T> filter)
			where T : XObject
		{
			Contract.Requires( collection != null );
			Contract.Requires( filter != null );

			return new PredicateFilteredCollection<T>( collection, filter );
		}

		public static XFilteredCollection<T> FilteredList<T>(IXList<T> collection, Func<T, bool> filter)
			where T : XObject
		{
			Contract.Requires( collection != null );
			Contract.Requires( filter != null );

			return new FuncFilteredCollection<T>( collection, filter );
		}
		#endregion

		#region class ReadOnlyXCollection`1
#if !SILVERLIGHT
		[Serializable]
#endif
		private class ReadOnlyXCollection<T> : IXCollection<T>
		{
			private readonly IXCollection<T> collection;

			#region Constructor
			public ReadOnlyXCollection(IXCollection<T> collection)
			{
				Contract.Requires(collection != null, "collection");

				if ( collection == null )
				{
					throw new ArgumentNullException("collection");
				}

				this.collection = collection;
			}
			#endregion

			#region ObjectInvariant
			[ContractInvariantMethod]
			[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts." )]
			private void ObjectInvariant()
			{
				Contract.Invariant(this.Count == this.collection.Count);
			}
			#endregion

			#region IXCollection`1 Members
			bool IXCollection<T>.Contains(XKey key)
			{
				return this.collection.Contains(key);
			}

			T IXCollection<T>.Find(XKey key)
			{
				return this.collection.Find( key );
			}

			bool IXCollection<T>.Remove(XKey key)
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			IEnumerable<XKey> IXCollection<T>.GetKeys()
			{
				return this.collection.GetKeys();
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
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			void IXUpdatable<IXCollection<T>>.EndUpdate()
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
			{
				return this.collection.IsUpdateRequired( source );
			}

			void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source )
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}
			#endregion

			#region ICollection`1 Members
			void ICollection<T>.Add(T item)
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			void ICollection<T>.Clear()
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
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
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
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
		}
		#endregion

		#region class ReadOnlyXList`1
#if !SILVERLIGHT
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

			#region ObjectInvariant
			[ContractInvariantMethod]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
			private void ObjectInvariant()
			{
				Contract.Invariant(this.Count == this.collection.Count);
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
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			void IList<T>.RemoveAt(int index)
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			T IList<T>.this[int index]
			{
				get { return this.collection[index]; }
				set { throw new NotSupportedException(Resources.XCollectionReadOnlyException); }
			}
			#endregion

			#region IEquatable`1 Members
			bool IEquatable<IXList<T>>.Equals(IXList<T> other)
			{
				return this.collection.Equals(other);
			}
			#endregion
		}
		#endregion

		#region class PredicateFilteredCollection`1
		private class PredicateFilteredCollection<T> : XFilteredCollection<T>
			where T : XObject
		{
			private readonly Predicate<T> filter;

			public PredicateFilteredCollection(IXList<T> collection, Predicate<T> filter)
				: base(collection)
			{
				Contract.Requires( collection != null );
				Contract.Requires( filter != null );

				this.filter = filter;
			}

			protected override bool Filter(T obj)
			{
				return this.filter(obj);
			}
		}
		#endregion

		#region class FuncFilteredCollection`1
		private class FuncFilteredCollection<T> : XFilteredCollection<T>
			where T : XObject
		{
			private Func<T, bool> filter;

			public FuncFilteredCollection(IXList<T> collection, Func<T, bool> filter)
				: base(collection)
			{
				Contract.Requires( collection != null );
				Contract.Requires( filter != null );

				this.filter = filter;
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
