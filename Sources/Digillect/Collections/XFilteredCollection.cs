using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using Digillect.Properties;

namespace Digillect.Collections
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public abstract class XFilteredCollection<T> : IXList<T>, IDisposable
#if !SILVERLIGHT
		, ICloneable
#endif
		where T : XObject
	{
		private IXList<T> m_originalCollection;

		private int m_count = -1;
		private int m_version;

		#region Constructor/Disposer
		protected XFilteredCollection(IXList<T> originalCollection)
		{
			if ( originalCollection == null )
			{
				throw new ArgumentNullException("originalCollection");
			}

			m_originalCollection = originalCollection;

			m_originalCollection.CollectionChanged += OriginalCollection_CollectionChanged;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if ( disposing )
			{
				m_originalCollection.CollectionChanged -= OriginalCollection_CollectionChanged;
			}
		}
		#endregion

		public IXList<T> OriginalCollection
		{
			get { return m_originalCollection; }
		}

		#region IXList`1 Members
		public int IndexOf(XKey key)
		{
			return CalcFilteredIndex(m_originalCollection.IndexOf(key));
		}

		public IList<XKey> GetKeys()
		{
			List<XKey> identifiers = new List<XKey>();

			foreach ( T obj in this )
			{
				identifiers.Add(obj.GetKey());
			}

			return new ReadOnlyCollection<XKey>(identifiers);
		}
		#endregion

		#region IXCollection`1 Members
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public bool Contains(XKey key)
		{
			return Find(key) != null;
		}

		public T Find(XKey key)
		{
			T obj = m_originalCollection.Find(key);
			int idx = m_originalCollection.IndexOf( obj );

			return Equals(obj, default(T)) || Filter(obj, idx) ? obj : default(T);
		}

		ICollection<XKey> IXCollection<T>.GetKeys()
		{
			return GetKeys();
		}

		bool IXCollection<T>.Remove(XKey key)
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}

		IXCollection<T> IXCollection<T>.Clone(bool deep)
		{
			return Clone(deep);
		}

		public virtual XFilteredCollection<T> Clone(bool deep)
		{
			IXList<T> collection = deep ? (IXList<T>) m_originalCollection.Clone(true) : m_originalCollection;

			return CreateInstanceOfSameType(collection);
		}
		#endregion

		#region IXUpdatable`1 Members
		event EventHandler IXUpdatable<IXCollection<T>>.Updated
		{
			add { m_originalCollection.Updated += value; }
			remove { m_originalCollection.Updated -= value; }
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
			return false;
		}

		void IXUpdatable<IXCollection<T>>.Update( IXCollection<T> source )
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}
		#endregion

		#region IList`1 Members
		public T this[int index]
		{
			get { return m_originalCollection[CalcOriginalIndex(index)]; }
		}

		T IList<T>.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(Resources.XCollectionReadOnlyException); }
		}

		public int IndexOf(T item)
		{
			return CalcFilteredIndex(m_originalCollection.IndexOf(item));
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}

		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}
		#endregion

		#region ICollection`1 Members
		public int Count
		{
			get
			{
				if ( m_count == -1 )
				{
					lock ( m_originalCollection )
					{
						if ( m_count == -1 )
						{
							int count = 0;

							for ( int i = 0; i < m_originalCollection.Count; i++ )
							{
								if ( Filter(m_originalCollection[i], i) )
								{
									count++;
								}
							}

							m_count = count;
						}
					}
				}

				return m_count;
			}
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return true; }
		}

		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}

		void ICollection<T>.Clear()
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}

		public bool Contains(T item)
		{
			return m_originalCollection.Contains(item) && Filter(item, m_originalCollection.IndexOf(item));
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			if ( array == null )
			{
				throw new ArgumentNullException("array");
			}

			foreach ( T obj in this )
			{
				array[arrayIndex++] = obj;
			}
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}
		#endregion

		#region IEnumerable`1 Members
		public IEnumerator<T> GetEnumerator()
		{
			int version = m_version;

			for ( int i = 0; i < m_originalCollection.Count; i++ )
			{
				if ( version != m_version )
				{
					throw new InvalidOperationException(Resources.XCollectionEnumFailedVersionException);
				}

				T obj = m_originalCollection[i];

				if ( Filter(obj, i) )
				{
					yield return obj;
				}
			}
		}
		#endregion

		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region IEquatable`1 Members
		public virtual bool Equals(IXCollection<T> other)
		{
			if ( other == null || this.Count != other.Count )
			{
				return false;
			}

			foreach ( T item in this )
			{
				if ( item == null || !Equals(item, other.Find(item.GetKey())) )
				{
					return false;
				}
			}

			return true;
		}

		public virtual bool Equals(IXList<T> other)
		{
			if ( other == null || this.Count != other.Count )
			{
				return false;
			}

			for ( int i = 0; i < this.Count; i++ )
			{
				T item = this[i];

				if ( !Equals(item, other[i]) )
				{
					return false;
				}
			}

			return true;
		}
		#endregion

#if !SILVERLIGHT
		#region ICloneable Members
		object ICloneable.Clone()
		{
			return Clone(true);
		}
		#endregion
#endif

		#region Protected Methods
		protected abstract XFilteredCollection<T> CreateInstanceOfSameType(IXList<T> collection);

		/// <summary>
		/// Determines whether an item satisfies a filtering criteria.
		/// </summary>
		/// <param name="obj">An item to check.</param>
		/// <param name="index"></param>
		/// <returns><see langword="true"/> if the item passes the filter; otherwise, <see langword="false"/>.</returns>
		protected abstract bool Filter(T obj, int index);

		protected int CalcFilteredIndex(int originalIndex)
		{
			if ( originalIndex < 0 )
			{
				return -1;
			}

			int filteredIndex = -1;

			for ( int i = 0; i <= originalIndex; i++ )
			{
				if ( Filter(m_originalCollection[i], i) )
				{
					filteredIndex++;
				}
			}

			return filteredIndex;
		}

		protected int CalcOriginalIndex(int filteredIndex)
		{
			if ( filteredIndex < 0 )
			{
				return -1;
			}

			int originalIndex = -1;
			int count = 0;

			for ( int i = 0; i < m_originalCollection.Count && count <= filteredIndex; i++ )
			{
				if ( Filter(m_originalCollection[i],i) )
				{
					originalIndex = i;
					count++;
				}
			}

			return originalIndex;
		}
		#endregion

		#region Event Handlers
		private void OriginalCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			m_version++;

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				m_count = -1;
			}

			if ( CollectionChanged != null )
			{
				NotifyCollectionChangedEventArgs args;

				switch ( e.Action )
				{
#if SILVERLIGHT
					case NotifyCollectionChangedAction.Add:
						if ( !Filter((T) e.NewItems[0], e.NewStartingIndex) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalcFilteredIndex(e.NewStartingIndex));
						break;
					case NotifyCollectionChangedAction.Remove:
						if ( !Filter((T) e.OldItems[0],e.OldStartingIndex) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalcFilteredIndex(e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						if ( !Filter((T) e.NewItems[0], e.NewStartingIndex) && !Filter((T) e.OldItems[0],e.OldStartingIndex) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalcFilteredIndex(e.NewStartingIndex));
						break;
#else
					case NotifyCollectionChangedAction.Add:
						IList newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();
						if ( newItems.Count == 0 )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, newItems, CalcFilteredIndex(e.NewStartingIndex));
						break;
					case NotifyCollectionChangedAction.Remove:
						IList oldItems = e.OldItems.Cast<T>().Where(Filter).ToArray();
						if ( oldItems.Count == 0 )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, oldItems, CalcFilteredIndex(e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Move:
						// e.NewItems and e.OldItems have the same content
						IList changedItems = e.NewItems.Cast<T>().Where(Filter).ToArray();
						if ( changedItems.Count == 0 )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, changedItems, CalcFilteredIndex(e.NewStartingIndex), CalcFilteredIndex(e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						newItems = e.NewItems.Cast<T>().Where(Filter).ToArray();
						oldItems = e.OldItems.Cast<T>().Where(Filter).ToArray();
						if ( newItems.Count == 0 && oldItems.Count == 0 )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, newItems, oldItems, CalcFilteredIndex(e.NewStartingIndex));
						break;
#endif
					case NotifyCollectionChangedAction.Reset:
						args = new NotifyCollectionChangedEventArgs(e.Action);
						break;
					default:
						throw new ArgumentException(e.Action.ToString(), "e");
				}

				CollectionChanged(this, args);
			}
		}
		#endregion
	}
}
