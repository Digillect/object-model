using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

using Digillect.Properties;

namespace Digillect
{
	/// <summary>
	/// Abstract base class that supports cloning and updating. Exposes <see cref="Digillect.XKey"/> as identifier.
	/// </summary>
	[DataContract]
#if !SILVERLIGHT
	[Serializable]
#endif
	public class XObject : ObservableObject, IXUpdatable<XObject>
	{
#if !SILVERLIGHT
		[NonSerialized]
#endif
		private short updateCount;
#if !SILVERLIGHT
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
		/// 	<c>true</c> if this instance is in update; otherwise, <c>false</c>.
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
		/// <exception cref="XKeyNotAvailableException">If key was not created properly.</exception>
		public XKey GetKey()
		{
			Contract.Ensures( Contract.Result<XKey>() != null );

			if( this.key == null )
			{
				this.key = CreateKey();

				if( this.key == null )
					throw new XKeyNotAvailableException();
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
			Contract.Ensures( Contract.Result<XKey>() != null );

			throw new XKeyNotAvailableException();
		}
		#endregion

		#region Update
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
				throw new ArgumentException( Resources.XObjectSourceNotCompatibleException, "source" );

			if( IsUpdateRequired( source ) )
			{
				ProcessUpdate( source );

				OnUpdated( EventArgs.Empty );
			}
		}

		/// <summary>
		/// Performs update. Override update properties of your class.
		/// </summary>
		/// <param name="source">The source of update.</param>
		protected virtual void ProcessUpdate( XObject source )
		{
			Contract.Requires( source != null );

			key = source.key;
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
