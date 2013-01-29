using System;

namespace Digillect
{
	internal static class Errors
	{
		internal const string XCollectionEnumFailedVersionException = "Collection was modified; enumeration operation may not execute.";
		internal const string XCollectionItemDuplicateException = "This object is already a member of the collection.";
		internal const string XCollectionReadOnlyException = "The target collection is read-only.";
		internal const string XObjectIdentifierNotCloneable = "The identifier does not support the ICloneable interface required for object cloning.";
		internal const string XObjectNullKeyException = "The object's key is null.";
		internal const string XObjectSourceNotCompatibleException = "Source object is not compatible with the current one.";

		internal const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
		internal const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
		internal const string Arg_ArrayPlusOffTooSmall = "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
	}
}
