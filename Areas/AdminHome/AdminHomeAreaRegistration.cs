using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome
{
    public class AdminHomeAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AdminHome";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AdminHome_default",
                "AdminHome/{controller}/{action}/{id}",
                new { action = "TrangChu", id = UrlParameter.Optional }
            );
        }
    }
}