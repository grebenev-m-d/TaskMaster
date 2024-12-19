using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Validation
{
	/// <summary>
	/// Предоставляет метод для валидации пароля.
	/// </summary>
	public static class PasswordValidation
	{
		private static readonly int _minLength = 8; // Минимальная длина пароля.
		private static readonly int _maxLength = 128; // Максимальная длина пароля.
		private static readonly bool _requireLowercase = true; // Требуется ли наличие строчных символов.
		private static readonly bool _requireUppercase = true; // Требуется ли наличие прописных символов.
		private static readonly bool _requireDigit = true; // Требуется ли наличие цифр.
		private static readonly bool _requireSpecialCharacter = true; // Требуется ли наличие специальных символов.

		/// <summary>
		/// Проверяет, соответствует ли указанный пароль заданным критериям.
		/// </summary>
		/// <param name="password">Строка, представляющая пароль для проверки.</param>
		/// <returns>Значение true, если пароль соответствует критериям валидации, в противном случае — значение false.</returns>
		public static bool Validate(string password)
		{
			if (string.IsNullOrEmpty(password))
			{
				throw new ArgumentException("Пароль не может быть пустым или равным null.", nameof(password));
			}

			if (password.Length < _minLength || password.Length > _maxLength)
			{
				throw new ArgumentException($"Пароль должен содержать от {_minLength} до {_maxLength} символов.", nameof(password));
			}

			if (_requireLowercase && !password.Any(char.IsLower))
			{
				throw new ArgumentException("Пароль должен содержать строчные символы.", nameof(password));
			}

			if (_requireUppercase && !password.Any(char.IsUpper))
			{
				throw new ArgumentException("Пароль должен содержать прописные символы.", nameof(password));
			}

			if (_requireDigit && !password.Any(char.IsDigit))
			{
				throw new ArgumentException("Пароль должен содержать цифры.", nameof(password));
			}

			if (_requireSpecialCharacter && !password.Any(c => !char.IsLetterOrDigit(c)))
			{
				throw new ArgumentException("Пароль должен содержать специальные символы.", nameof(password));
			}

			return true;
		}
	}
}
