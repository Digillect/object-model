using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Collections
{
	/// <summary>
	/// A collection of "X"-objects with support for cloning, updating and event notification.
	/// </summary>
	/// <typeparam name="T">Type of the collection's members.</typeparam>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XCollection<T> : ObservableCollection<T>, IXList<T>
#if !(SILVERLIGHT || WINDOWS8)
		, ICloneable
#endif
		where T : XObject
	{
		/// <summary>
		/// The name of the <see cref="ICollection&lt;T&gt;.Count"/> property for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// Contains the "Count" string.
		/// </summary>
		protected const string CountString = "Count";

		/// <summary>
		/// The name of the <see cref="IList&lt;T&gt;.this"/> property for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// Contains the "Item[]" string.
		/// </summary>
		protected const string IndexerName = "Item[]";

		/// <summary>
		/// The comparer used to compare objects for equality by their references.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		protected static readonly IEqualityComparer<T> ReferenceComparer = new ReferenceEqualityComparer();

#if !(SILVERLIGHT || WINDOWS8)
		[NonSerialized]
#endif
		private ushort updateCount;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XCollection&lt;T&gt;"/> class.
		/// </summary>
		public XCollection()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XCollection&lt;T&gt;"/> class that contains elements copied from the specified collection.
		/// </summary>
		/// <param name="collection">The collection from which the elements are copied.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> parameter cannot be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> parameter cannot contain <c>null</c> members.</exception>
		public XCollection(IEnumerable<T> collection)
			: base(collection)
		{
			XCollectionsUtil.ValidateCollection(collection);
		}

#if !WINDOWS8
		/// <summary>
		/// Initializes a new instance of the <see cref="XCollection&lt;T&gt;"/> class that contains elements copied from the specified list.
		/// </summary>
		/// <param name="list">The list from which the elements are copied.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="list"/> parameter cannot be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="list"/> parameter cannot contain <c>null</c> members.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Base type uses it")]
		public XCollection(List<T> list)
			: base(list)
		{
			XCollectionsUtil.ValidateCollection(list);
		}
#endif
		#endregion

		#region Protected Properties
		/// <summary>
		/// Gets a value indicating whether the collection is in update state
		/// (i.e. the <see cref="BeginUpdate"/> method is called more times than the <see cref="EndUpdate"/> one).
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is in update; otherwise, <c>false</c>.
		/// </value>
		protected bool IsInUpdate
		{
			get { return this.updateCount > 0; }
		}
		#endregion

		#region Events and Event Raisers
		/// <summary>
		/// Occurs when the collection is updated using the <see cref="Update(IEnumerable&lt;T&gt;,CollectionMergeOptions)"/> method
		/// or as a result of the <see cref="EndUpdate"/> method.
		/// </summary>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public event EventHandler Updated;

		/// <summary>
		/// Raises the <see cref="Updated"/> event.
		/// </summary>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		protected virtual void OnUpdated(EventArgs e)
		{
			if ( this.updateCount == 0 )
			{
				if ( Updated != null )
				{
#if !SILVERLIGHT
					using ( BlockReentrancy() )
#endif
					{
						Updated(this, e);
					}
				}
			}
		}

		/// <inheritdoc/>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if ( this.updateCount == 0 )
			{
				base.OnCollectionChanged(e);
			}
		}

		/// <summary>
		/// Raises the <see cref="ObservableCollection&lt;T&gt;.CollectionChanged"/> event with the <see cref="NotifyCollectionChangedAction.Reset"/> action.
		/// </summary>
		protected void OnCollectionReset()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		/// <summary>
		/// Raises the <see cref="ObservableCollection&lt;T&gt;.PropertyChanged"/> event with the provided arguments.
		/// </summary>
		/// <param name="propertyName">The name of a property being changed.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			if ( this.updateCount == 0 )
			{
				OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
			}
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if ( this.updateCount == 0 )
			{
				base.OnPropertyChanged(e);
			}
		}
		#endregion

		#region Public Methods
		public void AddRange(IEnumerable<T> collection)
		{
			Contract.Requires( collection != null );
			Contract.Requires(Contract.ForAll(collection, item => item != null));

			InsertRange(this.Count, collection);
		}

		/// <summary>
		/// Determines whether the <b>collection</b> contains an item with the specific key.
		/// </summary>
		/// <param name="key">The key of an item to locate in the <see cref="IXCollection&lt;T&gt;"/>.</param>
		/// <returns><see langword="true"/> if item is found in the <see cref="IXCollection&lt;T&gt;"/>; otherwise, <see langword="false"/>.</returns>
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

			derived.AddRange(this.Items.Where(predicate));

			return derived;
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

		/// <inheritdoc/>
		public IEnumerable<XKey> GetKeys()
		{
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
				throw new ArgumentOutOfRangeException("index", Errors.ArgumentOutOfRange_Index);
			}

			XCollectionsUtil.ValidateCollection(collection);

			Contract.EndContractBlock();

			// WPF is known to not support events resulted from ranged operations so do insertion one by one

			foreach ( T item in collection )
			{
				Insert(index++, item);
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

#if !(SILVERLIGHT || WINDOWS8)
		#region ICloneable Members
		object ICloneable.Clone()
		{
			return Clone( true );
		}
		#endregion
#endif

		#region Clone Methods
		IXCollection<T> IXUpdatable<IXCollection<T>>.Clone(bool deep)
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

		/// <summary>
		/// A helper for the <see cref="Clone" /> method.
		/// </summary>
		/// <param name="clone">A copy of this instance to process.</param>
		/// <param name="deep"><see langword="true"/> to deep-clone inner collections (including their members), <see langword="false"/> to clone only inner collections but not their members.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="clone"/> parameter cannot be null.</exception>
		protected virtual void ProcessClone(XCollection<T> clone, bool deep)
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

		/// <summary>
		/// A helper for the <see cref="Clone"/> method.
		/// </summary>
		/// <returns>A new object which has exactly the same type as this instance.</returns>
		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[Pure]
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
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public void BeginUpdate()
		{
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			++this.updateCount;
		}

		/// <summary>
		/// ��������� �������� ����������� ��������� ����������� ���������.
		/// </summary>
		/// <remarks>
		/// ��� ������ ����� ������ ��������� ����� <b>EndUpdate()</b>, ��������������� ������� ���������� ������ <see cref="BeginUpdate()"/>,
		/// ����� ���������� ������� <see cref="ObservableCollection&lt;T&gt;.CollectionChanged"/> � ����� �������� <see cref="NotifyCollectionChangedAction.Reset"/>.
		/// </remarks>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public void EndUpdate()
		{
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			if ( this.updateCount == 0 )
				return;

			if ( --this.updateCount == 0 )
			{
				OnUpdated(EventArgs.Empty);
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionReset();
			}
		}

		bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
		{
			return IsUpdateRequired(source, CollectionMergeOptions.Full);
		}

		/// <summary>
		/// Determines whether the update operation is needed.
		/// </summary>
		/// <param name="source">Source <b>collection</b> to compare with.</param>
		/// <param name="options">Update options.</param>
		/// <returns>
		/// <see langword="false"/> if two collections are the same (equal by reference) or <paramref name="options"/> are <see cref="CollectionMergeOptions.None"/>, otherwise, <see langword="true"/>.
		/// </returns>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public virtual bool IsUpdateRequired(IEnumerable<T> source, CollectionMergeOptions options)
		{
			return !Object.ReferenceEquals(this, source) && options != CollectionMergeOptions.None;
		}

#if !NET45
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "Requires", Justification = "Can't restrict interface requirements")]
#endif
		void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source)
		{
			Update(source, CollectionMergeOptions.Full);
		}

		/// <summary>
		/// ��������� ������� ��������� �� ������ ������ ���������.
		/// </summary>
		/// <param name="collection">�������� ���������.</param>
		/// <returns>The <see cref="CollectionMergeResults">results</see> of the operation.</returns>
		/// <remarks>
		/// ����� ������� ������ ������������ ������ ������ <see cref="Update(IEnumerable&lt;T&gt;,CollectionMergeOptions)"/> �� ������ ����������, ������ <see cref="CollectionMergeOptions.Full"/>.
		/// </remarks>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public CollectionMergeResults Update(IEnumerable<T> collection)
		{
			Contract.Requires(collection != null);
			Contract.Requires(Contract.ForAll(collection, item => item != null));
			Contract.Ensures(Contract.Result<CollectionMergeResults>() != null);

			return Update(collection, CollectionMergeOptions.Full);
		}

		/// <summary>
		/// ��������� ������� ��������� �� ������ ������ ���������.
		/// </summary>
		/// <param name="collection">�������� ���������.</param>
		/// <param name="options">��������, ������� ���� ���������� � ���������, ������������ � ������ ���������.</param>
		/// <returns>The <see cref="CollectionMergeResults">results</see> of the operation.</returns>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		/// <seealso cref="XCollectionsUtil.Merge"/>
		public virtual CollectionMergeResults Update(IEnumerable<T> collection, CollectionMergeOptions options)
		{
			XCollectionsUtil.ValidateCollection(collection);

			Contract.Ensures(Contract.Result<CollectionMergeResults>() != null);

#if !SILVERLIGHT
			CheckReentrancy();
#endif

			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			if ( !IsUpdateRequired(collection, options) )
			{
				return CollectionMergeResults.Empty;
			}

			var results = this.Items.Merge(collection.Distinct(ReferenceComparer), options);

			if ( !results.IsEmpty )
			{
				OnUpdated(EventArgs.Empty);

				if ( results.Added != results.Removed )
				{
					OnPropertyChanged(CountString);
				}

				OnPropertyChanged(IndexerName);
				OnCollectionReset();
			}

			return results;
		}
		#endregion

		#region IEquatable`1 Members
		/// <inheritdoc/>
		public virtual bool Equals(IXCollection<T> other)
		{
			if ( other == null || this.Items.Count != other.Count )
			{
				return false;
			}

			foreach ( T item in this.Items )
			{
				XKey key = item.GetKey();

				if ( !item.Equals(other.FirstOrDefault(x => x.GetKey() == key)) )
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc/>
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
		/// <inheritdoc/>
		protected override void InsertItem(int index, T item)
		{
			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			if ( this.Items.Contains(item, ReferenceComparer) )
			{
				throw new ArgumentException(Errors.XCollectionItemDuplicateException, "item");
			}

			base.InsertItem(index, item);
		}

		/// <inheritdoc/>
		protected override void SetItem(int index, T item)
		{
			T oldItem = this.Items[index];

			if ( Object.ReferenceEquals(item, oldItem) )
			{
				return;
			}

			if ( item == null )
			{
				throw new ArgumentNullException("item");
			}

			if ( this.Items.Contains(item, ReferenceComparer) )
			{
				throw new ArgumentException(Errors.XCollectionItemDuplicateException, "item");
			}

			base.SetItem(index, item);
		}
		#endregion

		#region Object Overrides
		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

#if DEBUG || CONTRACTS_FULL
		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.Items != null);
		}
		#endregion
#endif

		#region class ReferenceEqualityComparer
		private sealed class ReferenceEqualityComparer : IEqualityComparer<T>
		{
			public ReferenceEqualityComparer()
			{
			}

			bool IEqualityComparer<T>.Equals(T x, T y)
			{
				return Object.ReferenceEquals(x, y);
			}

			int IEqualityComparer<T>.GetHashCode(T obj)
			{
				return Object.ReferenceEquals(obj, null) ? 0 : obj.GetHashCode();
			}
		}
		#endregion
	}
}