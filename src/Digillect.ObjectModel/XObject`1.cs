using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// <see cref="Digillect.XObject"/> that uses <typeparamref name="TId"/> type as the indentifier and key.
	/// </summary>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	[DataContract]
	[DebuggerDisplay("Id = {_id}")]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XObject<TId> : XObject, IXIdentified<TId>
		where TId : IEquatable<TId>
	{
		private TId _id;

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
			_id = id;
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
			get { return _id; }
			set
			{
				if ( !EqualityComparer<TId>.Default.Equals(_id, value) )
				{
					OnPropertyChanging("Id", _id, value);
					_id = value;
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
			var key = base.CreateKey();

			if ( _id != null )
			{
				key = key.WithKey(XKey.IdKeyName, _id);
			}

			return key;
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

			if ( obj._id is ValueType || obj._id is string )
			{
				_id = obj._id;
			}
			else if ( cloning )
			{
#if !(SILVERLIGHT || WINDOWS8)
				ICloneable icl = obj._id as ICloneable;

				if ( icl != null )
				{
					_id = (TId) icl.Clone();
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

			return EqualityComparer<TId>.Default.Equals(_id, other._id);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <b>GetHashCode</b> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="Object"/>.</returns>
		/// <remarks>See <see cref="Object.GetHashCode"/>.</remarks>
		public override int GetHashCode()
		{
			return this._id == null ? 0 : _id.GetHashCode();
		}
		#endregion
	}
}
