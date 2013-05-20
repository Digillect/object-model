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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Digillect.Collections;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XSortedCollectionTest
	{
		private const int Many = 3;

		[Fact(DisplayName = "XSC: All items should be sorted according to a comparator function")]
		public void BasicSortTest()
		{
			// Setup
			var sut = CreateTestCollection(Many);

			// Exercise

			// Verify
			XIntegerObject value = sut[0];

			foreach ( var item in sut.Skip(1) )
			{
				IdComparison(item, value).ShouldBeGreaterThanOrEqualTo(0);

				value = item;
			}
		}

		[Fact(DisplayName = "XSC.BeginUpdate should block events and raise them after the EndUpdate call")]
		public void BeginEndUpdateEventsTest()
		{
			// Setup
			bool eventsBlocked = false;
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(Many, (sender, e) => {
				Assert.False(eventsBlocked, "CollectionChanged with " + e.Action);
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				Assert.False(eventsBlocked, "PropertyChanged with " + e.PropertyName);
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(null);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.BeginUpdate();
			eventsBlocked = true;
			sut.BaseCollection.Update(CreateSourceCollection(Many));
			sut.BaseCollection.Insert(0, XIntegerObject.Create());
			sut.BaseCollection.RemoveAt(0);
			eventsBlocked = false;
			sut.EndUpdate();

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSC.Base.Add should raise appropriate events")]
		public void InsertItemTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(Many, (sender, e) => {
				expectedCollectionChanged = true;
				e.Action.ShouldBe(NotifyCollectionChangedAction.Add);
			}, (sender, e) => {
				expectedPropertyChanged++;
			});

			// Exercise
			var item = sut[0].Clone();
			item.Id -= 1;
			sut.BaseCollection.Add(item);

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(2);
			sut.IndexOf(item).ShouldBe(0);
		}

		[Fact(DisplayName = "XSC.Base.Remove should raise appropriate events")]
		public void RemoveItemTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(Many, (sender, e) => {
				expectedCollectionChanged = true;
				e.Action.ShouldBe(NotifyCollectionChangedAction.Remove);
			}, (sender, e) => {
				expectedPropertyChanged++;
			});

			// Exercise
			sut.BaseCollection.RemoveAt(0);

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(2);
		}

		[Fact(DisplayName = "XSC.Base.Move should not produce any visible changes")]
		public void MoveItemTest()
		{
			// Setup
			var sut = CreateTestCollection(Many, (sender, e) => {
				Assert.False(true, "CollectionChanged: " + e.Action);
			}, (sender, e) => {
				Assert.False(true, "PropertyChanged: " + e.PropertyName);
			});
			var snaphot = sut.ToArray();

			// Exercise
			((XCollection<XIntegerObject>) sut.BaseCollection).Move(0, Many - 1);
			((XCollection<XIntegerObject>) sut.BaseCollection).Move(0, Many - 1);

			// Verify
			sut.SequenceEqual(snaphot).ShouldBe(true);
		}

		[Fact(DisplayName = "XSC: Collection should raise events when an item's property changes which affects comparator")]
		public void ItemPropertyChangeTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(Many, (sender, e) => {
				expectedCollectionChanged = true;
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
			}, (sender, e) => {
				expectedPropertyChanged = true;
				e.PropertyName.ShouldBe("Item[]");
			});
			var item = sut[0];

			// Exercise (move from the start to the end)
			item.Id = sut[Many - 1].Id + 10;

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
			sut.IndexOf(item).ShouldBe(Many - 1);

			// Setup 2
			expectedCollectionChanged = false;
			expectedPropertyChanged = false;

			// Exercise 2 (move from the end to the start)
			item.Id = sut[0].Id - 10;

			// Verify 2
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
			sut.IndexOf(item).ShouldBe(0);

			// Setup 3
			expectedCollectionChanged = false;
			expectedPropertyChanged = false;
			item = sut[Many - 1];

			// Exercise 3 (move into the middle)
			item.Id = sut[1].Id - 1;

			// Verify 3
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
			sut.IndexOf(item).ShouldBe(1);
		}

		[Fact(DisplayName = "XSC.Clone should produce an equivalent collection")]
		public void CloneTest()
		{
			// Setup
			var sut = CreateTestCollection(Many);

			// Exercise
			var result = sut.Clone(false);

			// Verify
			result.SequenceEqual(sut).ShouldBe(true);

			// Exercise 2
			result = sut.Clone(true);

			// Verify 2
			result.SequenceEqual(sut).ShouldBe(true);
		}

		#region Setup
		private static XCollection<XIntegerObject> CreateSourceCollection(int count)
		{
			return new XCollection<XIntegerObject>(XIntegerObject.CreateSeries(count).OrderByDescending(x => x.Id));
		}

		private static XSortedCollection<XIntegerObject> CreateTestCollection(int sourceCount, NotifyCollectionChangedEventHandler changedHandler = null, PropertyChangedEventHandler propertyChangedHandler = null)
		{
			var result = new XSortedCollection<XIntegerObject>(CreateSourceCollection(sourceCount), IdComparison);

			if ( changedHandler != null )
			{
				result.CollectionChanged += changedHandler;
			}

			if ( propertyChangedHandler != null )
			{
				result.PropertyChanged += propertyChangedHandler;
			}

			return result;
		}

		private static int IdComparison<TId>(IXIdentified<TId> obj1, IXIdentified<TId> obj2) where TId : IComparable<TId>
		{
			/*
			if ( obj1 == null || obj2 == null )
			{
				if ( obj1 == null )
				{
					return obj2 == null ? 0 : -1;
				}
				else if ( obj2 == null )
				{
					return 1;
				}
			}
			*/

			return obj1.Id.CompareTo(obj2.Id);
		}
		#endregion
	}
}
