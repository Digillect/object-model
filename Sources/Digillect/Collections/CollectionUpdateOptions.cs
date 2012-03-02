using System;

namespace Digillect.Collections
{
	[Flags]
	public enum CollectionUpdateOptions
	{
		None = 0,
		AddNew = 1,
		RemoveOld = 2,
		UpdateExisting = 4,
		All = AddNew | RemoveOld | UpdateExisting // 7
	}
}
