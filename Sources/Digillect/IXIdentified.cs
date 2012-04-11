using System;

namespace Digillect
{
	/// <summary>
	/// Specifies that class can be identified by <c>Id</c>.
	/// </summary>
	/// <typeparam name="TId">The type of the id.</typeparam>
	public interface IXIdentified<TId>
	{
		/// <summary>
		/// Gets the id.
		/// </summary>
		TId Id
		{
			get;
		}
	}
}
