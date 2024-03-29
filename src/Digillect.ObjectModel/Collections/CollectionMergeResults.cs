﻿#region Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
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

		#region Constructor
		private CollectionMergeResults()
		{
		}

		private CollectionMergeResults(int added, int removed, int updated)
		{
			Added = added;
			Removed = removed;
			Updated = updated;
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets the number of added objects.
		/// </summary>
		public int Added
		{
			get;
		}

		/// <summary>
		/// Gets the number of removed objects.
		/// </summary>
		public int Removed
		{
			get;
		}

		/// <summary>
		/// Gets the number of updated objects.
		/// </summary>
		public int Updated
		{
			get;
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

				return Added == 0 && Removed == 0 && Updated == 0;
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
				throw new ArgumentOutOfRangeException(nameof(added));
			}

			if ( removed < 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(removed));
			}

			if ( updated < 0 )
			{
				throw new ArgumentOutOfRangeException(nameof(updated));
			}

			Contract.Ensures(Contract.Result<CollectionMergeResults>() != null);

			if (added == Added && removed == Removed && updated == Updated)
			{
				return this;
			}

			return new CollectionMergeResults(added, removed, updated);
		}
		#endregion

		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(Added >= 0);
			Contract.Invariant(Removed >= 0);
			Contract.Invariant(Updated >= 0);
		}
	}
}
