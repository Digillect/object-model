using System;
using System.Diagnostics.Contracts;

namespace Digillect
{
	/// <summary>
	/// Specifies that class supports update operations.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[ContractClass( typeof( IXUpdatableContract<> ) )]
	public interface IXUpdatable<T>
		where T : IXUpdatable<T>
	{
		event EventHandler Updated;

		/// <summary>
		/// Begins the mass-update operation.
		/// </summary>
		/// <remarks>
		/// This method can be called multiple times.
		/// Until the last corresponding <see cref="EndUpdate"/> method is called none of the <see cref="Updated"/> events are raised.
		/// </remarks>
		void BeginUpdate();

		/// <summary>
		/// Ends the mass-update operation.
		/// </summary>
		/// <remarks>
		/// Do not forget to call this method for each corresponding <see cref="BeginUpdate"/> method you called.
		/// Until the last corresponding <see cref="EndUpdate()"/> method is called none of the <see cref="Updated"/> events are raised.
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
		/// At the end of update <see cref="Updated"/> event is raised if not blocked by call to <see cref="BeginUpdate"/>. In the
		/// later scenario <see cref="Updated"/> event will be raised upon the call to <see cref="EndUpdate"/>.
		/// </remarks>
		void Update(T source);
	}
}
