using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Base object with change notification support.
	/// </summary>
	[DataContract]
#if !(SILVERLIGHT || NETFX_CORE)
	[Serializable]
#endif
	public class ObservableObject
#if !(SILVERLIGHT || NETFX_CORE) || WINDOWS_PHONE
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
#if !(SILVERLIGHT || NETFX_CORE) || WINDOWS_PHONE
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
#if !(SILVERLIGHT || NETFX_CORE) || WINDOWS_PHONE
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
#if NET45
		protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}
#else
		protected void OnPropertyChanged( string propertyName )
		{
			OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		}
#endif

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
		{
			if( PropertyChanged != null )
				PropertyChanged( this, e );
		}

		/// <summary>
		/// Checks if a property already matches a desired value.  Sets the property and
		/// notifies listeners only when necessary.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="storage">Reference to a property with both getter and setter.</param>
		/// <param name="value">Desired value for the property.</param>
		/// <param name="propertyName">Name of the property used to notify listeners.</param>
		/// <returns>True if the value was changed, false if the existing value matched the
		/// desired value.</returns>
		/// <remarks>
		/// <b>.NET 4.5.</b> <paramref name="propertyName"/> is optional and can be provided automatically
		/// when invoked from compilers that support <c>CallerMemberName</c>.
		/// </remarks>
#if NET45
		protected bool SetProperty<T>( ref T storage, T value, [CallerMemberName] String propertyName = null )
#else
		protected bool SetProperty<T>(ref T storage, T value, string propertyName)
#endif
		{
			if( object.Equals( storage, value ) )
				return false;

			OnPropertyChanging( propertyName, value, storage );

			storage = value;
			
			OnPropertyChanged( propertyName );
			return true;
		}
		#endregion
	}
}
