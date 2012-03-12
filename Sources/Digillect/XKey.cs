using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Digillect
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public abstract class XKey : IComparable<XKey>, IEquatable<XKey>
	{
		/// <summary>
		/// The default "null" key which isn't equal to any other key but itself.
		/// </summary>
		public static readonly XKey Null = new NullKey();

		private XKey m_parentKey;

		#region Constructor
		protected XKey()
		{
		}

		protected XKey(XKey parentKey)
		{
			m_parentKey = parentKey;
		}
		#endregion

		public XKey Parent
		{
			get { return m_parentKey; }
		}

		public abstract int CompareTo(XKey other);
		public abstract bool Equals(XKey other);
		public abstract override bool Equals(object obj);
		public abstract override int GetHashCode();

		#region Compare Operators
		public static bool operator <(XKey key1, XKey key2)
		{
			return Comparer<XKey>.Default.Compare(key1, key2) < 0;
		}

		public static bool operator >(XKey key1, XKey key2)
		{
			return Comparer<XKey>.Default.Compare(key1, key2) > 0;
		}
		#endregion

		#region Equality Operators
		public static bool operator ==(XKey key1, XKey key2)
		{
			return EqualityComparer<XKey>.Default.Equals(key1, key2);
		}

		public static bool operator !=(XKey key1, XKey key2)
		{
			return !EqualityComparer<XKey>.Default.Equals(key1, key2);
		}
		#endregion

		#region Cast Operators
		public static XKey From<T>(T key, XKey parentKey)
			where T : IComparable<T>, IEquatable<T>
		{
			if ( key == null )
			{
				return Null;
			}

			return new SimpleKey<T>(key, parentKey);
		}

		public static explicit operator XKey(Guid key)
		{
			return From(key, null);
		}

		public static explicit operator XKey(int key)
		{
			return From(key, null);
		}

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

				if ( m_parentKey == null || other.m_parentKey == null )
				{
					if ( m_parentKey != null && other.m_parentKey == null )
						return 1;
					else if ( m_parentKey == null && other.m_parentKey != null )
						return -1;
				}

				int result = m_parentKey.CompareTo(other.m_parentKey);

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

				if ( m_parentKey == null || other.m_parentKey == null )
				{
					if ( m_parentKey != null && other.m_parentKey == null )
						return false;
					else if ( m_parentKey == null && other.m_parentKey != null )
						return false;
				}

				return m_parentKey.Equals(other.m_parentKey);
			}

			public override int GetHashCode()
			{
				if ( m_parentKey != null )
					return m_parentKey.GetHashCode() ^ m_key.GetHashCode();

				return m_key.GetHashCode();
			}

			public override string ToString()
			{
				if ( m_parentKey != null )
					return m_parentKey.ToString() + "+" + m_key.ToString();

				return m_key.ToString();
			}
		}
		#endregion
	}
}
