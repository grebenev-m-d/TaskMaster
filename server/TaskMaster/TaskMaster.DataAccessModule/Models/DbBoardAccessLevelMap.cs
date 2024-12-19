using System;
using System.Collections.Generic;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель уровня доступа к доске для пользователя в базе данных.
/// </summary>
public partial class DbBoardAccessLevelMap
{
	/// <summary>
	/// Идентификатор доски.
	/// </summary>
	public Guid BoardId { get; set; }

	/// <summary>
	/// Идентификатор пользователя.
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Идентификатор уровня доступа.
	/// </summary>
	public Guid? AccessLevelId { get; set; }

	/// <summary>
	/// Уровень доступа.
	/// </summary>
	public virtual DbAccessLevel? AccessLevel { get; set; }

	/// <summary>
	/// Доска.
	/// </summary>
	public virtual DbBoard Board { get; set; } = null!;

	/// <summary>
	/// Пользователь.
	/// </summary>
	public virtual DbUser User { get; set; } = null!;
}
