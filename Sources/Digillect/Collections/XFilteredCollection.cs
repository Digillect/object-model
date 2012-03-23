using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
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
		private readonly IXList<T> originalCollection;

		private int count = -1;
		private int version;

		#region Constructor/Disposer
		protected XFilteredCollection(IXList<T> originalCollection)
		{
			Contract.Requires(originalCollection != null, "originalCollection");

			if ( originalCollection == null )
			{
				throw new ArgumentNullException("originalCollection");
			}

			this.originalCollection = originalCollection;

			this.originalCollection.CollectionChanged += OriginalCollection_CollectionChanged;
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
				this.originalCollection.CollectionChanged -= OriginalCollection_CollectionChanged;
			}
		}
		#endregion

		#region OriginalCollection
		public IXList<T> OriginalCollection
		{
			get { return this.originalCollection; }
		}
		#endregion

		#region IXList`1 Members
		public int IndexOf(XKey key)
		{
			return CalcFilteredIndex(this.originalCollection.IndexOf(key));
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
			T obj = this.originalCollection.Find(key);

			return obj == null || Filter(obj) ? obj : null;
		}

		public IEnumerable<XKey> GetKeys()
		{
			return this.Select(obj => obj == null ? null : obj.GetKey());
		}

		bool IXCollection<T>.Remove(XKey key)
		{
			throw new NotSupportedException(Resources.XCollectionReadOnlyException);
		}

		IXCollection<T> IXCollection<T>.Clone( bool deep )
		{
			return Clone( deep );
		}

		public virtual XFilteredCollection<T> Clone( bool deep )
		{
			IXList<T> collection = deep ? (IXList<T>) this.originalCollection.Clone( true ) : this.originalCollection;

			return CreateInstanceOfSameType( collection );
		}
		#endregion

		#region IXUpdatable`1 Members
		event EventHandler IXUpdatable<IXCollection<T>>.Updated
		{
			add { this.originalCollection.Updated += value; }
			remove { this.originalCollection.Updated -= value; }
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
			get
			{
				var originalIndex = CalcOriginalIndex( index );

				return originalIndex >= 0 ? this.originalCollection[originalIndex] : default( T );
			}
		}

		T IList<T>.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(Resources.XCollectionReadOnlyException); }
		}

		public int IndexOf(T item)
		{
			Contract.Ensures( Contract.Result<int>() >= -1 );

			return CalcFilteredIndex( this.originalCollection.IndexOf( item ) );
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
				if ( this.count == -1 )
				{
					lock ( this.originalCollection )
					{
						if ( this.count == -1 )
						{
							this.count = this.originalCollection.Count(Filter);
						}
					}
				}

				Contract.Assume(this.count >= 0);

				return this.count;
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
			bool contains = this.originalCollection.Contains(item) && Filter(item);

			if ( contains )
			{
				Contract.Assume(this.Count > 0);
			}

			return contains;
			//return this.originalCollection.Contains(item) && Filter(item, this.originalCollection.IndexOf(item));
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
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
			int version = this.version;

			for ( int i = 0; i < originalCollection.Count; i++ )
			{
				if ( this.version != version )
				{
					throw new InvalidOperationException(Resources.XCollectionEnumFailedVersionException);
				}

				T obj = originalCollection[i];

				if ( Filter(obj) )
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
			return Clone( true );
		}
		#endregion
#endif

		#region Protected Methods
		protected abstract XFilteredCollection<T> CreateInstanceOfSameType( IXList<T> collection );

		/// <summary>
		/// Determines whether an item satisfies a filtering criteria.
		/// </summary>
		/// <param name="obj">An item to check.</param>
		/// <returns><see langword="true"/> if the item passes the filter; otherwise, <see langword="false"/>.</returns>
		protected abstract bool Filter(T obj);

		protected int CalcFilteredIndex(int originalIndex)
		{
			Contract.Ensures(Contract.Result<int>() >= -1);

			if ( originalIndex < 0 )
			{
				return -1;
			}

			int filteredIndex = -1;

			for ( int i = 0; i <= originalIndex; i++ )
			{
				if ( Filter(originalCollection[i]) )
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

			for ( int i = 0, counter = 0; i < originalCollection.Count && counter <= filteredIndex; i++ )
			{
				if ( Filter(originalCollection[i]) )
				{
					originalIndex = i;
					counter++;
				}
			}

			return originalIndex;
		}
		#endregion

		#region Event Handlers
		private void OriginalCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			version++;

			if ( e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset )
			{
				count = -1;
			}

			if ( CollectionChanged != null )
			{
				NotifyCollectionChangedEventArgs args;

				switch ( e.Action )
				{
#if SILVERLIGHT
					case NotifyCollectionChangedAction.Add:
						Contract.Assume(e.NewItems.Count > 0);
						if ( !Filter((T) e.NewItems[0]) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], CalcFilteredIndex(e.NewStartingIndex));
						break;
					case NotifyCollectionChangedAction.Remove:
						Contract.Assume(e.OldItems.Count > 0);
						if ( !Filter((T) e.OldItems[0]) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], CalcFilteredIndex(e.OldStartingIndex));
						break;
					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						Contract.Assume(e.NewItems.Count > 0);
						Contract.Assume(e.OldItems.Count > 0);
						if ( !Filter((T) e.NewItems[0]) && !Filter((T) e.OldItems[0]) )
						{
							return;
						}
						args = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], CalcFilteredIndex(e.NewStartingIndex));
						break;
#else
					case NotifyCollectionChangedAction.Add:
						Contract.Assume( e.NewItems != null );

						var newItems = e.NewItems.Cast<T>().Where( Filter ).ToArray();

						if( newItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, newItems, CalcFilteredIndex( e.NewStartingIndex ) );

						break;

					case NotifyCollectionChangedAction.Remove:
						Contract.Assume( e.OldItems != null );

						var oldItems = e.OldItems.Cast<T>().Where( Filter ).ToArray();

						if( oldItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, oldItems, CalcFilteredIndex( e.OldStartingIndex ) );
						break;

					case NotifyCollectionChangedAction.Move:
						// e.NewItems and e.OldItems have the same content
						Contract.Assume( e.NewItems != null );

						newItems = e.NewItems.Cast<T>().Where( Filter ).ToArray();

						if( newItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, newItems, CalcFilteredIndex( e.NewStartingIndex ), CalcFilteredIndex( e.OldStartingIndex ) );
						break;

					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						Contract.Assume( e.NewItems != null );
						Contract.Assume( e.OldItems != null );

						newItems = e.NewItems.Cast<T>().Where( Filter ).ToArray();
						oldItems = e.OldItems.Cast<T>().Where( Filter ).ToArray();

						if( newItems.Length == 0 && oldItems.Length == 0 )
							return;

						args = new NotifyCollectionChangedEventArgs( e.Action, newItems, oldItems, CalcFilteredIndex( e.NewStartingIndex ) );
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
