using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.BoardRepository
{
	/// <summary>
	/// Определяет интерфейс репозитория для работы с досками.
	/// </summary>
	public interface IBoardRepository : IBaseRepository<DbBoard>
	{
		/// <summary>
		/// Получить доску по идентификатору списка карточек.
		/// </summary>
		Task<DbBoard> GetBoardByCardListIdAsync(Guid cardListId);

		/// <summary>
		/// Получить доску по идентификатору карточки.
		/// </summary>
		Task<DbBoard> GetBoardByCardIdAsync(Guid cardId);

		/// <summary>
		/// Получить доску по идентификатору комментария к карточке.
		/// </summary>
		Task<DbBoard> GetBoardByCardCommentIdAsync(Guid commentId);

		/// <summary>
		/// Получить список всех досок пользователя по его идентификатору.
		/// </summary>
		Task<List<DbBoard>> GetAllByUserIdAsync(Guid userId);

		/// <summary>
		/// Получить список досок от владельца с персональным уровнем доступа для указанного пользователя.
		/// </summary>
		Task<List<DbBoard>> GetBoardsFromOwnerWithPersonalAccessLevelForUser(Guid invitedUserId, Guid inviterUserId);
	}
}
