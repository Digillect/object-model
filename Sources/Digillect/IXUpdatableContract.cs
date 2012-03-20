using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Digillect
{
	[ContractClassFor( typeof( IXUpdatable<> ) )]
	abstract class IXUpdatableContract<T> : IXUpdatable<T>
		where T : IXUpdatable<T>
	{
		protected IXUpdatableContract()
		{
		}

		public event EventHandler Updated;

		public void BeginUpdate()
		{
		}

		public void EndUpdate()
		{
			Updated( this, EventArgs.Empty ); // To keep compiler happy.
		}

		public bool IsUpdateRequired( T source )
		{
			return false;
		}

		public void Update( T source )
		{
			Contract.Requires( source != null );
		}
	}
}
