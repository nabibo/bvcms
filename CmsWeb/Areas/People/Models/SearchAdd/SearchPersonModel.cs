using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsWeb.Areas.People.Models.Person;
using CmsWeb.Code;
using UtilityExtensions;
using CmsData;
using System.Text;
using System.Data.Linq;
using CmsData.Codes;

namespace CmsWeb.Areas.People.Models
{
    public class SearchPersonModel
    {
        private static CodeValueModel cv = new CodeValueModel();

        public int index { get; set; }
        public string context { get; set; }

        [UIHint("Text")]
        public string First { get; set; }

        [UIHint("Text")]
        [DisplayName("Goes By")]
        public string GoesBy { get; set; }

        [UIHint("Text")]
        public string Middle { get; set; }

        [UIHint("Text")]
        public string Last { get; set; }

        [UIHint("Text")]
        public string Title { get; set; }

        [UIHint("Text")]
        public string Suffix { get; set; }

        [UIHint("Date")]
        [DisplayName("Birthday")]
        public string dob { get; set; }

        [UIHint("Text")]
        public string Phone { get; set; }

        [UIHint("Text")]
        public string Email { get; set; }

        [UIHint("Code")]
        public CodeInfo Gender { get; set; }

        [UIHint("Code")]
        public CodeInfo Marital { get; set; }

        [UIHint("Code")]
        public CodeInfo Campus { get; set; }

        [UIHint("Code")]
        [DisplayName("Entry Point")]
        public CodeInfo EntryPoint { get; set; }

        [UIHint("Text")]
        public string HomePhone { get; set; }

        [UIHint("Text")]
        [DisplayName("Line 1")]
        public string Address { get; set; }

        [UIHint("Text")]
        [DisplayName("Line 2")]
        public string Address2 { get; set; }

        [UIHint("Text")]
        public string City { get; set; }

        [UIHint("Code")]
        public CodeInfo State { get; set; }

        [UIHint("Text")]
        public string Zip { get; set; }

        [UIHint("Code")]
        public CodeInfo Country { get; set; }

        private DateTime? _Birthday;

        public DateTime? birthday
        {
            get
            {
                DateTime dt;
                if (!_Birthday.HasValue && dob.NotEqual("na"))
                    if (Util.DateValid(dob, out dt))
                        _Birthday = dt;
                return _Birthday;
            }
        }

        public int? age
        {
            get
            {
                if (birthday.HasValue)
                    return birthday.Value.AgeAsOf(Util.Now);
                return null;
            }
        }

        public bool IsNew
        {
            get { return !PeopleId.HasValue; }
        }

        public int FamilyId { get; set; }
        private Family _family;

        public Family family
        {
            get
            {
                if (_family == null && FamilyId > 0)
                    _family = DbUtil.Db.Families.Single(f => f.FamilyId == FamilyId);
                return _family;
            }
        }

        public int? PeopleId { get; set; }
        private CmsData.Person _Person;

        public CmsData.Person person
        {
            get
            {
                if (_Person == null && PeopleId.HasValue)
                    _Person = DbUtil.Db.LoadPersonById(PeopleId.Value);
                return _Person;
            }
        }

        internal void ValidateModelForNew(ModelStateDictionary ModelState, bool checkaddress)
        {
            if (!First.HasValue())
                ModelState.AddModelError("name", "first name required");

            if (!Last.HasValue())
                ModelState.AddModelError("name", "last name required");

            if (!birthday.HasValue && dob.NotEqual("na"))
                ModelState.AddModelError("dob", "valid birthday (or \"na\")");

            var d = Phone.GetDigits().Length;
            if (d != 7 && d < 10 && Phone.NotEqual("na"))
                ModelState.AddModelError("phone", "7 or 10+ digits (or \"na\")");

            int count = 0;
            var pids = DbUtil.Db.FindPerson(First, Last, birthday, Email, Phone.GetDigits());
            if (pids.Any())
                ModelState.AddModelError("name", "name/dob already exists in db");

            if (!Util.ValidEmail(Email) && Email.NotEqual("na"))
                ModelState.AddModelError("email", "valid email address (or \"na\")");

            if (Gender.Value == "99")
                ModelState.AddModelError("gender", "specify gender");

            if (Marital.Value == "99")
                ModelState.AddModelError("marital", "specify marital status");

            if (checkaddress)
            {

                if (!Address.HasValue())
                    ModelState.AddModelError("address", "address required (or \"na\")");

                bool addrok = (City.HasValue() && State.Value.HasValue()) 
                            || Zip.HasValue() 
                            || (City.Equal("na") && State.Value.Equal("na") && Zip.Equal("na"));
                if (!addrok)
                    ModelState.AddModelError("zip", "city/state required or zip required (or \"na\" in all)");

                if (ModelState.IsValid
                    && Address.NotEqual("na") && City.NotEqual("na") && State.Value.NotEqual("na")
                    && (Country.Value.Equal("United States") || !Country.Value.HasValue()))
                {
                    var r = AddressVerify.LookupAddress(Address, Address2, City, State.Value, Zip);
                    if (r.Line1 != "error")
                    {
                        if (!r.found)
                        {
                            ModelState.AddModelError("zip",
                                 r.address +
                                 ", if your address will not validate, change the country to 'USA, Not Validated'");
                            return;
                        }
                        if (r.Line1 != Address)
                        {
                            ModelState.AddModelError("address", "address changed from '{0}'".Fmt(Address));
                            Address = r.Line1;
                        }
                        if (r.Line2 != (Address2 ?? ""))
                        {
                            ModelState.AddModelError("address2", "address2 changed from '{0}'".Fmt(Address2));
                            Address2 = r.Line2;
                        }
                        if (r.City != (City ?? ""))
                        {
                            ModelState.AddModelError("city", "city changed from '{0}'".Fmt(City));
                            City = r.City;
                        }
                        if (r.State != (State.Value ?? ""))
                        {
                            ModelState.AddModelError("state", "state changed from '{0}'".Fmt(State));
                            State.Value = r.State;
                        }
                        if (r.Zip != (Zip ?? ""))
                        {
                            ModelState.AddModelError("zip", "zip changed from '{0}'".Fmt(Zip));
                            Zip = r.Zip;
                        }
                    }
                }
            }
        }

        internal void AddPerson(int originid, int? entrypointid, int? campusid)
        {
            Family f;
            string na = "na";
            if (FamilyId > 0)
                f = family;
            else
                f = new Family
                        {
                            HomePhone = HomePhone.GetDigits(),
                            AddressLineOne = Address.Disallow(na),
                            AddressLineTwo = Address2,
                            CityName = City.Disallow(na),
                            StateCode = State.Value.Disallow(na),
                            ZipCode = Zip.Disallow(na),
                            CountryName = Country.Value,
                        };

            if (GoesBy != null)
                GoesBy = GoesBy.Trim();
            var position = PositionInFamily.Child;
            if (!birthday.HasValue)
                position = PositionInFamily.PrimaryAdult;
            if (age >= 18)
                if (f.People.Count(per =>
                                   per.PositionInFamilyId == PositionInFamily.PrimaryAdult)
                    < 2)
                    position = PositionInFamily.PrimaryAdult;
                else
                    position = PositionInFamily.SecondaryAdult;

            _Person = CmsData.Person.Add(f, position,
                                 null, First.Trim(), GoesBy, Last.Trim(), dob, false, Gender.Value.ToInt(),
                                 originid, entrypointid);
            if (Title.HasValue())
                person.TitleCode = Title;
            person.EmailAddress = Email.Disallow(na);
            person.MaritalStatusId = Marital.Value.ToInt();
            person.SuffixCode = Suffix;
            person.MiddleName = Middle;
            if (campusid == 0)
                campusid = null;
            person.CampusId = Util.PickFirst(campusid.ToString(), DbUtil.Db.Setting("DefaultCampusId", "")).ToInt2();
            if (person.CampusId == 0)
                person.CampusId = null;

            person.CellPhone = Phone.GetDigits();
            DbUtil.Db.SubmitChanges();
            DbUtil.Db.Refresh(RefreshMode.OverwriteCurrentValues, person);
            PeopleId = person.PeopleId;
        }

        internal void LoadFamily()
        {
            if (FamilyId < 0)
                return;
            HomePhone = family.HomePhone;
            Address = family.AddressLineOne;
            Address2 = family.AddressLineTwo;
            City = family.CityName;
            State = new CodeInfo(family.StateCode, cv.GetStateListUnknown());
            Zip = family.ZipCode;
            Country = new CodeInfo(family.CountryName, CodeValueModel.GetCountryList());
        }

        public string ToolTip
        {
            get
            {
                return "{0} {1}|{2}|{3}|Birthday: {4}|c {5}|h {6}|{7}|Gender: {8}|Marital: {9}".Fmt(
                    GoesBy ?? First, Last,
                    Address,
                    CityStateZip,
                    birthday.FormatDate(),
                    Phone.FmtFone(),
                    HomePhone.FmtFone(),
                    Email,
                    Gender.ToString(),
                    Marital.ToString());
            }
        }

        public string CityStateZip
        {
            get { return "{0}, {1} {2}".Fmt(City, State, Zip); }
        }

    }
}
