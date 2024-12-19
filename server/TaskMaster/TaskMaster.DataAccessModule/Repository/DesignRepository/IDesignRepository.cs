using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.DesignRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с дизайнами.
	/// </summary>
	public interface IDesignRepository : IBaseRepository<DbDesign>
	{
		/// <summary>
		/// Получает дизайн по его типу.
		/// </summary>
		/// <param name="type">Тип дизайна.</param>
		/// <returns>Задача, представляющая асинхронную операцию получения дизайна по его типу.</returns>
		Task<DbDesign> GetByTypeAsync(DesignType type);
	}
}
