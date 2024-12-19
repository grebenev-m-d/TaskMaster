using System.Data;
using TaskMaster.AuthWebApi.Models;
using TaskMaster.DataAccessModule.Models;

namespace TaskMaster.AuthWebApi.Helpers
{
	public class СlientModelMappers
	{
		public static JwtPayload MapDbUserToJwtPayload(DbUser dbUser)
		{
			return new JwtPayload()
			{
				Subject = dbUser.Id.ToString(),
				Name = dbUser.Name,
				Email = dbUser.Email,
				Role = dbUser.Role.Type
			};
		}
	}
}
