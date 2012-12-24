using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Shouldly;

using Digillect;
using Digillect.Collections;

namespace Digillect.Tests
{
	public class XCacheTest
	{
		[Fact]
		public void cache_should_return_object_by_id()
		{
			var cache = new XCache<XIntegerObject>();
			var original = XIntegerObject.Create();
			
			cache.Cache( original );

			var retrieved = cache.Get( original.GetKey() );

			retrieved.ShouldBe( original );
		}

		[Fact]
		public void cache_should_return_cached_object()
		{
			var cache = new XCache<XIntegerObject>();
			var original = XIntegerObject.Create();
			var second = original.Clone();

			cache.Cache( original );

			var cached = cache.Cache( second );

			cached.ShouldBeSameAs( original );
			cached.ShouldNotBeSameAs( second );
		}

		[Fact]
		public void cache_should_return_equal_collection_for_the_same_query()
		{
			var cache = new XCache<XIntegerObject>();
			var query = new XQuery<XIntegerObject>();
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 3 ) );

			cache.Cache( query, original, this );

			var cached = cache.Get( query );

			cached.ShouldBe( original );
		}

		[Fact]
		public void cache_should_return_equal_collection_for_the_equal_query()
		{
			var cache = new XCache<XIntegerObject>();
			var query = new XQuery<XIntegerObject>( "hello" );
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 3 ) );

			var response = cache.Cache( query, original, this );

			var cached = cache.Get( new XQuery<XIntegerObject>( "hello" ) );

			cached.ShouldBe( response );
			cached.ShouldBe( original );
		}

		[Fact]
		public void cache_should_convert_query_results()
		{
			var cache = new XCache<XIntegerObject>();
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 100 ) );
			var queryForAll = new IntegerQuery( "all" );
			var queryForOdd = new IntegerQuery( "odd" );
			var queryForEven = new IntegerQuery( "even" );

			queryForAll.CanConvertFunction = q => q is IntegerQuery;
			queryForOdd.MatchFunction = o => o.Id % 2 != 0;
			queryForEven.MatchFunction = o => o.Id % 2 == 0;

			cache.Cache( queryForAll, original, this );

			var odd = cache.Get( queryForOdd, this );
			var even = cache.Get( queryForEven, this );

			odd.ShouldNotBe( null );
			even.ShouldNotBe( null );

			original.Count.ShouldBe( odd.Count + even.Count );

			odd.All( o => o.Id % 2 != 0 ).ShouldBe( true );
			even.All( o => o.Id % 2 == 0 ).ShouldBe( true );
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

			protected override void ProcessClone( XQuery<XIntegerObject> source )
			{
				base.ProcessClone( source );

				var other = (IntegerQuery) source;

				CanConvertFunction = other.CanConvertFunction;
				MatchFunction = other.MatchFunction;
			}
		}
	}
}
