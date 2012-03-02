using System;
using System.Collections.Generic;
using System.Linq;

namespace Digillect.Runtime.CompilerServices
{
	[AttributeUsage( AttributeTargets.Property )]
	public sealed class AutoNotifyAdditionalPropertiesAttribute : Attribute
	{
		public ICollection<string> AdditionalProperties { get; private set; }

		#region Constructor
		public AutoNotifyAdditionalPropertiesAttribute(string property)
			: this(new[] { property })
		{
		}
		
		public AutoNotifyAdditionalPropertiesAttribute(string property1, string property2)
			: this(new[] { property1, property2 })
		{
		}

		public AutoNotifyAdditionalPropertiesAttribute(string property1, string property2, string property3)
			: this(new[] { property1, property2, property3 })
		{
		}

		public AutoNotifyAdditionalPropertiesAttribute(string property1, string property2, string property3, string property4)
			: this(new[] { property1, property2, property3, property4 })
		{
		}

		public AutoNotifyAdditionalPropertiesAttribute(string[] properties)
		{
			this.AdditionalProperties = properties;
		}
		#endregion
	}
}
