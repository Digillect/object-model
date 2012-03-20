using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

using Digillect.Properties;

namespace Digillect.Collections
{
	/// <summary>
	/// A collection of "X"-objects with support for cloning, updating and event notification.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if !SILVERLIGHT
	[Serializable]
#endif
	public class XCollection<T> : Collection<T>, IXList<T>, INotifyPropertyChanged
		where T : XObject
	{
#if !SILVERLIGHT
		[NonSerialized]
#endif
		private short updateCount;

		#region Constructor
		/// <summary>
		/// Initializes new instance of the <see cref="XCollection&lt;T&gt;"/> class.
		/// </summary>
		public XCollection()
		{
		}

		/// <summary>
		/// Initializes new instance of the <see cref="XCollection&lt;T&gt;"/> class using elements of the provided enumeration as the source for this list.
		/// </summary>
		/// <param name="collection">The enumeration which copy is used as the parameter for the <see cref="Collection&lt;T&gt;(IList&lt;T&gt;)"/> constructor.</param>
		public XCollection(IEnumerable<T> collection)
			: base(new List<T>(collection))
		{
			Contract.Requires( collection != null );
		}

#if false
		/// <summary>
		/// Initializes new instance of the <see cref="XCollection&lt;T&gt;"/> class using provided list as the underlying one.
		/// </summary>
		/// <param name="items">The list used as the parameter for the <see cref="Collection&lt;T&gt;(IList&lt;T&gt;)"/> constructor.</param>
		private XCollection(IList<T> items)
			: base(items)
		{
		}
#endif
		#endregion

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts." )]
		private void ObjectInvariant()
		{
			Contract.Invariant( this.Items != null );
			Contract.Invariant( this.Items.Count >= 0 );
		}
		#endregion

		#region Protected Properties
		protected bool IsInUpdate
		{
			get { return this.updateCount > 0; }
		}
		#endregion

		#region Events and Event Raisers
		public event EventHandler Updated;
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnUpdated(EventArgs e)
		{
			if ( this.updateCount == 0 )
			{
				var handler = Updated;

				if( handler != null )
					handler(this, e);
			}
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if ( this.updateCount == 0 )
			{
				var handler = CollectionChanged;

				if( handler != null )
					handler(this, e);
			}
		}

		protected void OnPropertyChanged(string propertyName)
		{
			Contract.Requires( propertyName != null );

			if ( this.updateCount == 0 )
			{
				OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if ( this.updateCount == 0 )
			{
				var handler = PropertyChanged;

				if( handler != null )
					handler(this, e);
			}
		}
		#endregion

		#region Public Methods
		public void AddRange(IEnumerable<T> collection)
		{
			Contract.Requires( collection != null );

			InsertRange(this.Count, collection);
		}

		/// <summary>
		/// Determines whether the <b>collection</b> contains an item with the specific key.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <see cref="IXCollection&lt;T&gt;"/>.</param>
		/// <returns><see langword="true"/> if item is found in the <see cref="IXCollection&lt;T&gt;"/>; otherwise, <see langword="false"/>.</returns>
		public bool Contains(XKey key)
		{
			return IndexOf(key) != -1;
		}

		public XCollection<T> Derive(Predicate<T> filter)
		{
			Contract.Requires( filter != null );
			Contract.Ensures( Contract.Result<XCollection<T>>() != null );

			var derived = (XCollection<T>) CreateInstanceOfSameType();

			foreach ( var item in this.Items )
			{
				if ( filter(item) )
				{
					derived.Items.Add(item);
				}
			}

			return derived;
		}

		/// <summary>
		/// Gets an item with the specific key.
		/// </summary>
		/// <param name="key">The key of the item to find.</param>
		/// <returns>An item with the specified key if the item exists in the <b>collection</b>; otherwise, <see langword="null"/>.</returns>
		public T Find(XKey key)
		{
			int index = IndexOf(key);

			return index < 0 ? null : this.Items[index];
		}

		public void ForEach(Action<T> action)
		{
			Contract.Requires( action != null );

			if( this.Items.Count > 0 )
			{
				for ( int i = 0; i < this.Items.Count; i++ )
				{
					action(this.Items[i]);
				}
			}
		}

		IEnumerable<XKey> IXCollection<T>.GetKeys()
		{
			Contract.Ensures( Contract.Result<IEnumerable<XKey>>() != null );

			return this.Items.Select( obj => obj == null ? null : obj.GetKey() );
		}

		/// <summary>
		/// Determines the index of an item with the specific key within the collection.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <b>collection</b>.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int IndexOf(XKey key)
		{
			Contract.Ensures( Contract.Result<int>() >= -1 );

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				var item = this.Items[i];

				if ( item != null && Equals(item.GetKey(), key) )
				{
					return i;
				}
			}

			return -1;
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			Contract.Requires( collection != null );
			Contract.Requires( 0 <= index && index <= this.Count );

			/*
			List<T> is2 = this.Items as List<T>;

			if ( is2 != null )
			{
				Contract.Assert( index < is2.Count );
				is2.InsertRange( index, collection );
			}
			else
			{
				using ( IEnumerator<T> enumerator = collection.GetEnumerator() )
				{
					while ( enumerator.MoveNext() )
					{
						Contract.Assert( index <= this.Count );
						Insert(index++, enumerator.Current);
					}
				}
			}
			*/
			using( IEnumerator<T> enumerator = collection.GetEnumerator() )
			{
				while( enumerator.MoveNext() )
				{
					//Contract.Assert( index <= this.Count );
					Insert( index, enumerator.Current );
					
					index++;

					Contract.Assert( index <= this.Count );
				}
			}
		}

		/// <summary>
		/// Removes the first occurrence of an item with the specific key from the <b>collection</b>.
		/// </summary>
		/// <param name="key">The key of an item to remove from the <b>collection</b>.</param>
		/// <returns>
		/// <see langword="true"/> if item was successfully removed from the <b>collection</b>; otherwise, <see langword="false"/>.
		/// This method also returns <see langword="false"/> if an item was not found in the <b>collection</b>.
		/// </returns>
		public bool Remove(XKey key)
		{
			int index = IndexOf(key);

			if ( index < 0 )
			{
				return false;
			}

			RemoveAt(index);

			return true;
		}

		public T[] ToArray()
		{
			Contract.Ensures( Contract.Result<T[]>() != null );

			T[] array = new T[this.Items.Count];

			this.Items.CopyTo(array, 0);

			return array;
		}
		#endregion

		[EditorBrowsable(EditorBrowsableState.Advanced)]
#if false // !SILVERLIGHT
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Demand, RestrictedMemberAccess = true)]
#endif
		protected virtual XCollection<T> CreateInstanceOfSameType()
		{
			Contract.Ensures( Contract.Result<XCollection<T>>() != null );

			return (XCollection<T>) Activator.CreateInstance(GetType());
		}
		#region Update Methods
		/// <summary>
		/// �������� �������� ����������� ��������� ����������� ���������.
		/// </summary>
		/// <remarks>
		/// � �������� �������� �� ���� �� ������� �� ������������.
		/// �� �������� ������� ����� <see cref="EndUpdate()"/> ������� ���, ������� ��� ��������� ����� <b>BeginUpdate()</b>.
		/// </remarks>
		public void BeginUpdate()
		{
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			++this.updateCount;
		}

		/// <summary>
		/// ��������� �������� ����������� ��������� ����������� ���������.
		/// </summary>
		/// <remarks>
		/// ��� ������ ����� ������ ��������� ����� <b>EndUpdate()</b>, ��������������� ������� ���������� ������ <see cref="BeginUpdate()"/>,
		/// ����� ���������� ������� <see cref="CollectionChanged"/> � ����� �������� <see cref="NotifyCollectionChangedAction.Reset"/>.
		/// </remarks>
		public void EndUpdate()
		{
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			if ( this.updateCount == 0 )
				return;

			if ( --this.updateCount == 0 )
			{
				OnUpdated(EventArgs.Empty);
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
		{
			return IsUpdateRequired(source, CollectionUpdateOptions.All);
		}

		/// <summary>
		/// Determines whether the update operation is needed.
		/// </summary>
		/// <param name="source">Source <b>collection</b> to compare with.</param>
		/// <param name="options">Update options.</param>
		/// <returns><see langword="false"/> if two collections are the same (equal by reference), otherwise, <see langword="true"/>.</returns>
		public virtual bool IsUpdateRequired(IEnumerable<T> source, CollectionUpdateOptions options)
		{
			return !ReferenceEquals(this, source);
		}

		void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source)
		{
			Update(source, CollectionUpdateOptions.All);
		}

		/// <summary>
		/// ��������� ������� ��������� �� ������ ������ ���������.
		/// </summary>
		/// <param name="source">�������� ���������.</param>
		/// <returns>The <see cref="CollectionUpdateResults">results</see> of the operation.</returns>
		/// <remarks>
		/// ����� ������� ������ ������������ ������ ������ <see cref="Update(IEnumerable&lt;T&gt;,CollectionUpdateOptions)"/> �� ������ ����������, ������ <see cref="CollectionUpdateOptions.All"/>.
		/// </remarks>
		public CollectionUpdateResults Update(IEnumerable<T> source)
		{
			Contract.Requires( source != null );

			return Update(source, CollectionUpdateOptions.All);
		}

		/// <summary>
		/// ��������� ������� ��������� �� ������ ������ ���������.
		/// </summary>
		/// <param name="source">�������� ���������.</param>
		/// <param name="options">��������, ������� ���� ���������� � ���������, ������������ � ������ ���������.</param>
		/// <returns>The <see cref="CollectionUpdateResults">results</see> of the operation.</returns>
		[ContractVerification( false )]
		public virtual CollectionUpdateResults Update(IEnumerable<T> source, CollectionUpdateOptions options)
		{
			Contract.Requires( source != null );
			Contract.Ensures( Contract.Result<CollectionUpdateResults>() != null );

			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			if ( options == CollectionUpdateOptions.None || !IsUpdateRequired(source, options) )
			{
				return CollectionUpdateResults.Empty;
			}

			int added = 0;
			int updated = 0;
			int removed = 0;

			int index = 0;

			while ( index < this.Items.Count )
			{
				if ( this.Items[index] == null )
				{
					this.Items.RemoveAt(index);
					removed++;

					continue;
				}

				index++;
			}

			IDictionary<XKey, List<XUpdateItem>> updateCandidates = new Dictionary<XKey, List<XUpdateItem>>();

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				T item = this.Items[i];

				Contract.Assume( item != null ); // Since we'd removed all nulls in the previous step.

				XKey key = item.GetKey();
				List<XUpdateItem> items;

				if( updateCandidates.ContainsKey( key ) )
					items = updateCandidates[key];
				else
				{
					items = new List<XUpdateItem>();
					updateCandidates.Add(key, items);
				}

				items.Add(new XUpdateItem() { Item = item, Index = i });
			}

			index = 0;

			foreach ( T item in source )
			{
				XKey key;

				if ( item != null && updateCandidates.ContainsKey(key = item.GetKey()) )
				{
					var existing = updateCandidates[key];

					Contract.Assume( existing != null );

					if ( (options & CollectionUpdateOptions.UpdateExisting) != CollectionUpdateOptions.None )
					{
						var existing0 = existing[0];

						existing0.Item.Update(item);
						updated++;

						Debug.Assert(index <= existing0.Index);

						// Move
						for ( int i = existing0.Index; i > index; i-- )
						{
							this.Items[i] = this.Items[i - 1];
						}

						this.Items[index] = existing0.Item;

						// Recalculate original indexes upon moving
						foreach ( var items in updateCandidates.Values )
						{
							items.ForEach(x => {
								if ( index <= x.Index && x.Index < existing0.Index )
								{
									x.Index++;
								}
							});
						}
					}

					existing.RemoveAt(0);

					if ( existing.Count == 0 )
					{
						updateCandidates.Remove(key);
					}
				}
				else if ( (options & CollectionUpdateOptions.AddNew) != CollectionUpdateOptions.None )
				{
					this.Items.Insert(index, item);
					added++;

					// Recalculate original indexes upon insertion
					foreach ( var items in updateCandidates.Values )
					{
						items.ForEach(x => {
							if ( x.Index >= index )
							{
								x.Index++;
							}
						});
					}
				}

				index++;
			}

			if ( (options & CollectionUpdateOptions.RemoveOld) != CollectionUpdateOptions.None )
			{
				foreach (var items in updateCandidates.Values )
				{
					// ��� ����� ���������� �� ������� � ������� ��������, ����� �� �������� �� �������
					// ���������, ��� ��� ����� ������ ����������.

					foreach( var item in items.OrderByDescending( i => i.Index ) )
					{
						// � ��� ���� safeguard, ��� �������
						if( item.Index < this.Items.Count )
						{
							this.Items.RemoveAt( item.Index );
							++removed;
						}
					}

					/*
					items.ForEach(x => {
						this.Items.RemoveAt(x.Index);
						removed++;
					});
					*/
				}
			}

			if ( added != 0 || updated != 0 || removed != 0 )
			{
				OnUpdated(EventArgs.Empty);

				if ( added != 0 || removed != 0 )
				{
					OnPropertyChanged("Count");
				}

				OnPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			return new CollectionUpdateResults(added, updated, removed);
		}
		#endregion

		#region IEquatable`1 Members
		public virtual bool Equals(IXCollection<T> other)
		{
			if ( other == null || this.Items.Count != other.Count )
			{
				return false;
			}

			foreach ( T item in this.Items )
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
			if ( other == null || this.Items.Count != other.Count )
			{
				return false;
			}

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				T item = this.Items[i];

				if ( !Equals(item, other[i]) )
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region Collection`1 Overrides
		protected override void ClearItems()
		{
			OnClear();
			base.ClearItems();
			OnClearComplete();

			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void InsertItem(int index, T item)
		{
			ValidateItem(item);
			OnInsert(index, item);
			base.InsertItem(index, item);

			try
			{
				OnInsertComplete(index, item);
			}
			finally
			{
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			}
		}

		protected override void RemoveItem(int index)
		{
			T item = this.Items[index];

			OnRemove(index, item);
			base.RemoveItem(index);

			try
			{
				OnRemoveComplete(index, item);
			}
			finally
			{
				OnPropertyChanged("Count");
				OnPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
			}
		}

		protected override void SetItem(int index, T item)
		{
			T oldItem = this.Items[index];

			if ( ReferenceEquals(item, oldItem) )
			{
				return;
			}

			ValidateItem(item);
			OnSet(index, oldItem, item);
			base.SetItem(index, item);

			try
			{
				OnSetComplete(index, oldItem, item);
			}
			finally
			{
				OnPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
			}
		}
		#endregion
		#region Protected Collection Callbacks
		/// <exclude/>
		protected virtual void OnClear()
		{
		}

		/// <exclude/>
		protected virtual void OnClearComplete()
		{
		}

		/// <exclude/>
		protected virtual void OnInsert(int index, T item)
		{
		}

		/// <exclude/>
		protected virtual void OnInsertComplete(int index, T item)
		{
		}

		/// <exclude/>
		protected virtual void OnRemove(int index, T item)
		{
		}

		/// <exclude/>
		protected virtual void OnRemoveComplete(int index, T item)
		{
		}

		/// <exclude/>
		protected virtual void OnSet(int index, T oldItem, T newItem)
		{
		}

		/// <exclude/>
		protected virtual void OnSetComplete(int index, T oldItem, T newItem)
		{
		}

		/// <exclude/>
		protected virtual void ValidateItem(T item)
		{
		}
		#endregion

		#region Object Overrides
		public override bool Equals(object obj)
		{
			if ( obj == null || GetType() != obj.GetType() )
				return false;

			XCollection<T> src = (XCollection<T>) obj;

			if ( this.Items.Count != src.Items.Count )
				return false;

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				if ( !Equals(this.Items[i], src.Items[i]) )
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = this.Items.Count;

			foreach ( T item in this.Items )
				if( item != null )
					hashCode ^= item.GetHashCode();

			return hashCode;
		}
		#endregion

		#region class XUpdateItem
		private class XUpdateItem
		{
			public T Item;
			public int Index;
		}
		#endregion
	}
}
