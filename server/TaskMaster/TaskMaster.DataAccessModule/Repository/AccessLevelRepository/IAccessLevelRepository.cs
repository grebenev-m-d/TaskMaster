using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.AccessLevelRole
{
	/// <summary>
	/// Интерфейс репозитория уровней доступа.
	/// </summary>
	public interface IAccessLevelRepository : IBaseRepository<DbAccessLevel>
	{
		/// <summary>
		/// Получение уровня доступа по имени.
		/// </summary>
		/// <param name="permission">Тип уровня доступа.</param>
		/// <returns>Уровень доступа.</returns>
		Task<DbAccessLevel> GetByNameAsync(AccessLevelType permission);
	}

}
