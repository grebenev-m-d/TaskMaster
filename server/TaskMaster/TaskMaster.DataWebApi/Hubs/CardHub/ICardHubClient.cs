using System.Net.Mail;
using System.Xml.Linq;
using TaskMaster.DataWebApi.Models;

namespace TaskMaster.DataWebApi.Hubs.CardHub
{
	/// <summary>
	/// Клиентский интерфейс для получения обновлений, связанных с карточками.
	/// </summary>
	public partial interface ICardHubClient
	{
		/// <summary>
		/// Получает уведомление о создании новой карточки.
		/// </summary>
		/// <param name="cardListId">Идентификатор списка карточек, в котором создается карточка.</param>
		/// <param name="card">Новая созданная карточка.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardCreated(Guid cardListId, Card card);

		/// <summary>
		/// Получает уведомление о добавлении обложки к карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardCoverImageAdded(Guid cardId);

		/// <summary>
		/// Получает уведомление о удалении обложки карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardCoverImageDeleted(Guid cardId);

		/// <summary>
		/// Получает уведомление об обновлении заголовка карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="title">Обновленный заголовок карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardTitleUpdated(Guid cardId, string title);

		/// <summary>
		/// Получает уведомление об обновлении описания карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="description">Обновленное описание карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardDescriptionUpdated(Guid cardId, string description);

		/// <summary>
		/// Получает уведомление о добавлении вложения к карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="cardAttachment">Добавленное вложение.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardAttachmentAdded(Guid cardId, CardAttachment cardAttachment);

		/// <summary>
		/// Получает уведомление об удалении вложения из карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="cardAttachmentId">Идентификатор удаленного вложения.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardAttachmentDeleted(Guid cardId, Guid cardAttachmentId);

		/// <summary>
		/// Получает уведомление о перемещении карточки в рамках доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="currentCardListId">Идентификатор текущего списка карточек.</param>
		/// <param name="movedCardId">Идентификатор перемещаемой карточки.</param>
		/// <param name="prevCardId">Идентификатор карточки, перед которой размещается перемещаемая карточка (если есть).</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardMoved(Guid boardId, Guid currentCardListId, Guid movedCardId, Guid? prevCardId);

		/// <summary>
		/// Получает уведомление об удалении карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор удаленной карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardDeleted(Guid cardId);
	}
}
