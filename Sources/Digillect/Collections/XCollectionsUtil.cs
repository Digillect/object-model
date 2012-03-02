using System;
using System.Collections.Generic;
using System.Collections.Specialized;

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
		public static bool RetainAll<T>(this ICollection<T> source, IEnumerable<T> collection)
		{
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
			return new ReadOnlyXCollection<T>(collection);
		}

		public static IXList<T> UnmodifiableList<T>(IXList<T> collection)
		{
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
			return new PredicateXFilteredCollection<T>(collection, filter);
		}

		public static XFilteredCollection<T> FilteredList<T>(IXList<T> collection, Func<T, bool> filter)
			where T : XObject
		{
			return new FuncXFilteredCollection<T>(collection, filter);
		}

		public static XFilteredCollection<T> FilteredList<T>( IXList<T> collection, int start, int count )
			where T : XObject
		{
			return new RangeFilteredCollection<T>( collection, start, count );
		}
		#endregion

		#region class ReadOnlyXCollection`1
#if !SILVERLIGHT
		[Serializable]
#endif
		private class ReadOnlyXCollection<T> : IXCollection<T>
#if !SILVERLIGHT
			, ICloneable
#endif
		{
			private readonly IXCollection<T> m_collection;

			#region Constructor
			public ReadOnlyXCollection(IXCollection<T> collection)
			{
				if ( collection == null )
				{
					throw new ArgumentNullException("collection");
				}

				m_collection = collection;
			}
			#endregion

			#region IXCollection`1 Members
			bool IXCollection<T>.Contains(XKey key)
			{
				return m_collection.Contains(key);
			}

			T IXCollection<T>.Find(XKey key)
			{
				return m_collection.Find(key);
			}

			bool IXCollection<T>.Remove(XKey key)
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			ICollection<XKey> IXCollection<T>.GetKeys()
			{
				return m_collection.GetKeys();
			}

			IXCollection<T> IXCollection<T>.Clone(bool deep)
			{
				return m_collection.Clone(deep);
			}
			#endregion

			#region IXUpdatable`1 Members
			event EventHandler IXUpdatable<IXCollection<T>>.Updated
			{
				add { m_collection.Updated += value; }
				remove { m_collection.Updated -= value; }
			}

			void IXUpdatable<IXCollection<T>>.BeginUpdate()
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			void IXUpdatable<IXCollection<T>>.EndUpdate()
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			bool IXUpdatable<IXCollection<T>>.UpdateRequired(IXCollection<T> source)
			{
				return m_collection.UpdateRequired(source);
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
				return m_collection.Contains(item);
			}

			void ICollection<T>.CopyTo(T[] array, int arrayIndex)
			{
				m_collection.CopyTo(array, arrayIndex);
			}

			int ICollection<T>.Count
			{
				get { return m_collection.Count; }
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
				return m_collection.GetEnumerator();
			}
			#endregion

			#region IEnumerable Members
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return m_collection.GetEnumerator();
			}
			#endregion

			#region IEquatable`1 Members
			bool IEquatable<IXCollection<T>>.Equals(IXCollection<T> other)
			{
				return m_collection.Equals(other);
			}
			#endregion

			#region INotifyCollectionChanged Members
			event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
			{
				add { m_collection.CollectionChanged += value; }
				remove { m_collection.CollectionChanged -= value; }
			}
			#endregion

#if !SILVERLIGHT
			#region ICloneable Members
			object ICloneable.Clone()
			{
				return m_collection.Clone(true);
			}
			#endregion
#endif

			#region Object Overrides
			public override bool Equals(object obj)
			{
				return m_collection.Equals(obj);
			}

			public override int GetHashCode()
			{
				return m_collection.GetHashCode();
			}

			public override string ToString()
			{
				return m_collection.ToString();
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
			private readonly IXList<T> m_collection;

			#region Constructor
			public ReadOnlyXList(IXList<T> collection)
				: base(collection)
			{
				m_collection = collection;
			}
			#endregion

			#region IXList`1 Members
			int IXList<T>.IndexOf(XKey key)
			{
				return m_collection.IndexOf(key);
			}

			IList<XKey> IXList<T>.GetKeys()
			{
				return m_collection.GetKeys();
			}
			#endregion

			#region IList`1 Members
			int IList<T>.IndexOf(T item)
			{
				return m_collection.IndexOf(item);
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
				get { return m_collection[index]; }
				set { throw new NotSupportedException(Resources.XCollectionReadOnlyException); }
			}
			#endregion

			#region IEquatable`1 Members
			bool IEquatable<IXList<T>>.Equals(IXList<T> other)
			{
				return m_collection.Equals(other);
			}
			#endregion
		}
		#endregion

		#region class PredicateXFilteredCollection`1
		private class PredicateXFilteredCollection<T> : XFilteredCollection<T>
			where T : XObject
		{
			private Predicate<T> m_filter;

			public PredicateXFilteredCollection(IXList<T> collection, Predicate<T> filter)
				: base(collection)
			{
				if ( filter == null )
				{
					throw new ArgumentNullException("filter");
				}

				m_filter = filter;
			}

			protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> collection)
			{
#if !SILVERLIGHT
				Predicate<T> filter = (Predicate<T>) m_filter.Clone();
#else
				Predicate<T> filter = m_filter;
#endif

				return new PredicateXFilteredCollection<T>(collection, filter);
			}

			protected override bool Filter(T obj, int index)
			{
				return m_filter(obj);
			}
		}
		#endregion

		#region class FuncXFilteredCollection`1
		private class FuncXFilteredCollection<T> : XFilteredCollection<T>
			where T : XObject
		{
			private Func<T, bool> m_filter;

			public FuncXFilteredCollection(IXList<T> collection, Func<T, bool> filter)
				: base(collection)
			{
				if ( filter == null )
				{
					throw new ArgumentNullException("filter");
				}

				m_filter = filter;
			}

			protected override XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> collection)
			{
#if !SILVERLIGHT
				Func<T, bool> filter = (Func<T, bool>) m_filter.Clone();
#else
				Func<T, bool> filter = m_filter;
#endif

				return new FuncXFilteredCollection<T>(collection, filter);
			}

			protected override bool Filter(T obj, int index)
			{
				return m_filter(obj);
			}
		} 
		#endregion
		#region class RangeFilteredCollection`1
		private class RangeFilteredCollection<T> : XFilteredCollection<T>
			where T : XObject
		{
			private int m_start;
			private int m_count;

			public RangeFilteredCollection( IXList<T> collection, int start, int count )
				: base( collection )
			{
				m_start = start;
				m_count = count;
			}

			protected override XFilteredCollection<T> CreateInstanceOfSameType( IXList<T> collection )
			{
				return new RangeFilteredCollection<T>( collection, m_start, m_count );
			}

			protected override bool Filter( T obj, int index )
			{
				if( index < m_start )
					return false;

				if( m_count == 0 )
					return true;

				return index < m_start + m_count;
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
