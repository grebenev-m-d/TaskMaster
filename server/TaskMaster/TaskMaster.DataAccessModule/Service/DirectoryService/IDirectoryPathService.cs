using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.DataAccessModule.Service.DirectoryService
{
	/// <summary>
	/// Сервис для работы с путями к директориям.
	/// </summary>
	public interface IDirectoryPathService
	{
		/// <summary>
		/// Возвращает путь к директории доски по её идентификатору.
		/// </summary>
		/// <param name="boardId">Идентификатор доски.</param>
		/// <returns>Путь к директории доски.</returns>
		Task<string> GetBoardFolderPath(Guid boardId);

		/// <summary>
		/// Возвращает путь к директории карточки по её идентификатору.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Путь к директории карточки.</returns>
		Task<string> GetCardFolderPath(Guid cardId);
	}
}
