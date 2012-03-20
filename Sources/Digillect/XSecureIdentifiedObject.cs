using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// <see cref="Digillect.XObject"/> that uses <typeparamref name="TId"/> type as the indentifier that can't be changed.
	/// </summary>
	/// <typeparam name="TId">The type of the id.</typeparam>
	[DataContract]
	[DebuggerDisplay("Id = {id}")]
#if !SILVERLIGHT
	[Serializable]
#endif
	public class XSecureIdentifiedObject<TId> : XObject, IXIdentifiable<TId>
		where TId : IComparable<TId>, IEquatable<TId>
	{
		private TId id;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XSecureIdentifiedObject&lt;TId&gt;"/> class.
		/// </summary>
		protected XSecureIdentifiedObject()
		{
			this.id = CreateDefaultId();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XSecureIdentifiedObject&lt;TId&gt;"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		protected XSecureIdentifiedObject(TId id)
		{
			this.id = EqualityComparer<TId>.Default.Equals(id, default(TId)) ? CreateDefaultId() : id;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value>
		/// The id.
		/// </value>
		[DataMember]
		public TId Id
		{
			[DebuggerStepThrough]
			get { return this.id; }
			protected set
			{
				if ( !EqualityComparer<TId>.Default.Equals(this.id, value) )
				{
					OnPropertyChanging("Id", this.id, value);
					this.id = value;
					ResetKey();
					OnPropertyChanged("Id");
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a copy of this object setting the identifier to the specified one.
		/// </summary>
		/// <param name="id">Identifier for the new object.</param>
		/// <returns></returns>
		public virtual XSecureIdentifiedObject<TId> ChangeId(TId id)
		{
			XSecureIdentifiedObject<TId> copy = CreateInstanceOfSameType();

			copy.ProcessUpdate(this);
			copy.id = Equals(id, default(TId)) ? CreateDefaultId() : id;

			return copy;
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Creates the key.
		/// </summary>
		/// <returns>
		/// Created key.
		/// </returns>
		protected override XKey CreateKey()
		{
			return XKey.From( this.id, base.CreateKey() );
		}

		/// <summary>
		/// Creates the default id.
		/// </summary>
		/// <returns>Default identifier.</returns>
		protected virtual TId CreateDefaultId()
		{
			return default(TId);
		}

		/// <summary>
		/// Creates the instance of the same type.
		/// </summary>
		/// <returns></returns>
		protected virtual XSecureIdentifiedObject<TId> CreateInstanceOfSameType()
		{
			return (XSecureIdentifiedObject<TId>) Activator.CreateInstance( GetType() );
		}
		/// <summary>
		/// Performs update/clone operation. Override to clone or update properties of your class.
		/// </summary>
		/// <param name="source">The source.</param>
		protected override void ProcessUpdate( XObject source )
		{
			base.ProcessUpdate( source );

			/*
			 * GN: IMHO not needed, since IsObjectCompatible ensures that source object has the same id.
			XSecureIdentifiedObject<TId> obj = (XSecureIdentifiedObject<TId>) source;

			this.id = obj.id;
			*/
		}

		/// <summary>
		/// Checks whether specified object is compatible with this instance and can be used in update operation.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns>
		///   <c>true</c> if <paramref name="obj"/> can be used as source for <see cref="Digillect.XObject.Update"/>, otherwise <c>false</c>.
		/// </returns>
		public override bool IsObjectCompatible(XObject obj)
		{
			if ( !base.IsObjectCompatible(obj) )
			{
				return false;
			}

			XSecureIdentifiedObject<TId> other = (XSecureIdentifiedObject<TId>) obj;

			return EqualityComparer<TId>.Default.Equals(this.id, other.id);
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

			return EqualityComparer<TId>.Default.Equals(this.id, other.id);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <b>GetHashCode</b> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="Object"/>.</returns>
		/// <remarks>See <see cref="Object.GetHashCode"/>.</remarks>
		public override int GetHashCode()
		{
			return this.id == null ? 0 : this.id.GetHashCode();
		}
		#endregion
	}
}
