using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель комментария к карточке в базе данных.
/// </summary>
public partial class DbCardComment 
	: DbBaseEntity
{
	/// <summary>
	/// Текст комментария.
	/// </summary>
	public string Text { get; set; } = null!;

	/// <summary>
	/// Время создания комментария.
	/// </summary>
	public DateTime? CreatedAt { get; set; }

	/// <summary>
	/// Время последнего обновления комментария.
	/// </summary>
	public DateTime? UpdatedAt { get; set; }

	/// <summary>
	/// Идентификатор карточки, к которой относится комментарий.
	/// </summary>
	public Guid? CardId { get; set; }

	/// <summary>
	/// Идентификатор пользователя, создавшего комментарий.
	/// </summary>
	public Guid? UserId { get; set; }

	/// <summary>
	/// Карточка, к которой относится данный комментарий.
	/// </summary>
	public virtual DbCard? Card { get; set; }

	/// <summary>
	/// Пользователь, создавший комментарий.
	/// </summary>
	public virtual DbUser? User { get; set; }
}