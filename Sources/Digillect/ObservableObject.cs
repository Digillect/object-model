using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Base object with change notification support.
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
		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableObject"/> class.
		/// </summary>
		protected ObservableObject()
		{
		}
		#endregion

		#region Events and Event Raisers
#if !SILVERLIGHT || WINDOWS_PHONE
		/// <summary>
		/// Occurs when a property value is changing. Not guaranteed to be raised.
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;
#endif
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="E:PropertyChanging"/> event.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="currentValue">The current value.</param>
		/// <param name="proposedValue">The proposed value.</param>
		protected void OnPropertyChanging( string propertyName, object currentValue, object proposedValue )
		{
			OnPropertyChanging( new Digillect.ComponentModel.PropertyChangingEventArgs( propertyName, currentValue, proposedValue ) );
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanging"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Digillect.ComponentModel.PropertyChangingEventArgs"/> instance containing the event data.</param>
		protected virtual void OnPropertyChanging( Digillect.ComponentModel.PropertyChangingEventArgs e )
		{
#if !SILVERLIGHT || WINDOWS_PHONE
			if( PropertyChanging != null )
			{
				PropertyChanging( this, e );
			}
#endif
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged( string propertyName )
		{
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if( PropertyChanged != null )
				PropertyChanged( this, e );
		}
		#endregion
	}
}
