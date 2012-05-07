using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Xunit.Sdk;

namespace Xunit
{
	public static class CollectionAssert
	{
		public static void SubsetOf( ICollection subset, ICollection superset )
		{
			Dictionary<object, int> counts = new Dictionary<object, int>();
			int nulls = 0;

			foreach( var obj in subset )
			{
				if( obj == null )
				{
					++nulls;
				}
				else
				{
					if( !counts.ContainsKey( obj ) )
						counts.Add( obj, 1 );
					else
						counts[obj] = counts[obj] + 1;
				}
			}

			foreach( var obj in superset )
			{
				if( obj == null )
				{
					--nulls;
				}
				else
				{
					if( counts.ContainsKey( obj ) )
						counts[obj] = counts[obj] - 1;
				}
			}

			foreach( var kv in counts )
			{
				if( kv.Value > 0 )
					throw new SubsetOfException();
			}

			if( nulls > 0 )
				throw new SubsetOfException();
		}

		public static void NotSubsetOf( ICollection subset, ICollection superset )
		{
			try
			{
				SubsetOf( subset, superset );
			}
			catch( SubsetOfException )
			{
				return;
			}

			throw new NotSubsetOfException();
		}
	}
}

namespace Xunit.Sdk
{
	[SuppressMessage( "Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors" )]
	[Serializable]
	public class SubsetOfException : AssertException
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SubsetException"/> class.
		/// </summary>
		/// <param name="expected">The expected object value</param>
		public SubsetOfException()
			: base( "CollectionAssert.SubsetOf() failure: [subset] collection is not subset of [superset] collection." ) { }

		/// <inheritdoc/>
		protected SubsetOfException( SerializationInfo info, StreamingContext context )
			: base( info, context ) { }
	}

	[SuppressMessage( "Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors" )]
	[Serializable]
	public class NotSubsetOfException : AssertException
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SubsetException"/> class.
		/// </summary>
		/// <param name="expected">The expected object value</param>
		public NotSubsetOfException()
			: base( "CollectionAssert.NotSubsetOf() failure: [subset] collection is subset of [superset] collection." ) { }

		/// <inheritdoc/>
		protected NotSubsetOfException( SerializationInfo info, StreamingContext context )
			: base( info, context ) { }
	}
}
