using System;
using System.Collections.Generic;
using TaskMaster.DataAccessModule.Constants;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.DataAccessModule.Models;

/// <summary>
/// Модель роли в базе данных.
/// </summary>
public partial class DbRole
	: DbBaseEntity
{
	/// <summary>
	/// Тип роли.
	/// </summary>
	public RoleType Type { get; set; }

	/// <summary>
	/// Коллекция пользователей, связанных с данной ролью.
	/// </summary>
	public virtual ICollection<DbUser> Users { get; set; } = new List<DbUser>();
}