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
	public class XKeyTest
	{
		[Fact(DisplayName = "XKey: building key in various fashion should produce equal keys")]
		public void EqualityTest()
		{
			// Setup
			var sut = XKey.Empty.WithKey("string", "string").WithKey("int", 999);

			// Exercise
			var result = XKey.Empty.ToBuilder().AddKey("int", 999).AddKey("string", "string").ToImmutable();

			// Verify
			result.ShouldBe(sut);
		}

		[Fact(DisplayName = "XKey: clearing all keys from empty builder should produce empty key")]
		[Trait("Category", "Immutability")]
		public void XKeyBuilderClearEmptyTest()
		{
			// Setup
			var sut = XKey.Empty;

			// Exercise
			var result = sut.ToBuilder().ClearKeys().ToImmutable();

			// Verify
			result.ShouldBeSameAs(sut);
		}

		[Fact(DisplayName = "XKey: clearing all keys from non-empty builder should produce empty key")]
		[Trait("Category", "Immutability")]
		public void XKeyBuilderClearNonEmptyTest()
		{
			// Setup
			var sut = XKey.From(XKey.IdKeyName, XIntegerObject.NewId());

			// Exercise
			var result = sut.ToBuilder().AddKey("name", String.Empty).ClearKeys().ToImmutable();

			// Verify
			sut.ShouldNotBe(XKey.Empty);
			result.ShouldBe(XKey.Empty);
		}

		[Fact(DisplayName = "XKey: serializing key and deserializing it back should produce equal key")]
		[Trait("Category", "Serialization")]
		public void XKeySerializerTest()
		{
			// Setup
			var sut = XKey.From( XKey.IdKeyName, XIntegerObject.NewId() );

			// Exercise
			var keyString = XKeySerializer.Serialize( sut );
			var result = XKeySerializer.Deserialize( keyString );

			// Verify
			sut.ShouldNotBe( XKey.Empty );
			result.ShouldBe( sut );
		}
	}
}
