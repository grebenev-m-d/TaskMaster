using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
	/// Репозиторий для работы с дизайнами.
	/// </summary>
	public class DesignRepository : BaseRepository<DbDesign>, IDesignRepository
	{
		/// <summary>
		/// Поставщик служб для доступа к контексту.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр репозитория дизайнов.
		/// </summary>
		/// <param name="serviceProvider">Поставщик служб для доступа к контексту.</param>
		public DesignRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает дизайн по его типу.
		/// </summary>
		/// <param name="type">Тип дизайна.</param>
		/// <returns>Задача, представляющая асинхронную операцию получения дизайна по его типу.</returns>
		public async Task<DbDesign> GetByTypeAsync(DesignType type)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Designs.FirstOrDefaultAsync(i => i.Type == type);
			}
		}
	}
}
