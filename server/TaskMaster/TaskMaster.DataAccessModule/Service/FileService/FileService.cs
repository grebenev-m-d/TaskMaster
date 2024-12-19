using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;

namespace TaskMaster.DataAccessModule.Service.FileService
{
	/// <summary>
	/// Сервис для работы с файлами.
	/// </summary>
	public class FileService : IFileService
	{
		/// <summary>
		/// Сохраняет файл в указанную директорию.
		/// </summary>
		/// <param name="file">Файл для сохранения.</param>
		/// <param name="directoryPath">Путь к директории сохранения.</param>
		/// <returns>Путь к сохраненному файлу.</returns>
		public async Task<string> SaveFile(IFormFile file, string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			string uniqueFileName = GetUniqueFileName(file.FileName, directoryPath);
			string filePath = Path.Combine(directoryPath, uniqueFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			return filePath;
		}

		/// <summary>
		/// Получает файл по указанному пути.
		/// </summary>
		/// <param name="filePath">Путь к файлу.</param>
		/// <returns>Поток файла или null, если файл не найден.</returns>
		public async Task<FileStream> GetFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				return new FileStream(filePath, FileMode.Open);
			}
			return null;
		}

		/// <summary>
		/// Удаляет файл по указанному пути, если он существует.
		/// </summary>
		/// <param name="filePath">Путь к файлу.</param>
		public async Task DeleteFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}

		/// <summary>
		/// Генерирует уникальное имя файла в указанной директории.
		/// </summary>
		/// <param name="fileName">Имя файла.</param>
		/// <param name="directoryPath">Путь к директории, в которой будет сохранен файл.</param>
		/// <returns>Уникальное имя файла.</returns>
		private string GetUniqueFileName(string fileName, string directoryPath)
		{
			string name = Path.GetFileNameWithoutExtension(fileName);
			string extension = Path.GetExtension(fileName);
			string filePath = Path.Combine(directoryPath, fileName);
			string newName = name;

			if (!File.Exists(filePath))
			{
				return newName + extension;
			}

			int fileCount = 0;
			if (Regex.IsMatch(newName, @" \(\d+\)$"))
			{
				Match match = Regex.Match(newName, @" \((\d+)\)$");
				if (match.Success)
				{
					fileCount = int.Parse(match.Groups[1].Value);
					newName = newName.Substring(0, newName.LastIndexOf(" ("));
				}
			}

			while (File.Exists(filePath))
			{
				fileCount++;
				newName = $"{name} ({fileCount})";
				filePath = Path.Combine(directoryPath, newName + extension);
			}

			return newName + extension;
		}
	}
}
