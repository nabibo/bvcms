using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Views;
using CmsData;
using UtilityExtensions;

namespace CmsWeb
{
    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        public void Configure(BundleCollection bundles)
        {
            bundles.Add<StylesheetBundle>("css", new[] 
            { 
                  "Content/styles/jquery-ui-1.10.0.custom.css"
                , "Content/styles/dropdown.css"
                , "Content/styles/site.css"
                , "Content/styles/style2.css"
                , "Content/styles/pager.css"
                , "Content/styles/jquery.jscrollpane.css"
                , "Content/styles/joyride-1.0.5.css"
                , "Content/styles/jquery.multiselect.css"
                , "Content/styles/jquery.tooltip.css"
                , "Content/css/font-awesome.css"
        	});

            bundles.Add<StylesheetBundle>("newcss", new[]
            {
                  "Content/styles/jquery-ui-1.10.0.custom.css"
                , "Content/styles/jquery.jscrollpane.css"
                , "Content/styles/joyride-1.0.5.css"
                , "Content/styles/jquery.multiselect.css"
                , "Content/styles/jquery.tooltip.css"
                , "Content/styles/jquery.alerts.css"
                , "Content/styles/layout.css"
                , "Content/styles/Main.css"
                , "Content/styles/pager.css"
                , "Content/css/font-awesome.css"
//              , "Content/css/chosen.css"
            });
            bundles.Add<StylesheetBundle>("new2css", new[]
            {
                  "Content/css/normalize.css"
                , "Content/css/bvcms.css"
                , "Content/bootstrap-modal/css/bootstrap-modal.css"
              //, "Content/styles/joyride-1.0.5.css"
                // Dave : need example of bootstrap tooltip
              //, "Content/styles/jquery.tooltip.css"
                // Dave : need example of alert
              //, "Content/styles/jquery.alerts.css"
                // Dave : where should I put any tweaks in css?
                , "Content/css/font-awesome.css"
                , "Content/css/chosen.css"
                , "Content/moment-datepicker/datepicker.css"
                , "Content/css/bootstrap-editable.css"
                , "Content/jquery-dialog2/jquery.dialog2.css"
                , "Content/css/main.css"
            });
            bundles.Add<StylesheetBundle>("orgcss", new[]
            {
                "Content/styles/organization.css"
            });
            bundles.Add<StylesheetBundle>("dialogcss", new[]
            {
                "Content/styles/Dialog.css"
            });
            bundles.Add<ScriptBundle>("js", new[]
            {
                  "Scripts/jquery/jquery-1.9.1.js"
                , "Scripts/jquery/jquery-migrate-1.1.1.js"
                , "Scripts/jquery/jquery-ui-1.10.0.custom.js"
                , "Scripts/jquery/jquery.cookie.js"
                , "Scripts/jquery/jquery.blockUI.js"
                , "Scripts/jquery/jquery.mousewheel.js"
                , "Scripts/jquery/jquery.jscrollpane.js"
                , "Scripts/jquery/jquery.jeditable.js"
                , "Scripts/jquery/jquery.validate.js"
                , "Scripts/jquery/jquery.joyride-1.0.5.js"
                , "Scripts/modernizr-2.6.2.js"
                , "Scripts/tutorial.js"
                , "Scripts/jquery/jquery.multiSelect.js"
                , "Scripts/jquery/jquery.sortElements.js"
                , "Scripts/jquery/jquery.textarea.js"
                , "Scripts/jquery/jquery.transpose.js"
                , "Scripts/jquery/jquery.tooltip.js"
                , "Scripts/js/dropdown.js"
                , "Scripts/js/ExportToolBar.js"
                , "Scripts/js/Pager.js"
                , "Scripts/js/headermenu.js"
            });
            bundles.Add<ScriptBundle>("main", new[]
            {
                  "Scripts/jquery/jquery-1.9.1.js"
                , "Scripts/jquery/jquery-migrate-1.1.1.js"
                , "Scripts/jquery/jquery-ui-1.10.0.custom.js"
                , "Scripts/jquery/jquery.cookie.js"
                , "Scripts/jquery/jquery.blockUI.js"
                , "Scripts/jquery/jquery.mousewheel.js"
                , "Scripts/jquery/jquery.jscrollpane.js"
                , "Scripts/jquery/jquery.jeditable.js"
                , "Scripts/jquery/jquery.validate.js"
                , "Scripts/jquery/jquery.joyride-1.0.5.js"
                , "Scripts/modernizr-2.6.2.js"
                , "Scripts/tutorial.js"
                , "Scripts/jquery/jquery.multiSelect.js"
                , "Scripts/jquery/jquery.sortElements.js"
                , "Scripts/jquery/jquery.textarea.js"
                , "Scripts/jquery/jquery.tooltip.js"
                , "Scripts/jquery/jquery.transpose.js"
                , "Scripts/jquery/jquery.alerts.js"
                , "Scripts/chosen/chosen.jquery.js"
                , "Scripts/js/dropdown.js"
                , "Scripts/js/Pager.js"
                , "Scripts/js/ExportToolBar.js"
                , "Scripts/js/headermenu.js"
            });
            bundles.Add<ScriptBundle>("main2", new[]
            {
                  "Scripts/jquery/jquery-1.9.1.js"
                , "Scripts/jquery/jquery-migrate-1.1.1.js"
                , "Scripts/bootstrap/bootstrap.js"
                , "Scripts/bootstrap/bootstrap-modalmanager.js"
                , "Scripts/bootstrap/bootstrap-modal.js"
                , "Scripts/bootstrap/bootstrap-editable.js"
//                , "Scripts/bootstrap/bootbox.js"
//                , "Scripts/jquery/jquery.controls.js"
//                , "Scripts/jquery/jquery.form.js"
//                , "Scripts/bootstrap/jquery.dialog2.js"
//                , "Scripts/bootstrap/jquery.dialog2.helpers.js"
                , "Scripts/jquery/jquery.cookie.js"
                , "Scripts/jquery/jquery.blockUI.js"
                , "Scripts/jquery/jquery.mousewheel.js"
                , "Scripts/jquery/jquery.validate.js"
                , "Scripts/jquery/jquery.joyride-1.0.5.js"
                , "Scripts/modernizr-2.6.2.js"
                , "Scripts/tutorial.js"
                , "Scripts/jquery/jquery.sortElements.js"
                , "Scripts/jquery/jquery.textarea.js"
                , "Scripts/jquery/jquery.tooltip.js"
                , "Scripts/jquery/jquery.alerts.js"
                , "Scripts/moment/moment.js"
                , "Scripts/moment/moment-datepicker.js"
                , "Scripts/Autocomplete/jquery-ui-1.10.2.custom.js"
                , "Scripts/chosen/chosen.jquery.js"
                , "Scripts/js/Pager3.js"
//                , "Scripts/ExportToolBar.js"
                , "Scripts/js/headermenu2.js"
            });
            bundles.Add<ScriptBundle>("onlineregister", new[]
            {
                  "Scripts/jquery/jquery-1.9.1.js"
                , "Scripts/jquery/jquery-migrate-1.1.1.js"
                , "Scripts/jquery/jquery-ui-1.10.0.custom.js"
                , "Scripts/jquery/jquery.validate.js"
                , "Scripts/jquery/jquery.idle-timer.js"
                , "Scripts/jquery/jquery.blockUI.js"
                , "Scripts/jquery/jquery.sortElements.js"
                , "Scripts/jquery/jquery.showpassword.js"
            });
            bundles.Add<ScriptBundle>("org", new[]
            {
                "Scripts/jquery/jquery-ui-timepicker-addon.js",
                "Scripts/Org/Organization.js",
                "Scripts/Dialog/SearchUsers.js",
                "Scripts/js/RegSetting.js"
            });
            bundles.Add<ScriptBundle>("task", new[]
            {
                "Scripts/jquery/jquery.form.js",
                "Scripts/jquery/jquery.form2.js",
                "Scripts/js/Task.js"
            });
            bundles.Add<ScriptBundle>("querybuilder", new[]
            {
                "Scripts/jquery/jquery.contextmenu.r2.js",
                "Scripts/Search/QueryBuilder.js"
            });
            bundles.Add<ScriptBundle>("person12", new[]
            {
                 "Scripts/jquery/jquery.address-1.5.js"
                ,"Scripts/People/person1.js"
                ,"Scripts/People/SearchAdd2.js"
//                ,"Scripts/Bootstrap/bootstrap-ajax.js"
//                ,"Scripts/Bootstrap/spin.min.js"
//                ,"Scripts/Bootstrap/bootstrapx-clickover.js"
//              ,"Scripts/js/person2.js"
            });
            AddOneScript(bundles, "Admin/Divisions");
            AddOneScript(bundles, "Admin/merge");
            AddOneScript(bundles, "Admin/TransactionHistory");
            AddOneScript(bundles, "Admin/Users");

            AddOneScript(bundles, "Dialog/OrgMemberDialog");
            AddOneScript(bundles, "Dialog/OrgMembersDialog");
            AddOneScript(bundles, "Dialog/UserDialog");
            AddOneScript(bundles, "Dialog/SearchAdd");
            AddOneScript(bundles, "Dialog/SearchDivisions");
            AddOneScript(bundles, "Dialog/SearchUsers");

            AddOneScript(bundles, "Email/compose");
            AddOneScript(bundles, "Email/Email");

            AddOneScript(bundles, "Finance/Bundle");
            AddOneScript(bundles, "Finance/Bundles");
            //AddOneScript(bundles, "Contributions");
            AddOneScript(bundles, "Finance/Funds");
            AddOneScript(bundles, "Finance/PostBundle");
            AddOneScript(bundles, "Finance/TotalsByFund");
            AddOneScript(bundles, "Finance/TotalsByRange");
            AddOneScript(bundles, "Finance/Transactions");

            AddOneScript(bundles, "js/CheckIn");
            AddOneScript(bundles, "js/CheckinActivity");
            AddOneScript(bundles, "js/ChurchAttendance");
            AddOneScript(bundles, "js/HomeTest");
            AddOneScript(bundles, "js/RegSettings");
            AddOneScript(bundles, "js/Tags");
            AddOneScript(bundles, "js/TestAPI");

            AddOneScript(bundles, "Meeting/meeting-ticket");
            AddOneScript(bundles, "Meeting/meeting");
            AddOneScript(bundles, "Meeting/Meetings");

            AddOneScript(bundles, "OnlineReg/OnlineReg");
            AddOneScript(bundles, "OnlineReg/OnlineRegPayment");

            AddOneScript(bundles, "Org/OrgChildren");
            AddOneScript(bundles, "Org/OrgGroups");
            AddOneScript(bundles, "Org/OrgMembers");
            AddOneScript(bundles, "Org/OrgMemberDialog2");
            AddOneScript(bundles, "Org/OrgSearch");

            AddOneScript(bundles, "People/family");
            AddOneScript(bundles, "People/Person");
            AddOneScript(bundles, "People/Person2");
            AddOneScript(bundles, "People/SearchAdd2");

            AddOneScript(bundles, "Search/AdvancedSearch");
            AddOneScript(bundles, "Search/ContactSearch");
            AddOneScript(bundles, "Search/QuickSearch");
            AddOneScript(bundles, "Search/SavedQuery");
            AddOneScript(bundles, "Search/PeopleSearch");
            AddOneScript(bundles, "Search/SearchOrgs");

            AddOneScript(bundles, "Volunteer/ManageVolunteer");
            AddOneScript(bundles, "Volunteer/ManageVolunteer2");
            AddOneScript(bundles, "Volunteer/Calendar");
            AddOneScript(bundles, "Volunteer/Calendar2");
            AddOneScript(bundles, "Volunteer/volunteering");
        }
        private static void AddOneScript(BundleCollection bundles, string f)
        {
            var a = f.Split('/');
            bundles.Add<ScriptBundle>(a[1], new[] { "Scripts/{0}.js".Fmt(f) });
        }
        public static void BundleRefJs()
        {
            var newlook = DbUtil.Db.UserPreference("newlook3", "false") == "true";
            Bundles.Reference(newlook ? "main" : "js");
        }

        public static void BundleRefCss()
        {
            var newlook = DbUtil.Db.UserPreference("newlook3", "false") == "true";
            Bundles.Reference(newlook ? "newcss" : "css");
        }
    }
}