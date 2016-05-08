using SilentAuction.Models;
using System.Web;
using System.Web.Mvc;

namespace SilentAuction
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());

		}
	}
}
