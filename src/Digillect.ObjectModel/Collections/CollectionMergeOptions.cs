#region Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

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
