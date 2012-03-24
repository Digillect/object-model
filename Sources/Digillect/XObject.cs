using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Abstract base class that supports updating. Exposes <see cref="XKey"/> as an identifier.
	/// </summary>
	[DataContract]
#if !(SILVERLIGHT || NETFX_CORE)
	[Serializable]
#endif
	public class XObject : ObservableObject, IXUpdatable<XObject>
#if !(SILVERLIGHT || NETFX_CORE)
		, ICloneable
#endif
	{
#if !(SILVERLIGHT || NETFX_CORE)
		[NonSerialized]
#endif
		private ushort updateCount;

#if !(SILVERLIGHT || NETFX_CORE)
		[NonSerialized]
#endif
		private bool isKeyCreated;
#if !(SILVERLIGHT || NETFX_CORE)
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
			if ( !this.isKeyCreated )
			{
				this.isKeyCreated = true;
				this.key = CreateKey();
			}

			return this.key;
		}

		/// <summary>
		/// Resets cached key, forcing the new one to be created.
		/// </summary>
		protected void ResetKey()
		{
			this.isKeyCreated = false;
			this.key = null;
		}

		/// <summary>
		/// Creates the key.
		/// </summary>
		/// <returns>Created key.</returns>
		protected virtual XKey CreateKey()
		{
			return null;
		}
		#endregion


#if !(SILVERLIGHT || NETFX_CORE)
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
			XObject clone = CreateInstanceOfSameType();

			clone.ProcessCopy( this, true, deep );

			return clone;
		}
		/// <summary>
		/// Determines whether update operation is needed.
		/// </summary>
		/// <param name="source">Source object to compare with.</param>
		/// <returns>
		///   <c>true</c> if update is required; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>Overrride to provide custom logic. Default implementation requires update any time when
		/// objects are not equal by reference.</remarks>
		public virtual bool IsUpdateRequired( XObject source )
		{
			return !ReferenceEquals( this, source );
		}

		/// <summary>
		/// Updates object from the specified source.
		/// </summary>
		/// <param name="source">The source.</param>
		public void Update( XObject source )
		{
			if( !IsObjectCompatible( source ) )
				throw new ArgumentException( "Source object is not compatible with the current one.", "source" );

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
			Contract.Requires(source != null, "source");

			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			this.key = source.key;
		}

		/// <summary>
		/// Checks whether specified object is compatible with this instance and can be used in update operation.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns><c>true</c> if <paramref name="obj"/> can be used as source for <see cref="Update"/>, otherwise <c>false</c>.</returns>
		[Pure, EditorBrowsable( EditorBrowsableState.Advanced )]
		public virtual bool IsObjectCompatible( XObject obj )
		{
			return obj != null && obj.GetType() == GetType();
		}

		[EditorBrowsable( EditorBrowsableState.Advanced )]
#if false // !(SILVERLIGHT || NETFX_CORE)
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Demand, RestrictedMemberAccess = true)]
#endif
		protected virtual XObject CreateInstanceOfSameType()
		{
			Contract.Ensures( Contract.Result<XObject>() != null );

			return (XObject) Activator.CreateInstance( GetType() );
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
	}
}
