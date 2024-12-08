using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TaskMaster.Validation
{
	/// <summary>
	/// Класс, предоставляющий методы для проверки аргументов.
	/// </summary>
	public class ArgumentValidation
	{
		/// <summary>
		/// Проверяет, что объект не равен null.
		/// </summary>
		/// <param name="obj">Проверяемый объект.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="objName">Имя объекта для сообщения об ошибке.</param>
		/// <exception cref="ArgumentException">Бросает исключение, если объект равен null.</exception>
		public static void CheckNotNull(object obj,
			string message = "{0} не должен быть равен null",
			[CallerArgumentExpression("obj")] string objName = "")
		{
			if (obj == null)
			{
				throw new ArgumentException(string.Format(message, objName));
			}
		}

		/// <summary>
		/// Проверяет, что строка не является пустой или null.
		/// </summary>
		/// <param name="str">Проверяемая строка.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="strName">Имя строки для сообщения об ошибке.</param>
		/// <exception cref="ArgumentException">Бросает исключение, если строка пустая или null.</exception>
		public static void CheckNotNullOrEmpty(string str,
			string message = "{0} не должен быть пустым или null",
			[CallerArgumentExpression("str")] string strName = "")
		{
			if (string.IsNullOrEmpty(str))
			{
				throw new ArgumentException(string.Format(message, strName));
			}
		}

		/// <summary>
		/// Проверяет, что GUID не пустой.
		/// </summary>
		/// <param name="guid">Проверяемый GUID.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="guidName">Имя GUID для сообщения об ошибке.</param>
		/// <exception cref="ArgumentException">Бросает исключение, если GUID пустой.</exception>
		public static void CheckNotEmptyGuid(Guid guid,
			string message = "{0} не должен быть пустым",
			[CallerArgumentExpression("guid")] string guidName = "")
		{
			if (guid == Guid.Empty)
			{
				throw new ArgumentException(string.Format(message, guidName));
			}
		}

		/// <summary>
		/// Проверяет, что число положительное.
		/// </summary>
		/// <param name="number">Проверяемое число.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="numberName">Имя числа для сообщения об ошибке.</param>
		/// <exception cref="ArgumentException">Бросает исключение, если число не положительное.</exception>
		public static void CheckPositiveNumber(long number,
			string message = "{0} должно быть больше нуля",
			[CallerArgumentExpression("number")] string numberName = "")
		{
			if (number <= 0)
			{
				throw new ArgumentException(string.Format(message, numberName));
			}
		}

		/// <summary>
		/// Проверяет, что число неотрицательное.
		/// </summary>
		/// <param name="number">Проверяемое число.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="numberName">Имя числа для сообщения об ошибке.</param>
		/// <exception cref="ArgumentException">Бросает исключение, если число отрицательное.</exception>
		public static void CheckNonNegativeNumber(long number,
			string message = "{0} должно быть больше нуля или равным нулю",
			[CallerArgumentExpression("number")] string numberName = "")
		{
			if (number < 0)
			{
				throw new ArgumentException(string.Format(message, numberName));
			}
		}

		/// <summary>
		/// Проверяет, что словарь не содержит указанный ключ.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа в словаре.</typeparam>
		/// <typeparam name="TValue">Тип значения в словаре.</typeparam>
		/// <param name="key">Проверяемый ключ.</param>
		/// <param name="dictionary">Проверяемый словарь.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="keyName">Имя ключа для сообщения об ошибке.</param>
		/// <param name="dictionaryName">Имя словаря для сообщения об ошибке.</param>
		/// <exception cref="ArgumentNullException">Бросает исключение, если словарь не содержит указанный ключ.</exception>
		public static void CheckNotContainsKey<TKey, TValue>(TKey key, Dictionary<TKey, TValue> dictionary,
			string message = "{0} не должен содержать ключ '{1}'",
			[CallerArgumentExpression("key")] string keyName = "",
			[CallerArgumentExpression("dictionary")] string dictionaryName = "") where TKey : notnull
		{
			if (!dictionary.ContainsKey(key))
			{
				throw new ArgumentNullException(string.Format(message, dictionaryName, keyName));
			}
		}

		/// <summary>
		/// Проверяет, что словарь содержит указанный ключ.
		/// </summary>
		/// <typeparam name="TKey">Тип ключа в словаре.</typeparam>
		/// <typeparam name="TValue">Тип значения в словаре.</typeparam>
		/// <param name="key">Проверяемый ключ.</param>
		/// <param name="dictionary">Проверяемый словарь.</param>
		/// <param name="message">Сообщение об ошибке, если проверка не пройдена.</param>
		/// <param name="keyName">Имя ключа для сообщения об ошибке.</param>
		/// <param name="dictionaryName">Имя словаря для сообщения об ошибке.</param>
		/// <exception cref="ArgumentNullException">Бросает исключение, если словарь не содержит указанный ключ.</exception>
		public static void CheckContainsKey<TKey, TValue>(TKey key, Dictionary<TKey, TValue> dictionary,
			string message = "{0} должен содержать ключ '{1}'",
			[CallerArgumentExpression("key")] string keyName = "",
			[CallerArgumentExpression("dictionary")] string dictionaryName = "") where TKey : notnull
		{
			if (!dictionary.ContainsKey(key))
			{
				throw new ArgumentNullException(string.Format(message, dictionaryName, keyName));
			}
		}
	}
}
