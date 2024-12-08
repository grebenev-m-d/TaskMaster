using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace TaskMaster.DataWebApi.Helpers
{
	/// <summary>
	/// Помощник для обработки исключений.
	/// </summary>
	public static class ExceptionHelper
	{
		/// <summary>
		/// Выполняет асинхронное действие с обработкой исключений.
		/// </summary>
		/// <typeparam name="T">Тип возвращаемого значения.</typeparam>
		/// <typeparam name="K">Тип логгера.</typeparam>
		/// <param name="action">Действие, которое необходимо выполнить.</param>
		/// <param name="logger">Логгер для записи сообщений об ошибках.</param>
		/// <returns>Результат выполнения действия.</returns>
		/// <exception cref="HubException">Выбрасывается при возникновении исключения.</exception>
		public static async Task<T> ExecuteWithExceptionHandlingAsync<T,K>(Func<Task<T>> action, ILogger<K> logger)
		{
			try
			{
				return await action();
			}
			catch (UnauthorizedAccessException ex)
			{
				logger.LogWarning(ex.ToString());

				throw new HubException(ex.Message);
			}
			catch (ArgumentException ex)
			{
				logger.LogWarning(ex.ToString());

				throw new HubException(ex.Message);
			}
			catch (DbUpdateException ex)
			{
				logger.LogWarning(ex.ToString());

				throw new HubException("Ошибка базы данных, попробуйте позже.");
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex.ToString());

				throw new HubException("Неизвестная ошибка, попробуйте позже.");
			}
		}
	}
}
