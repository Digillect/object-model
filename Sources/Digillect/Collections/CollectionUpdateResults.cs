using System;

namespace Digillect.Collections
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public sealed class CollectionUpdateResults
	{
		public static readonly CollectionUpdateResults Empty = new CollectionUpdateResults();

		public int Added { get; private set; }
		public int Removed { get; private set; }
		public int Updated { get; private set; }

		#region Constructor
		private CollectionUpdateResults()
		{
		}

		public CollectionUpdateResults(int added, int updated, int removed)
		{
			this.Added = added;
			this.Removed = removed;
			this.Updated = updated;
		}
		#endregion

		public bool IsEmpty
		{
			get { return this.Added == 0 && this.Removed == 0 && this.Updated == 0; }
		}
	}
}
