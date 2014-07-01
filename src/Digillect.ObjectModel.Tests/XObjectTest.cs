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

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XObjectTest
	{
		[Fact(DisplayName = "XO.Id.set should correctly assign new identifier")]
		public void IdTest()
		{
			// Setup
			var id = XIntegerObject.NewId();
			var sut = XIntegerObject.Create();

			sut.Id.ShouldNotBe(id);

			// Excercise
			sut.Id = id;

			// Verify
			sut.Id.ShouldBe(id);
		}

		[Fact(DisplayName = "XO.BeginUpdate should block events and raise them after the EndUpdate call")]
		public void BeginEndUpdateEventsTest()
		{
			// Setup
			var eventsBlocked = false;
			var expectedPropertyChanged = false;
			var sut = XIntegerObject.Create();

			sut.PropertyChanged += (sender, e) => {
				Assert.False(eventsBlocked, "PropertyChanged with " + e.PropertyName);
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(null);
				expectedPropertyChanged = true;
			};

			// Exercise
			sut.BeginUpdate();
			eventsBlocked = true;
			sut.Update(XIntegerObject.Create());
			eventsBlocked = false;
			sut.EndUpdate();

			// Verify
			expectedPropertyChanged.ShouldBe(true);
		}


		[Fact(DisplayName = "XO.Clone should result in an equal object")]
		public void CloneTest()
		{
			// Setup
			var sut = XIntegerObject.Create();

			// Excercise
			var result = sut.Clone();

			// Verify
			result.ShouldBe(sut);
			result.GetKey().ShouldBe(sut.GetKey());
		}

		[Fact(DisplayName = "XO.Update should correctly update an object and raise an event")]
		public void UpdateTest()
		{
			// Setup
			var expectedPropertyChanged = false;
			var obj = XIntegerObject.Create();
			var sut = XIntegerObject.Create();

			sut.PropertyChanged += (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(null);
				expectedPropertyChanged = true;
			};

			sut.ShouldNotBe(obj);

			// Excercise
			sut.Update(obj);

			// Verify
			sut.ShouldBe(obj);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XO.GetKey should produce proper key")]
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
