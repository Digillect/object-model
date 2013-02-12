using System;

namespace Digillect.ComponentModel
{
#if !(WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE)
	/// <summary>
	/// Provides data for the <see cref="ObservableObject.PropertyChanging"/> event.
	/// </summary>
	/// <seealso cref="System.ComponentModel.PropertyChangingEventArgs"/>
	/// <seealso cref="System.ComponentModel.INotifyPropertyChanging"/>
#else
	/// <summary>
	/// Provides data for the <b>PropertyChanging</b> event.
	/// </summary>
#endif
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class PropertyChangingEventArgs
#if !(WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE)
		: System.ComponentModel.PropertyChangingEventArgs
#else
		: EventArgs
#endif
	{
#if WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE
		/// <summary>
		/// Gets the name of the property whose value is changing.
		/// </summary>
		/// <value>
		/// The name of the property whose value is changing.
		/// </value>
		public virtual string PropertyName { get; private set; }
#endif

		/// <summary>
		/// Current property value.
		/// </summary>
		public object CurrentValue { get; private set; }

		/// <summary>
		/// Proposed property value.
		/// </summary>
		public object ProposedValue { get; private set; }

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyChangingEventArgs"/> class.
		/// </summary>
		/// <param name="propertyName">Name of the property being changed.</param>
		/// <param name="currentValue">Current property value.</param>
		/// <param name="proposedValue">Proposed property value.</param>
		public PropertyChangingEventArgs(string propertyName, object currentValue, object proposedValue)
#if !(WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE)
			: base(propertyName)
#endif
		{
#if WINDOWS8 || SILVERLIGHT && !WINDOWS_PHONE
			this.PropertyName = propertyName;
#endif
			this.CurrentValue = currentValue;
			this.ProposedValue = proposedValue;
		}
		#endregion
	}
}