using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.AccessLevelRole
{
	/// <summary>
	/// Репозиторий уровней доступа.
	/// </summary>
	public class AccessLevelRepository 
		: BaseRepository<DbAccessLevel>, IAccessLevelRepository
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Конструктор репозитория уровней доступа.
		/// </summary>
		/// <param name="serviceProvider">Сервис-провайдер для доступа к контексту базы данных.</param>
		public AccessLevelRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получение уровня доступа по идентификатору с подгрузкой зависимостей.
		/// </summary>
		/// <param name="id">Идентификатор уровня доступа.</param>
		/// <returns>Уровень доступа.</returns>
		public override async Task<DbAccessLevel> GetByIdAsyncIncludes(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.AccessLevels.FindAsync(id);
			}
		}

		/// <summary>
		/// Получение уровня доступа по имени.
		/// </summary>
		/// <param name="permission">Тип уровня доступа.</param>
		/// <returns>Уровень доступа.</returns>
		public async Task<DbAccessLevel> GetByNameAsync(AccessLevelType permission)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.AccessLevels.FirstAsync(i => i.Type == permission);
			}
		}

		/// <summary>
		/// Получение всех уровней доступа.
		/// </summary>
		/// <returns>Список всех уровней доступа.</returns>
		public override async Task<List<DbAccessLevel>> GetAllAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.AccessLevels.ToListAsync();
			}
		}

		/// <summary>
		/// Добавление нового уровня доступа.
		/// </summary>
		/// <param name="entity">Добавляемый уровень доступа.</param>
		/// <returns>Добавленный уровень доступа.</returns>
		public override async Task<DbAccessLevel> AddAsync(DbAccessLevel entity)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				await dbContext.AccessLevels.AddAsync(entity);
				await dbContext.SaveChangesAsync();

				return entity;
			}
		}

		/// <summary>
		/// Удаление уровня доступа по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор удаляемого уровня доступа.</param>
		/// <returns>Удаленный уровень доступа.</returns>
		public override async Task<DbAccessLevel> DeleteByIdAsync(Guid id)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				var permission = await dbContext.AccessLevels.FindAsync(id);

				if (permission != null)
				{
					dbContext.AccessLevels.Remove(permission);
					await dbContext.SaveChangesAsync();
				}

				return permission;
			}
		}
	}
}
