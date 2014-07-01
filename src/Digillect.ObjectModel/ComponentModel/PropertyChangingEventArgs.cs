#region Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
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
