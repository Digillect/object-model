using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Runtime.CompilerServices
{
	[AttributeUsage( AttributeTargets.Class )]
	public sealed class GenerateEqualsAndGetHashCodeAttribute : Attribute
	{
		public bool GenerateEquals { get; set; }
		public bool GenerateGetHashCode { get; set; }

		public GenerateEqualsAndGetHashCodeAttribute()
		{
			GenerateEquals = GenerateGetHashCode = true;
		}
	}
}
