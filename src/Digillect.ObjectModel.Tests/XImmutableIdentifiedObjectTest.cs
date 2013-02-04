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
