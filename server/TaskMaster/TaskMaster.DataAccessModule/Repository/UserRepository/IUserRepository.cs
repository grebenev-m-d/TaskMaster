using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.UserRepository
{
	/// <summary>
	/// Репозиторий пользователей.
	/// </summary>
	public interface IUserRepository : IBaseRepository<DbUser>
	{
		/// <summary>
		/// Получает пользователя по электронной почте асинхронно.
		/// </summary>
		/// <param name="email">Электронная почта пользователя.</param>
		/// <returns>Задача, представляющая операцию получения пользователя.</returns>
		public Task<DbUser> GetByEmailAsync(string email);

		/// <summary>
		/// Получает всех пользователей с общими досками по идентификатору пользователя асинхронно.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Задача, представляющая операцию получения всех пользователей с общими досками.</returns>
		Task<List<DbUser>> GetAllUsersWithSharedBoardsByUserIdAsync(Guid userId);
	}
}
