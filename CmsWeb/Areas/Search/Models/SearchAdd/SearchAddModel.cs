/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church 
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CmsData.Codes;
using CmsWeb.Code;
using UtilityExtensions;
using CmsData;

namespace CmsWeb.Areas.Search.Models
{
    public class SearchAddModel : SearchResultsModel
    {
        private readonly string[] noaddtypes = { "relatedfamily", "mergeto", "contactor", "taskdelegate", "taskowner", "taskdelegate2" };
        private readonly string[] onlyonetypes = { "taskdelegate", "taskowner", "taskdelegate2", "mergeto", "relatedfamily" };

        public List<PendingPersonModel> PendingList { get; set; }

        public string DialogTitle
        {
            get
            {
                switch (AddContext.ToLower())
                {
                    case "addpeople":
                    case "menu":
                        return "Add to Database";
                    case "mergeto":
                        return "Merge Duplicate Into";
                    case "addtotag":
                        return "Add to Tag";
                    case "family":
                        return "Add to Family";
                    case "relatedfamily":
                        return "Add as Related Family";
                    case "org":
                        return "Add as Member of Organization";
                    case "prospect":
                        return "Add as Prospect of Organization";
                    case "pending":
                        return "Add as Pending Member of Organization";
                    case "inactive":
                        if(org == null)
                            org = DbUtil.Db.LoadOrganizationById(PrimaryKeyForContextType.ToInt());
                        return "Add as {0}".Fmt(org.IsMissionTrip == true ? "Sender of Mission Trip" : "Inactive Member of Organization");
                    case "visitor":
                        return "Add as Visitor to Meeting";
                    case "registered":
                        return "Add as Registered for Future Meeting";
                    case "contactee":
                        return "Add as Contactee for Ministry Contact";
                    case "contactor":
                        return "Add as Contactor for Ministry Contact";
                    case "contributor":
                        return "Add for a Gift";
                }
                return "";
            }
        }

        public string PrimaryKeyForContextType { get; set; }
        public bool CanAdd { get { return !noaddtypes.Contains(AddContext.ToLower()); } }
        public bool OnlyOne { get { return onlyonetypes.Contains(AddContext.ToLower()); } }
        public int NewFamilyId { get; set; }
        public int? EntryPointId { get; set; }
        public int? CampusId { get; set; }
        public int Index { get; set; }
        private Organization org;

        public SearchAddModel()
        {
            PendingList = new List<PendingPersonModel>();
        }
        public SearchAddModel(string context, string contextid) 
            : this()
        {
            AddContext = context;
            PrimaryKeyForContextType = contextid;
            CampusId = null;
            switch (AddContext.ToLower())
            {
                case "addpeople":
                case "menu":
                    EntryPointId = 0;
                    break;
                case "addtotag":
                    EntryPointId = null;
                    break;
                case "mergeto":
                    EntryPointId = null;
                    break;
                case "family":
                case "relatedfamily":
                    EntryPointId = 0;
                    break;
                case "org":
                case "pending":
                case "prospect":
                case "inactive":
                    org = DbUtil.Db.LoadOrganizationById(contextid.ToInt());
                    CampusId = org.CampusId;
                    EntryPointId = org.EntryPointId ?? 0;
                    break;
                case "visitor":
                case "registered":
                    org = (from meeting in DbUtil.Db.Meetings
                           where meeting.MeetingId == contextid.ToInt()
                           select meeting.Organization).Single();
                    EntryPointId = org.EntryPointId ?? 0;
                    CampusId = org.CampusId;
                    break;
                case "contactee":
                    EntryPointId = 0;
                    break;
                case "contactor":
                    EntryPointId = 0;
                    break;
                case "contributor":
                    EntryPointId = 0;
                    break;
            }
        }

        public int NextNewFamilyId()
        {
            NewFamilyId--;
            return NewFamilyId;
        }

        public PendingPersonModel NewPerson(int familyid)
        {
            var p = new PendingPersonModel
            {
                FamilyId = familyid,
                index = PendingList.Count,
                Gender = new CodeInfo(99, "Gender"),
                MaritalStatus = new CodeInfo(99, "MaritalStatus"),
                Campus = new CodeInfo(CampusId, "Campus"),
                EntryPoint = new CodeInfo(EntryPointId, "EntryPoint"),
                context = AddContext,
            };
            if (familyid == 0)
            {
                p.FamilyId = NextNewFamilyId();
                p.IsNewFamily = true;
            }
#if DEBUG
            p.FirstName = "David";
            p.LastName = "Carr." + DateTime.Now.Millisecond;
#endif
            PendingList.Add(p);
            return p;
        }
        public static string AddRelatedFamily(int peopleid, int relatedPersonId)
        {
            var p = DbUtil.Db.LoadPersonById(peopleid);
            var p2 = DbUtil.Db.LoadPersonById(relatedPersonId);
            var rf = DbUtil.Db.RelatedFamilies.SingleOrDefault(r =>
                (r.FamilyId == p.FamilyId && r.RelatedFamilyId == p2.FamilyId)
                || (r.FamilyId == p2.FamilyId && r.RelatedFamilyId == p.FamilyId)
                );
            if (rf == null)
            {
                rf = new RelatedFamily
                {
                    FamilyId = p.FamilyId,
                    RelatedFamilyId = p2.FamilyId,
                    FamilyRelationshipDesc = "",
                    CreatedBy = Util.UserId1,
                    CreatedDate = Util.Now,
                };
                DbUtil.Db.RelatedFamilies.InsertOnSubmit(rf);
                DbUtil.Db.SubmitChanges();
            }
            return "#rf-{0}-{1}".Fmt(rf.FamilyId, rf.RelatedFamilyId);
        }


        internal void AddExisting(int id)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            var pp = new PendingPersonModel();
            pp.CopyPropertiesFrom(p);
            pp.LoadAddress();
            PendingList.Add(pp);
        }

        internal ReturnResult CommitAdd()
        {
            var id = PrimaryKeyForContextType;
			var iid = PrimaryKeyForContextType.ToInt();
            switch (AddContext.ToLower())
            {
                case "menu":
                case "addpeople":
                    return AddPeople(OriginCode.MainMenu);
                case "addtotag":
                    return AddPeopleToTag(id, 0);
                case "family":
                    return AddFamilyMembers(iid, OriginCode.NewFamilyMember);
                case "relatedfamily":
                    return AddRelatedFamilys(iid, OriginCode.NewFamilyMember);
                case "org":
                    return AddOrgMembers(iid, OriginCode.Enrollment);
                case "pending":
                    return AddOrgMembers(iid, OriginCode.Enrollment, pending: true);
                case "inactive":
                    return AddOrgMembers(iid, OriginCode.Enrollment, MemberTypeCode.InActive);
                case "prospect":
                    return AddOrgMembers(iid, OriginCode.Enrollment, MemberTypeCode.Prospect);
                case "visitor":
                    return AddVisitors(iid, OriginCode.Visit);
                case "registered":
                    return AddRegistered(iid, OriginCode.Visit);
                case "contactee":
                    return AddContactees(iid, OriginCode.Visit);
                case "contactor":
                    return AddContactors(iid, 0);
                case "contributor":
                    return AddContributor(iid, OriginCode.Contribution);
                case "taskdelegate":
                    if (PendingList.Count > 0)
                        return new ReturnResult { close = true, how = "addselected", url = "/Task/Delegate/", pid = PendingList[0].PeopleId, from = AddContext };
                    break;
                case "taskdelegate2":
                    if (PendingList.Count > 0)
                        return new ReturnResult { close = true, how = "addselected2", url = "/Task/Action/1", pid = PendingList[0].PeopleId, from = AddContext };
                    break;
                case "taskabout":
                    if (PendingList.Count > 0)
                        return new ReturnResult { close = true, how = "addselected", url = "/Task/ChangeAbout/", pid = PendingList[0].PeopleId, from = AddContext };
                    break;
                case "taskowner":
                    if (PendingList.Count > 0)
                        return new ReturnResult{ close = true, how = "addselected", url = "/Task/ChangeOwner/", pid = PendingList[0].PeopleId, from = AddContext };
                    break;
                case "mergeto":
                    if (PendingList.Count > 0)
                        return new ReturnResult { close = true, how = "addselected", pid = PrimaryKeyForContextType.ToInt(), pid2 = PendingList[0].PeopleId, from = AddContext };
                    break;
            }
            return new ReturnResult { close = true, from = AddContext };
        }

        public class ReturnResult
        {
            public bool close { get; set; }
            public string how { get; set; }
            public string url { get; set; }
            public int? pid { get; set; }
            public int? pid2 { get; set; }
            public string from { get; set; }
            public string name { get; set; }
            public int? cid { get; set; }
            public string message { get; set; }
            public string error { get; set; }
            public string key { get; set; }
        }

        private ReturnResult AddContactees(int id, int origin)
        {
            if (id > 0)
            {
                var c = DbUtil.Db.Contacts.Single(ct => ct.ContactId == id);
                foreach (var p in PendingList)
                {
                    AddPerson(p, PendingList, OriginCode.Visit, EntryPointId);
                    var ctee = c.contactees.SingleOrDefault(ct =>
                        ct.ContactId == id && ct.PeopleId == p.Person.PeopleId);
                    if (ctee == null)
                    {
                        ctee = new Contactee
                        {
                            ContactId = id,
                            PeopleId = p.Person.PeopleId,
                        };
                        c.contactees.Add(ctee);
                    }
                }
                DbUtil.Db.SubmitChanges();
            }
            return new ReturnResult{ close = true, from = AddContext, cid = id };
        }
        private ReturnResult AddContactors(int id, int origin)
        {
            if (id > 0)
            {
                var c = DbUtil.Db.Contacts.SingleOrDefault(ct => ct.ContactId == id);
                if (c == null)
                    return new ReturnResult { close = true, how = "CloseAddDialog", from = AddContext };
                foreach (var p in PendingList)
                {
                    AddPerson(p, PendingList, origin, EntryPointId);
                    var ctor = c.contactsMakers.SingleOrDefault(ct =>
                        ct.ContactId == id && ct.PeopleId == p.Person.PeopleId);
                    if (ctor == null)
                    {
                        ctor = new Contactor
                        {
                            ContactId = id,
                            PeopleId = p.Person.PeopleId,
                        };
                        c.contactsMakers.Add(ctor);
                    }
                }
                DbUtil.Db.SubmitChanges();
            }
            return new ReturnResult { close = true, cid = id, from = AddContext };
        }
        private ReturnResult AddFamilyMembers(int id, int origin)
        {
            if (id > 0)
            {
                var p = DbUtil.Db.LoadPersonById(id);

                foreach (var i in PendingList)
                {
                    var isnew = i.IsNew;
                    AddPerson(i, PendingList, origin, EntryPointId);
                    if (!isnew)
                    {
                        var fm = p.Family.People.SingleOrDefault(fa => fa.PeopleId == i.Person.PeopleId);
                        if (fm != null)
                            continue; // already a member of this family

                        if (i.Person.Age < 18)
                            i.Person.PositionInFamilyId = PositionInFamily.Child;
                        else if (p.Family.People.Count(per =>
                                    per.PositionInFamilyId == PositionInFamily.PrimaryAdult) < 2)
                            i.Person.PositionInFamilyId = PositionInFamily.PrimaryAdult;
                        else
                            i.Person.PositionInFamilyId = PositionInFamily.SecondaryAdult;
                        p.Family.People.Add(i.Person);
                    }
                }
                DbUtil.Db.SubmitChanges();
            }
            return new ReturnResult { pid = id, from = AddContext };
        }
        private ReturnResult AddRelatedFamilys(int id, int origin)
        {
            var p = PendingList[0];
            AddPerson(p, PendingList, origin, EntryPointId);
            var key = AddRelatedFamily(id, p.PeopleId.Value);
            try
            {
                DbUtil.Db.SubmitChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return new ReturnResult { from = AddContext, pid = id, key = key };
        }
        private ReturnResult AddPeople(int origin)
        {
            foreach (var p in PendingList)
                AddPerson(p, PendingList, origin, EntryPointId);
            DbUtil.Db.SubmitChanges();
            return new ReturnResult { pid = PendingList[0].PeopleId, from = AddContext };
        }
        private ReturnResult  AddOrgMembers(int id, int origin, int membertypeid = MemberTypeCode.Member, bool pending = false )
        {
            string message = null;
            if (id > 0)
            {
                var org = DbUtil.Db.LoadOrganizationById(id);
                if (pending == false && PendingList.Count == 1 && org.AllowAttendOverlap != true)
                {
                    var om = DbUtil.Db.OrganizationMembers.FirstOrDefault(mm => 
                        mm.OrganizationId != id
                        && mm.MemberTypeId != 230 // inactive
                        && mm.MemberTypeId != 500 // inservice
                        && mm.Organization.AllowAttendOverlap != true
                        && mm.PeopleId == PendingList[0].PeopleId
                        && mm.Organization.OrgSchedules.Any(ss => 
                            DbUtil.Db.OrgSchedules.Any(os => 
                                os.OrganizationId == id 
                                && os.ScheduleId == ss.ScheduleId)));
                    if (om != null)
                    {
                        message = "Already a member of {0} (orgid) with same schedule".Fmt(om.OrganizationId);
                        return new ReturnResult { close = true, how = "CloseAddDialog", message = message, from = AddContext };
                    }
                }
                foreach (var p in PendingList)
                {
                    AddPerson(p, PendingList, origin, EntryPointId);
                    var om = OrganizationMember.InsertOrgMembers(DbUtil.Db,
                        id, p.PeopleId.Value, membertypeid, Util.Now, null, pending);
                    if (membertypeid == MemberTypeCode.InActive && org.IsMissionTrip == true)
                        om.AddToGroup(DbUtil.Db, "Sender");
                }
                DbUtil.Db.SubmitChanges();
				DbUtil.Db.UpdateMainFellowship(id);
            }
            return new ReturnResult { close = true, how = "rebindgrids", message = message, from = AddContext };
        }
        private ReturnResult AddContributor(int id, int origin)
        {
            if (id > 0)
            {
                var p = PendingList[0];
                var c = DbUtil.Db.Contributions.Single(cc => cc.ContributionId == id);
                AddPerson(p, PendingList, origin, EntryPointId);
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
                return new  ReturnResult{ close = true, how = "addselected", cid = id, pid = p.PeopleId, name = p.Person.Name2, from = AddContext };
            }
            return new ReturnResult { close = true, how = "addselected", from = AddContext };
        }
        private ReturnResult AddPeopleToTag(string id, int origin)
        {
            if (id.HasValue())
            {
                foreach (var p in PendingList)
                {
                    AddPerson(p, PendingList, origin, EntryPointId);
					Person.Tag(DbUtil.Db, p.Person.PeopleId, id, Util2.CurrentTagOwnerId, DbUtil.TagTypeId_Personal);
                }
                DbUtil.Db.SubmitChanges();
            }
            return new ReturnResult { close = true, how = "addselected", from = AddContext };
        }
        private ReturnResult AddVisitors(int id, int origin)
        {
            var sb = new StringBuilder();
            if (id > 0)
            {
                var meeting = DbUtil.Db.Meetings.SingleOrDefault(me => me.MeetingId == id);
                foreach (var p in PendingList)
                {
                    var isnew = p.IsNew;
                    AddPerson(p, PendingList, origin, EntryPointId);
					if (isnew)
						p.Person.UpdateValue("CampusId", meeting.Organization.CampusId);
                    var err = Attend.RecordAttendance(p.PeopleId.Value, id, true);
                    if (err != "ok")
                        sb.AppendLine(err);
                }
                DbUtil.Db.SubmitChanges();
                DbUtil.Db.UpdateMeetingCounters(meeting.MeetingId);
            }
            return new ReturnResult { close = true, how = "addselected", error = sb.ToString(), from = AddContext };
        }
        private ReturnResult AddRegistered(int id, int origin)
        {
            if (id > 0)
            {
                var meeting = DbUtil.Db.Meetings.SingleOrDefault(me => me.MeetingId == id);
                foreach (var p in PendingList)
                {
                    var isnew = p.IsNew;
                    AddPerson(p, PendingList, origin, EntryPointId);
                    if (isnew)
                        p.Person.CampusId = meeting.Organization.CampusId;
                    Attend.MarkRegistered(DbUtil.Db, p.PeopleId.Value, id, 1);
                }
                DbUtil.Db.SubmitChanges();
                DbUtil.Db.UpdateMeetingCounters(meeting.MeetingId);
            }
            return new ReturnResult { close = true, how = "addselected", from = AddContext };
        }
        private void AddPerson(PendingPersonModel p, List<PendingPersonModel> list, int originid, int? entrypointid)
        {
            if (p.IsNew)
                p.AddPerson(originid, p.EntryPoint.Value.ToInt(), p.Campus.Value.ToInt());
            else 
            {
                if (entrypointid != 0 && 
                        (!p.Person.EntryPointId.HasValue || p.Person.EntryPointId == 0))
                    p.Person.EntryPointId = entrypointid;
                if (originid != 0 &&
                        (!p.Person.OriginId.HasValue || p.Person.OriginId == 0))
                    p.Person.OriginId = originid;
                DbUtil.Db.SubmitChanges();
            }
            if (p.FamilyId < 0) // fix up new family pointers
            {
                var q = from m in list
                        where m.FamilyId == p.FamilyId
                        select m;
                var list2 = q.ToList();
                foreach (var m in list2)
                    m.FamilyId = p.Person.FamilyId;
            }
            Util2.CurrentPeopleId = p.Person.PeopleId;
            HttpContext.Current.Session["ActivePerson"] = p.Person.Name;
        }
    }
}
