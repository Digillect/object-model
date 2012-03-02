using System;
using System.Runtime.Serialization;

namespace Digillect
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public sealed class XKeyNotAvailableException : Exception
	{
		#region Constructors
		public XKeyNotAvailableException()
		{
		}

		public XKeyNotAvailableException( string message )
			: base(message)
		{
		}

		public XKeyNotAvailableException( string message, Exception innerException )
			: base(message, innerException)
		{
		}

#if !SILVERLIGHT
		private XKeyNotAvailableException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion
	}
}
