using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace TaskMaster.DataAccessModule.Repository.UserRepository
{
	/// <summary>
	/// Репозиторий для работы с пользователями.
	/// </summary>
	public class UserRepository : BaseRepository<DbUser>, IUserRepository
	{
		/// <summary>
		/// Провайдер служб.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр класса UserRepository.
		/// </summary>
		/// <param name="serviceProvider">Провайдер служб.</param>
		public UserRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает пользователя по его идентификатору вместе с дополнительными данными.
		/// </summary>
		/// <param name="id">Идентификатор пользователя.</param>
		/// <returns>Задача, представляющая операцию получения пользователя.</returns>
		public override async Task<DbUser> GetByIdAsyncIncludes(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Users
					.Include(u => u.Role)
					.FirstOrDefaultAsync(u => u.Id == id);
			}
		}

		/// <summary>
		/// Получает пользователя по его электронной почте асинхронно.
		/// </summary>
		/// <param name="email">Электронная почта пользователя.</param>
		/// <returns>Задача, представляющая операцию получения пользователя.</returns>
		public async Task<DbUser> GetByEmailAsync(string email)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Users
					.Include(u => u.Role)
					.FirstOrDefaultAsync(i => i.Email == email);
			}
		}

		/// <summary>
		/// Получает список всех пользователей вместе с дополнительными данными.
		/// </summary>
		/// <returns>Задача, представляющая операцию получения списка пользователей.</returns>
		public override async Task<List<DbUser>> GetAllAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Users
					.Include(u => u.Role)
					.ToListAsync();
			}
		}

		/// <summary>
		/// Добавляет нового пользователя.
		/// </summary>
		/// <param name="entity">Добавляемый пользователь.</param>
		/// <returns>Задача, представляющая операцию добавления пользователя.</returns>
		public override async Task<DbUser> AddAsync(DbUser entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				entity.Role = await dbContext.Roles.FirstOrDefaultAsync(i => i.Type == entity.Role.Type);

				await dbContext.Users.AddAsync(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Удаляет пользователя по его идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор пользователя.</param>
		/// <returns>Задача, представляющая операцию удаления пользователя.</returns>
		public override async Task<DbUser> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var user = await dbContext.Users.FindAsync(id);

				if (user != null)
				{
					dbContext.Users.Remove(user);
					await dbContext.SaveChangesAsync();
				}

				return user;
			}
		}

		/// <summary>
		/// Получает список всех пользователей с общими досками по идентификатору пользователя.
		/// </summary>
		/// <param name="userId">Идентификатор пользователя.</param>
		/// <returns>Задача, представляющая операцию получения списка пользователей с общими досками.</returns>
		public async Task<List<DbUser>> GetAllUsersWithSharedBoardsByUserIdAsync(Guid userId)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Users
					.Where(i => i.Boards
						.Any(j => j.BoardAccessLevelMaps
						.Any(k => k.UserId == userId))).ToListAsync();
			}
		}
	}
}
