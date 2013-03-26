using System;
using System.Diagnostics.Contracts;

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
		/// <summary>
		/// Represents an empty merge result.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Indeed it is immutable")]
		public static readonly CollectionMergeResults Empty = new CollectionMergeResults();

		private readonly int _added;
		private readonly int _removed;
		private readonly int _updated;

		#region Constructor
		private CollectionMergeResults()
		{
		}

		private CollectionMergeResults(int added, int removed, int updated)
		{
			_added = added;
			_removed = removed;
			_updated = updated;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the number of added objects.
		/// </summary>
		public int Added
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _added;
			}
		}

		/// <summary>
		/// Gets the number of removed objects.
		/// </summary>
		public int Removed
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _removed;
			}
		}

		/// <summary>
		/// Gets the number of updated objects.
		/// </summary>
		public int Updated
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() >= 0);

				return _updated;
			}
		}

		/// <summary>
		/// Gets a value indicating whether all counts are zero.
		/// </summary>
		/// <value>
		/// <c>true</c> if there were no added, removed or updated objects; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty
		{
			get
			{
				Contract.Ensures(!Contract.Result<bool>() || this.Added == 0 && this.Removed == 0 && this.Updated == 0);

				return _added == 0 && _removed == 0 && _updated == 0;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns a new instance of the <see cref="CollectionMergeResults"/> class with the <see cref="Added"/>, <see cref="Removed"/> and <see cref="Updated"/> counts set to the specified values.
		/// </summary>
		/// <param name="added">Number of added objects.</param>
		/// <param name="updated">Number of updated objects.</param>
		/// <param name="removed">Number of removed objects.</param>
		public CollectionMergeResults With(int added, int removed, int updated)
		{
			if ( added < 0 )
			{
				throw new ArgumentOutOfRangeException("added");
			}

			if ( removed < 0 )
			{
				throw new ArgumentOutOfRangeException("removed");
			}

			if ( updated < 0 )
			{
				throw new ArgumentOutOfRangeException("updated");
			}

			Contract.Ensures(Contract.Result<CollectionMergeResults>() != null);

			if ( added == _added && removed == _removed && updated == _updated )
			{
				return this;
			}

			return new CollectionMergeResults(added, removed, updated);
		}
		#endregion
	}
}
