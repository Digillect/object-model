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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect
{
	/// <summary>
	///     Parameters that are used for queries.
	/// </summary>
	/// <remarks>
	///     This class is immutable.
	/// </remarks>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	[SuppressMessage( "Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix" )]
	public sealed class XParameters : IEnumerable<KeyValuePair<string, object>>, IEquatable<XParameters>
	{
		[SuppressMessage( "Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes" )]
		public static readonly XParameters Empty = new XParameters();

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private static readonly StringComparer KeyNameComparer = StringComparer.OrdinalIgnoreCase;

		private readonly KeyValuePair<string, object>[] _parameters;
		private readonly int _hashCode = 17;

		#region Constructor
		private XParameters()
		{
			_parameters = new KeyValuePair<string, object>[0];
		}

		private XParameters( IDictionary<string, object> parameters )
		{
			Contract.Requires( parameters != null );

			_parameters = parameters.ToArray();

			Array.Sort( _parameters, ( x, y ) => KeyNameComparer.Compare( x.Key, y.Key ) );

			foreach( var pair in _parameters )
			{
				_hashCode = (_hashCode * 37 + KeyNameComparer.GetHashCode( pair.Key )) * 37 + pair.Value.GetHashCode();
			}
		}

		private XParameters( KeyValuePair<string, object>[] baseParameters, string name, object value )
		{
			Contract.Requires( baseParameters != null );

#if NET40 && SILVERLIGHT
			int index = -1;

			for ( int i = 0; i < baseParameters.Length; i++ )
			{
				if ( KeyNameComparer.Equals(baseParameters[i].Key, name) )
				{
					index = i;

					break;
				}
			}
#else
			var index = Array.FindIndex( baseParameters, x => KeyNameComparer.Equals( x.Key, name ) );
#endif

			if( index == -1 )
			{
				_parameters = new KeyValuePair<string, object>[baseParameters.Length + 1];

				Array.Copy( baseParameters, 0, _parameters, 0, baseParameters.Length );
				_parameters[_parameters.Length - 1] = new KeyValuePair<string, object>( name, value );
				Array.Sort( _parameters, ( x, y ) => KeyNameComparer.Compare( x.Key, y.Key ) );
			}
			else
			{
				_parameters = (KeyValuePair<string, object>[]) _parameters.Clone();

				_parameters[index] = new KeyValuePair<string, object>( name, value );
			}

			foreach( var pair in _parameters )
			{
				_hashCode = (_hashCode * 37 + KeyNameComparer.GetHashCode( pair.Key )) * 37 + pair.Value.GetHashCode();
			}
		}
		#endregion

		#region IEnumerable<KeyValuePair<string,object>> Members
		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>) _parameters).GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _parameters.GetEnumerator();
		}
		#endregion

		#region Equals/GetHashCode
		/// <summary>
		///     Checks that parameters objects are equal.
		/// </summary>
		/// <param name="other">The other parameters object.</param>
		/// <returns>
		///     <c>true</c> if parameters objects are equal; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals( XParameters other )
		{
			if( ReferenceEquals( other, null ) )
			{
				return false;
			}

			if( ReferenceEquals( other, this ) )
			{
				return true;
			}

			if( other._hashCode != _hashCode || other._parameters.Length != _parameters.Length )
			{
				return false;
			}

			for( var i = 0; i < _parameters.Length; i++ )
			{
				if( !KeyNameComparer.Equals( other._parameters[i].Key, _parameters[i].Key ) || !Equals( other._parameters[i].Value, _parameters[i].Value ) )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///     Checks that objects are equal.
		/// </summary>
		/// <param name="obj">The other object.</param>
		/// <returns>
		///     <c>true</c> if <paramref name="obj" /> is parameters objects and both parameters objects are equal; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			if( obj == null || obj.GetType() != GetType() )
			{
				return false;
			}

			return Equals( obj as XParameters );
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return _hashCode;
		}
		#endregion

		#region Public Methods
		/// <summary>
		///     Returns a value for the specified name.
		/// </summary>
		/// <typeparam name="T">The desired type of the value.</typeparam>
		/// <param name="name">The name of the parameter's value.</param>
		/// <returns>Value or default value for the <typeparamref name="T"/> if specified name doesn't exists.</returns>
		[Pure]
		public T GetValue<T>( string name )
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			Contract.EndContractBlock();

#if NET40 && SILVERLIGHT
			var pair = _parameters.FirstOrDefault(x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
#else
			var pair = Array.Find( _parameters, x => KeyNameComparer.Equals( x.Key, name ) );
#endif

			return pair.Key == null ? default(T) : (T) pair.Value;
		}

		/// <summary>
		///     Returns a value for the specified name.
		/// </summary>
		/// <typeparam name="T">The desired type of the value.</typeparam>
		/// <param name="name">The name of the parameter's value.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <returns>Value or <paramref name="defaultValue"/>.</returns>
		[Pure]
		public T GetValue<T>( string name, T defaultValue )
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			Contract.EndContractBlock();

#if NET40 && SILVERLIGHT
			var pair = _parameters.FirstOrDefault(x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
#else
			var pair = Array.Find( _parameters, x => KeyNameComparer.Equals( x.Key, name ) );
#endif

			return pair.Key == null ? defaultValue : (T) pair.Value;
		}

		/// <summary>
		///     Determines whether parameters contains the specified name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>
		///     <c>true</c> if parameters contains the specified name; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		///     If <paramref name="name" /> is <c>null</c>.
		/// </exception>		[Pure]
		public bool Contains( string name )
		{
			if( name == null )
			{
				throw new ArgumentNullException( "name" );
			}

			Contract.EndContractBlock();

#if NET40 && SILVERLIGHT
			var pair = _parameters.FirstOrDefault(x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
#else
			var pair = Array.Find( _parameters, x => KeyNameComparer.Equals( x.Key, name ) );
#endif

			return pair.Key != null;
		}
		/// <summary>
		///     Returns the <see cref="Digillect.XParameters.Builder" /> object which allows mutation in a multistep fashion.
		/// </summary>
		/// <returns>
		///     An instance of the <see cref="Digillect.XParameters.Builder" /> class.
		/// </returns>
		[Pure]
		public Builder ToBuilder()
		{
			return new Builder( this );
		}

		/// <summary>
		///     Returns a new instance of this object with a new value being added/changed.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="name">Name of the value.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     Thrown if the <paramref name="name" /> is <c>null</c>.
		/// </exception>
		[Pure]
		public XParameters WithValue<T>( string name, T value )
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

			return new XParameters( _parameters, name, value );
		}
		#endregion

		#region Equality Operators
		/// <summary>
		///     Implements the operator ==.
		/// </summary>
		/// <param name="parameters1">The first parameters object to compare.</param>
		/// <param name="parameters2">The second parameters object to compare.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator ==( XParameters parameters1, XParameters parameters2 )
		{
			if( ReferenceEquals( parameters1, parameters2 ) )
			{
				return true;
			}

			if( ReferenceEquals( parameters1, null ) )
			{
				// parameters2 != null since previous comparison
				return false;
			}

			return parameters1.Equals( parameters2 );
		}

		/// <summary>
		///     Implements the operator !=.
		/// </summary>
		/// <param name="parameters1">The first parameters object to compare.</param>
		/// <param name="parameters2">The second parameters object to compare.</param>
		/// <returns>
		///     The result of the operator.
		/// </returns>
		public static bool operator !=( XParameters parameters1, XParameters parameters2 )
		{
			if( ReferenceEquals( parameters1, parameters2 ) )
			{
				return false;
			}

			if( ReferenceEquals( parameters1, null ) )
			{
				// parameters2 != null since previous comparison
				return true;
			}

			return !parameters1.Equals( parameters2 );
		}
		#endregion

		/// <summary>
		///     Creates a parameters object from the specified value.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="name">The name of the value.</param>
		/// <param name="value">The value.</param>
		/// <returns>Created value.</returns>
		public static XParameters Create<T>( string name, T value )
		{
			Contract.Requires( name != null );
			Contract.Requires( value != null );
			Contract.Ensures( Contract.Result<XParameters>() != null );

			return Empty.WithValue( name, value );
		}

		#region class Builder
		/// <summary>
		///     <see cref="Digillect.XParameters" /> builder which allows mutation in a multistep fashion.
		/// </summary>
		[SuppressMessage( "Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible" )]
		public sealed class Builder
		{
			private readonly IDictionary<string, object> _parameters;
			[DebuggerBrowsable( DebuggerBrowsableState.Never )]
			private XParameters _immutable;

			#region Constructors/Disposer
			internal Builder( XParameters immutable )
			{
				Contract.Requires( immutable != null );

				_immutable = immutable;
				_parameters = _immutable._parameters.ToDictionary( x => x.Key, x => x.Value, KeyNameComparer );
			}
			#endregion

			#region Public methods
			public Builder AddValue<T>( string name, T value )
			{
				if( name == null )
				{
					throw new ArgumentNullException( "name" );
				}

				if( value == null )
				{
					throw new ArgumentNullException( "value" );
				}

				Contract.Ensures( _parameters.Count >= Contract.OldValue( _parameters.Count ) );

				_immutable = null;
				_parameters[name] = value;

				return this;
			}

			public Builder ClearValues()
			{
				Contract.Ensures( _parameters.Count == 0 );

				if( _parameters.Count != 0 )
				{
					_immutable = null;
					_parameters.Clear();
				}

				return this;
			}

			public Builder RemoveValue( string name )
			{
				if( name == null )
				{
					throw new ArgumentNullException( "name" );
				}

				Contract.Ensures( _parameters.Count <= Contract.OldValue( _parameters.Count ) );

				if( _parameters.Remove( name ) )
				{
					_immutable = null;
				}

				return this;
			}

			/// <summary>
			///     Returns the <see cref="Digillect.XParameters" /> instance.
			/// </summary>
			/// <returns></returns>
			public XParameters ToImmutable()
			{
				Contract.Ensures( Contract.Result<XParameters>() != null );

				if( _immutable == null )
				{
					_immutable = _parameters.Count == 0 ? Empty : new XParameters( _parameters );
				}

				return _immutable;
			}
			#endregion
		}
		#endregion
	}
}