using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// <see cref="Digillect.XObject"/> that uses <typeparamref name="TId"/> type as the indentifier and key.
	/// </summary>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	[DataContract]
	[DebuggerDisplay("Id = {id}")]
#if !SILVERLIGHT
	[Serializable]
#endif
	public class XObject<TId> : XObject, IXIdentifiable<TId>
		where TId : IComparable<TId>, IEquatable<TId>
	{
		private TId id;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XObject&lt;TId&gt;"/> class.
		/// </summary>
		protected XObject()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XObject&lt;TId&gt;"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		protected XObject( TId id )
		{
			this.id = id;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[DataMember]
		public TId Id
		{
			[DebuggerStepThrough]
			get { return this.id; }
			set
			{
				if ( !EqualityComparer<TId>.Default.Equals(this.id, value) )
				{
					OnPropertyChanging( "Id", this.id, value );
					this.id = value;
					ResetKey();
					OnPropertyChanged( "Id" );
				}
			}
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
		/// Performs update. Override update properties of your class.
		/// </summary>
		/// <param name="source">The source of update.</param>
		protected override void ProcessUpdate( XObject source )
		{
			base.ProcessUpdate( source );

			XObject<TId> obj = (XObject<TId>) source;

			this.id = obj.id;
		}
		#endregion

		#region Object Overrides
		/// <summary>
		/// Determines whether the specified <b>Object</b> is equal to the current <b>Object</b>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare with the current <b>Object</b>.</param>
		/// <returns><see langword="true"/> if the specified <b>Object</b> is equal to the current <b>Object</b>; otherwise <see langword="false"/>.</returns>
		/// <remarks>See <see cref="Object.Equals(object)"/>.</remarks>
		public override bool Equals( object obj )
		{
			if ( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			XObject<TId> other = (XObject<TId>) obj;

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
