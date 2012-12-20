using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// <see cref="Digillect.XObject"/> that uses <typeparamref name="TId"/> type as the indentifier and key.
	/// </summary>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	[DataContract]
	[DebuggerDisplay("Id = {id}")]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XObject<TId> : XObject, IXIdentified<TId>
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

		#region CreateKey
		/// <summary>
		/// Creates key for the specified identifier.
		/// </summary>
		/// <param name="id">Object identifier.</param>
		/// <returns>Created key.</returns>
		public static XKey CreateKey( TId id )
		{
			return CreateKey( id, typeof( XObject<TId> ) );
		}

		/// <summary>
		/// Creates key for the specified identifier.
		/// </summary>
		/// <param name="id">Object identifier.</param>
		/// <param name="type">Type of the target object.</param>
		/// <returns>Created key.</returns>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public static XKey CreateKey( TId id, Type type )
		{
			return XKey.From( id, CreateKey( type ) );
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
		/// <param name="cloning"><c>true</c> if cloning source, otherwise, <c>false</c>.</param>
		/// <param name="deepCloning"><c>true</c> if performing deep cloning, otherwise, <c>false</c>.</param>
		protected override void ProcessCopy( XObject source, bool cloning, bool deepCloning )
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.EndContractBlock();

			base.ProcessCopy(source, cloning, deepCloning);

			XObject<TId> obj = (XObject<TId>) source;

			if ( obj.id is ValueType || obj.id is string )
			{
				this.id = obj.id;
			}
			else if ( cloning )
			{
#if !(SILVERLIGHT || WINDOWS8)
				ICloneable icl = obj.id as ICloneable;

				if ( icl != null )
				{
					this.id = (TId) icl.Clone();
				}
				else
#endif
				{
					throw new InvalidOperationException(Errors.XObjectIdentifierNotCloneable);
				}
			} 
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
