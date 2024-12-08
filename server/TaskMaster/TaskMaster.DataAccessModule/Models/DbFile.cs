using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель файла в базе данных.
/// </summary>
public partial class DbFile
	: DbBaseEntity
{
	/// <summary>
	/// Относительный путь файла.
	/// </summary>
	public string RelativePath { get; set; } = null!;

	/// <summary>
	/// Имя файла.
	/// </summary>
	public string FileName { get; set; } = null!;

	/// <summary>
	/// Дата и время создания файла.
	/// </summary>
	public DateTime? CreatedAt { get; set; }

	/// <summary>
	/// Коллекция досок, связанных с данным файлом.
	/// </summary>
	public virtual ICollection<DbBoard> Boards { get; set; } = new List<DbBoard>();

	/// <summary>
	/// Коллекция вложений карточек, связанных с данным файлом.
	/// </summary>
	public virtual ICollection<DbCardAttachment> CardAttachments { get; set; } = new List<DbCardAttachment>();

	/// <summary>
	/// Коллекция карточек, связанных с данным файлом.
	/// </summary>
	public virtual ICollection<DbCard> Cards { get; set; } = new List<DbCard>();
}