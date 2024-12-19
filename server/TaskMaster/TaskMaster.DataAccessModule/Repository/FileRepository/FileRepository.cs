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
using TaskMaster.DataAccessModule.Repository.DesignRepository;

namespace TaskMaster.DataAccessModule.Repository.FileRepository
{

	/// <summary>
	/// Репозиторий для работы с файлами.
	/// </summary>
	public class FileRepository 
		: BaseRepository<DbFile>, IFileRepository
	{
		/// <summary>
		/// Поставщик служб.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Инициализирует новый экземпляр класса FileRepository.
		/// </summary>
		/// <param name="serviceProvider">Поставщик служб.</param>
		public FileRepository(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		/// <summary>
		/// Получает файл по его относительному пути.
		/// </summary>
		/// <param name="relativePath">Относительный путь файла.</param>
		/// <returns>Задача, представляющая асинхронную операцию получения файла по его относительному пути.</returns>
		public async Task<DbFile> GetByRelativePathAsync(string relativePath)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TaskMasterContext>();

				return await dbContext.Files.FirstOrDefaultAsync(i => i.RelativePath == relativePath);
			}
		}
	}
}
