using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель пользователя в базе данных.
/// </summary>
public partial class DbUser
	: DbBaseEntity
{
	/// <summary>
	/// Имя пользователя.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Хэш пароля пользователя.
	/// </summary>
	public string PasswordHash { get; set; } = null!;

	/// <summary>
	/// Адрес электронной почты пользователя.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// Дата и время создания записи о пользователе.
	/// </summary>
	public DateTime? CreatedAt { get; set; }

	/// <summary>
	/// Идентификатор роли пользователя.
	/// </summary>
	public Guid? RoleId { get; set; }

	/// <summary>
	/// Коллекция связей между досками и уровнями доступа пользователя.
	/// </summary>
	public virtual ICollection<DbBoardAccessLevelMap> BoardAccessLevelMaps { get; set; } = new List<DbBoardAccessLevelMap>();

	/// <summary>
	/// Коллекция истории просмотров досок пользователем.
	/// </summary>
	public virtual ICollection<DbBoardViewHistoryMap> BoardViewHistoryMaps { get; set; } = new List<DbBoardViewHistoryMap>();

	/// <summary>
	/// Коллекция досок, связанных с пользователем.
	/// </summary>
	public virtual ICollection<DbBoard> Boards { get; set; } = new List<DbBoard>();

	/// <summary>
	/// Коллекция комментариев пользователя к карточкам.
	/// </summary>
	public virtual ICollection<DbCardComment> CardComments { get; set; } = new List<DbCardComment>();

	/// <summary>
	/// Роль пользователя.
	/// </summary>
	public virtual DbRole? Role { get; set; }
}

