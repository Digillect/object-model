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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;

namespace Digillect
{
	/// <summary>
	/// Key that is used for unique object identification.
	/// </summary>
	/// <remarks>
	/// This class is immutable.
	/// </remarks>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed class XKey : IEnumerable<KeyValuePair<string, object>>, IEquatable<XKey>
	{
		public const string IdKeyName = "ID";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly XKey Empty = new XKey();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly StringComparer KeyNameComparer = StringComparer.OrdinalIgnoreCase;

		private readonly KeyValuePair<string, object>[] _keys;
		private readonly int _hashCode = 17;

		#region Constructor
		private XKey()
		{
			_keys = new KeyValuePair<string, object>[0];
		}

		private XKey(IDictionary<string, object> keys)
		{
			Contract.Requires(keys != null);

			_keys = keys.ToArray();

			Array.Sort(_keys, (x, y) => KeyNameComparer.Compare(x.Key, y.Key));

			foreach ( var pair in _keys )
			{
				_hashCode = (_hashCode * 37 + KeyNameComparer.GetHashCode(pair.Key)) * 37 + pair.Value.GetHashCode();
			}
		}

		private XKey(KeyValuePair<string, object>[] baseKeys, string name, object value)
		{
			Contract.Requires(baseKeys != null);

#if NET40 && SILVERLIGHT
			int index = -1;

			for ( int i = 0; i < baseKeys.Length; i++ )
			{
				if ( KeyNameComparer.Equals(baseKeys[i].Key, name) )
				{
					index = i;

					break;
				}
			}
#else
			int index = Array.FindIndex(baseKeys, x => KeyNameComparer.Equals(x.Key, name));
#endif

			if ( index == -1 )
			{
				_keys = new KeyValuePair<string, object>[baseKeys.Length + 1];

				Array.Copy(baseKeys, 0, _keys, 0, baseKeys.Length);
				_keys[_keys.Length - 1] = new KeyValuePair<string, object>(name, value);
				Array.Sort(_keys, (x, y) => KeyNameComparer.Compare(x.Key, y.Key));
			}
			else
			{
				_keys = (KeyValuePair<string, object>[]) _keys.Clone();

				_keys[index] = new KeyValuePair<string, object>(name, value);
			}

			foreach ( var pair in _keys )
			{
				_hashCode = (_hashCode * 37 + KeyNameComparer.GetHashCode(pair.Key)) * 37 + pair.Value.GetHashCode();
			}
		}
		#endregion

		#region IEnumerable<KeyValuePair<string,object>> Members
		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>) _keys).GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _keys.GetEnumerator();
		}
		#endregion

		#region Equals/GetHashCode
		/// <summary>
		/// Checks that keys are equal.
		/// </summary>
		/// <param name="other">The other key.</param>
		/// <returns><c>true</c> if keys are equal; otherwise, <c>false</c>.</returns>
		public bool Equals(XKey other)
		{
			if ( Object.ReferenceEquals(other, null) )
			{
				return false;
			}

			if ( Object.ReferenceEquals(other, this) )
			{
				return true;
			}

			if ( other._hashCode != _hashCode || other._keys.Length != _keys.Length )
			{
				return false;
			}

			for ( int i = 0; i < _keys.Length; i++ )
			{
				if ( !KeyNameComparer.Equals(other._keys[i].Key, _keys[i].Key) || !Object.Equals(other._keys[i].Value, _keys[i].Value) )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks that objects are equal.
		/// </summary>
		/// <param name="obj">The other object.</param>
		/// <returns><c>true</c> if <paramref name="obj"/> is key and keys are equal; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if ( obj == null || obj.GetType() != GetType() )
			{
				return false;
			}

			return Equals(obj as XKey);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return _hashCode;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns a part of the key with the specified name.
		/// </summary>
		/// <typeparam name="T">The desired type of the value.</typeparam>
		/// <param name="name">The name of the key's value.</param>
		/// <returns></returns>
		[Pure]
		public T GetValue<T>(string name)
		{
#if NET40 && SILVERLIGHT
			return (T) _keys.FirstOrDefault(x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
#else
			return (T) Array.Find(_keys, x => KeyNameComparer.Equals(x.Key, name)).Value;
#endif
		}

		/// <summary>
		/// Returns the <see cref="Builder"/> object which allows mutation in a multistep fashion.
		/// </summary>
		/// <returns>An instance of the <see cref="Builder"/> class.</returns>
		[Pure]
		public Builder ToBuilder()
		{
			return new Builder(this);
		}

		/// <summary>
		/// Returns a new instance of this object with a new key part being added/changed.
		/// </summary>
		/// <typeparam name="T">The type of the key value.</typeparam>
		/// <param name="keyName">Name of the key.</param>
		/// <param name="keyValue">The key value.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="keyName"/> is <c>null</c>.</exception>
		[Pure]
		public XKey WithKey<T>(string keyName, T keyValue) where T : IEquatable<T>
		{
			if ( keyName == null )
			{
				throw new ArgumentNullException("keyName");
			}

			if ( keyValue == null )
			{
				throw new ArgumentNullException("keyValue");
			}

			Contract.Ensures(Contract.Result<XKey>() != null);

			return new XKey(_keys, keyName, keyValue);
		}
		#endregion

		#region Equality Operators
		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="key1">The first key to compare.</param>
		/// <param name="key2">The second key to compare.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(XKey key1, XKey key2)
		{
			if ( Object.ReferenceEquals(key1, key2) )
			{
				return true;
			}

			if ( Object.ReferenceEquals(key1, null) )
			{
				// key2 != null since previous comparison
				return false;
			}

			return key1.Equals(key2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="key1">The first key to compare.</param>
		/// <param name="key2">The second key to compare.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(XKey key1, XKey key2)
		{
			if ( Object.ReferenceEquals(key1, key2) )
			{
				return false;
			}

			if ( Object.ReferenceEquals(key1, null) )
			{
				// key2 != null since previous comparison
				return true;
			}

			return !key1.Equals(key2);
		}
		#endregion

		#region Cast Operators
		/// <summary>
		/// Creates a key from the specified value.
		/// </summary>
		/// <typeparam name="T">Type of key value.</typeparam>
		/// <param name="keyName">The name of the value.</param>
		/// <param name="key">The value.</param>
		/// <returns>Created key.</returns>
		public static XKey From<T>(string keyName, T key) where T : IEquatable<T>
		{
			Contract.Requires(keyName != null);
			Contract.Requires(key != null);
			Contract.Ensures(Contract.Result<XKey>() != null);

			return Empty.WithKey(keyName, key);
		}

		public static explicit operator XKey(Guid key)
		{
			return From(IdKeyName, key);
		}

		public static explicit operator XKey(int key)
		{
			return From(IdKeyName, key);
		}

		public static explicit operator XKey(string key)
		{
			Contract.Requires(key != null);

			return From(IdKeyName, key);
		}
		#endregion

		#region class Builder
		/// <summary>
		/// <see cref="XKey"/> builder which allows mutation in a multistep fashion.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public sealed class Builder
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private XKey _immutable;
			private readonly IDictionary<string, object> _keys;

			internal Builder(XKey immutable)
			{
				Contract.Requires(immutable != null);

				_immutable = immutable;
				_keys = _immutable._keys.ToDictionary(x => x.Key, x => x.Value, KeyNameComparer);
			}

			public Builder AddKey<T>(string name, T value) where T : IEquatable<T>
			{
				if ( name == null )
				{
					throw new ArgumentNullException("name");
				}

				if ( value == null )
				{
					throw new ArgumentNullException("value");
				}

				Contract.Ensures(_keys.Count >= Contract.OldValue(_keys.Count));

				_immutable = null;
				_keys[name] = value;

				return this;
			}

			public Builder ClearKeys()
			{
				Contract.Ensures(_keys.Count == 0);

				if ( _keys.Count != 0 )
				{
					_immutable = null;
					_keys.Clear();
				}

				return this;
			}

			public Builder RemoveKey(string name)
			{
				if ( name == null )
				{
					throw new ArgumentNullException("name");
				}

				Contract.Ensures(_keys.Count <= Contract.OldValue(_keys.Count));

				if ( _keys.Remove(name) )
				{
					_immutable = null;
				}

				return this;
			}

			/// <summary>
			/// Returns the <see cref="XKey"/> instance.
			/// </summary>
			/// <returns></returns>
			public XKey ToImmutable()
			{
				Contract.Ensures(Contract.Result<XKey>() != null);

				if ( _immutable == null )
				{
					_immutable = _keys.Count == 0 ? Empty : new XKey(_keys);
				}

				return _immutable;
			}
		}
		#endregion
	}
}
