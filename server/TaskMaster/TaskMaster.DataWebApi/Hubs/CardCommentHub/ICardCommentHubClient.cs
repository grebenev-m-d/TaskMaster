using TaskMaster.DataWebApi.Models;

namespace TaskMaster.DataWebApi.Hubs.CardCommentHub
{
	/// <summary>
	/// Клиент хаба комментариев карточки.
	/// </summary>
	public interface ICardCommentHubClient
	{
		/// <summary>
		/// Принимает уведомление о создании комментария карточки.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <param name="comment">Комментарий карточки.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardCommentCreated(Guid cardId, CardComment comment);

		/// <summary>
		/// Принимает уведомление об обновлении текста комментария карточки.
		/// </summary>
		/// <param name="commentId">Идентификатор комментария.</param>
		/// <param name="text">Текст комментария.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveTextUpdated(Guid commentId, string text);

		/// <summary>
		/// Принимает уведомление о удалении комментария карточки.
		/// </summary>
		/// <param name="commentId">Идентификатор комментария.</param>
		/// <returns>Асинхронная задача.</returns>
		Task ReceiveCardCommentDeleted(Guid commentId);
	}
}
