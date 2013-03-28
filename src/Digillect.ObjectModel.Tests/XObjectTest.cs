#region Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
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
