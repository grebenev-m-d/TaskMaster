namespace TaskMaster.AuthWebApi.Models
{
	/// <summary>
	/// Модель формы восстановления пароля пользователя.
	/// </summary>
	public class FormRestorePassword
	{
		/// <summary>
		/// Email пользователя для восстановления пароля.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Новый пароль пользователя.
		/// </summary>
		public string NewPassword { get; set; }

		/// <summary>
		/// Подтверждение нового пароля пользователя.
		/// </summary>
		public string ConfirmNewPassword { get; set; }
	}
}
