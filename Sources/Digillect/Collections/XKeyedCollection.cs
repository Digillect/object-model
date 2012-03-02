using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

using Digillect.Properties;

namespace Digillect.Collections
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public class XKeyedCollection<TId, TObject> : XUniqueCollection<TObject>
#if !SILVERLIGHT
		, IDeserializationCallback
#endif
		where TId : IEquatable<TId>
		where TObject : XObject, IXIdentifiable<TId>
	{
#if !SILVERLIGHT
		[NonSerialized]
#endif
		private IDictionary<TId, TObject> m_dictionary = new Dictionary<TId, TObject>();

		#region Constructor
		/// <summary>
		/// Initializes new instance of the <see cref="XKeyedCollection&lt;TId,TObject&gt;"/> class.
		/// </summary>
		public XKeyedCollection()
		{
		}

		/// <summary>
		/// Initializes new instance of the <see cref="XKeyedCollection&lt;TId,TObject&gt;"/> class using elements of the provided enumeration as the source for this list.
		/// </summary>
		/// <param name="collection">The enumeration which elements are used to construct a new list to use as the parameter for the <see cref="Collection&lt;T&gt;(IList&lt;T&gt;)"/> constructor.</param>
		public XKeyedCollection(IEnumerable<TObject> collection)
			: base(collection)
		{
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
		protected IDictionary<TId, TObject> Dictionary
		{
			get { return m_dictionary; }
		}
		#endregion

		#region Public Methods
		public bool Contains(TId id)
		{
			return m_dictionary.ContainsKey(id);
		}

		public new bool Contains(TObject item)
		{
			if ( item == null )
				throw new ArgumentNullException("item");

			return Contains(item.Id);
		}

		public int IndexOf(TId id)
		{
			if ( m_dictionary.ContainsKey(id) )
				return IndexOf(m_dictionary[id]);

			return -1;
		}

		public bool Remove(TId id)
		{
			// TODO: Should we be so paranoic?
			/*
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Properties.Resources.XListReadOnlyException);
			}
			*/

			if ( m_dictionary.ContainsKey(id) )
				return Remove(m_dictionary[id]);

			return false;
		}

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

		#region Clone Methods
		protected override void ProcessClone(XCollection<TObject> clone, bool deep)
		{
			base.ProcessClone(clone, deep);

			foreach ( var item in Items )
			{
				m_dictionary.Add(item.Id, item);
			}
		}
		#endregion
		#region Update Methods
		/// <summary>
		/// Обновляет текущую коллекцию на основе другой коллекции.
		/// </summary>
		/// <param name="source">Источник изменений.</param>
		/// <param name="options">Операции, которые надо произвести с объектами, находящимися в данной коллекции.</param>
		/// <returns>The <see cref="CollectionUpdateResults">results</see> of the operation.</returns>
		public override CollectionUpdateResults Update(IEnumerable<TObject> source, CollectionUpdateOptions options)
		{
			if ( this.Items.IsReadOnly )
			{
				throw new NotSupportedException(Resources.XCollectionReadOnlyException);
			}

			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			if ( options == CollectionUpdateOptions.None || !UpdateRequired(source, options) )
			{
				return CollectionUpdateResults.Empty;
			}

			int added = 0;
			int updated = 0;
			int removed = 0;

			IDictionary<TId, TObject> toRemove = new Dictionary<TId, TObject>();

			foreach ( TObject item in this.Items )
			{
				toRemove.Add(item.Id, item);
			}

			foreach ( TObject item in source )
			{
				if ( toRemove.ContainsKey(item.Id) )
				{
					TObject existing = toRemove[item.Id];

					if ( (options & CollectionUpdateOptions.UpdateExisting) != CollectionUpdateOptions.None )
					{
						existing.Update(item);
						updated++;
					}

					toRemove.Remove(existing.Id);
				}
				else if ( (options & CollectionUpdateOptions.AddNew) != CollectionUpdateOptions.None )
				{
					Add(item);
					added++;
				}
			}

			if ( (options & CollectionUpdateOptions.RemoveOld) != CollectionUpdateOptions.None )
			{
				foreach ( TId id in toRemove.Keys )
				{
					if ( Remove(id) )
					{
						removed++;
					}
				}
			}

			return new CollectionUpdateResults(added, updated, removed);
		}
		#endregion

		#region IDeserializationCallback Members
#if !SILVERLIGHT
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
				if ( item != null )
				{
					m_dictionary.Add(item.Id, item);
				}
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
	}
}
