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
		private ushort _updateCount;

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
			get { return _updateCount > 0; }
		}
		#endregion

		#region Events and Event Raisers
		/// <inheritdoc/>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if ( _updateCount == 0 )
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
		/// Raises the <see cref="ObservableCollection&lt;T&gt;.PropertyChanged"/> event with the specified name of a property.
		/// </summary>
		/// <param name="propertyName">The name of a property being changed.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			if ( _updateCount == 0 )
			{
				OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
			}
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if ( _updateCount == 0 )
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
		/// Начинает операцию глобального изменения содержимого коллекции.
		/// </summary>
		/// <remarks>
		/// В процессе операции ни одно из событий не возбуждается.
		/// Не забудьте вызвать метод <see cref="EndUpdate()"/> столько раз, сколько раз вызывался метод <b>BeginUpdate()</b>.
		/// </remarks>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public void BeginUpdate()
		{
			++_updateCount;
		}

		/// <summary>
		/// Завершает операцию глобального изменения содержимого коллекции.
		/// </summary>
		/// <remarks>
		/// Как только будет вызван последний метод <b>EndUpdate()</b>, соответствующий первому вызванному методу <see cref="BeginUpdate()"/>,
		/// будет возбуждено событие <see cref="ObservableCollection&lt;T&gt;.CollectionChanged"/> с типом операции <see cref="NotifyCollectionChangedAction.Reset"/>.
		/// </remarks>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public void EndUpdate()
		{
			if ( _updateCount == 0 )
				return;

			if ( --_updateCount == 0 )
			{
				OnPropertyChanged(null);
				OnCollectionReset();
			}
		}

		[ContractVerification(false)]
		bool IXUpdatable<IXCollection<T>>.IsUpdateRequired(IXCollection<T> source)
		{
			return IsUpdateRequired(source, CollectionMergeOptions.Full);
		}

		/// <summary>
		/// Determines whether the update operation is needed.
		/// </summary>
		/// <param name="collection">Source <b>collection</b> to compare with.</param>
		/// <param name="options">The desired update options.</param>
		/// <returns>
		/// <see langword="false"/> if two collections are the same (equal by reference) or <paramref name="options"/> are <see cref="CollectionMergeOptions.None"/>, otherwise, <see langword="true"/>.
		/// </returns>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		public virtual bool IsUpdateRequired(IEnumerable<T> collection, CollectionMergeOptions options)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			Contract.EndContractBlock();

			return !Object.ReferenceEquals(this, collection) && options != CollectionMergeOptions.None;
		}

		[ContractVerification(false)]
		void IXUpdatable<IXCollection<T>>.Update(IXCollection<T> source)
		{
			Update(source, CollectionMergeOptions.Full);
		}

		/// <summary>
		/// Обновляет текущую коллекцию на основе другой коллекции.
		/// </summary>
		/// <param name="collection">Источник изменений.</param>
		/// <returns>The <see cref="CollectionMergeResults">results</see> of the operation.</returns>
		/// <remarks>
		/// Вызов данного метода эквивалентен вызову метода <see cref="Update(IEnumerable&lt;T&gt;,CollectionMergeOptions)"/> со вторым параметром, равным <see cref="CollectionMergeOptions.Full"/>.
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
		/// Обновляет текущую коллекцию на основе другой коллекции.
		/// </summary>
		/// <param name="collection">Источник изменений.</param>
		/// <param name="options">Операции, которые надо произвести с объектами, находящимися в данной коллекции.</param>
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

			collection = collection.Distinct(ReferenceComparer);

			var results = this.Items.Merge(collection, options);

			if ( !results.IsEmpty )
			{
				OnPropertyChanged(null);
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

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.Items != null);
		}
		#endregion

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
