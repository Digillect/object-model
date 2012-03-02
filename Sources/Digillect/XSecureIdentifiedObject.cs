using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Digillect
{
	[DataContract]
	[DebuggerDisplay("Id = {m_id}")]
#if !SILVERLIGHT
	[Serializable]
#endif
	public class XSecureIdentifiedObject<TId> : XObject, IXIdentifiable<TId>
		where TId : IComparable<TId>, IEquatable<TId>
	{
		private TId m_id;

		#region Constructor
		protected XSecureIdentifiedObject()
		{
			m_id = CreateDefaultId();
		}

		protected XSecureIdentifiedObject(TId id)
		{
			m_id = EqualityComparer<TId>.Default.Equals(id, default(TId)) ? CreateDefaultId() : id;
		}
		#endregion

		#region Public Properties
		[DataMember]
		public TId Id
		{
			[DebuggerStepThrough]
			get { return m_id; }
			protected set
			{
				if ( !EqualityComparer<TId>.Default.Equals(m_id, value) )
				{
					OnPropertyChanging("Id", m_id, value);
					m_id = value;
					ResetKey();
					OnPropertyChanged("Id");
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a copy of this object (using non-deep clone operation) setting the identifier to the specified one.
		/// </summary>
		/// <param name="id">Identifier for the new object.</param>
		/// <returns></returns>
		public XSecureIdentifiedObject<TId> ChangeId(TId id)
		{
			return ChangeId(id, false);
		}

		/// <overloads>
		/// Creates a copy of this object setting the identifier to the specified one.
		/// </overloads>
		/// <summary>
		/// Creates a copy of this object setting the identifier to the specified one.
		/// </summary>
		/// <param name="id">Identifier for the new object.</param>
		/// <param name="deepClone"><see langword="true"/> to perform a deep clone (see <see cref="XObject.Clone(bool)"/>).</param>
		/// <returns></returns>
		public virtual XSecureIdentifiedObject<TId> ChangeId(TId id, bool deepClone)
		{
			XSecureIdentifiedObject<TId> copy = (XSecureIdentifiedObject<TId>) Clone(deepClone); //CreateInstanceOfSameType();

			//copy.ProcessCopy(this, true, false);
			copy.m_id = Equals(id, default(TId)) ? CreateDefaultId() : id;

			return copy;
		}
		#endregion

		#region Protected Methods
		protected override XKey CreateKey()
		{
			return XKey.From( m_id, base.CreateKey() );
		}

		protected virtual TId CreateDefaultId()
		{
			return default(TId);
		}

		protected override void ProcessCopy(XObject source, bool clone, bool deep)
		{
			base.ProcessCopy(source, clone, deep);

			XSecureIdentifiedObject<TId> obj = (XSecureIdentifiedObject<TId>) source;

			m_id = obj.m_id;
		}

		protected override bool TargetIsCompatible(XObject obj)
		{
			if ( !base.TargetIsCompatible(obj) )
			{
				return false;
			}

			XSecureIdentifiedObject<TId> other = (XSecureIdentifiedObject<TId>) obj;

			return EqualityComparer<TId>.Default.Equals(m_id, other.m_id);
		}
		#endregion

		#region Object Overrides
		/// <summary>
		/// Determines whether the specified <b>Object</b> is equal to the current <b>Object</b>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare with the current <b>Object</b>.</param>
		/// <returns><see langword="true"/> if the specified <b>Object</b> is equal to the current <b>Object</b>; otherwise <see langword="false"/>.</returns>
		/// <remarks>See <see cref="Object.Equals(object)"/>.</remarks>
		public override bool Equals(object obj)
		{
			if ( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			XSecureIdentifiedObject<TId> other = (XSecureIdentifiedObject<TId>) obj;

			return EqualityComparer<TId>.Default.Equals(m_id, other.m_id);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <b>GetHashCode</b> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="Object"/>.</returns>
		/// <remarks>See <see cref="Object.GetHashCode"/>.</remarks>
		public override int GetHashCode()
		{
			return m_id == null ? 0 : m_id.GetHashCode();
		}
		#endregion
	}
}
