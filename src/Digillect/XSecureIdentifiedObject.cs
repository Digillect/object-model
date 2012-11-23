using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// <see cref="Digillect.XObject"/> that uses <typeparamref name="TId"/> type as the indentifier that can't be changed.
	/// </summary>
	/// <typeparam name="TId">The type of the id.</typeparam>
#if DEBUG || CONTRACTS_FULL
	[ContractClass(typeof(XSecureIdentifiedObjectContract<>))]
#endif
	[DataContract]
	[DebuggerDisplay("Id = {id}")]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public abstract class XSecureIdentifiedObject<TId> : XObject, IXIdentified<TId>
		where TId : IComparable<TId>, IEquatable<TId>
	{
		private TId id;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XSecureIdentifiedObject&lt;TId&gt;"/> class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected XSecureIdentifiedObject()
		{
			this.id = CheckId(CreateDefaultId());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XSecureIdentifiedObject&lt;TId&gt;"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected XSecureIdentifiedObject(TId id)
		{
			//if ( id == null )
			//{
			//	throw new ArgumentNullException("id");
			//}

			//Contract.EndContractBlock();

			if ( EqualityComparer<TId>.Default.Equals(id, default(TId)) )
			{
				id = CheckId(CreateDefaultId());
			}

			Contract.Assume(id != null);

			this.id = id;
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
				if ( value == null )
				{
					throw new ArgumentNullException("value");
				}

				Contract.EndContractBlock();

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
		/// Creates a copy of this object (using non-deep clone operation) setting the identifier to the specified one.
		/// </summary>
		/// <param name="newId">Identifier for the new object.</param>
		/// <returns></returns>
		public XSecureIdentifiedObject<TId> ChangeId(TId newId)
		{
			return ChangeId(newId, false);
		}

		/// <summary>
		/// Creates a copy of this object setting the identifier to the specified one.
		/// </summary>
		/// <param name="newId">Identifier for the new object.</param>
		/// <param name="deepCloning"><c>true</c> if performing deep cloning, otherwise, <c>false</c>.</param>
		/// <returns></returns>
		public virtual XSecureIdentifiedObject<TId> ChangeId(TId newId, bool deepCloning)
		{
			XSecureIdentifiedObject<TId> copy = (XSecureIdentifiedObject<TId>) Clone( deepCloning );

			if ( EqualityComparer<TId>.Default.Equals(newId, default(TId)) )
			{
				newId = CheckId(CreateDefaultId());
			}

			copy.id = newId;

			return copy;
		}
		#endregion

		#region Protected Methods
		/// <summary>
		/// Creates the default id.
		/// </summary>
		/// <returns>Default identifier.</returns>
		protected abstract TId CreateDefaultId();

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
		/// Performs update/clone operation. Override to clone or update properties of your class.
		/// </summary>
		/// <param name="source">The source.</param>
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

			XSecureIdentifiedObject<TId> obj = (XSecureIdentifiedObject<TId>) source;

			if ( cloning )
			{
				if ( obj.id is ValueType || obj.id is string )
				{
					this.id = obj.id;
				}
				else
				{
#if !(SILVERLIGHT || WINDOWS8)
					ICloneable icl = obj.id as ICloneable;

					if ( icl != null )
					{
						this.id = (TId) icl.Clone();

						Contract.Assume(this.id != null);
					}
					else
#endif
					{
						throw new InvalidOperationException(Errors.XObjectIdentifierNotCloneable);
					}
				}
			}
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

		private static TId CheckId(TId id)
		{
			Contract.Ensures(Contract.Result<TId>() != null);

			if ( id == null )
			{
				throw new ArgumentNullException("id");
			}

			return id;
		}

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(this.id != null);
		}
		#endregion
	}

	#region XSecureIdentifiedObject`1 contract binding
#if DEBUG || CONTRACTS_FULL
	[ContractClassFor(typeof(XSecureIdentifiedObject<>))]
	abstract class XSecureIdentifiedObjectContract<TId> : XSecureIdentifiedObject<TId>
		where TId : IComparable<TId>, IEquatable<TId>
	{
		protected XSecureIdentifiedObjectContract()
		{
		}

		protected override TId CreateDefaultId()
		{
			Contract.Ensures(Contract.Result<TId>() != null);

			return default(TId);
		}
	}
#endif
	#endregion
}
