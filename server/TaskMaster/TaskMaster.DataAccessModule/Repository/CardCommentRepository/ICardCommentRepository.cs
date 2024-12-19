using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.CardCommentRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с комментариями к карточкам.
	/// </summary>
	public interface ICardCommentRepository 
		: IBaseRepository<DbCardComment>
	{
		/// <summary>
		/// Получает все комментарии к указанной карточке.
		/// </summary>
		/// <param name="cardId">Идентификатор карточки.</param>
		/// <returns>Список комментариев для указанной карточки.</returns>
		Task<List<DbCardComment>> GetAllByCardIdAsync(Guid cardId);
	}
}
