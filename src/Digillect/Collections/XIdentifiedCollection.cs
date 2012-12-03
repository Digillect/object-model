using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Digillect.Collections
{
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XIdentifiedCollection<TId, TObject> : XUniqueCollection<TObject>
#if !(SILVERLIGHT || WINDOWS8)
		, IDeserializationCallback
#endif
		where TObject : XObject, IXIdentified<TId>
	{
#if !(SILVERLIGHT || WINDOWS8)
		[NonSerialized]
#endif
		private IDictionary<TId, TObject> m_dictionary = new Dictionary<TId, TObject>();

		#region Constructor
		/// <summary>
		/// Initializes new instance of the <see cref="XIdentifiedCollection&lt;TId,TObject&gt;"/> class.
		/// </summary>
		public XIdentifiedCollection()
		{
		}

		/// <summary>
		/// Initializes new instance of the <see cref="XIdentifiedCollection&lt;TId,TObject&gt;"/> class using elements of the provided enumeration as the source for this list.
		/// </summary>
		/// <param name="collection">The enumeration which elements are used to construct a new list to use as the parameter for the <see cref="Collection&lt;T&gt;(IList&lt;T&gt;)"/> constructor.</param>
		public XIdentifiedCollection(IEnumerable<TObject> collection)
			: base(collection)
		{
			Contract.Requires( collection != null );
			Contract.Requires(Contract.ForAll(collection, XCollectionsUtil.CollectionMemberNotNull));

			OnDeserialization();
		}
		#endregion

		#region Public Indexers
		public TObject this[TId id]
		{
			get
			{
				TObject value;

				m_dictionary.TryGetValue(id, out value);

				return value;
			}
		}
		#endregion

		#region Protected Properties
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected IDictionary<TId, TObject> Dictionary
		{
			get { return m_dictionary; }
		}
		#endregion

		#region Public Methods
		public bool Contains(TId id)
		{
			Contract.Ensures(!Contract.Result<bool>() || this.Count > 0);

			return m_dictionary.ContainsKey(id);
		}

		public int IndexOf(TId id)
		{
			Contract.Ensures(Contract.Result<int>() >= -1);

			if ( m_dictionary.ContainsKey(id) )
			{
				return IndexOf(m_dictionary[id]);
			}

			return -1;
		}

		public bool Remove(TId id)
		{
			// TODO: Should we be so paranoic?
			/*
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}
			*/

			if ( m_dictionary.ContainsKey(id) )
			{
				return Remove(m_dictionary[id]);
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public IList<TId> GetIdentifiers()
		{
			TId[] identifiers = new TId[this.Items.Count];

			for ( int i = 0; i < this.Items.Count; i++ )
			{
				identifiers[i] = this.Items[i].Id;
			}

			return new ReadOnlyCollection<TId>(identifiers);
		}
		#endregion

		#region Update Methods
		/// <summary>
		/// Обновляет текущую коллекцию на основе другой коллекции.
		/// </summary>
		/// <param name="collection">Источник изменений.</param>
		/// <param name="options">Операции, которые надо произвести с объектами, находящимися в данной коллекции.</param>
		/// <returns>The <see cref="CollectionMergeResults">results</see> of the operation.</returns>
		public override CollectionMergeResults Update(IEnumerable<TObject> collection, CollectionMergeOptions options)
		{
			XCollectionsUtil.ValidateCollection(collection);

			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Errors.XCollectionReadOnlyException);
			}

			if ( !IsUpdateRequired(collection, options) )
			{
				return CollectionMergeResults.Empty;
			}

			var results = this.Items.Merge(collection.Distinct(ReferenceEqualityComparer.Default), options);

			if ( !results.IsEmpty )
			{
				OnDeserialization();
				OnUpdated(EventArgs.Empty);

				if ( results.Added != 0 || results.Removed != 0 )
				{
					OnPropertyChanged("Count");
				}

				OnPropertyChanged("Item[]");
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			return results;
		}
		#endregion

		#region IDeserializationCallback Members
#if !(SILVERLIGHT || WINDOWS8)
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			OnDeserialization();
		}
#endif

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void OnDeserialization()
		{
			if ( m_dictionary == null )
			{
				m_dictionary = new Dictionary<TId, TObject>();
			}
			else
			{
				m_dictionary.Clear();
			}

			foreach ( TObject item in this.Items )
			{
				m_dictionary.Add(item.Id, item);
			}
		}
		#endregion

		#region XCollection`1 Overrides
		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			m_dictionary.Clear();
		}

		protected override void OnInsertComplete(int index, TObject item)
		{
			base.OnInsertComplete(index, item);
			m_dictionary.Add(item.Id, item);
		}

		protected override void OnRemoveComplete(int index, TObject item)
		{
			base.OnRemoveComplete(index, item);
			m_dictionary.Remove(item.Id);
		}

		protected override void OnSetComplete(int index, TObject oldItem, TObject newItem)
		{
			base.OnSetComplete(index, oldItem, newItem);
			m_dictionary.Remove(oldItem.Id);
			m_dictionary.Add(newItem.Id, newItem);
		}
		#endregion

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(m_dictionary.Count == this.Count);
		}
		#endregion
	}
}
