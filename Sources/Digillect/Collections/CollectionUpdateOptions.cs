using System;

namespace Digillect.Collections
{
	/// <summary>
	/// Options that are used in collection update operation.
	/// </summary>
	[Flags]
	public enum CollectionUpdateOptions
	{
		/// <summary>
		/// Do nothing.
		/// </summary>
		None = 0,
		/// <summary>
		/// Add objects that exists in source collection but doesn't exists in target collection to target collection.
		/// </summary>
		AddNew = 1,
		/// <summary>
		/// Remove objects that are still exist in target collection but no more exists in source collection from target collection.
		/// </summary>
		RemoveOld = 2,
		/// <summary>
		/// Perform update on objects that exists in both source and target collections.
		/// </summary>
		UpdateExisting = 4,
		/// <summary>
		/// Perform all possible update operations.
		/// </summary>
		All = AddNew | RemoveOld | UpdateExisting // 7
	}
}
