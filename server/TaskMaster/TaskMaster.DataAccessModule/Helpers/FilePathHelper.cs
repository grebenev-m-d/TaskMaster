namespace TaskMaster.DataWebApi.Helpers
{

	/// <summary>
	/// Помощник для работы с путями к файлам
	/// </summary>
	public static class FilePathHelper
	{
		/// <summary>
		/// Базовая директория, относительно которой вычисляются относительные пути
		/// </summary>
		public static string _baseDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())));

		/// <summary>
		/// Получает полный путь к файлу, используя относительный путь от базовой директории
		/// </summary>
		/// <param name="relativePath">Относительный путь к файлу</param>
		/// <returns>Полный путь к файлу</returns>
		public static string GetFullPath(string relativePath)
		{
			return System.IO.Path.Combine(_baseDirectory, relativePath);
		}

		/// <summary>
		/// Получает относительный путь к файлу из его полного пути
		/// </summary>
		/// <param name="fullPath">Полный путь к файлу</param>
		/// <returns>Относительный путь к файлу</returns>
		public static string GetRelativePath(string fullPath)
		{
			if (fullPath.StartsWith(_baseDirectory))
			{
				return fullPath.Substring(_baseDirectory.Length + 1); // +1 чтобы убрать разделитель каталогов
			}
			else
			{
				throw new ArgumentException("Полный путь не находится в базовом каталоге");
			}
		}
	}
}


