using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting;
using AttributeRouting.Web.Mvc;
using CmsData;
using CmsWeb.Areas.People.Models.Person;
using CmsWeb.Models;
using UtilityExtensions;
using System.Text;
using System.Diagnostics;
using System.Web.Routing;
using System.Threading;
using System.Web.Security;
using CmsData.Codes;
using System.Globalization;

namespace CmsWeb.Areas.People.Controllers
{
    [ValidateInput(false)]
    [SessionExpire]
    [RouteArea("People", AreaUrl = "Person2")]
    public class PersonController : CmsStaffController
    {
        protected override void Initialize(RequestContext requestContext)
        {
            NoCheckRole = true;
            base.Initialize(requestContext);
        }
        public ActionResult Current()
        {
            return Redirect("/Person/Index/" + Util2.CurrentPeopleId);
        }
        [GET("Person2/Index/{id}")]
        [GET("{id:int}")]
        public ActionResult Index(int? id)
        {
            if (!id.HasValue)
                return Content("no id");
            if (!DbUtil.Db.UserPreference("newlook3", "false").ToBool()
                || !DbUtil.Db.UserPreference("newpeoplepage", "false").ToBool())
                return Redirect(Request.RawUrl.ToLower().Replace("person2", "person"));
            var m = new PersonModel(id);
            if (User.IsInRole("Access"))
            {
                if (m == null)
                    return Content("no person");
                if (m.displayperson == null)
                    return Content("person not found");
            }
            else
                if (m.Person == null || !m.Person.CanUserSee)
                    return Content("no access");
            if (Util2.OrgMembersOnly)
            {
                var omotag = DbUtil.Db.OrgMembersOnlyTag2();
                if (!DbUtil.Db.TagPeople.Any(pt => pt.PeopleId == id && pt.Id == omotag.Id))
                {
                    DbUtil.LogActivity("Trying to view person: {0}".Fmt(m.displayperson.Name));
                    return Content("<h3 style='color:red'>{0}</h3>\n<a href='{1}'>{2}</a>"
                        .Fmt("You must be a member one of this person's organizations to have access to this page",
                        "javascript: history.go(-1)", "Go Back"));
                }
            }
            else if (Util2.OrgLeadersOnly)
            {
                var olotag = DbUtil.Db.OrgLeadersOnlyTag2();
                if (!DbUtil.Db.TagPeople.Any(pt => pt.PeopleId == id && pt.Id == olotag.Id))
                {
                    DbUtil.LogActivity("Trying to view person: {0}".Fmt(m.displayperson.Name));
                    return Content("<h3 style='color:red'>{0}</h3>\n<a href='{1}'>{2}</a>"
                        .Fmt("You must be a leader of one of this person's organizations to have access to this page",
                        "javascript: history.go(-1)", "Go Back"));
                }
            }
            ViewData["Comments"] = Util.SafeFormat(m.Person.Comments);
            ViewData["PeopleId"] = id.Value;
            Util2.CurrentPeopleId = id.Value;
            Session["ActivePerson"] = m.displayperson.Name;
            DbUtil.LogActivity("Viewing Person: {0}".Fmt(m.displayperson.Name));
            InitExportToolbar(id);
            return View(m);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Move(int id, int to)
        {
            var p = DbUtil.Db.People.Single(pp => pp.PeopleId == id);
            try
            {
                p.MovePersonStuff(DbUtil.Db, to);
                DbUtil.Db.SubmitChanges();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return Content("ok");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Impersonate(string id)
        {
            var user = DbUtil.Db.Users.SingleOrDefault(uu => uu.Username == id);
            if (user == null)
                return Content("no user");
            if (user.Roles.Contains("Finance") && !User.IsInRole("Finance"))
                return Content("cannot impersonate finance");
            Session.Remove("CurrentTag");
            FormsAuthentication.SetAuthCookie(id, false);
            CmsWeb.Models.AccountModel.SetUserInfo(id, Session);
            Util.FormsBasedAuthentication = true;
            Util.UserPeopleId = user.PeopleId;
            Util.UserPreferredName = user.Username;
            return Redirect("/");
        }

        [HttpPost]
        public ActionResult Tag(int id)
        {
            Person.Tag(DbUtil.Db, id, Util2.CurrentTagName, Util2.CurrentTagOwnerId, DbUtil.TagTypeId_Personal);
            DbUtil.Db.SubmitChanges();
            return new EmptyResult();
        }
        [HttpPost]
        public ActionResult UnTag(int id)
        {
            Person.UnTag(id, Util2.CurrentTagName, Util2.CurrentTagOwnerId, DbUtil.TagTypeId_Personal);
            DbUtil.Db.SubmitChanges();
            return new EmptyResult();
        }
        [POST("Person2/FamilyGrid/{id}")]
        public ActionResult FamilyGrid(int id)
        {
            var m = new PersonModel(id);
            //UpdateModel(m.Pager);
            return View(m);
        }
        [POST("Person2/EnrollGrid/{id}/{page?}/{size?}/{sort?}/{dir?}")]
        public ActionResult EnrollGrid(int id, int? page, int? size, string sort, string dir)
        {
            var m = new CurrentEnrollments(id);
            m.Pager.Set("/Person2/EnrollGrid/" + id, page, size, sort, dir);
            DbUtil.LogActivity("Viewing Enrollments for: {0}".Fmt(m.person.Name));
            return View("CurrentEnrollments", m);
        }
        [POST("PrevEnrollGrid/{id}/{page?}/{size?}/{sort?}/{dir?}")]
        public ActionResult PrevEnrollGrid(int id, int? page, int? size, string sort, string dir)
        {
            var m = new PreviousEnrollments(id);
            m.Pager.Set("/Person2/PrevEnrollGrid/" + id, page, size, sort, dir);
            DbUtil.LogActivity("Viewing Prev Enrollments for: {0}".Fmt(m.person.Name));
            return View("PreviousEnrollments", m);
        }
        //		[HttpPost]
        //		public ActionResult PrevEnrollGrid(int id)
        //		{
        //			var m = new PersonPrevEnrollmentsModel(id);
        //			DbUtil.LogActivity("Viewing Prev Enrollments for: {0}".Fmt(m.person.Name));
        //			UpdateModel(m.Pager);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult PendingEnrollGrid(int id)
        //		{
        //			var m = new PersonPendingEnrollmentsModel(id);
        //			DbUtil.LogActivity("Viewing Pending Enrollments for: {0}".Fmt(m.person.Name));
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult AttendanceGrid(int id, bool? future)
        //		{
        //			var m = new PersonAttendHistoryModel(id, future == true);
        //			DbUtil.LogActivity("Viewing Attendance History for: {0}".Fmt(Session["ActivePerson"]));
        //			UpdateModel(m.Pager);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult ContactsMadeGrid(int id)
        //		{
        //			var m = new PersonContactsMadeModel(id);
        //			DbUtil.LogActivity("Viewing Contacts Tab for: {0}".Fmt(Session["ActivePerson"]));
        //			UpdateModel(m.Pager);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult ContactsReceivedGrid(int id)
        //		{
        //			var m = new PersonContactsReceivedModel(id);
        //			UpdateModel(m.Pager);
        //			return View(m);
        //		}
        [HttpPost]
        public ActionResult IncompleteTasksGrid(int id)
        {
            var m = new CmsWeb.Models.TaskModel();
            return View(m.IncompleteTasksList(id));
        }
        [HttpPost]
        public ActionResult PendingTasksGrid(int id)
        {
            var m = new CmsWeb.Models.TaskModel();
            return View(m.TasksAboutList(id));
        }
        [HttpPost]
        public ActionResult AddContactMade(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            DbUtil.LogActivity("Adding contact from: {0}".Fmt(p.Name));
            var c = new CmsData.Contact
            {
                CreatedDate = Util.Now,
                CreatedBy = Util.UserId1,
                ContactDate = Util.Now.Date,
                ContactTypeId = 99,
                ContactReasonId = 99,
            };

            DbUtil.Db.Contacts.InsertOnSubmit(c);
            DbUtil.Db.SubmitChanges();

            var cp = new Contactor
            {
                PeopleId = p.PeopleId,
                ContactId = c.ContactId
            };

            DbUtil.Db.Contactors.InsertOnSubmit(cp);
            DbUtil.Db.SubmitChanges();

            return Content("/Contact.aspx?id=" + c.ContactId);
        }
        [HttpPost]
        public ActionResult AddContactReceived(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            DbUtil.LogActivity("Adding contact to: {0}".Fmt(p.Name));
            var c = new CmsData.Contact
            {
                CreatedDate = Util.Now,
                CreatedBy = Util.UserId1,
                ContactDate = Util.Now.Date,
                ContactTypeId = 99,
                ContactReasonId = 99,
            };

            DbUtil.Db.Contacts.InsertOnSubmit(c);
            DbUtil.Db.SubmitChanges();

            var pc = new Contactee
            {
                PeopleId = p.PeopleId,
                ContactId = c.ContactId
            };

            DbUtil.Db.Contactees.InsertOnSubmit(pc);
            DbUtil.Db.SubmitChanges();

            return Content("/Contact.aspx?id=" + c.ContactId);
        }
        [HttpPost]
        public ActionResult AddAboutTask(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            if (p == null || !Util.UserPeopleId.HasValue)
                return Content("no id");
            var t = p.AddTaskAbout(DbUtil.Db, Util.UserPeopleId.Value, "Please Contact");
            DbUtil.Db.SubmitChanges();
            return Content("/Task/List/{0}".Fmt(t.Id));
        }
        [POST("Snapshot/{id}")]
        public ActionResult Snapshot(int id)
        {
            var m = new PersonModel(id);
            return View(m);
        }
        [POST("RelatedFamilies/{id}")]
        public ActionResult RelatedFamilies(int id)
        {
            var m = new PersonModel(id);
            return View(m);
        }

        [POST("PostData")]
        public ActionResult PostData(int pk, string name, string value)
        {
            var p = DbUtil.Db.LoadPersonById(pk);
            switch (name)
            {
                case "position":
                    p.PositionInFamilyId = value.ToInt();
                    break;
            }
            DbUtil.Db.SubmitChanges();
            return new EmptyResult();
        }

        [POST("Person2/Schools")]
        public JsonResult Schools(string query)
        {
            var qu = from p in DbUtil.Db.People
                     where p.SchoolOther.Contains(query)
                     group p by p.SchoolOther into g
                     select g.Key;
            return Json(qu.Take(10).ToArray(), JsonRequestBehavior.AllowGet);
        }
        [POST("Person2/Employers")]
        public JsonResult Employers(string query)
        {
            var qu = from p in DbUtil.Db.People
                     where p.EmployerOther.Contains(query)
                     group p by p.EmployerOther into g
                     select g.Key;
            return Json(qu.Take(10).ToArray(), JsonRequestBehavior.AllowGet);
        }
        [POST("Person2/Occupations")]
        public JsonResult Occupations(string query)
        {
            var qu = from p in DbUtil.Db.People
                     where p.OccupationOther.Contains(query)
                     group p by p.OccupationOther into g
                     select g.Key;
            return Json(qu.Take(10).ToArray(), JsonRequestBehavior.AllowGet);
        }
        [POST("Person2/Churches")]
        public JsonResult Churches(string query)
        {
            var qu = from r in DbUtil.Db.ViewChurches
                     where r.C.Contains(query)
                     select r.C;
            return Json(qu.Take(10).ToArray(), JsonRequestBehavior.AllowGet);
        }
        [POST("Person2/BasicDisplay/{id}")]
        public ActionResult BasicDisplay(int id)
        {
            InitExportToolbar(id);
            var m = BasicPersonInfo.GetBasicPersonInfo(id);
            return View("BasicPersonInfoDisplay", m);
        }
        [POST("Person2/BasicEdit/{id}")]
        public ActionResult BasicEdit(int id)
        {
            var m = BasicPersonInfo.GetBasicPersonInfo(id);
            return View("BasicPersonInfoEdit", m);
        }
        [POST("Person2/BasicUpdate/{id}")]
        public ActionResult BasicUpdate(int id)
        {
            var m = BasicPersonInfo.GetBasicPersonInfo(id);
            UpdateModel(m);
            m.UpdatePerson();
            m = BasicPersonInfo.GetBasicPersonInfo(id);
            DbUtil.LogActivity("Update Basic Info for: {0}".Fmt(m.person.Name));
            InitExportToolbar(id);
            return View("BasicPersonInfoDisplay", m);
        }
        [HttpPost]
        public ActionResult Reverse(int id, string field, string value, string pf)
        {
            var m = new PersonModel(id);
            m.Reverse(field, value, pf);
            return View("ChangesGrid", m);
        }
        [POST("Person2/AddressEdit/{type}/{id}")]
        public ActionResult AddressEdit(int id, string type)
        {
            var m = AddressInfo.GetAddressInfo(id, type);
            return View(m);
        }
        [POST("Person2/AddressEditAgain")]
        public ActionResult AddressEditAgain(AddressInfo m)
        {
            return View("AddressEdit", m);
        }
        [POST("Person2/AddressUpdate/{noCheck?}")]
        public ActionResult AddressUpdate(AddressInfo m, string noCheck)
		{
            if(noCheck.HasValue() == false)
                m.ValidateAddress(ModelState);
            if (!ModelState.IsValid)
                return View("AddressEdit", m);
            if (m.Error.HasValue())
            {
                ModelState.Clear();
                return View("AddressEdit", m);
            }
            m.UpdateAddress(ModelState);
            return View("AddressSaved", m);
		}
        [POST("Person2/AddressSave")]
        public ActionResult AddressSave(AddressInfo m)
		{
            m.UpdateAddress(ModelState, forceSave: true);
            return View("AddressEdit", m);
		}

        //		[HttpPost]
        //		public ActionResult MemberDisplay(int id)
        //		{
        //			var m = MemberInfo.GetMemberInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult MemberEdit(int id)
        //		{
        //			var m = MemberInfo.GetMemberInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult MemberUpdate(int id)
        //		{
        //			var m = MemberInfo.GetMemberInfo(id);
        //			UpdateModel(m);
        //			var ret = m.UpdateMember();
        //			if (ret != "ok")
        //			{
        //				ModelState.AddModelError("MemberTab", ret);
        //				return View("MemberEdit", m);
        //			}
        //			m = MemberInfo.GetMemberInfo(id);
        //			DbUtil.LogActivity("Update Member Info for: {0}".Fmt(Session["ActivePerson"]));
        //			return View("MemberDisplay", m);
        //		}
        //		[HttpPost]
        //		public ActionResult GrowthDisplay(int id)
        //		{
        //			var m = GrowthInfo.GetGrowthInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult GrowthEdit(int id)
        //		{
        //			var m = GrowthInfo.GetGrowthInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult GrowthUpdate(int id)
        //		{
        //			var m = GrowthInfo.GetGrowthInfo(id);
        //			UpdateModel(m);
        //			m.UpdateGrowth();
        //			DbUtil.LogActivity("Update Growth Info for: {0}".Fmt(Session["ActivePerson"]));
        //			return View("GrowthDisplay", m);
        //		}
        //		[HttpPost]
        //		public ActionResult CommentsDisplay(int id)
        //		{
        //			ViewData["Comments"] = Util.SafeFormat(DbUtil.Db.People.Single(p => p.PeopleId == id).Comments);
        //			ViewData["PeopleId"] = id;
        //			return View();
        //		}
        //		[HttpPost]
        //		public ActionResult CommentsEdit(int id)
        //		{
        //			ViewData["Comments"] = DbUtil.Db.People.Single(p => p.PeopleId == id).Comments;
        //			ViewData["PeopleId"] = id;
        //			return View();
        //		}
        //		[HttpPost]
        //		public ActionResult CommentsUpdate(int id, string Comments)
        //		{
        //			var p = DbUtil.Db.LoadPersonById(id);
        //			p.Comments = Comments;
        //			DbUtil.Db.SubmitChanges();
        //			ViewData["Comments"] = Util.SafeFormat(Comments);
        //			ViewData["PeopleId"] = id;
        //			DbUtil.LogActivity("Update Comments for: {0}".Fmt(Session["ActivePerson"]));
        //			return View("CommentsDisplay");
        //		}
        //		[HttpPost]
        //		public ActionResult MemberNotesDisplay(int id)
        //		{
        //			var m = MemberNotesInfo.GetMemberNotesInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult MemberNotesEdit(int id)
        //		{
        //			var m = MemberNotesInfo.GetMemberNotesInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult MemberNotesUpdate(int id)
        //		{
        //			var m = MemberNotesInfo.GetMemberNotesInfo(id);
        //			UpdateModel(m);
        //			m.UpdateMemberNotes();
        //			DbUtil.LogActivity("Update Member Notes for: {0}".Fmt(Session["ActivePerson"]));
        //			return View("MemberNotesDisplay", m);
        //		}
        //		[HttpPost]
        //		public ActionResult RecRegDisplay(int id)
        //		{
        //			var m = RecRegInfo.GetRecRegInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult RecRegEdit(int id)
        //		{
        //			var m = RecRegInfo.GetRecRegInfo(id);
        //			return View(m);
        //		}
        //		[HttpPost]
        //		public ActionResult RecRegUpdate(int id)
        //		{
        //			var m = RecRegInfo.GetRecRegInfo(id);
        //			UpdateModel(m);
        //			m.UpdateRecReg();
        //			DbUtil.LogActivity("Update Registration Tab for: {0}".Fmt(Session["ActivePerson"]));
        //			return View("RecRegDisplay", m);
        //		}
        [HttpPost]
        public ActionResult AddContact(int id)
        {
            var c = new ContentResult();
            c.Content = CmsData.Contact.AddContact(id).ToString();
            return c;
        }
        [HttpPost]
        public ActionResult AddTasks(int id)
        {
            var c = new ContentResult();
            c.Content = Task.AddTasks(id).ToString();
            return c;
        }
        [Authorize(Roles = "Admin")]
        public ActionResult UserDialog(int? id)
        {
            User u = null;
            if (id.HasValue)
                u = DbUtil.Db.Users.Single(us => us.UserId == id);
            else
            {
                u = CmsWeb.Models.AccountModel.AddUser(Util2.CurrentPeopleId);
                DbUtil.LogActivity("New User for: {0}".Fmt(Session["ActivePerson"]));
            }
            return View(u);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult UserUpdate(int id, string username, string password2, bool islockedout, string[] role)
        {
            var u = DbUtil.Db.Users.Single(us => us.UserId == id);
            if (u.Username != username)
            {
                var uu = DbUtil.Db.Users.SingleOrDefault(us => us.Username == username);
                if (uu != null)
                    return Content("error: username already exists");
            }
            u.Username = username;
            u.IsLockedOut = islockedout;
            u.SetRoles(DbUtil.Db, role, User.IsInRole("Finance"));
            if (password2.HasValue())
                u.ChangePassword(password2);
            DbUtil.Db.SubmitChanges();
            DbUtil.LogActivity("Update User for: {0}".Fmt(Session["ActivePerson"]));
            return Content("ok");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult UserWelcome(int id, string username, string password2, bool islockedout, string[] role)
        {
            var u = DbUtil.Db.Users.Single(us => us.UserId == id);
            if (u.Username != username)
            {
                var uu = DbUtil.Db.Users.SingleOrDefault(us => us.Username == username);
                if (uu != null)
                    return Content("error: username already exists");
            }
            u.Username = username;
            u.IsLockedOut = islockedout;
            u.SetRoles(DbUtil.Db, role, User.IsInRole("Finance"));
            if (password2.HasValue())
                u.ChangePassword(password2);
            DbUtil.Db.SubmitChanges();
            CmsWeb.Models.AccountModel.SendNewUserEmail(username);
            DbUtil.LogActivity("Welcome Email for: {0}".Fmt(Session["ActivePerson"]));
            return Content("ok");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult UserDelete(int id)
        {
            var Db = DbUtil.Db;
            Db.PurgeUser(id);
            return Content("ok");
        }
        [HttpPost]
        public ActionResult UserInfoGrid(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            return View(p);
        }
        [HttpPost]
        public ActionResult OptoutsGrid(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            return View(p);
        }
        [HttpPost]
        public ActionResult DeleteOptout(int id, string email)
        {
            var oo = DbUtil.Db.EmailOptOuts.SingleOrDefault(o => o.FromEmail == email && o.ToPeopleId == id);
            if (oo == null)
                return Content("not found");
            DbUtil.Db.EmailOptOuts.DeleteOnSubmit(oo);
            DbUtil.Db.SubmitChanges();
            return Content("ok");
        }
        [HttpPost]
        public ActionResult VolunteerDisplay(int id)
        {
            var m = new Main.Models.Other.VolunteerModel(id);
            return View(m);
        }
        [HttpPost]
        public ContentResult DeleteExtra(int id, string field)
        {
            var e = DbUtil.Db.PeopleExtras.First(ee => ee.PeopleId == id && ee.Field == field);
            DbUtil.Db.PeopleExtras.DeleteOnSubmit(e);
            DbUtil.Db.SubmitChanges();
            return Content("done");
        }
        [HttpPost]
        public ContentResult EditExtra(string id, string value)
        {
            var a = id.SplitStr("-", 2);
            var b = a[1].SplitStr(".", 2);
            var p = DbUtil.Db.LoadPersonById(b[1].ToInt());
            switch (a[0])
            {
                case "s":
                    p.AddEditExtraValue(b[0], value);
                    break;
                case "t":
                    p.AddEditExtraData(b[0], value);
                    break;
                case "d":
                    {
                        DateTime dt;
                        if (DateTime.TryParse(value, out dt))
                        {
                            p.AddEditExtraDate(b[0], dt);
                            value = dt.ToShortDateString();
                        }
                        else
                        {
                            p.RemoveExtraValue(DbUtil.Db, b[0]);
                            value = "";
                        }
                    }
                    break;
                case "i":
                    p.AddEditExtraInt(b[0], value.ToInt());
                    break;
                case "b":
                    if (value == "True")
                        p.AddEditExtraBool(b[0], true);
                    else
                        p.RemoveExtraValue(DbUtil.Db, b[0]);
                    break;
                case "m":
                    {
                        if (value == null)
                            value = Request.Form["value[]"];
                        var cc = Code.StandardExtraValues.ExtraValueBits(b[0], b[1].ToInt());
                        var aa = value.Split(',');
                        foreach (var c in cc)
                        {
                            if (aa.Contains(c.Key)) // checked now
                                if (!c.Value) // was not checked before
                                    p.AddEditExtraBool(c.Key, true);
                            if (!aa.Contains(c.Key)) // not checked now
                                if (c.Value) // was checked before
                                    p.RemoveExtraValue(DbUtil.Db, c.Key);
                        }
                        DbUtil.Db.SubmitChanges();
                        break;
                    }
            }
            DbUtil.Db.SubmitChanges();
            if (value == "null")
                return Content(null);
            return Content(value);
        }
        [HttpPost]
        public JsonResult ExtraValues(string id)
        {
            var a = id.SplitStr("-", 2);
            var b = a[1].SplitStr(".", 2);
            var c = Code.StandardExtraValues.Codes(b[0]);
            var j = Json(c);
            return j;
        }
        //		[HttpPost]
        //		public ContentResult EditExtra2()
        //		{
        //			var a = Request.Form["id"].SplitStr("-", 2);
        //			var b = a[1].SplitStr(".", 2);
        //			var values = Request.Form["value[]"];
        //			if (a[0] == "m")
        //			{
        //				var p = DbUtil.Db.LoadPersonById(b[1].ToInt());
        //				DbUtil.Db.SubmitChanges();
        //			}
        //			return Content(values);
        //		}
        [HttpPost]
        public JsonResult ExtraValues2(string id)
        {
            var a = id.SplitStr("-", 2);
            var b = a[1].SplitStr(".", 2);
            var c = Code.StandardExtraValues.ExtraValueBits(b[0], b[1].ToInt());
            var j = Json(c);
            return j;
        }
        [HttpPost]
        public ActionResult NewExtraValue(int id, string field, string type, string value)
        {
            var v = new PeopleExtra { PeopleId = id, Field = field };
            DbUtil.Db.PeopleExtras.InsertOnSubmit(v);
            switch (type)
            {
                case "string":
                    v.StrValue = value;
                    break;
                case "text":
                    v.Data = value;
                    break;
                case "date":
                    var dt = DateTime.MinValue;
                    DateTime.TryParse(value, out dt);
                    v.DateValue = dt;
                    break;
                case "int":
                    v.IntValue = value.ToInt();
                    break;
            }
            try
            {
                DbUtil.Db.SubmitChanges();
            }
            catch (Exception ex)
            {
                return Content("error: " + ex.Message);
            }
            return Content("ok");
        }
        [HttpPost]
        public ActionResult ExtrasGrid(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            return View(p);
        }
        [HttpPost]
        public ActionResult ChangesGrid(int id)
        {
            var m = new PersonModel(id);
            return View(m);
        }
        //		[HttpPost]
        //		public ActionResult DuplicatesGrid(int id)
        //		{
        //			var m = new DuplicatesModel(id);
        //			return View(m);
        //		}
        public ActionResult ShowMeetings(int id, bool all)
        {
            if (all == true)
                Session["showallmeetings"] = true;
            else
                Session.Remove("showallmeetings");
            return Redirect("/Person/Index/" + id);
        }
        private void InitExportToolbar(int? id)
        {
            var qb = DbUtil.Db.QueryBuilderIsCurrentPerson();
            ViewData["queryid"] = qb.QueryId;
            ViewData["TagAction"] = "/Person/Tag/" + id;
            ViewData["UnTagAction"] = "/Person/UnTag/" + id;
            ViewData["AddContact"] = "/Person/AddContactReceived/" + id;
            ViewData["AddTasks"] = "/Person/AddAboutTask/" + id;
        }
        public class CurrentRegistration
        {
            public int OrgId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public ActionResult CurrentRegistrations(bool? html)
        {
            var types = new[] 
			{
				CmsData.Codes.RegistrationTypeCode.JoinOrganization,
				CmsData.Codes.RegistrationTypeCode.ComputeOrganizationByAge2,
				CmsData.Codes.RegistrationTypeCode.UserSelectsOrganization2,
				CmsData.Codes.RegistrationTypeCode.ChooseVolunteerTimes,
			};
            var picklistorgs = DbUtil.Db.ViewPickListOrgs.Select(pp => pp.OrgId).ToArray();
            var dt = DateTime.Today;
            var q = from o in DbUtil.Db.Organizations
                    where !picklistorgs.Contains(o.OrganizationId)
                    where types.Contains(o.RegistrationTypeId ?? 0)
                    where (o.RegistrationClosed ?? false) == false
                    where (o.ClassFilled ?? false) == false
                    where o.RegEnd > dt || o.RegEnd == null
                    where o.RegStart <= dt || o.RegStart == null
                    where o.OrganizationStatusId == OrgStatusCode.Active
                    orderby o.OrganizationName
                    select new CurrentRegistration()
                    {
                        OrgId = o.OrganizationId,
                        Name = o.OrganizationName,
                        Description = o.Description
                    };
            if ((html ?? false) == true)
                return View("CurrentRegistrationsHtml", q);
            return View(q);
        }
        public ActionResult ContributionStatement(int id, string fr, string to)
        {
            if (Util.UserPeopleId != id && !User.IsInRole("Finance"))
                return Content("No permission to view statement");
            var p = DbUtil.Db.LoadPersonById(id);
            if (p == null)
                return Content("Invalid Id");

            var frdt = Util.ParseMMddyy(fr);
            var todt = Util.ParseMMddyy(to);
            if (!(frdt.HasValue && todt.HasValue))
                return Content("date formats invalid");

            DbUtil.LogActivity("Contribution Statement for ({0})".Fmt(id));

            return new CmsWeb.Areas.Finance.Models.Report.ContributionStatementResult
            {
                PeopleId = id,
                FromDate = frdt.Value,
                ToDate = todt.Value,
                typ = p.PositionInFamilyId == PositionInFamily.PrimaryAdult ? 2 : 1,
                noaddressok = true,
                useMinAmt = false,
            };
        }
        [GET("Person2/Image/{s}/{id}/{v}")]
        public ActionResult Image(int id, int s, string v)
        {
            return new PictureResult(id, s);
        }
        [POST("Person2/EditRelation/{id1}/{id2}")]
        public ContentResult EditRelation(int id1, int id2, string value)
        {
            var r = DbUtil.Db.RelatedFamilies.SingleOrDefault(m => m.FamilyId == id1 && m.RelatedFamilyId == id2);
            r.FamilyRelationshipDesc = value;
            DbUtil.Db.SubmitChanges();
            return Content(value);
        }
        [POST("Person2/DeleteRelation/{id}/{id1}/{id2}")]
        public ActionResult DeleteRelation(int id, int id1, int id2)
        {
            var r = DbUtil.Db.RelatedFamilies.SingleOrDefault(rf => rf.FamilyId == id1 && rf.RelatedFamilyId == id2);
            DbUtil.Db.RelatedFamilies.DeleteOnSubmit(r);
            DbUtil.Db.SubmitChanges();
            var m = new PersonModel(id);
            return View("RelatedFamilies", m);
        }

        [POST("Person2/Split/{id}")]
        [Authorize(Roles = "Edit")]
        public ActionResult Split(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            var f = new Family
            {
                CreatedDate = Util.Now,
                CreatedBy = Util.UserId1,
                AddressLineOne = p.PrimaryAddress,
                AddressLineTwo = p.PrimaryAddress2,
                CityName = p.PrimaryCity,
                StateCode = p.PrimaryState,
                ZipCode = p.PrimaryZip,
                HomePhone = p.Family.HomePhone
            };
            f.People.Add(p);
            DbUtil.Db.Families.InsertOnSubmit(f);
            DbUtil.Db.SubmitChanges();
            DbUtil.LogActivity("Splitting Family for {0}".Fmt(p.Name));
            var m = new PersonModel(id);
            return Content("/Person2/" + id);
        }
        [Authorize(Roles = "Admin")]
        [POST("Person2/Delete/{id}")]
        public ActionResult Delete(int id)
        {
            Util.Auditing = false;
            var person = DbUtil.Db.LoadPersonById(id);
            if (person == null)
                return Content("error, bad peopleid");

            var p = person.Family.People.FirstOrDefault(m => m.PeopleId != id);
            if (p != null)
            {
                Util2.CurrentPeopleId = p.PeopleId;
                Session["ActivePerson"] = p.Name;
            }
            else
            {
                Util2.CurrentPeopleId = 0;
                Session.Remove("ActivePerson");
            }

            DbUtil.Db.PurgePerson(id);

            DbUtil.LogActivity("Deleted Record {0}".Fmt(person.PeopleId));
            return Content("/Person2/DeletedPerson");
        }

        [GET("Person2/DeletedPerson")]
        public ActionResult DeletedPerson()
        {
            return View();
        }
    }
}
