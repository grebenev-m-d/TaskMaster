namespace TaskMaster.DataWebApi.Models
{
	/// <summary>
	/// Вложение к карточке.
	/// </summary>
	public class CardAttachment
	{
		/// <summary>
		/// Идентификатор вложения.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Имя файла вложения.
		/// </summary>
		public string FileName { get; set; }
	}
}
