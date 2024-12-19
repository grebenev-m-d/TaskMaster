using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.DataAccessModule.Repository.BaseRepository;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Constants;

namespace TaskMaster.DataAccessModule.Repository.RoleRepository
{
	/// <summary>
	/// Репозиторий для работы с ролями в базе данных.
	/// </summary>
	public class RoleRepository : BaseRepository<DbRole>, IDbBoardViewHistoryMapRepository
	{
		/// <summary>
		/// Поставщик сервисов для работы с репозиторием.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр репозитория ролей с заданным поставщиком сервисов.
		/// </summary>
		/// <param name="serviceProvider">Поставщик сервисов для внедрения зависимостей.</param>
		public RoleRepository(IServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает роль по указанному идентификатору включая связанные сущности.
		/// </summary>
		/// <param name="id">Идентификатор роли.</param>
		/// <returns>Задача, представляющая операцию и получении роли.</returns>
		public override async Task<DbRole> GetByIdAsyncIncludes(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Roles.FirstOrDefaultAsync(i => i.Id == id);
			}
		}

		/// <summary>
		/// Получает роль по указанному типу.
		/// </summary>
		/// <param name="roleType">Тип роли.</param>
		/// <returns>Задача, представляющая операцию и получении роли.</returns>
		public async Task<DbRole> GetByNameAsync(RoleType roleType)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Roles.FirstOrDefaultAsync(i => i.Type == roleType);
			}
		}

		/// <summary>
		/// Получает список всех ролей в базе данных.
		/// </summary>
		/// <returns>Задача, представляющая операцию и получении списка всех ролей.</returns>
		public override async Task<List<DbRole>> GetAllAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Roles.ToListAsync();
			}
		}

		/// <summary>
		/// Добавляет новую роль в базу данных.
		/// </summary>
		/// <param name="entity">Добавляемая роль.</param>
		/// <returns>Задача, представляющая операцию добавления роли.</returns>
		public override async Task<DbRole> AddAsync(DbRole entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				await dbContext.Roles.AddAsync(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Удаляет роль из базы данных по указанному идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор удаляемой роли.</param>
		/// <returns>Задача, представляющая операцию удаления роли.</returns>
		public override async Task<DbRole> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var role = await dbContext.Roles.FindAsync(id);

				if (role != null)
				{
					dbContext.Roles.Remove(role);
					await dbContext.SaveChangesAsync();
				}

				return role;
			}
		}
	}
}
