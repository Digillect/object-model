using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Base "X"-object with change notification.
	/// </summary>
	[DataContract]
#if !SILVERLIGHT
	[Serializable]
#endif
	public class ObservableObject
#if !SILVERLIGHT || WINDOWS_PHONE
		: INotifyPropertyChanging, INotifyPropertyChanged
#else
		: INotifyPropertyChanged
#endif
	{
		#region Constructor
		protected ObservableObject()
		{
		}
		#endregion

		#region Events and Event Raisers
#if !SILVERLIGHT || WINDOWS_PHONE
		public event PropertyChangingEventHandler PropertyChanging;
#endif
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanging( string propertyName, object currentValue, object proposedValue )
		{
			OnPropertyChanging( new Digillect.ComponentModel.PropertyChangingEventArgs( propertyName, currentValue, proposedValue ) );
		}

		protected virtual void OnPropertyChanging( Digillect.ComponentModel.PropertyChangingEventArgs e )
		{
#if !SILVERLIGHT || WINDOWS_PHONE
			if( PropertyChanging != null )
			{
				PropertyChanging( this, e );
			}
#endif
		}

		protected void OnPropertyChanged( string propertyName )
		{
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if( PropertyChanged != null )
				PropertyChanged( this, e );
		}
		#endregion
	}
}
