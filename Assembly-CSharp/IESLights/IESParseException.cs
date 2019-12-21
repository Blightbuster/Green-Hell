using System;
using System.Runtime.Serialization;

namespace IESLights
{
	[Serializable]
	public class IESParseException : Exception
	{
		public IESParseException()
		{
		}

		public IESParseException(string message) : base(message)
		{
		}

		public IESParseException(string message, Exception inner) : base(message, inner)
		{
		}

		protected IESParseException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
