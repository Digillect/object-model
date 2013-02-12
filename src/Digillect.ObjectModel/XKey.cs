using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Digillect
{
	/// <summary>
	/// Key that is used for unique object identification.
	/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public sealed class XKey : IEquatable<XKey>
	{
		private const string IdKeyName = "id";

		public static readonly XKey Empty = new XKey();

		private readonly KeyValuePair<string, object>[] _keys;
		private readonly int _hashCode = 17;

		#region Constructor
		private XKey()
		{
			_keys = new KeyValuePair<string, object>[0];
		}

		private XKey(KeyValuePair<string, object>[] baseKeys, string name, object value)
		{
#if NET40 && SILVERLIGHT
			int index = -1;

			for ( int i = 0; i < baseKeys.Length; i++ )
			{
				if ( String.Equals(baseKeys[i].Key, name, StringComparison.OrdinalIgnoreCase) )
				{
					index = i;

					break;
				}
			}
#else
			int index = Array.FindIndex(baseKeys, x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
#endif

			if ( index == -1 )
			{
				_keys = new KeyValuePair<string, object>[baseKeys.Length + 1];

				Array.Copy(baseKeys, 0, _keys, 0, baseKeys.Length);
				_keys[_keys.Length - 1] = new KeyValuePair<string, object>(name, value);
				Array.Sort(_keys, (x, y) => String.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				_keys = (KeyValuePair<string, object>[]) _keys.Clone();

				_keys[index] = new KeyValuePair<string, object>(name, value);
			}

			foreach ( var pair in _keys )
			{
				_hashCode = (_hashCode * 37 + pair.Key.GetHashCode()) * 37 + pair.Value.GetHashCode();
			}
		}
		#endregion

		public object this[string name]
		{
			get
			{
#if NET40 && SILVERLIGHT
				return _keys.FirstOrDefault(x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
#else
				return Array.Find(_keys, x => String.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));
#endif
			}
		}

		#region Public Methods
		public XKey WithKey<T>(T keyValue, string keyName = IdKeyName) where T : IEquatable<T>
		{
			if ( keyName == null )
			{
				throw new ArgumentNullException("keyName");
			}

			Contract.Ensures(Contract.Result<XKey>() != null);

			return new XKey(_keys, keyName, keyValue);
		}

		/// <summary>
		/// Checks that keys are equal.
		/// </summary>
		/// <param name="other">The other key.</param>
		/// <returns><c>true</c> if keys are equal, otherwise <c>false</c>.</returns>
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
				if ( other._keys[i].Key != _keys[i].Key || !Object.Equals(other._keys[i].Value, _keys[i].Value) )
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
		/// <returns><c>true</c> if <paramref name="obj"/> is key and keys are equal, otherwise <c>false</c>.</returns>
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

		#region Compare Operators
		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="key1">The key1.</param>
		/// <param name="key2">The key2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator <(XKey key1, XKey key2)
		{
			return Comparer<XKey>.Default.Compare(key1, key2) < 0;
		}

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="key1">The key1.</param>
		/// <param name="key2">The key2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator >(XKey key1, XKey key2)
		{
			return Comparer<XKey>.Default.Compare(key1, key2) > 0;
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
		/// <param name="key">The value.</param>
		/// <param name="keyName">The name of the value.</param>
		/// <returns>Created key.</returns>
		public static XKey From<T>(T key, string keyName = IdKeyName) where T : IEquatable<T>
		{
			Contract.Ensures(Contract.Result<XKey>() != null);

			return Empty.WithKey(key, keyName);
		}

		public static explicit operator XKey(Guid key)
		{
			return From(key);
		}

		public static explicit operator XKey(int key)
		{
			return From(key);
		}

		public static explicit operator XKey(string key)
		{
			return From(key);
		}
		#endregion
	}
}
