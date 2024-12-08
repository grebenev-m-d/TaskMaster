using System.ComponentModel.DataAnnotations;

namespace TaskMaster.AuthWebApi.Models
{
	/// <summary>
	/// Модель формы входа пользователя.
	/// </summary>
	public class FormLogin
	{
		/// <summary>
		/// Email пользователя для входа.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Пароль пользователя для входа.
		/// </summary>
		public string Password { get; set; }
	}
}
