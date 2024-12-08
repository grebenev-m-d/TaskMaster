using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.CardListRepository
{
	/// <summary>
	/// Предоставляет интерфейс для работы с репозиторием списков карточек.
	/// </summary>
	public interface ICardListRepository : IBaseRepository<DbCardList>
	{
		/// <summary>
		/// Перемещает список карточек на новую позицию.
		/// </summary>
		/// <param name="movedCardListId">Идентификатор перемещаемого списка карточек.</param>
		/// <param name="prevCardListId">Идентификатор списка карточек, после которого следует разместить перемещаемый список. 
		/// Если null, то перемещаемый список будет размещен в начале.</param>
		Task MoveCardList(Guid movedCardListId, Guid? prevCardListId);
	}
}
