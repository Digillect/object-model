using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

using Digillect.Properties;

namespace Digillect
{
	[DataContract]
#if !SILVERLIGHT
	[Serializable]
#endif
	public abstract class XObject : ObservableObject, IXUpdatable<XObject>
#if !SILVERLIGHT
		, ICloneable
#endif
	{
#if !SILVERLIGHT
		[NonSerialized]
#endif
		private short m_updateCount;
#if !SILVERLIGHT
		[NonSerialized]
#endif
		private XKey m_key;

		#region Constructor
		protected XObject()
		{
		}
		#endregion

		#region Protected Properties
		protected bool InUpdate
		{
			get { return m_updateCount > 0; }
		}
		#endregion

		#region Events
		public event EventHandler Updated;

		protected override void OnPropertyChanging( Digillect.ComponentModel.PropertyChangingEventArgs e )
		{
			if( m_updateCount == 0 )
				base.OnPropertyChanging( e );
		}

		protected override void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if( m_updateCount == 0 )
				base.OnPropertyChanged( e );
		}

		protected virtual void OnUpdated( EventArgs e )
		{
			if( m_updateCount == 0 )
			{
				if( Updated != null )
					Updated( this, e );
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
			if( m_key == null )
			{
				m_key = CreateKey();

				if( m_key == null )
					throw new XKeyNotAvailableException();
			}

			return m_key;
		}

		/// <summary>
		/// Resets cached key, forcing the new one to be created.
		/// </summary>
		protected void ResetKey()
		{
			m_key = null;
		}

		protected virtual XKey CreateKey()
		{
			return null;
		}
		#endregion

#if !SILVERLIGHT
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

		public virtual bool UpdateRequired(XObject source)
		{
			return !ReferenceEquals(this, source);
		}

		public void Update( XObject source )
		{
			if( source == null )
				throw new ArgumentNullException( "source" );

			if( !TargetIsCompatible( source ) )
				throw new ArgumentException(Resources.XObjectSourceNotCompatibleException, "source");

			if( UpdateRequired( source ) )
			{
				ProcessCopy( source, false, false );

				OnUpdated( EventArgs.Empty );
			}
		}

		protected virtual void ProcessCopy( XObject source, bool clone, bool deep )
		{
			if ( source == null )
			{
				throw new ArgumentNullException("source");
			}

			m_key = source.m_key;
		}

		[EditorBrowsable( EditorBrowsableState.Advanced )]
		protected virtual bool TargetIsCompatible( XObject obj )
		{
			return obj != null && obj.GetType() == GetType();
		}

		[EditorBrowsable( EditorBrowsableState.Advanced )]
#if false // !SILVERLIGHT
		[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Demand, RestrictedMemberAccess = true)]
#endif
		protected virtual XObject CreateInstanceOfSameType()
		{
			return (XObject) Activator.CreateInstance( GetType() );
		}
		#endregion

		#region BeginUpdate/EndUpdate
		public void BeginUpdate()
		{
			++m_updateCount;
		}

		public void EndUpdate()
		{
			if( m_updateCount == 0 )
				return;
			if( --m_updateCount == 0 )
				OnUpdated( EventArgs.Empty );
		}
		#endregion
	}
}
