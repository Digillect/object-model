﻿using System;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Exception that is thrown when <see cref="Digillect.XObject"/> can not provide valid key.
	/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public sealed class XKeyNotAvailableException : Exception
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="XKeyNotAvailableException"/> class.
		/// </summary>
		public XKeyNotAvailableException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XKeyNotAvailableException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public XKeyNotAvailableException( string message )
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XKeyNotAvailableException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public XKeyNotAvailableException( string message, Exception innerException )
			: base(message, innerException)
		{
		}

#if !(SILVERLIGHT || WINDOWS8)
		private XKeyNotAvailableException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion
	}
}