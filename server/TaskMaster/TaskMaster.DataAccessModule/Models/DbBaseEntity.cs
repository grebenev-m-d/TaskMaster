using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.DataAccessModule.Models
{
	/// <summary>
	/// Базовая сущность базы данных.
	/// </summary>
	public abstract class DbBaseEntity
	{
		/// <summary>
		/// Уникальный идентификатор сущности.
		/// </summary>
		public Guid Id { get; set; }
	}
}
