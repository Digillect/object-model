using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

using Digillect.Properties;

namespace Digillect.Collections
{
	/// <summary>
	/// A collection of "X"-objects with support for cloning, updating and event notification.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if !(SILVERLIGHT || NETFX_CORE)
	[Serializable]
#endif
	public class XCollection<T> : Collection<T>, IXList<T>, INotifyPropertyChanged
#if !(SILVERLIGHT || NETFX_CORE)
		, ICloneable
#endif
		where T : XObject
	{
#if !(SILVERLIGHT || NETFX_CORE)
		[NonSerialized]
#endif
		private ushort updateCount;

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed in IndexOf method")]
		public bool ContainsKey(XKey key)
		{
			return IndexOf(key) != -1;
		}

		[Pure]
		public XCollection<T> Derive(Func<T, bool> predicate)
		{
			if ( predicate == null )
			{
				throw new ArgumentNullException("predicate");
			}

			Contract.Ensures(Contract.Result<XCollection<T>>() != null);

			var derived = CreateInstanceOfSameType();

			Contract.Assume(this.Items != null);

			derived.AddRange(this.Items.Where(predicate));

			return derived;
		}

		[Pure]
		public XCollection<T> Derive(Predicate<T> predicate)
		{
			Contract.Requires(predicate != null);
			Contract.Ensures(Contract.Result<XCollection<T>>() != null);

			return Derive(predicate.ToFunction());
		}

		/// <summary>
		/// Gets an item with the specific key.
		/// </summary>
		/// <param name="key">The key of the item to find.</param>
		/// <returns>An item with the specified key if the item exists in the <b>collection</b>; otherwise, <see langword="null"/>.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed in IndexOf method")]
		public T Find(XKey key)
		{
			int index = IndexOf(key);

			Contract.Assume(index < this.Items.Count);

			return index == -1 ? null : this.Items[index];
		}

		public void ForEach(Action<T> action)
		{
			if ( action == null )
			{
				throw new ArgumentNullException("action");
			}

			Contract.EndContractBlock();

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				action(this.Items[i]);
			}
		}

		public IEnumerable<XKey> GetKeys()
		{
			Contract.Assume(this.Items != null);

			return this.Items.Select(x => x.GetKey());
		}

		/// <summary>
		/// Determines the index of an item with the specific key within the collection.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <b>collection</b>.</param>
		/// <returns>The index of item if found in the list; otherwise, -1.</returns>
		public int IndexOf(XKey key)
		{
			if ( key == null )
			{
				throw new ArgumentNullException("key");
			}

			Contract.EndContractBlock();

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				if ( key.Equals(this.Items[i].GetKey()) )
				{
					Contract.Assume(i < this.Count);

					return i;
				}
			}

			return -1;
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			if ( index < 0 || index > this.Count )
			{
				throw new ArgumentOutOfRangeException("index");
			}

			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			Contract.EndContractBlock();

			List<T> is2 = this.Items as List<T>;

			if ( is2 != null )
			{
				var items = collection.Where(x => x != null).ToArray();
				Contract.Assume(index <= is2.Count);
				is2.InsertRange(index, items);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
			}
			else
			{
				foreach ( var item in collection )
				{
					if ( item != null )
					{
						Contract.Assume(0 <= index && index <= this.Count);
						Insert(index++, item);
					}
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed in IndexOf method")]
		public bool Remove(XKey key)
		{
			int index = IndexOf(key);

			if ( index == -1 )
			{
				return false;
			}

			RemoveAt(index);

			return true;
		}

		[Pure]
		public T[] ToArray()
		{
			Contract.Ensures( Contract.Result<T[]>() != null );

			T[] array = new T[this.Items.Count];

			this.Items.CopyTo(array, 0);

			return array;
		}
		#endregion

#if !(SILVERLIGHT || NETFX_CORE)
		#region ICloneable Members
		object ICloneable.Clone()
		{
			return Clone( true );
		}
		#endregion
#endif

		#region Clone Methods
		IXCollection<T> IXCollection<T>.Clone( bool deep )
		{
			return Clone( deep );
		}

		/// <summary>
		/// Creates a copy of this collection.
		/// </summary>
		/// <param name="deep"><see langword="true"/> to deep-clone inner collections (including their members), <see langword="false"/> to clone only inner collections but not their members.</param>
		/// <returns>Cloned copy of the collection.</returns>
		public virtual XCollection<T> Clone( bool deep )
		{
			Contract.Ensures(Contract.Result<XCollection<T>>() != null);

			var clone = CreateInstanceOfSameType();

			ProcessClone( clone, deep );

			return clone;
		}

		protected virtual void ProcessClone( XCollection<T> clone, bool deep )
		{
			if ( clone == null )
			{
				throw new ArgumentNullException("clone");
			}

			Contract.EndContractBlock();

			foreach( var item in this.Items )
			{
				T itemClone = (T) item.Clone( deep );

				clone.Items.Add( itemClone );
			}
		}
		#endregion

		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[Pure]
#if false // !(SILVERLIGHT || NETFX_CORE)
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
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed by delegated method")]
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
			return !Object.ReferenceEquals(this, source);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Validation performed by delegated method")]
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
			Contract.Requires(source != null);
			Contract.Ensures(Contract.Result<CollectionUpdateResults>() != null);

			return Update(source, CollectionUpdateOptions.All);
		}

		/// <summary>
		/// ��������� ������� ��������� �� ������ ������ ���������.
		/// </summary>
		/// <param name="source">�������� ���������.</param>
		/// <param name="options">��������, ������� ���� ���������� � ���������, ������������ � ������ ���������.</param>
		/// <returns>The <see cref="CollectionUpdateResults">results</see> of the operation.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual CollectionUpdateResults Update(IEnumerable<T> source, CollectionUpdateOptions options)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.Ensures(Contract.Result<CollectionUpdateResults>() != null);

			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException( Resources.XCollectionReadOnlyException );
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

				Contract.Assume(item != null); // Since we've removed all nulls in the previous step.

				XKey key = item.GetKey();
				List<XUpdateItem> items;

				if ( updateCandidates.ContainsKey(key) )
				{
					items = updateCandidates[key];
				}
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
					Contract.Assume(existing.Count > 0);

					if ( (options & CollectionUpdateOptions.UpdateExisting) == CollectionUpdateOptions.UpdateExisting )
					{
						// Take first item to update
						var existing0 = existing[0];

						existing0.Item.Update(item);
						updated++;

						Contract.Assert(index <= existing0.Index);
						Contract.Assume(existing0.Index < this.Items.Count);

						// Move the updated item to the current (source) position
						// HACK: instead of remove-then-insert we will change indexes of all affected items

						// Move range [index..existing0.Index) one element up
						for ( int i = existing0.Index; i > index; i-- )
						{
							this.Items[i] = this.Items[i - 1];
						}

						// Set updated item at the corect (source) position
						this.Items[index] = existing0.Item;

						// Recalculate original indexes upon moving
						foreach ( var items in updateCandidates.Values )
						{
							foreach( var x in items )
								if( index <= x.Index && x.Index < existing0.Index )
									x.Index++;
							/* ForEach is not supported in NETFX_CORE (WinRT)
							items.ForEach(x => {
								if ( index <= x.Index && x.Index < existing0.Index )
								{
									x.Index++;
								}
							});
							*/
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
				else if ( (options & CollectionUpdateOptions.AddNew) == CollectionUpdateOptions.AddNew )
				{
					this.Items.Insert(index, item);
					added++;

					// Recalculate original indexes upon insertion
					foreach ( var items in updateCandidates.Values )
					{
						foreach( var x in items )
							if( x.Index >= index )
								x.Index++;

						/* ForEach is not supported in NETFX_CORE (WinRT)
						items.ForEach(x => {
							if ( x.Index >= index )
							{
								x.Index++;
							}
						});
						*/
					}
				}

				index++;
			}

			if ( (options & CollectionUpdateOptions.RemoveOld) == CollectionUpdateOptions.RemoveOld )
			{
				foreach ( var items in updateCandidates.Values )
				{
					// ��� ����� ���������� �� ������� � ������� ��������, ����� �� �������� �� ������� ���������
					// �.�. items � ���������� ��� ������ �� �����, ����� ����� ��� "���������"
					// ���������� �������������, ��� ��� �������� ���� � ���������� �������, �.�. �� ����������� �������� ������������ ���������
					items.Reverse();

					foreach( var x in items )
					{
						Contract.Assume( x.Index >= 0 );
						this.Items.RemoveAt( x.Index );
						removed++;
					}
					/* ForEach is not supported in NETFX_CORE (WinRT)
					items.ForEach(x => {
						// � ��� ���� safeguard, ��� �������
						//Contract.Assert(x.Index < this.Items.Count);
						Contract.Assume(x.Index >= 0);
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
				XKey key = item.GetKey();

				if ( key == null || !item.Equals(other.Find(key)) )
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

#if true
			return this.Items.SequenceEqual(other);
#else
			for ( int i = 0; i < this.Items.Count; i++ )
			{
				T item = this.Items[i];

				if ( !Object.Equals(item, other[i]) )
				{
					return false;
				}
			}

			return true;
#endif
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

			if ( Object.ReferenceEquals(item, oldItem) )
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
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}
		}
		#endregion

		#region Object Overrides
		public override bool Equals(object obj)
		{
			if ( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			XCollection<T> other = (XCollection<T>) obj;

			if ( this.Items.Count != other.Items.Count )
			{
				return false;
			}

#if true
			return this.Items.SequenceEqual(other.Items);
#else
			for ( int i = 0; i < this.Items.Count; i++ )
			{
				if ( !Object.Equals(this.Items[i], other.Items[i]) )
				{
					return false;
				}
			}

			return true;
#endif
		}

		public override int GetHashCode()
		{
			int hashCode = this.Items.Count;

			foreach ( T item in this.Items )
			{
				hashCode ^= item.GetHashCode();
			}

			return hashCode;
		}
		#endregion

		#region class XUpdateItem
		private sealed class XUpdateItem
		{
			public T Item;
			public int Index;
		}
		#endregion
	}
}
