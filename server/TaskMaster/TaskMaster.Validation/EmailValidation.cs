using System.Text.RegularExpressions;

namespace TaskMaster.Validation
{
	/// <summary>
	/// Предоставляет метод для валидации электронной почты.
	/// </summary>
	public static class EmailValidation
	{
		/// <summary>
		/// Регулярное выражение для проверки электронной почты.
		/// </summary>
		private static readonly Regex _emailRegex = new Regex(@"^[\w\.-]+@[\w\.-]+\.[a-zA-Z]{2,}$");

		/// <summary>
		/// Проверяет, является ли указанная строка допустимым адресом электронной почты.
		/// </summary>
		/// <param name="email">Строка, представляющая адрес электронной почты для проверки.</param>
		/// <returns>Значение true, если адрес электронной почты допустим, в противном случае — значение false.</returns>
		public static bool Validate(string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return false;
			}

			return _emailRegex.IsMatch(email);
		}
	}
}
