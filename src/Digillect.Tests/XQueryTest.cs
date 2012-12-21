using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using Digillect;
using Digillect.Collections;

namespace Digillect.Tests
{
	public class XQueryTest
	{
		[Fact]
		public void query_should_clone()
		{
			var original = new IntegerQuery( IntegerQuery.All, XParameters.From( "Id", 1 ) );
			var clone = original.Clone();

			Assert.NotSame( original, clone );
			Assert.Equal( original, clone );
		}

		class IntegerQuery : XQuery<XIntegerObject>
		{
			public IntegerQuery( string method = None, XParameters parameters = null )
				: base( method, parameters )
			{
			}

			protected IntegerQuery()
			{
			}

			public Func<XQuery<XIntegerObject>, bool> CanConvertFunction { get; set; }
			public Func<XIntegerObject, bool> MatchFunction { get; set; }

			public override bool CanConvertTo( XQuery<XIntegerObject> query )
			{
				if( CanConvertFunction != null )
				{
					return CanConvertFunction( query );
				}
				else
				{
					return false;
				}
			}

			public override bool SupportsMatch
			{
				get { return MatchFunction != null; }
			}

			public override bool Match( XIntegerObject value )
			{
				if( MatchFunction != null )
				{
					return MatchFunction( value );
				}
				else
				{
					return false;
				}
			}
		}
	}
}
