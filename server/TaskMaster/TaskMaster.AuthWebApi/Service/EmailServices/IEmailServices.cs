using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Services.EmailServices
{
	/// <summary>
	/// Интерфейс для сервиса отправки электронных писем.
	/// </summary>
	public interface IEmailServices
	{
		/// <summary>
		/// Асинхронный метод для отправки электронного письма.
		/// </summary>
		/// <param name="email">Email получателя.</param>
		/// <param name="linkConfirmation">Ссылка для подтверждения.</param>
		/// <returns>Успешность отправки письма.</returns>
		Task<bool> SendConfirmationEmailAsync(string email, string linkConfirmation);
	}
}
