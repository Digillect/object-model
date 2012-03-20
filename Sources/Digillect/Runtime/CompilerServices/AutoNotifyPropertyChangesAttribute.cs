using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Digillect.Runtime.CompilerServices
{
	[Flags]
	public enum AutoNotifyPropertyChangesModes
	{
		None = 0,
		Before = 1,
		After = 2,
		Both = Before | After
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Property )]
	public sealed class AutoNotifyPropertyChangesAttribute : Attribute
	{
		public AutoNotifyPropertyChangesModes Mode { get; private set; }

		public AutoNotifyPropertyChangesAttribute()
		{
			this.Mode = AutoNotifyPropertyChangesModes.Both;
		}

		public AutoNotifyPropertyChangesAttribute(AutoNotifyPropertyChangesModes mode)
		{
			this.Mode = mode;
		}

		public AutoNotifyPropertyChangesAttribute( bool notify )
		{
			this.Mode = notify ? AutoNotifyPropertyChangesModes.Both : AutoNotifyPropertyChangesModes.None;
		}
	}
}
