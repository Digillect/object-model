using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Abstract base class that supports updating. Exposes <see cref="XKey"/> as an identifier.
	/// </summary>
	[DataContract]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XObject : ObservableObject, IXUpdatable<XObject>
#if !(SILVERLIGHT || WINDOWS8)
		, ICloneable
#endif
	{
#if !(SILVERLIGHT || WINDOWS8)
		[NonSerialized]
#endif
		private ushort updateCount;

#if !(SILVERLIGHT || WINDOWS8)
		[NonSerialized]
#endif
		private XKey key;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XObject"/> class.
		/// </summary>
		protected XObject()
		{
		}
		#endregion

		#region Protected Properties
		/// <summary>
		/// Gets a value indicating whether this instance is in update.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is in update; otherwise, <c>false</c>.
		/// </value>
		protected bool IsInUpdate
		{
			get { return this.updateCount > 0; }
		}
		#endregion

		#region Events
		/// <summary>
		/// Occurs when object has been updated.
		/// </summary>
		public event EventHandler Updated;

		/// <summary>
		/// Raises the <see cref="E:PropertyChanging"/> event if object is not updating.
		/// </summary>
		/// <param name="e">The <see cref="Digillect.ComponentModel.PropertyChangingEventArgs"/> instance containing the event data.</param>
		protected override void OnPropertyChanging( Digillect.ComponentModel.PropertyChangingEventArgs e )
		{
			if( this.updateCount == 0 )
				base.OnPropertyChanging( e );
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event if object is not updating.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected override void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if( this.updateCount == 0 )
				base.OnPropertyChanged( e );
		}

		/// <summary>
		/// Raises the <see cref="E:Updated"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void OnUpdated( EventArgs e )
		{
			if( this.updateCount == 0 )
			{
				var handler = this.Updated;

				if( handler != null )
					handler( this, e );
			}
		}
		#endregion

		#region Key management
		/// <summary>
		/// Gets key, identifying current object. If no cached key available, the new one will be created
		/// through the call to <see cref="CreateKey"/>.
		/// </summary>
		/// <returns>Key, identifying this object.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public XKey GetKey()
		{
			Contract.Ensures(Contract.Result<XKey>() != null);
			Contract.EnsuresOnThrow<XKeyNotAvailableException>(this.key == null);

			if ( Object.ReferenceEquals(this.key, null) )
			{
				this.key = CreateKey();

				if ( Object.ReferenceEquals(this.key, null) )
				{
					throw new XKeyNotAvailableException(Errors.XObjectNullKeyException);
				}
			}

			return this.key;
		}

		/// <summary>
		/// Resets cached key, forcing the new one to be created.
		/// </summary>
		protected void ResetKey()
		{
			this.key = null;
		}

		/// <summary>
		/// Creates the key.
		/// </summary>
		/// <returns>Created key.</returns>
		protected virtual XKey CreateKey()
		{
			Contract.Ensures(Contract.Result<XKey>() != null);

			return new RootKey(GetType());
		}
		#endregion

#if !(SILVERLIGHT || WINDOWS8)
		#region ICloneable Members
		object ICloneable.Clone()
		{
			return Clone( true );
		}
		#endregion
#endif

		#region Clone/Update
		public XObject Clone( bool deep )
		{
			Contract.Ensures(Contract.Result<XObject>() != null);

			XObject clone = CreateInstanceOfSameType();

			clone.ProcessCopy( this, true, deep );

			return clone;
		}
		/// <summary>
		/// Determines whether update operation is needed.
		/// </summary>
		/// <param name="source">Source object to compare with.</param>
		/// <returns>
		/// <c>true</c> if update is required; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// Overrride to provide custom logic. Default implementation requires update any time when objects are not equal by reference.
		/// </remarks>
		public virtual bool IsUpdateRequired(XObject source)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.EndContractBlock();

			return !Object.ReferenceEquals(this, source);
		}

		/// <summary>
		/// Updates object from the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void Update(XObject source)
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			Contract.EndContractBlock();

			if ( !IsObjectCompatible(source) )
				throw new ArgumentException(Errors.XObjectSourceNotCompatibleException, "source");

			if( IsUpdateRequired( source ) )
			{
				ProcessCopy( source, false, false );

				OnUpdated( EventArgs.Empty );
			}
		}

		/// <summary>
		/// Performs update. Override update properties of your class.
		/// </summary>
		/// <param name="source">The source of update.</param>
		/// <param name="cloning"><c>true</c> if cloning source, otherwise, <c>false</c>.</param>
		/// <param name="deepCloning"><c>true</c> if performing deep cloning, otherwise, <c>false</c>.</param>
		protected virtual void ProcessCopy( XObject source, bool cloning, bool deepCloning )
		{
			// Drop the key to overcome potentional cloning issues
			this.key = null;
		}

		/// <summary>
		/// Performs complex clone/update procedure for XObject-derived property.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="thisField">Reference to the local field.</param>
		/// <param name="otherField">Same field in other instance.</param>
		/// <param name="cloning"><c>true</c> if cloning source, otherwise <c>false</c>.</param>
		/// <param name="deepCloning"><c>true</c> if performing deep cloning, otherwise <c>false</c>.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
		protected static void ProcessCopyProperty<T>(ref T thisField, T otherField, bool cloning, bool deepCloning) where T : XObject
		{
			if( cloning )
			{
				thisField = otherField == null ? null : (T) otherField.Clone( deepCloning );
			}
			else
			{
				if( thisField != null )
				{
					if( otherField != null )
					{
						thisField.Update( otherField );
					}
					else
					{
						thisField = null;
					}
				}
				else
				{
					if( otherField != null )
					{
						thisField = (T) otherField.Clone( false );
					}
				}
			}
		}

		/// <summary>
		/// Performs complex clone/update procedure for XCollection-derived property.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="thisField">Reference to the local field.</param>
		/// <param name="otherField">Same field in other instance.</param>
		/// <param name="cloning"><c>true</c> if cloning source, otherwise <c>false</c>.</param>
		/// <param name="deepCloning"><c>true</c> if performing deep cloning, otherwise <c>false</c>.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
		protected static void ProcessCopyProperty<T>(ref Collections.XCollection<T> thisField, Collections.XCollection<T> otherField, bool cloning, bool deepCloning) where T : XObject
		{
			if( cloning )
			{
				thisField = otherField == null ? null : otherField.Clone(deepCloning);
			}
			else
			{
				if( thisField != null )
				{
					if( otherField != null )
					{
						thisField.Update( otherField );
					}
					else
					{
						thisField = null;
					}
				}
				else
				{
					if( otherField != null )
					{
						thisField = otherField.Clone(false);
					}
				}
			}
		}

		/// <summary>
		/// Checks whether specified object is compatible with this instance and can be used in update operation.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns><c>true</c> if <paramref name="obj"/> can be used as source for <see cref="Update"/>, otherwise <c>false</c>.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Pure]
		public virtual bool IsObjectCompatible( XObject obj )
		{
			return obj != null && obj.GetType() == GetType();
		}

		[EditorBrowsable( EditorBrowsableState.Advanced )]
		[Pure]
#if false // !(SILVERLIGHT || WINDOWS8)
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Demand, RestrictedMemberAccess = true)]
#endif
		protected virtual XObject CreateInstanceOfSameType()
		{
			Contract.Ensures( Contract.Result<XObject>() != null );

#if WINDOWS8
			return (XObject) Activator.CreateInstance(GetType());
#else
			return (XObject) Activator.CreateInstance(GetType(), true);
#endif
		}
		#endregion

		#region BeginUpdate/EndUpdate
		/// <summary>
		/// Begins the update, preventing <see cref="E:Digillect.ObservableObject.PropertyChanging"/> and <see cref="E:Digillect.ObservableObject.PropertyChanged"/> events
		/// from being raised until <see cref="EndUpdate"/> methos is called.
		/// </summary>
		public void BeginUpdate()
		{
			++updateCount;
		}

		/// <summary>
		/// Marks the end of update scope. If this was the last scope of update, then <see cref="Updated"/> event will be raised.
		/// </summary>
		public void EndUpdate()
		{
			if( updateCount == 0 )
				return;
			if( --updateCount == 0 )
				OnUpdated( EventArgs.Empty );
		}
		#endregion

		#region class RootKey
		[DebuggerDisplay("Type = {_type}")]
#if !(SILVERLIGHT || WINDOWS8)
		[Serializable]
#endif
		private sealed class RootKey : XKey, IEquatable<RootKey>
		{
			private readonly Type _type;

			public RootKey(Type type)
			{
				Contract.Requires(type != null);

				this._type = type;
			}

			public override int CompareTo(XKey other)
			{
				if ( Object.ReferenceEquals(other, null) )
				{
					return 1;
				}

				RootKey otherKey = other as RootKey;

				if ( otherKey == null )
				{
					throw new ArgumentException("Invalid key.", "other");
				}

				if ( this._type == otherKey._type )
				{
					return 0;
				}

				return String.Compare(this._type.AssemblyQualifiedName, otherKey._type.AssemblyQualifiedName, StringComparison.OrdinalIgnoreCase);
			}

			public bool Equals(RootKey other)
			{
				if ( Object.ReferenceEquals(other, null) )
				{
					return false;
				}

				return Object.ReferenceEquals(this, other) || this._type == other._type;
			}

			public override bool Equals(XKey other)
			{
				return Equals(other as RootKey);
			}

			public override int GetHashCode()
			{
				return this._type.GetHashCode();
			}

			public override string ToString()
			{
				return _type.ToString();
			}
		}
		#endregion
	}
}
