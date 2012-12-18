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
	/// <summary>
	/// A collection of unique non-null objects which can be additionally accessed by their identifiers (dictionary-like).
	/// </summary>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	/// <typeparam name="TObject">The type of the collection's members.</typeparam>
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
		/// Initializes a new instance of the <see cref="XIdentifiedCollection&lt;TId,TObject&gt;"/> class that contains elements copied from the specified collection.
		/// </summary>
		/// <param name="collection">The collection from which the elements are copied.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> parameter cannot be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> parameter cannot contain <c>null</c> members.</exception>
		public XIdentifiedCollection(IEnumerable<TObject> collection)
			: base(collection)
		{
			Contract.Requires( collection != null );
			Contract.Requires(Contract.ForAll(collection, item => item != null));

			OnDeserialization();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XIdentifiedCollection&lt;TId,TObject&gt;"/> class that contains elements copied from the specified list.
		/// </summary>
		/// <param name="list">The list from which the elements are copied.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="list"/> parameter cannot be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="list"/> parameter cannot contain <c>null</c> members.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Base type uses it")]
		public XIdentifiedCollection(List<TObject> list)
			: base(list)
		{
			Contract.Requires(list != null);
			Contract.Requires(Contract.ForAll(list, item => item != null));

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
		/// ��������� ������� ��������� �� ������ ������ ���������.
		/// </summary>
		/// <param name="collection">�������� ���������.</param>
		/// <param name="options">��������, ������� ���� ���������� � ���������, ������������ � ������ ���������.</param>
		/// <returns>The <see cref="CollectionMergeResults">results</see> of the operation.</returns>
		/// <seealso cref="IXUpdatable&lt;T&gt;"/>
		/// <seealso cref="XCollectionsUtil.Merge"/>
		public override CollectionMergeResults Update(IEnumerable<TObject> collection, CollectionMergeOptions options)
		{
			XCollectionsUtil.ValidateCollection(collection);

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
				OnDeserialization();
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
		/// <inheritdoc/>
		protected override void ClearItems()
		{
#if !SILVERLIGHT
			CheckReentrancy();
#endif

			m_dictionary.Clear();

			base.ClearItems();
		}

		/// <inheritdoc/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Should not be null in any case since the method is an event raiser")]
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
#if !SILVERLIGHT
			using ( BlockReentrancy() )
#endif
			{
				if ( e.Action == NotifyCollectionChangedAction.Add )
				{
					foreach ( TObject item in e.NewItems )
					{
						m_dictionary.Add(item.Id, item);
					}
				}
				else if ( e.Action == NotifyCollectionChangedAction.Remove )
				{
					foreach ( TObject item in e.OldItems )
					{
						m_dictionary.Remove(item.Id);
					}
				}
				else if ( e.Action == NotifyCollectionChangedAction.Replace )
				{
					foreach ( TObject oldItem in e.OldItems )
					{
						m_dictionary.Remove(oldItem.Id);
					}

					foreach ( TObject newItem in e.NewItems )
					{
						m_dictionary.Add(newItem.Id, newItem);
					}
				}

				base.OnCollectionChanged(e);
			}
		}
		#endregion

#if DEBUG || CONTRACTS_FULL
		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(m_dictionary.Count == this.Count);
		}
		#endregion
#endif
	}
}
