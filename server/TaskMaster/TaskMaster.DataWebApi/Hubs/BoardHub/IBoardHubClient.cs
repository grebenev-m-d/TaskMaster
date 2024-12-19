using System.Collections.Generic;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataWebApi.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace TaskMaster.DataWebApi.Hubs.BoardHub
{
	/// <summary>
	/// Интерфейс клиента доски в хабе.
	/// </summary>
	public interface IBoardHubClient
	{
		/// <summary>
		/// Получает уведомление о создании доски.
		/// </summary>
		/// <param name="board">Созданная доска.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveBoardCreated(Board board);

		/// <summary>
		/// Получает уведомление об обновлении заголовка доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="title">Новый заголовок доски.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveTitleUpdated(Guid boardId, string title);

		/// <summary>
		/// Получает уведомление об обновлении цвета доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="colorCode">Код цвета доски.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveColorUpdated(Guid boardId, string colorCode);

		/// <summary>
		/// Получает уведомление об обновлении изображения доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveImageUpdated(Guid boardId);

		/// <summary>
		/// Получает уведомление об обновлении типа дизайна доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="designType">Новый тип дизайна доски.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveDesignTypeUpdated(Guid boardId, DesignType designType);

		/// <summary>
		/// Получает уведомление об обновлении владельца доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <param name="owner">Новый владелец доски.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveOwnerUpdated(Guid boardId, User owner);

		/// <summary>
		/// Получает уведомление об удалении доски.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveBoardDeleted(Guid boardId);

		/// <summary>
		/// Получает уведомление об обновлении истории последних просмотренных досок.
		/// </summary>
		/// <param name="boards">Список досок.</param>
		/// <returns>Задача получения уведомления.</returns>
		Task ReceiveUpdatedHistoryLastViewedBoards(List<Board> boards);
	}
}
