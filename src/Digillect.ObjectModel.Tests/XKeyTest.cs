using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	}
}
