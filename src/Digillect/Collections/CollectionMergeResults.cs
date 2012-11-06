using System;

namespace Digillect.Collections
{
	/// <summary>
	/// Contains results of the collection merge operation.
	/// </summary>
	/// <seealso cref="XCollectionsUtil.Merge"/>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public sealed class CollectionMergeResults
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Indeed it is immutable")]
		public static readonly CollectionMergeResults Empty = new CollectionMergeResults();

		private readonly int _added;
		private readonly int _removed;
		private readonly int _updated;

		#region Constructor
		private CollectionMergeResults()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionMergeResults"/> class.
		/// </summary>
		/// <param name="added">Number of added objects.</param>
		/// <param name="updated">Number of updated objects.</param>
		/// <param name="removed">Number of removed objects.</param>
		public CollectionMergeResults(int added, int updated, int removed)
		{
			this._added = added;
			this._removed = removed;
			this._updated = updated;
		}
		#endregion

		/// <summary>
		/// Gets the number of added objects.
		/// </summary>
		public int Added
		{
			get { return this._added; }
		}

		/// <summary>
		/// Gets the number of removed objects.
		/// </summary>
		public int Removed
		{
			get { return this._removed; }
		}

		/// <summary>
		/// Gets the number of updated objects.
		/// </summary>
		public int Updated
		{
			get { return this._updated; }
		}

		/// <summary>
		/// Gets a value indicating whether all counts are zero.
		/// </summary>
		/// <value>
		/// <c>true</c> if there were no added, removed or updated objects; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty
		{
			get { return this._added == 0 && this._removed == 0 && this._updated == 0; }
		}
	}
}
