using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XObjectTest
	{
		[Fact]
		public void IdTest()
		{
			var id = XIntegerObject.NewId();
			var obj = XIntegerObject.Create();

			Assert.NotEqual(obj.Id, id);

			obj.Id = id;

			Assert.Equal(obj.Id, id);
		}

		[Fact]
		public void CloneTest()
		{
			var obj = XIntegerObject.Create();
			var actual = obj.Clone();

			Assert.Equal(obj, actual);
			Assert.Equal(obj.GetKey(), actual.GetKey());
		}

		[Fact]
		public void UpdateTest()
		{
			var obj = XIntegerObject.Create();
			var actual = XIntegerObject.Create();

			Assert.NotEqual(obj, actual);

			actual.Update(obj);

			Assert.Equal(obj, actual);
		}

		[Fact]
		public void CreateKeyTest()
		{
			// Setup
			var sut = XIntegerObject.Create();

			// Excercise
			var result = sut.GetKey();

			// Verify
			result.ShouldBe(XKey.From(XKey.IdKeyName, sut.Id));
		}
	}
}
