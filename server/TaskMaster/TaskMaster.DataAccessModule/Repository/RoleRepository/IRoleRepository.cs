using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.RoleRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с историей просмотра доски в базе данных.
	/// </summary>
	public interface IDbBoardViewHistoryMapRepository 
		: IBaseRepository<DbRole>
	{
		/// <summary>
		/// Получает роль по ее типу.
		/// </summary>
		/// <param name="roleType">Тип роли.</param>
		/// <returns>Задача, представляющая асинхронную операцию получения роли по ее типу.</returns>
		Task<DbRole> GetByNameAsync(RoleType roleType);
	}

}
