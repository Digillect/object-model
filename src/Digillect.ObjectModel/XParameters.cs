using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Digillect
{
	[DataContract]
	[DebuggerDisplay( "Count: {Count}" )]
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	public class XParameters : IEnumerable<KeyValuePair<string, object>>, IEquatable<XParameters>
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string,object>( StringComparer.OrdinalIgnoreCase );

		#region Properties
		public int Count
		{
			get { return _values.Count; }
		}
		#endregion

		#region From<T>
		[Pure]
		public static XParameters From<T>( string name, T value )
			where T : IEquatable<T>
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}
			
			Contract.Ensures( Contract.Result<XParameters>() != null );

			var parameters = new XParameters();

			parameters.Add( name, value );

			return parameters;
		}
		#endregion
		#region From
		[Pure]
		public static XParameters From( XParameters other )
		{
			if( other == null )
			{
				throw new ArgumentNullException( "other" );
			}

			Contract.Ensures( Contract.Result<XParameters>() != null );

			XParameters parameters = new XParameters();

			foreach( var pair in other._values )
			{
				parameters._values.Add( pair.Key, pair.Value );
			}

			return parameters;
		}
		#endregion

		#region Add<T>
		public XParameters Add<T>( string name, T value )
			where T : IEquatable<T>
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			if( value == null )
			{
				throw new ArgumentNullException( "value" );
			}

			Contract.Ensures( Contract.Result<XParameters>() != null );

			_values.Add( name, value );

			return this;
		}
		#endregion
		#region Get/Get<T>
		[Pure]
		public T Get<T>( string name )
			where T : IEquatable<T>
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			Contract.EndContractBlock();

			if( !_values.ContainsKey( name ) )
			{
				return default( T );
			}

			return (T) _values[name];
		}

		[Pure]
		public T Get<T>( string name, T defaultValue )
			where T : IEquatable<T>
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			Contract.EndContractBlock();

			if( !_values.ContainsKey( name ) )
			{
				return defaultValue;
			}

			return (T) _values[name];
		}
		#endregion

		#region IEnumerable implementation
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _values.GetEnumerator();
		}
		#endregion

		#region Equals
		public bool Equals( XParameters other )
		{
			if( other == null )
			{
				return false;
			}

			if( other._values.Count != _values.Count )
			{
				return false;
			}

			foreach( var pair in _values )
			{
				object otherValue = null;

				if( !other._values.TryGetValue( pair.Key, out otherValue ) )
				{
					return false;
				}

				if( !pair.Value.Equals( otherValue ) )
				{
					return false;
				}
			}

			return true;
		}

		public override bool Equals( object obj )
		{
			if( obj == null || obj.GetType() != GetType() )
			{
				return false;
			}

			return Equals( (XParameters) obj );
		}
		#endregion

		public override int GetHashCode()
		{
			int hashCode = 17;

			foreach( var pair in _values )
			{
				hashCode = hashCode * 37 + pair.Key.GetHashCode();
				hashCode = hashCode * 37 + pair.Value.GetHashCode();
			}

			return hashCode;
		}
	}
}
