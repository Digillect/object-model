using System;

namespace Digillect.Collections
{
	/// <summary>
	/// Options that are used in collection merge operation.
	/// </summary>
	/// <seealso cref="XCollectionsUtil.Merge"/>
	[Flags]
	public enum CollectionMergeOptions
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
		/// Preserve order of objects in target collection as found in source one.
		/// </summary>
		PreserveSourceOrder = 8,

		/// <summary>
		/// Perform all possible merge operations.
		/// </summary>
		Full = AddNew | RemoveOld | UpdateExisting | PreserveSourceOrder
	}
}
