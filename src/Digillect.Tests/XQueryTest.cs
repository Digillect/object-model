using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Shouldly;

using Digillect;
using Digillect.Collections;

namespace Digillect.Tests
{
	public class XQueryTest
	{
		[Fact]
		public void query_should_clone()
		{
			var original = new IntegerQuery( "Entity", XParameters.From( "Id", 1 ) );
			var clone = original.Clone();

			clone.ShouldNotBeSameAs( original );
			clone.ShouldBe( original );
		}

		class IntegerQuery : XQuery<XIntegerObject>
		{
			#region Constructors/Disposer
			public IntegerQuery()
			{
			}

			public IntegerQuery( string method )
				: base( method )
			{
			}

			public IntegerQuery( XParameters parameters )
				: base( parameters )
			{
			}

			public IntegerQuery( string method, XParameters parameters )
				: base( method, parameters )
			{
			}
			#endregion

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
