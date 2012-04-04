﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace Digillect
{
	/// <summary>
	/// Key that is used for unique object identification.
	/// </summary>
#if !(SILVERLIGHT || NETFX_CORE)
	[Serializable]
#endif
	public abstract class XKey : IComparable<XKey>, IEquatable<XKey>
	{
		private readonly XKey _parentKey;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XKey"/> class.
		/// </summary>
		protected XKey()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XKey"/> class.
		/// </summary>
		/// <param name="parentKey">The parent key.</param>
		protected XKey(XKey parentKey)
		{
			this._parentKey = parentKey;
		}
		#endregion

		/// <summary>
		/// Gets the parent key.
		/// </summary>
		public XKey ParentKey
		{
			get { return _parentKey; }
		}

		/// <summary>
		/// Compares key to other key.
		/// </summary>
		/// <param name="other">The other key.</param>
		/// <returns>Negative value if this key is less then other key, positive value if other key is less then this one or zero if keys are equal.</returns>
		public abstract int CompareTo(XKey other);

		/// <summary>
		/// Checks that keys are equal.
		/// </summary>
		/// <param name="other">The other key.</param>
		/// <returns><c>true</c> if keys are equal, otherwise <c>false</c>.</returns>
		public abstract bool Equals(XKey other);

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
		public abstract override int GetHashCode();

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
		/// <param name="key1">The key1.</param>
		/// <param name="key2">The key2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(XKey key1, XKey key2)
		{
			return EqualityComparer<XKey>.Default.Equals(key1, key2);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="key1">The key1.</param>
		/// <param name="key2">The key2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(XKey key1, XKey key2)
		{
			return !EqualityComparer<XKey>.Default.Equals(key1, key2);
		}
		#endregion

		#region Cast Operators
		/// <summary>
		/// Creates key froms the specified type and parent key.
		/// </summary>
		/// <typeparam name="T">Type of key value.</typeparam>
		/// <param name="key">The value.</param>
		/// <param name="parentKey">The parent key.</param>
		/// <returns>Created key.</returns>
		public static XKey From<T>(T key, XKey parentKey)
			where T : IComparable<T>, IEquatable<T>
		{
			Contract.Ensures(Contract.Result<XKey>() != null);

			return new SimpleKey<T>(key, parentKey);
		}
		#endregion

		#region class SimpleKey`1
		[DebuggerDisplay("Key = {m_key}")]
#if !(SILVERLIGHT || NETFX_CORE)
		[Serializable]
#endif
		private sealed class SimpleKey<T> : XKey, IComparable<SimpleKey<T>>, IEquatable<SimpleKey<T>>
			where T: IComparable<T>, IEquatable<T>
		{
			private T _key;

			public SimpleKey(T key, XKey parentKey)
				: base(parentKey)
			{
				this._key = key;
			}

			public int CompareTo(SimpleKey<T> other)
			{
				if ( other == null )
				{
					return 1;
				}

				int result = Comparer<XKey>.Default.Compare(this._parentKey, other._parentKey);

				if ( result == 0 )
				{
					result = Comparer<T>.Default.Compare(this._key, other._key);
				}

				return result;
			}

			public override int CompareTo(XKey other)
			{
				if ( other == null )
				{
					return 1;
				}

				return CompareTo(other as SimpleKey<T>);
			}

			public bool Equals(SimpleKey<T> other)
			{
				if ( other == null )
				{
					return false;
				}

				return EqualityComparer<T>.Default.Equals(this._key, other._key) && this._parentKey == other._parentKey;
			}

			public override bool Equals(XKey other)
			{
				return Equals(other as SimpleKey<T>);
			}

			public override int GetHashCode()
			{
				if ( this._parentKey != null )
					return this._parentKey.GetHashCode() ^ this._key.GetHashCode();

				return this._key.GetHashCode();
			}

			public override string ToString()
			{
				if ( this._parentKey != null )
					return this._parentKey.ToString() + "+" + this._key.ToString();

				return this._key.ToString();
			}
		}
		#endregion
	}
}
