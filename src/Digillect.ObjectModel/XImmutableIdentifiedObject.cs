using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
#if WINDOWS8
using System.Reflection;
#endif
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// <see cref="Digillect.XObject"/> which uses <typeparamref name="TId"/> type as the indentifier that can't be ever changed.
	/// </summary>
	/// <typeparam name="TId">The type of the identifier.</typeparam>
	[DataContract]
	[DebuggerDisplay("Id = {_id}")]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public abstract class XImmutableIdentifiedObject<TId> : XObject, IXIdentified<TId>
		where TId : IComparable<TId>, IEquatable<TId>
	{
		[DataMember(Name = "Id", IsRequired = true)]
		private readonly TId _id;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XImmutableIdentifiedObject&lt;TId&gt;"/> class.
		/// </summary>
		/// <param name="id">The id.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected XImmutableIdentifiedObject(TId id)
		{
			if ( id == null )
			{
				throw new ArgumentNullException("id");
			}

			Contract.EndContractBlock();

			_id = id;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the identifier.
		/// </summary>
		public TId Id
		{
			[DebuggerStepThrough]
			get { return _id; }
		}
		#endregion

		#region Public Methods
		/// <inheritdoc/>
		public override bool IsObjectCompatible(XObject obj)
		{
			if ( !base.IsObjectCompatible(obj) )
			{
				return false;
			}

			XImmutableIdentifiedObject<TId> other = (XImmutableIdentifiedObject<TId>) obj;

			return EqualityComparer<TId>.Default.Equals(_id, other._id);
		}

		/// <summary>
		/// Creates a copy of this object (using non-deep clone operation) setting the identifier to the specified one.
		/// </summary>
		/// <param name="id">Identifier for the new object.</param>
		/// <returns></returns>
		[Pure]
		public XImmutableIdentifiedObject<TId> WithId(TId id)
		{
			Contract.Requires(id != null);

			return WithId(id, false);
		}

		/// <summary>
		/// Creates a copy of this object setting the identifier to the specified one.
		/// </summary>
		/// <param name="id">Identifier for the new object.</param>
		/// <param name="deepCloning"><c>true</c> if performing deep cloning, otherwise, <c>false</c>.</param>
		/// <returns></returns>
		[Pure]
		public virtual XImmutableIdentifiedObject<TId> WithId(TId id, bool deepCloning)
		{
			if ( id == null )
			{
				throw new ArgumentNullException("id");
			}

			Contract.EndContractBlock();

			XImmutableIdentifiedObject<TId> copy = CreateInstanceOfSameType(id);

			copy.ProcessCopy(this, true, deepCloning);

			return copy;
		}
		#endregion

		#region CreateKey
		/// <summary>
		/// Creates key for the specified identifier.
		/// </summary>
		/// <param name="id">Object identifier.</param>
		/// <param name="type">Type of the target object.</param>
		/// <returns>Created key.</returns>
		[Pure]
		protected static XKey CreateKey( TId id, Type type )
		{
			Contract.Requires(type != null);
#if !WINDOWS8
			Contract.Requires(typeof(XImmutableIdentifiedObject<TId>).IsAssignableFrom(type));
#else
			Contract.Requires(typeof(XImmutableIdentifiedObject<TId>).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()));
#endif

			return CreateKey(type).WithKey(XKey.IdKeyName, id);
		}
		#endregion

		#region Protected Methods
		/// <inheritdoc/>
		protected override XKey CreateKey()
		{
			return base.CreateKey().WithKey(XKey.IdKeyName, _id);
		}

		/// <inheritdoc/>
		/// <seealso cref="CreateInstanceOfSameType(TId)"/>
		protected sealed override XObject CreateInstanceOfSameType()
		{
			return CreateInstanceOfSameType(CloneId(_id));
		}

		/// <summary>
		/// A helper for the <see cref="XObject.Clone"/> method.
		/// </summary>
		/// <param name="id">The identifier for the created object.</param>
		/// <returns>A new object which has exactly the same type as this instance and the specified <paramref name="id"/>.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Pure]
		protected virtual XImmutableIdentifiedObject<TId> CreateInstanceOfSameType(TId id)
		{
			Contract.Requires(id != null);

			return (XImmutableIdentifiedObject<TId>) Activator.CreateInstance(GetType(), id);
		}

		[Pure]
		protected static TId CloneId(TId id)
		{
			if ( id == null )
			{
				throw new ArgumentNullException("id");
			}

			Contract.Ensures(Contract.Result<TId>() != null);

			if ( id is ValueType || id is string )
			{
				return id;
			}
			else
			{
#if !(SILVERLIGHT || WINDOWS8)
				ICloneable icl = id as ICloneable;

				if ( icl != null )
				{
					return (TId) icl.Clone();
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
		public override bool Equals(object obj)
		{
			if ( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			XImmutableIdentifiedObject<TId> other = (XImmutableIdentifiedObject<TId>) obj;

			return EqualityComparer<TId>.Default.Equals(_id, other._id);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <b>GetHashCode</b> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="Object"/>.</returns>
		/// <remarks>See <see cref="Object.GetHashCode"/>.</remarks>
		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}
		#endregion

#if DEBUG || CONTRACTS_FULL
		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_id != null);
		}
		#endregion
#endif
	}
}
