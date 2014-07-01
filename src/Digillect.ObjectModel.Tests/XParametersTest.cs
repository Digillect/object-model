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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Digillect.Runtime.Serialization;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XParametersTest
	{
		[Fact(DisplayName = "XParameters: building key in various fashion should produce equal parameters objects")]
		public void EqualityTest()
		{
			// Setup
			var sut = XParameters.Create( "string", "string" ).WithValue( "int", 999 );

			// Exercise
			var result = XParameters.Empty.ToBuilder().AddValue("int", 999).AddValue("string", "string").ToImmutable();

			// Verify
			result.ShouldBe(sut);
		}

		[Fact( DisplayName = "XParameters: clearing all values from empty builder should produce empty parameters object" )]
		[Trait("Category", "Immutability")]
		public void XParametersBuilderClearEmptyTest()
		{
			// Setup
			var sut = XParameters.Empty;

			// Exercise
			var result = sut.ToBuilder().ClearValues().ToImmutable();

			// Verify
			result.ShouldBeSameAs(sut);
		}

		[Fact( DisplayName = "XParameters: clearing all values from non-empty builder should produce empty parameters object" )]
		[Trait("Category", "Immutability")]
		public void XParametersBuilderClearNonEmptyTest()
		{
			// Setup
			var sut = XParameters.Create("test", XIntegerObject.NewId());

			// Exercise
			var result = sut.ToBuilder().AddValue("name", String.Empty).ClearValues().ToImmutable();

			// Verify
			sut.ShouldNotBe(XParameters.Empty);
			result.ShouldBe(XParameters.Empty);
		}

		[Fact( DisplayName = "XParameters: serializing parameters object and deserializing it back should produce equal parameters objects" )]
		[Trait("Category", "Serialization")]
		public void XKeySerializerTest()
		{
			// Setup
			var sut = XParameters.Create( "test", XIntegerObject.NewId() );

			// Exercise
			var parametersString = XParametersSerializer.Serialize( sut );
			var result = XParametersSerializer.Deserialize( parametersString );

			// Verify
			sut.ShouldNotBe( XParameters.Empty );
			result.ShouldBe( sut );
		}
	}
}
