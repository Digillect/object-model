using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace Digillect
{
#if !SILVERLIGHT
	/// <summary>
	/// Key that is used for unique object identification.
	/// </summary>
	[Serializable]
#endif
	public abstract class XKey : IComparable<XKey>, IEquatable<XKey>
	{
		/// <summary>
		/// The default "null" key which isn't equal to any other key but itself.
		/// </summary>
		public static readonly XKey Null = new NullKey();

		private readonly XKey parentKey;

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="XKey"/> class.
		/// </summary>
		protected XKey()
		{
			Contract.Ensures( this.parentKey == null );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XKey"/> class.
		/// </summary>
		/// <param name="parentKey">The parent key.</param>
		protected XKey(XKey parentKey)
		{
			Contract.Ensures( this.parentKey == parentKey );

			this.parentKey = parentKey;
		}
		#endregion

		/// <summary>
		/// Gets the parent key.
		/// </summary>
		public XKey Parent
		{
			get { return parentKey; }
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
		public abstract override bool Equals( object obj );
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
			if ( key == null )
			{
				return Null;
			}

			return new SimpleKey<T>(key, parentKey);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="System.Guid"/> to <see cref="Digillect.XKey"/>.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator XKey(Guid key)
		{
			return From(key, null);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="System.Int32"/> to <see cref="Digillect.XKey"/>.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator XKey(int key)
		{
			return From(key, null);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="System.String"/> to <see cref="Digillect.XKey"/>.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator XKey(string key)
		{
			return From(key, null);
		}
		#endregion

		#region class NullKey
#if !SILVERLIGHT
		[Serializable]
#endif
		private sealed class NullKey : XKey
		{
			public override int CompareTo(XKey other)
			{
				if ( other == null )
				{
					return 1;
				}
				else if ( other is NullKey )
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}

			public override bool Equals(XKey other)
			{
				return other is NullKey;
			}

			public override bool Equals(object obj)
			{
				return obj is NullKey;
			}

			public override int GetHashCode()
			{
				return -1;
			}

			public override string ToString()
			{
				return "Null";
			}
		}
		#endregion

		#region class SimpleKey`1
		[DebuggerDisplay("Key = {Key}")]
#if !SILVERLIGHT
		[Serializable]
#endif
		private sealed class SimpleKey<T> : XKey, IComparable<SimpleKey<T>>, IEquatable<SimpleKey<T>>
			where T: IComparable<T>, IEquatable<T>
		{
			private T m_key;

			public SimpleKey(T key, XKey parentKey)
				: base(parentKey)
			{
				if ( key == null )
				{
					throw new ArgumentNullException("key");
				}

				m_key = key;
			}

			public override int CompareTo( XKey other )
			{
				if ( other == null || other is NullKey )
				{
					return 1;
				}

				SimpleKey<T> otherKey = other as SimpleKey<T>;

				return CompareTo(otherKey);
			}

			public int CompareTo(SimpleKey<T> other)
			{
				if ( other == null )
				{
					//throw new ArgumentException("Invalid key type.", "other");
					return 1;
				}

				if ( parentKey == null || other.parentKey == null )
				{
					if ( parentKey != null && other.parentKey == null )
						return 1;
					else if ( parentKey == null && other.parentKey != null )
						return -1;
				}

				int result = parentKey.CompareTo(other.parentKey);

				if ( result == 0 )
				{
					result = m_key.CompareTo(other.m_key);
				}

				return result;
			}

			public override bool Equals( XKey other )
			{
				return Equals(other as SimpleKey<T>);
			}

			public override bool Equals( object obj )
			{
				if ( obj == null || obj.GetType() != GetType() )
				{
					return false;
				}

				return Equals(obj as SimpleKey<T>);
			}

			public bool Equals(SimpleKey<T> other)
			{
				if ( other == null || !m_key.Equals(other.m_key) )
				{
					return false;
				}

				if ( parentKey == null || other.parentKey == null )
				{
					if ( parentKey != null && other.parentKey == null )
						return false;
					else if ( parentKey == null && other.parentKey != null )
						return false;
				}

				return parentKey.Equals(other.parentKey);
			}

			public override int GetHashCode()
			{
				if ( parentKey != null )
					return parentKey.GetHashCode() ^ m_key.GetHashCode();

				return m_key.GetHashCode();
			}

			public override string ToString()
			{
				if ( parentKey != null )
					return parentKey.ToString() + "+" + m_key.ToString();

				return m_key.ToString();
			}
		}
		#endregion
	}
}
