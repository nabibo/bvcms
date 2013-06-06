using System.Web.Mvc;

namespace CmsWeb.Areas.Org
{
    public class OrgAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Org";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            AddRoute(context, "OrgMemberDialog", "OrgMemberDialog2");
            context.MapRoute(
                "Org_default",
                "Org/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
        private void AddRoute(AreaRegistrationContext context, string controller)
        {
            context.MapRoute(controller, controller + "/{action}/{id}",
                new { controller, action = "Index", id = "" });
        }
        private static void AddRoute(AreaRegistrationContext context, string name, string controller, string path, string action)
        {
            context.MapRoute(name, path,
                new { controller, action = action, id = "" });
        }
        private static void AddRoute(AreaRegistrationContext context, string controller, string path)
        {
            context.MapRoute(path, path + "/{action}/{id}",
                new { controller, action = "Index", id = "" });
        }
    }
}
