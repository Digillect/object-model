using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Runtime.CompilerServices
{
	[AttributeUsage( AttributeTargets.Class )]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	public sealed class XKeyAttribute : Attribute
	{
		public ICollection<string> KeyProperties { get; private set; }

		#region Constructors/Disposer
		public XKeyAttribute( string property )
		{
			this.KeyProperties = new string[] { property };
		}

		public XKeyAttribute( string property1, string property2 )
		{
			this.KeyProperties = new string[] { property1, property2 };
		}

		public XKeyAttribute( string property1, string property2, string property3 )
		{
			this.KeyProperties = new string[] { property1, property2, property3 };
		}

		public XKeyAttribute( string property1, string property2, string property3, string property4 )
		{
			this.KeyProperties = new string[] { property1, property2, property3, property4 };
		}

		public XKeyAttribute( string[] properties )
		{
			this.KeyProperties = properties;
		}
		#endregion
	}
}
