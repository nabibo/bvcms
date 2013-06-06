using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AttributeRouting;
using AttributeRouting.Web.Mvc;
using CmsData;
using CmsWeb.Areas.People.Models;
using CmsWeb.Code;
using UtilityExtensions;
using System.Text.RegularExpressions;
using System.Data.Linq;
using CmsData.Codes;

namespace CmsWeb.Areas.People.Controllers
{
    [RouteArea("People", AreaUrl = "SearchAdd2")]
    public class SearchAddController : CmsStaffController
    {
        [POST("Index/{type}/{typeid}")]
        public ActionResult Index(string type, string typeid)
        {
            var m = new SearchAddModel { typeid = typeid, type = type };
            Organization org = null;
            m.CampusId = null;
            switch (m.type)
            {
                case "addpeople":
                    m.EntryPointId = 0;
                    break;
                case "addtotag":
                    m.EntryPointId = null;
                    break;
                case "family":
                case "relatedfamily":
                    m.EntryPointId = 0;
                    break;
                case "org":
                case "pending":
                    org = DbUtil.Db.LoadOrganizationById(typeid.ToInt());
                    m.CampusId = org.CampusId;
                    m.EntryPointId = org.EntryPointId ?? 0;
                    break;
                case "visitor":
                case "registered":
                    org = (from meeting in DbUtil.Db.Meetings
                           where meeting.MeetingId == typeid.ToInt()
                           select meeting.Organization).Single();
                    m.EntryPointId = org.EntryPointId ?? 0;
                    m.CampusId = org.CampusId;
                    break;
                case "contactee":
                    m.EntryPointId = 0;
                    break;
                case "contactor":
                    m.EntryPointId = 0;
                    break;
                case "contributor":
                    m.EntryPointId = 0;
                    break;
            }
            return View("SearchPerson", m);
        }
        [POST("Results")]
        public ActionResult Results(SearchAddModel m)
        {
            DbUtil.Db.SetNoLock();
            return View(m);
        }
        [HttpPost]
        public ActionResult ResultsFamily(SearchAddModel m)
        {
            DbUtil.Db.SetNoLock();
            return View(m);
        }
        [POST("ResultsNext/{todo}")]
        public ActionResult ResultsNext(string todo, SearchAddModel m)
        {
            switch (todo)
            {
                case "BackToSearch":
                    return View("SearchPerson", m);
                case "AddNewPerson":
                    return View("SearchPerson", m);
                case "AddNewFamily":
                    return View("SearchPerson", m);
            }
            m.dob = null;
            var a = m.Name.SplitStr(" ");
            m.Name = a[a.Length - 1];
            return Content("ok");
        }
        [POST("ListNext/{todo}")]
        public ActionResult ListNext(string todo, SearchAddModel m)
        {
            switch (todo)
            {
                case "CommitAdd":
                    return Complete(m);
                case "AnotherSearch":
                    return View("SearchPerson", m);
                case "LastFamily":
                    return AddToFamily(m);
            }
            m.dob = null;
            var a = m.Name.SplitStr(" ");
            m.Name = a[a.Length - 1];
            return Content("ok");
        }
        [HttpPost]
        public ActionResult SearchCancel(SearchAddModel m)
        {
            if (m.List.Count > 0)
                return View("List", m);
            m.typeid = "0";
            return Complete(m);
        }
        [HttpPost]
        public ActionResult SearchFamilyCancel(SearchAddModel m)
        {
            return View("SearchPerson", m);
        }
        [HttpPost]
        public ActionResult PersonCancel(int id, SearchAddModel m)
        {
            m.List.RemoveAt(id);
            if (m.List.Count > 0)
                return View("List", m);
            return View("SearchPerson", m);
        }
        [POST("Select/{id}")]
        public ActionResult Select(int id, SearchAddModel m)
        {
            if (m.List.AsEnumerable().Any(li => li.PeopleId == id))
                return View("List", m);

            var cv = new CodeValueModel();
            var p = DbUtil.Db.LoadPersonById(id);
            var s = new SearchPersonModel
            {
                PeopleId = id,
                FamilyId = m.type == "family" ? m.typeid.ToInt() : p.FamilyId,
                First = p.FirstName,
                GoesBy = p.NickName,
                Last = p.LastName,
                Marital = new CodeInfo(p.GenderId, cv.MaritalStatusCodes99()),
                Email = p.EmailAddress,
                Suffix = p.SuffixCode,
                Title = p.TitleCode,
                dob = p.DOB,
                Gender = new CodeInfo(p.GenderId, cv.GenderCodesWithUnspecified()),
                Phone = p.CellPhone,
                context = m.type,
                EntryPoint = new CodeInfo(p.EntryPointId, cv.EntryPoints()),
                Campus = new CodeInfo(p.CampusId, cv.AllCampuses0()),
            };
            s.LoadFamily();
            m.List.Add(s);
			if (m.OnlyOne)
				return Complete(m);
            return View("List", m);
        }

        [HttpPost]
        public ActionResult AddNewFamily(SearchAddModel m)
        {
            var p = m.List[m.List.Count - 1];
            p.ValidateModelForNew(ModelState, true);
            if (!ModelState.IsValid)
                return View("FormFull", m);
            return View("List", m);
        }
        [HttpPost]
        public ActionResult AddToFamily(SearchAddModel m)
        {
            var p = m.List[m.List.Count - 1];
            p.ValidateModelForNew(ModelState, false);
            if (!ModelState.IsValid)
                return View("FormAbbreviated", m);
            return View("List", m);
        }
        private SearchPersonModel NewPerson(int FamilyId, SearchAddModel m)
        {
            var cv = new CodeValueModel();
            var p = new SearchPersonModel
            {
                FamilyId = FamilyId,
                index = m.List.Count,
                Gender = new CodeInfo(99, cv.GenderCodesWithUnspecified()),
                Marital = new CodeInfo(99, cv.MaritalStatusCodes99()),
                Campus = new CodeInfo(m.CampusId, cv.AllCampuses0()),
                EntryPoint = new CodeInfo(m.EntryPointId, cv.EntryPoints()),
                context = m.type,
            };
#if DEBUG
            p.First = "David";
            p.Last = "Carr." + DateTime.Now.Millisecond;
            p.Gender = new CodeInfo(0, cv.GenderCodesWithUnspecified());
            p.Marital = new CodeInfo(0, cv.MaritalStatusCodes99());
            p.dob = "na";
            p.Email = "na";
            p.Phone = "na";
            p.Address = "na";
            p.Zip = "na";
            p.HomePhone = "na";
#endif
            m.List.Add(p);
            return p;
        }
        [HttpPost]
        public ActionResult FormAbbreviated(int id, SearchAddModel m)
        {
            var org = DbUtil.Db.LoadOrganizationById(id);
            var p = NewPerson(id, m);
            if (id < 0)
            {
                var f = m.List.FirstOrDefault(fm => fm.FamilyId == id);
                p.Address = f.Address;
                p.City = f.City;
                p.State = f.State;
                p.Zip = f.Zip;
                p.HomePhone = f.HomePhone;
            }
            else
                p.LoadFamily();
            return View(m);
        }
        [POST("FormFull")]
        public ActionResult FormFull(SearchAddModel m)
        {
            int id = 0;
            if (m.List.Count > 0)
                id = m.List.Min(i => i.FamilyId) - 1;
            if (id >= 0)
                id = -1;
            var p = NewPerson(id, m);
#if DEBUG
            //p.address = "235 Riveredge Cv";
            //p.city = "Cordova";
            //p.state = "TN";
            //p.zip = "38018";
            //p.homephone = "9017581862";
#endif
            return View(m);
        }
        private ActionResult Complete(SearchAddModel m)
        {
            var id = m.typeid;
			var iid = m.typeid.ToInt();
            switch (m.type)
            {
                case "addpeople":
                    return AddPeople(m, OriginCode.MainMenu);
                case "addtotag":
                    return AddPeopleToTag(id, m, 0);
                case "family":
                    return AddFamilyMembers(iid, m, OriginCode.NewFamilyMember);
                case "relatedfamily":
                    return AddRelatedFamilys(iid, m, OriginCode.NewFamilyMember);
                case "org":
                    return AddOrgMembers(iid, m, false, OriginCode.Enrollment);
                case "pending":
                    return AddOrgMembers(iid, m, true, OriginCode.Enrollment);
                case "visitor":
                    return AddVisitors(iid, m, OriginCode.Visit);
                case "registered":
                    return AddRegistered(iid, m, OriginCode.Visit);
                case "contactee":
                    return AddContactees(iid, m, OriginCode.Visit);
                case "contactor":
                    return AddContactors(iid, m, 0);
                case "contributor":
                    return AddContributor(iid, m, OriginCode.Contribution);
                case "taskdelegate":
                    if (m.List.Count > 0)
                        return Json(new { close = true, how = "addselected", url="/Task/Delegate/", pid = m.List[0].PeopleId });
                    break;
                case "taskdelegate2":
                    if (m.List.Count > 0)
                        return Json(new { close = true, how = "addselected2", url = "/Task/Action/", pid = m.List[0].PeopleId });
                    break;
                case "taskabout":
                    if (m.List.Count > 0)
                        return Json(new { close = true, how = "addselected", url = "/Task/ChangeAbout/", pid = m.List[0].PeopleId });
                    break;
                case "mergeto":
                    if (m.List.Count > 0)
						return Json(new { close = true, how = "addselected", pid = m.List[0].PeopleId });
                    break;
                case "taskowner":
                    if (m.List.Count > 0)
                        return Json(new { close = true, how = "addselected", url = "/Task/ChangeOwner/", pid = m.List[0].PeopleId });
                    break;
            }
            return Json(new { close = true });
        }

        private JsonResult AddContactees(int id, SearchAddModel m, int origin)
        {
            if (id > 0)
            {
                var c = DbUtil.Db.Contacts.Single(ct => ct.ContactId == id);
                foreach (var p in m.List)
                {
                    AddPerson(p, m.List, OriginCode.Visit, m.EntryPointId);
                    var ctee = c.contactees.SingleOrDefault(ct =>
                        ct.ContactId == id && ct.PeopleId == p.person.PeopleId);
                    if (ctee == null)
                    {
                        ctee = new Contactee
                        {
                            ContactId = id,
                            PeopleId = p.person.PeopleId,
                        };
                        c.contactees.Add(ctee);
                    }
                }
                DbUtil.Db.SubmitChanges();
            }
            return Json(new { close = true, how = "addselected" });
        }
        private JsonResult AddContactors(int id, SearchAddModel m, int origin)
        {
            if (id > 0)
            {
                var c = DbUtil.Db.Contacts.SingleOrDefault(ct => ct.ContactId == id);
                if (c == null)
                    return Json(new { close = true, how = "CloseAddDialog" });
                foreach (var p in m.List)
                {
                    AddPerson(p, m.List, origin, m.EntryPointId);
                    var ctor = c.contactsMakers.SingleOrDefault(ct =>
                        ct.ContactId == id && ct.PeopleId == p.person.PeopleId);
                    if (ctor == null)
                    {
                        ctor = new Contactor
                        {
                            ContactId = id,
                            PeopleId = p.person.PeopleId,
                        };
                        c.contactsMakers.Add(ctor);
                    }
                }
                DbUtil.Db.SubmitChanges();
            }
            return Json(new { close = true, how = "addselected" });
        }
        private JsonResult AddFamilyMembers(int id, SearchAddModel m, int origin)
        {
            if (id > 0)
            {
                var f = DbUtil.Db.Families.Single(fa => fa.FamilyId == id);

                foreach (var i in m.List)
                {
                    var isnew = i.IsNew;
                    AddPerson(i, m.List, origin, m.EntryPointId);
                    if (!isnew)
                    {
                        var fm = f.People.SingleOrDefault(fa => fa.PeopleId == i.person.PeopleId);
                        if (fm != null)
                            continue; // already a member of this family

                        if (i.person.Age < 18)
                            i.person.PositionInFamilyId = PositionInFamily.Child;
                        else if (i.family.People.Count(per =>
                                    per.PositionInFamilyId == PositionInFamily.PrimaryAdult)
                                    < 2)
                            i.person.PositionInFamilyId = PositionInFamily.PrimaryAdult;
                        else
                            i.person.PositionInFamilyId = PositionInFamily.SecondaryAdult;
                        i.family.People.Add(i.person); // add selected person to target family
                    }
                }
                DbUtil.Db.SubmitChanges();
            }
            return Json(new { close = true, how = "addselected" });
        }
        private JsonResult AddRelatedFamilys(int id, SearchAddModel m, int origin)
        {
            if (id > 0)
            {
                foreach (var p in m.List)
                {
                    AddPerson(p, m.List, origin, m.EntryPointId);
                    SearchAddModel.AddRelatedFamily(id, p.PeopleId.Value);
                }
                DbUtil.Db.SubmitChanges();
            }
            return Json(new { close = true, how = "addselected" });
        }
        private JsonResult AddPeople(SearchAddModel m, int origin)
        {
            foreach (var p in m.List)
                AddPerson(p, m.List, origin, m.EntryPointId);
            DbUtil.Db.SubmitChanges();
            return Json(new { close = true, how = "CloseAddDialog" });
        }
        private JsonResult AddOrgMembers(int id, SearchAddModel m, bool pending, int origin)
        {
            string message = null;
            if (id > 0)
            {
                var org = DbUtil.Db.LoadOrganizationById(id);
                if (pending == false && m.List.Count == 1 && org.AllowAttendOverlap != true)
                {
                    var om = DbUtil.Db.OrganizationMembers.FirstOrDefault(mm => 
                        mm.OrganizationId != id
                        && mm.MemberTypeId != 230 // inactive
                        && mm.MemberTypeId != 500 // inservice
                        && mm.Organization.AllowAttendOverlap != true
                        && mm.PeopleId == m.List[0].PeopleId
                        && mm.Organization.OrgSchedules.Any(ss => 
                            DbUtil.Db.OrgSchedules.Any(os => 
                                os.OrganizationId == id 
                                && os.ScheduleId == ss.ScheduleId)));
                    if (om != null)
                    {
                        message = "Already a member of {0} (orgid) with same schedule".Fmt(om.OrganizationId);
                        return Json(new { close = true, how = "CloseAddDialog", message = message });
                    }
                }
                foreach (var p in m.List)
                {
                    AddPerson(p, m.List, origin, m.EntryPointId);
                    OrganizationMember.InsertOrgMembers(DbUtil.Db,
                        id, p.PeopleId.Value, 220, Util.Now, null, pending);
                }
                DbUtil.Db.SubmitChanges();
				DbUtil.Db.UpdateMainFellowship(id);
            }
            return Json(new { close = true, how = "rebindgrids", message = message });
        }
        private JsonResult AddContributor(int id, SearchAddModel m, int origin)
        {
            if (id > 0)
            {
                var p = m.List[0];
                var c = DbUtil.Db.Contributions.Single(cc => cc.ContributionId == id);
                AddPerson(p, m.List, origin, m.EntryPointId);
                c.PeopleId = p.PeopleId;

                if (c.BankAccount.HasValue())
                {
                    var ci = DbUtil.Db.CardIdentifiers.SingleOrDefault(k => k.Id == c.BankAccount);
                    if (ci == null)
                    {
                        ci = new CardIdentifier
                        {
                            Id = c.BankAccount,
                            CreatedOn = Util.Now,
                        };
                        DbUtil.Db.CardIdentifiers.InsertOnSubmit(ci);
                    }
                    ci.PeopleId = p.PeopleId;
                }
                DbUtil.Db.SubmitChanges();
                return Json(new { close = true, how = "addselected", cid = id, pid = p.PeopleId, name = p.person.Name2 });
            }
            return Json(new { close = true, how = "addselected" });
        }
        private JsonResult AddPeopleToTag(string id, SearchAddModel m, int origin)
        {
            if (id.HasValue())
            {
                foreach (var p in m.List)
                {
                    AddPerson(p, m.List, origin, m.EntryPointId);
					Person.Tag(DbUtil.Db, p.person.PeopleId, id, Util2.CurrentTagOwnerId, DbUtil.TagTypeId_Personal);
                }
                DbUtil.Db.SubmitChanges();
            }
			return Json(new { close = true, how = "addselected" });
        }

        private JsonResult AddVisitors(int id, SearchAddModel m, int origin)
        {
            var sb = new StringBuilder();
            if (id > 0)
            {
                var meeting = DbUtil.Db.Meetings.SingleOrDefault(me => me.MeetingId == id);
                foreach (var p in m.List)
                {
                    var isnew = p.IsNew;
                    AddPerson(p, m.List, origin, m.EntryPointId);
					if (isnew)
						p.person.UpdateValue("CampusId", meeting.Organization.CampusId);
                    var err = Attend.RecordAttendance(p.PeopleId.Value, id, true);
                    if (err != "ok")
                        sb.AppendLine(err);
                }
                DbUtil.Db.SubmitChanges();
                DbUtil.Db.UpdateMeetingCounters(meeting.MeetingId);
            }
            return Json(new { close = true, how = "addselected", error = sb.ToString() });
        }
        private JsonResult AddRegistered(int id, SearchAddModel m, int origin)
        {
            if (id > 0)
            {
                var meeting = DbUtil.Db.Meetings.SingleOrDefault(me => me.MeetingId == id);
                foreach (var p in m.List)
                {
                    var isnew = p.IsNew;
                    AddPerson(p, m.List, origin, m.EntryPointId);
                    if (isnew)
                        p.person.CampusId = meeting.Organization.CampusId;
                    Attend.MarkRegistered(DbUtil.Db, p.PeopleId.Value, id, 1);
                }
                DbUtil.Db.SubmitChanges();
                DbUtil.Db.UpdateMeetingCounters(meeting.MeetingId);
            }
            return Json(new { close = true, how = "addselected" });
        }
        private void AddPerson(SearchPersonModel p, IList<SearchPersonModel> list, int originid, int? entrypointid)
        {
            if (p.IsNew)
                p.AddPerson(originid, p.EntryPoint.Value.ToInt(), p.Campus.Value.ToInt());
            else 
            {
                if (entrypointid != 0 && 
                        (!p.person.EntryPointId.HasValue || p.person.EntryPointId == 0))
                    p.person.EntryPointId = entrypointid;
                if (originid != 0 &&
                        (!p.person.OriginId.HasValue || p.person.OriginId == 0))
                    p.person.OriginId = originid;
                DbUtil.Db.SubmitChanges();
            }
            if (p.FamilyId < 0) // fix up new family pointers
            {
                var q = from m in list
                        where m.FamilyId == p.FamilyId
                        select m;
                var list2 = q.ToList();
                foreach (var m in list2)
                    m.FamilyId = p.person.FamilyId;
            }
            Util2.CurrentPeopleId = p.person.PeopleId;
            Session["ActivePerson"] = p.person.Name;
        }
    }
}
