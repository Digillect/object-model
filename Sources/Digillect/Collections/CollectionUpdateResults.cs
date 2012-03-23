using System;

namespace Digillect.Collections
{
#if !SILVERLIGHT
	/// <summary>
	/// Contains results of the collection update operation
	/// </summary>
	[Serializable]
#endif
	public sealed class CollectionUpdateResults
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly CollectionUpdateResults Empty = new CollectionUpdateResults();

		/// <summary>
		/// Gets the number of added objects.
		/// </summary>
		public int Added { get; private set; }
		/// <summary>
		/// Gets the number of removed objects.
		/// </summary>
		public int Removed { get; private set; }
		/// <summary>
		/// Gets the number of updated objects.
		/// </summary>
		public int Updated { get; private set; }

		#region Constructor
		private CollectionUpdateResults()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionUpdateResults"/> class.
		/// </summary>
		/// <param name="added">Number of added objects.</param>
		/// <param name="updated">Number of updated objects.</param>
		/// <param name="removed">Number of removed objects.</param>
		public CollectionUpdateResults(int added, int updated, int removed)
		{
			this.Added = added;
			this.Removed = removed;
			this.Updated = updated;
		}
		#endregion

		/// <summary>
		/// Gets a value indicating whether all counts are zero.
		/// </summary>
		/// <value>
		///   <c>true</c> if there were no added, removed or updated objects; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty
		{
			get { return this.Added == 0 && this.Removed == 0 && this.Updated == 0; }
		}
	}
}
