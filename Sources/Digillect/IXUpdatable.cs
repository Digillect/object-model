using System;

namespace Digillect
{
	public interface IXUpdatable<T>
		where T : IXUpdatable<T>
	{
		event EventHandler Updated;

		/// <summary>
		/// Begins the mass-update operation.
		/// </summary>
		/// <remarks>
		/// This method can be called multiple times.
		/// Until the last corresponding <see cref="EndUpdate"/> method is called none of the <see cref="Updated"/> events are raised.
		/// </remarks>
		void BeginUpdate();

		/// <summary>
		/// Ends the mass-update operation.
		/// </summary>
		/// <remarks>
		/// Do not forget to call this method for each corresponding <see cref="BeginUpdate"/> method you called.
		/// Until the last corresponding <see cref="EndUpdate()"/> method is called none of the <see cref="Updated"/> events are raised.
		/// </remarks>
		void EndUpdate();

		/// <summary>
		/// Determines whether the update operation is needed.
		/// </summary>
		/// <param name="source">Source <b>object</b> to compare with.</param>
		/// <returns><see langword="false"/> if the <see cref="Update">update operation</see> is not required (i.e, the two objects are equal by reference), otherwise, <see langword="true"/>.</returns>
		bool UpdateRequired(T source);

		/// <summary>
		/// Обновляет текущий объект на основе другого объекта.
		/// </summary>
		/// <param name="source">Источник изменений.</param>
		/// <remarks>
		/// В конце операции возбуждается событие <see cref="Updated"/>, если это не заблокировано вызовом <see cref="BeginUpdate"/>;
		/// в этом случае событие <b>Updated</b> будет возбуждено в результате вызова последнего соответствующего метода <see cref="EndUpdate"/>.
		/// </remarks>
		void Update(T source);
	}
}
