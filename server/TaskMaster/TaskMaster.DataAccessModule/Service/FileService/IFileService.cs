using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.DataAccessModule.Service.FileService
{
	/// <summary>
	/// Сервис для работы с файлами.
	/// </summary>
	public interface IFileService
	{
		/// <summary>
		/// Сохраняет файл в указанную директорию.
		/// </summary>
		/// <param name="file">Файл для сохранения.</param>
		/// <param name="directoryPath">Путь к директории сохранения.</param>
		/// <returns>Путь к сохраненному файлу.</returns>
		Task<string> SaveFile(IFormFile file, string directoryPath);

		/// <summary>
		/// Получает файл по указанному пути.
		/// </summary>
		/// <param name="filePath">Путь к файлу.</param>
		/// <returns>Поток файла или null, если файл не найден.</returns>
		Task<FileStream> GetFile(string filePath);

		/// <summary>
		/// Удаляет файл по указанному пути, если он существует.
		/// </summary>
		/// <param name="filePath">Путь к файлу.</param>
		Task DeleteFile(string filePath);
	}
}
