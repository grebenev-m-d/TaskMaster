using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель вложения к карточке в базе данных.
/// </summary>
public partial class DbCardAttachment 
	: DbBaseEntity
{
	/// <summary>
	/// Идентификатор файла вложения.
	/// </summary>
	public Guid FileId { get; set; }

	/// <summary>
	/// Идентификатор карточки, к которой относится вложение.
	/// </summary>
	public Guid? CardId { get; set; }

	/// <summary>
	/// Время создания вложения.
	/// </summary>
	public DateTime? CreatedAt { get; set; }

	/// <summary>
	/// Карточка, к которой относится данное вложение.
	/// </summary>
	public virtual DbCard? Card { get; set; }

	/// <summary>
	/// Файл вложения.
	/// </summary>
	public virtual DbFile File { get; set; } = null!;
}
