using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.DataAccessModule.Models;
using TaskMaster.DataAccessModule.Repository.BaseRepository;

namespace TaskMaster.DataAccessModule.Repository.FileRepository
{
	/// <summary>
	/// Интерфейс репозитория для работы с файлами.
	/// </summary>
	public interface IFileRepository : IBaseRepository<DbFile>
	{
		/// <summary>
		/// Получает файл по его относительному пути.
		/// </summary>
		/// <param name="relativePath">Относительный путь файла.</param>
		/// <returns>Задача, представляющая асинхронную операцию получения файла по его относительному пути.</returns>
		Task<DbFile> GetByRelativePathAsync(string relativePath);
	}
}
