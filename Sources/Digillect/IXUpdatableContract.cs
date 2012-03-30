using System;
using System.Diagnostics.Contracts;

namespace Digillect
{
	[ContractClassFor(typeof(IXUpdatable<>))]
	abstract class IXUpdatableContract<T> : IXUpdatable<T>
		where T : IXUpdatable<T>
	{
		protected IXUpdatableContract()
		{
		}

		public abstract event EventHandler Updated;

		public abstract void BeginUpdate();
		public abstract void EndUpdate();

		public bool IsUpdateRequired(T source)
		{
			Contract.Requires<ArgumentNullException>(source != null, "source");

			return false;
		}

		public void Update(T source)
		{
			Contract.Requires<ArgumentNullException>(source != null, "source");
		}
	}
}
