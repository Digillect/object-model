#region Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

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
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class ObservableObject : INotifyPropertyChanged
#if !(WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE)
		, INotifyPropertyChanging
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
#if !(WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE)
		/// <summary>
		/// Occurs when a property value is changing. Not guaranteed to be raised.
		/// </summary>
		public event PropertyChangingEventHandler PropertyChanging;
#endif
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
		[field: NonSerialized]
#endif
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
#if !(WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE)
			if ( PropertyChanging != null )
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Required for the CallerMemberName attribute.")]
		protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
#else
		protected void OnPropertyChanged( string propertyName )
#endif
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

		/// <summary>
		/// Checks if a property already matches a desired value.  Sets the property and
		/// notifies listeners only when necessary.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="location">The variable to set to the specified value.</param>
		/// <param name="value">The value to which the <paramref name="location"/> parameter is set.</param>
		/// <param name="propertyName">Name of the property used to notify listeners.</param>
		/// <returns><c>True</c> if the value has changed; <c>false</c> if the <paramref name="location"/> matches (by equality) the <paramref name="value"/>.</returns>
		/// <remarks>
		/// <b>.NET 4.5.</b> <paramref name="propertyName"/> is optional and can be provided automatically
		/// when invoked from compilers which support the <c>CallerMemberName</c> attribute.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
#if NET45
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Required for the CallerMemberName attribute.")]
		protected bool SetProperty<T>(ref T location, T value, [CallerMemberName] string propertyName = null)
#else
		protected bool SetProperty<T>(ref T location, T value, string propertyName)
#endif
		{
			if ( Object.ReferenceEquals(location, value) )
				return false;

			if ( propertyName != null )
			{
				OnPropertyChanging(propertyName, location, value);
			}

			location = value;

			if ( propertyName != null )
			{
				OnPropertyChanged(propertyName);
			}

			return true;
		}
		#endregion
	}
}
