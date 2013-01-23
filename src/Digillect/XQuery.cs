using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// ������� ����� ��� ��������, ������������ ��������� ��������.
	/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	[DataContract]
	public class XQuery<T> : IEquatable<XQuery<T>>
	{
		private string _method;
		private XParameters _parameters;

		#region Constructors/Disposer
		public XQuery()
			: this( null, null )
		{
		}

		public XQuery( string method )
			: this( method, null )
		{
		}

		public XQuery( XParameters parameters )
			: this( null, parameters )
		{
		}

		public XQuery( string method, XParameters parameters )
		{
			_method = method;
			_parameters = parameters == null ? new XParameters() : XParameters.From( parameters );
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets query method.
		/// </summary>
		public string Method
		{
			get { return _method; }
		}

		/// <summary>
		/// Gets query parameters.
		/// </summary>
		public XParameters Parameters
		{
			get { return _parameters; }
		}

		/// <summary>
		/// ���������� <c>true</c>, ���� ������ ������ ������������ ����� <see cref="Match"/>.
		/// </summary>
		/// <value>
		/// <c>false</c>.
		/// </value>
		public virtual bool SupportsMatch
		{
			get { return false; }
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// ������ ����� ��������� ������������ ���������� ������� ������� �������.
		/// </summary>
		/// <param name="value">������ ��� ��������.</param>
		/// <returns><see langword="true"/> � ������ ������������, ����� <see langword="false"/>.</returns>
		[Pure]
		public virtual bool Match( T value )
		{
			return false;
		}

		/// <summary>
		/// ������ ����� ������ ������� <c>true</c>, ���� �� ����������� �������� �������
		/// ����� ���������� ������ <see cref="Match"/> ����� �������� ���������� ���������� �������.
		/// </summary>
		/// <param name="query">������ ��� ���������</param>
		/// <returns><see langword="true"/> ���� ���������� ������� ����� �������������, ����� <see langword="false"/>.</returns>
		/// <remarks>
		/// ������ ���������� ���������� ����� <see cref="Equals"/> ��� ����������� ����������� ��������������.
		/// </remarks>
		[Pure]
		public virtual bool CanConvertTo( XQuery<T> query )
		{
			return Equals( query );
		}
		#endregion

		#region Clone
		[Pure]
		public virtual XQuery<T> Clone()
		{
			Contract.Ensures( Contract.Result<XQuery<T>>() != null );

			var clone = CreateInstanceOfSameType();

			clone.ProcessClone( this );

			return clone;
		}

		[Pure]
		protected virtual XQuery<T> CreateInstanceOfSameType()
		{
			Contract.Ensures(Contract.Result<XQuery<T>>() != null);

#if WINDOWS8
			return (XQuery<T>) Activator.CreateInstance(GetType());
#else
			return (XQuery<T>) Activator.CreateInstance( GetType(), true );
#endif
		}

		protected virtual void ProcessClone( XQuery<T> source )
		{
			if( source == null )
			{
				throw new ArgumentNullException( "source" );
			}

			Contract.EndContractBlock();

			_method = source._method;
			_parameters = XParameters.From( source._parameters );
		}
		#endregion

		#region Object Overrides
		bool IEquatable<XQuery<T>>.Equals( XQuery<T> otherQuery )
		{
			if( otherQuery == null )
			{
				return false;
			}

			return object.Equals( _method, otherQuery._method ) && _parameters.Equals( otherQuery._parameters );
		}

		public override bool Equals( object obj )
		{
			if( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			return ((IEquatable<XQuery<T>>) this).Equals( (XQuery<T>) obj );
		}

		public override int GetHashCode()
		{
			int hashCode = 17;

			if( _method != null )
			{
				hashCode = hashCode * 37 + _method.GetHashCode();
			}

			hashCode = hashCode * 37 + _parameters.GetHashCode();

			return hashCode;
		}
		#endregion
	}
}
