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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XImmutableIdentifiedObjectTest
	{
		[Fact(DisplayName = "XImmutableIdentifiedObject.Clone should correctly clone the identifier")]
		[Trait("Category", "Clone/Update")]
		public void CloneTest()
		{
			// Setup
			var sut = new DummyImmutableObject(XIntegerObject.NewId());

			// Exercise
			var result = (DummyImmutableObject) sut.Clone(true);

			// Verify
			result.ShouldNotBeSameAs(sut);
			result.Id.ShouldBe(sut.Id);
		}

		[Fact(DisplayName = "XImmutableIdentifiedObject should pass data (de)serialization roundtrip")]
		[Trait("Category", "DataContract")]
		public void DataContractTest()
		{
			var serializer = new DataContractSerializer(typeof(DummyImmutableObject));
			var sut = new DummyImmutableObject(XIntegerObject.NewId());

			using ( var s = new MemoryStream() )
			{
				serializer.WriteObject(s, sut);
				s.Position = 0;

				// Excercise
				var result = (DummyImmutableObject) serializer.ReadObject(s);

				// Verify
				result.ShouldNotBeSameAs(sut);
				result.Id.ShouldBe(sut.Id);
			}
		}

		[Fact(DisplayName = "XImmutableIdentifiedObject.WithId should return a copy of the object with the specified identifier")]
		[Trait("Category", "Clone/Update")]
		public void WithIdTest()
		{
			// Setup
			var sut = new DummyImmutableObject(XIntegerObject.NewId());

			// Exercise
			var id = XIntegerObject.NewId();
			var result = sut.WithId(id);

			// Verify
			result.ShouldNotBeSameAs(sut);
			result.Id.ShouldNotBe(sut.Id);
			result.Id.ShouldBe(id);
		}

		[DataContract]
		private sealed class DummyImmutableObject : XImmutableIdentifiedObject<int>
		{
			public DummyImmutableObject(int id)
				: base(id)
			{
			}
		}
	}
}
