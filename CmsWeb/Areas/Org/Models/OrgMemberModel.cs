using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.Linq;
using System.Web;
using CmsData;
using System.Web.Mvc;
using CmsData.Registration;
using CmsWeb.Code;
using UtilityExtensions;
using System.Text.RegularExpressions;
using CmsData.Codes;

namespace CmsWeb.Areas.Org.Models
{
    public class OrgMemberModel
    {
        private CmsData.OrganizationMember om;
        private int? _orgId;
        private int? _peopleId;

        public void Populate()
        {
            var i = (from mm in DbUtil.Db.OrganizationMembers
                     where mm.OrganizationId == _orgId && mm.PeopleId == _peopleId
                     select new
                         {
                             mm,
                             mm.Person.Name,
                             mm.Organization.OrganizationName,
                             mm.Organization.RegSetting
                         }).SingleOrDefault();
            if (i == null)
                throw new Exception("missing OrgMember at oid={0}, pid={0}".Fmt(_orgId, _peopleId));
            om = i.mm;
            Name = i.Name;
            OrgName = i.OrganizationName;
            MemberType = new CodeInfo(om.MemberTypeId, "MemberType");
            AttendStr = om.AttendStr;
            Setting = new Settings(i.RegSetting, DbUtil.Db, _orgId.Value);
        }

        public int? OrgId
        {
            get { return _orgId; }
            set
            {
                _orgId = value;
                if(_peopleId.HasValue)
                    Populate();
            }
        }

        public int? PeopleId
        {
            get { return _peopleId; }
            set
            {
                _peopleId = value;
                if(_orgId.HasValue)
                    Populate();
            }
        }

        public string Name { get; set; }
        public string OrgName { get; set; }
        public string AttendStr { get; set; }
        public Settings Setting { get; set; }

        [DisplayName("Member Type")]
        public CodeInfo MemberType { get; set; }

        [UIHint("Date")]
        [DisplayName("Inactive Date")]
        public string InactiveDate
        {
            get { return om.InactiveDate.FormatDate(); }
            set { om.InactiveDate = value.ToDate(); }
        }

        [UIHint("Date")]
        [DisplayName("Enrollment Date")]
        public string Enrollment
        {
            get { return om.EnrollmentDate.FormatDate(); }
            set { om.EnrollmentDate = value.ToDate(); }
        }

        [UIHint("Bool")]
        public bool? Pending
        {
            get { return om.Pending; }
            set { om.Pending = value; }
        }

        [UIHint("Text")]
        [DisplayName("Register Email")]
        public string RegisterEmail
        {
            get { return om.RegisterEmail; }
            set { om.RegisterEmail = value; }
        }

        [UIHint("Text")]
        public string Request
        {
            get { return om.Request; }
            set { om.Request = value; }
        }

        [UIHint("Int")]
        public int? Grade
        {
            get { return om.Grade; }
            set { om.Grade = value; }
        }

        [UIHint("Int")]
        public int? Tickets
        {
            get { return om.Tickets; }
            set { om.Tickets = value; }
        }

        [UIHint("Decimal")]
        public decimal? Amount
        {
            get { return om.Amount; }
            set { om.Amount = value; }
        }

        [UIHint("Decimal")]
        [DisplayName("Amount Due")]
        public decimal? AmountDue
        {
            get { return om.AmountDue(DbUtil.Db); }
        }

        [UIHint("Text")]
        public string PayLink
        {
            get { return om.PayLink; }
            set { om.PayLink = value; }
        }

        [UIHint("Text")]
        public string ShirtSize
        {
            get { return om.ShirtSize; }
            set { om.ShirtSize = value; }
        }

        [UIHint("TextArea")]
        [DisplayName("Extra Member Info")]
        public string UserData
        {
            get { return om.UserData; }
            set { om.UserData = value; }
        }
    }
}
