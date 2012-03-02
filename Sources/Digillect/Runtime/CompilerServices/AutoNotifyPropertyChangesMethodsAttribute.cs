using System;
using System.Collections.Generic;
using System.Linq;

namespace Digillect.Runtime.CompilerServices
{
	[AttributeUsage( AttributeTargets.Class )]
	public sealed class AutoNotifyPropertyChangesMethodsAttribute : Attribute
	{
		public string Before { get; set; }
		public string After { get; set; }
	}
}
