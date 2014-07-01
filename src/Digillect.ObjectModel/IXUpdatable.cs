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
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace Digillect
{
	/// <summary>
	/// Specifies that class supports update operations.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[ContractClass( typeof( IXUpdatableContract<> ) )]
	public interface IXUpdatable<T> : INotifyPropertyChanged
		where T : IXUpdatable<T>
	{
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <param name="deep"><see langword="true"/> to deep-clone inner collectios (including their members), <see langword="false"/> to clone only inner collections but not their members.</param>
		/// <returns>A new object that is a copy of this instance.</returns>
		[Pure]
		T Clone(bool deep);

		/// <summary>
		/// Begins the mass-update operation.
		/// </summary>
		/// <remarks>
		/// This method can be called multiple times.
		/// Until the last corresponding <see cref="EndUpdate"/> method is called none of the <see cref="INotifyPropertyChanged.PropertyChanged"/> events are raised.
		/// </remarks>
		void BeginUpdate();

		/// <summary>
		/// Ends the mass-update operation.
		/// </summary>
		/// <remarks>
		/// Do not forget to call this method for each corresponding <see cref="BeginUpdate"/> method you've called.
		/// Until the last corresponding <see cref="EndUpdate()"/> method is called none of the <see cref="INotifyPropertyChanged.PropertyChanged"/> events are raised.
		/// </remarks>
		void EndUpdate();

		/// <summary>
		/// Determines whether the update operation is needed.
		/// </summary>
		/// <param name="source">Source <b>object</b> to compare with.</param>
		/// <returns><c>false</c> if the <see cref="Update">update operation</see> is not required (i.e, the two objects are equal by reference), otherwise, <c>true</c>.</returns>
		[Pure]
		bool IsUpdateRequired(T source);

		/// <summary>
		/// Updates instance using <paramref name="source"/> as reference.
		/// </summary>
		/// <param name="source">Source object.</param>
		/// <remarks>
		/// At the end of the update the <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised if not blocked by a previous call to the <see cref="BeginUpdate"/> method.
		/// Otherwise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event will be raised upon the call to the last nested <see cref="EndUpdate"/> method.
		/// The corresponding <see cref="PropertyChangedEventArgs.PropertyName"/> equals <c>null</c> in both cases.
		/// </remarks>
		void Update(T source);
	}
}
