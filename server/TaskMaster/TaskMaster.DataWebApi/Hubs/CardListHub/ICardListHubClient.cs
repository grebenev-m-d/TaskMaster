using TaskMaster.DataWebApi.Models;

namespace TaskMaster.DataWebApi.Hubs.CardListHub
{
	/// <summary>
	/// Клиентский интерфейс для уведомлений о событиях в списке карточек.
	/// </summary>
	public partial interface ICardListHubClient
	{
		/// <summary>
		/// Получает уведомление о создании списка карточек.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="cardList">Созданный список карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		Task ReceiveCardListCreated(Guid boardId, CardList cardList);

		/// <summary>
		/// Получает уведомление об обновлении заголовка списка карточек.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек.</param>
		/// <param name="title">Новый заголовок списка карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		Task ReceiveCardListTitleUpdated(Guid cardListId, string title);

		/// <summary>
		/// Получает уведомление о перемещении списка карточек.
		/// </summary>
		/// <param name="movedCardListId">Идентификатор перемещенного списка карточек.</param>
		/// <param name="prevCardListId">Идентификатор предыдущего списка карточек, если есть.</param>
		/// <returns>Задача асинхронной операции.</returns>
		Task ReceiveCardListMoved(Guid movedCardListId, Guid? prevCardListId);

		/// <summary>
		/// Получает уведомление о удалении списка карточек.
		/// </summary>
		/// <param name="cardListId">Идентификатор удаленного списка карточек.</param>
		/// <returns>Задача асинхронной операции.</returns>
		Task ReceiveCardListDeleted(Guid cardListId);
	}
}
