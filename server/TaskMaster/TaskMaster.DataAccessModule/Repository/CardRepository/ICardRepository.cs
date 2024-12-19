using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.CardRepository
{
	/// <summary>
	/// Интерфейс, определяющий операции над репозиторием карточек.
	/// </summary>
	public interface ICardRepository : IBaseRepository<DbCard>
	{
		/// <summary>
		/// Перемещает карточку из одного списка карточек в другой.
		/// </summary>
		/// <param name="currentCardListId">Идентификатор текущего списка карточек, из которого перемещается карточка.</param>
		/// <param name="movedCardId">Идентификатор перемещаемой карточки.</param>
		/// <param name="prevCardId">Идентификатор карточки, после которой должна быть 
		/// размещена перемещаемая карточка в новом списке карточек.</param>
		/// <returns>Задача, представляющая асинхронную операцию перемещения карточки.</returns>
		Task MoveCard(Guid currentCardListId, Guid movedCardId, Guid? prevCardId);
	}
}
