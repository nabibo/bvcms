using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Code;
using UtilityExtensions;

namespace CmsWeb.Areas.People.Models.Person
{
    public class AddressInfo
    {
        private CmsWeb.Code.CodeValueModel cv = new CmsWeb.Code.CodeValueModel();

        public int PeopleId { get; set; }
        public CmsData.Person person { get; set; }

        public string Name { get; set; }
        private bool? _CanUserEditAddress;
        public bool CanUserEditAddress
        {
            get
            {
                if (!_CanUserEditAddress.HasValue)
                {
                    switch (Name)
                    {
                        case "PersonalAddr":
                            _CanUserEditAddress = person.CanUserEditBasic;
                            break;
                        case "FamilyAddr":
                            _CanUserEditAddress = person.CanUserEditFamilyAddress;
                            break;
                    }
                }
                return _CanUserEditAddress.Value;
            }
        }

        public string Display
        {
            get
            {
                switch (Name)
                {
                    case "PersonalAddr": return "Personal Address";
                    case "FamilyAddr": return "Family Address";
                }
                return "Address";
            }
        }

        [UIHint("Text")]
        [DisplayName("Addr Line 1")]
        public string Address1 { get; set; }

        [UIHint("Text")]
        [DisplayName("Addr Line 2")]
        public string Address2 { get; set; }

        [UIHint("Text")]
        public string City { get; set; }

        [UIHint("Code")]
        public CodeInfo State { get; set; }

        [UIHint("Text")]
        public string Zip { get; set; }

        [UIHint("Code")]
        public CodeInfo Country { get; set; }

        public string AddrCityStateZip()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Address1);
            if (Address2.HasValue())
                sb.AppendLine(Address2);
            sb.AppendLine(Util.FormatCSZ(City, State.Value, Zip));
            return sb.ToString();
        }
        public string CityStateZip()
        {
            return Util.FormatCSZ(City, State.Value, Zip);
        }

        public AddressVerify.AddressResult Result { get; set; }

        [UIHint("Bool")]
        [DisplayName("Bad Address Flag")]
        public bool? BadAddress { get; set; }

        [UIHint("Code")]
        [DisplayName("Resident Code")]
        public CodeInfo ResCode { get; set; }

        [UIHint("Bool")]
        [DisplayName("Primary Address")]
        public bool Preferred { get; set; }

        [UIHint("Date")]
        [DisplayName("From Date")]
        public DateTime? FromDt { get; set; }

        [UIHint("Date")]
        [DisplayName("To Date")]
        public DateTime? ToDt { get; set; }

        public static IEnumerable<SelectListItem> ResCodes()
        {
            return CodeValueModel.ConvertToSelect(CodeValueModel.ResidentCodesWithZero(), "Id");
        }
        public static IEnumerable<SelectListItem> States()
        {
            return CodeValueModel.ConvertToSelect(CodeValueModel.GetStateList(), "Code");
        }
        public static IEnumerable<SelectListItem> Countries()
        {
            var list = CodeValueModel.ConvertToSelect(CodeValueModel.GetCountryList(), null);
            list.Insert(0, new SelectListItem { Text = "(not specified)", Value = "" });
            return list;
        }
        public static AddressInfo GetAddressInfo(int id, string typeid)
        {
            var p = DbUtil.Db.LoadPersonById(id);
            DbUtil.Db.Refresh(RefreshMode.OverwriteCurrentValues, p);
            var a = new AddressInfo();
            a.person = p;
            switch (typeid)
            {
                case "PrimaryAddr":
                    a.Name = typeid;
                    a.PeopleId = p.PeopleId;
                    a.Address1 = p.PrimaryAddress;
                    a.Address2 = p.PrimaryAddress2;
                    a.BadAddress = p.PrimaryBadAddrFlag == 1;
                    a.City = p.PrimaryCity;
                    a.Zip = p.PrimaryZip;
                    a.State = new CodeInfo(p.PrimaryState, StateCodes());
                    a.Country = new CodeInfo(p.PrimaryCountry, Countries());
                    a.ResCode = new CodeInfo(p.PrimaryResCode, ResCodes());
                    break;
                case "FamilyAddr":
                    a.Name = typeid;
                    a.PeopleId = p.PeopleId;
                    a.Address1 = p.Family.AddressLineOne;
                    a.Address2 = p.Family.AddressLineTwo;
                    a.ToDt = p.Family.AddressToDate;
                    a.BadAddress = p.Family.BadAddressFlag;
                    a.City = p.Family.CityName;
                    a.Zip = p.Family.ZipCode;
                    a.State = new CodeInfo(p.Family.StateCode, StateCodes());
                    a.Country = new CodeInfo(p.Family.CountryName, Countries());
                    a.ResCode = new CodeInfo(p.Family.ResCodeId, ResCodes());
                    a.Preferred = p.AddressTypeId == 10;
                    break;
                case "PersonalAddr":
                    a.Name = typeid;
                    a.PeopleId = p.PeopleId;
                    a.Address1 = p.AddressLineOne;
                    a.Address2 = p.AddressLineTwo;
                    a.ToDt = p.AddressToDate;
                    a.BadAddress = p.BadAddressFlag;
                    a.City = p.CityName;
                    a.Zip = p.ZipCode;
                    a.State = new CodeInfo(p.StateCode, StateCodes());
                    a.Country = new CodeInfo(p.CountryName, Countries());
                    a.ResCode = new CodeInfo(p.ResCodeId, ResCodes());
                    a.Preferred = p.AddressTypeId == 30;
                    break;
            }
            return a;
        }
        public void SetAddressInfo(int id, string typeid)
        {
            Address1 = Result.Line1;
            Address2 = Result.Line2;
            City = Result.City;
            Zip = Result.Zip;
            State = new CodeInfo(Result.State, StateCodes());
        }

        public bool addrok
        {
            get { return City.HasValue() && State.Value.HasValue() || Zip.HasValue(); }
        }

        public string error { get; set; }
        public bool saved { get; set; }
        public bool? resultok { get; set; }
        public bool resultchanged { get; set; }
        public bool resultnotfound
        {
            get { return Result != null && !Result.found; }
        }

        public void UpdateAddress(bool forceSave = false)
        {
            var p = DbUtil.Db.LoadPersonById(PeopleId);
            var f = p.Family;

            if (!forceSave)
            {
                if (!addrok)
                    return;

                if (Address1.HasValue() && (City.HasValue() || State.Value.HasValue() || Zip.HasValue())
                    && (Country.Value == "United States" || !Country.Value.HasValue()))
                {
                    Result = AddressVerify.LookupAddress(Address1, Address2, City, State.Value, Zip);
                    if (Result.Line1 == "error")
                    {
                        error = "network error";
                        Result.address = AddrCityStateZip();
                        return;
                    }
                    if (resultnotfound)
                        return;
                    if (Result.Changed(Address1, Address2, City, State.Value, Zip))
                    {
                        resultchanged = true;
                        SetAddressInfo(PeopleId, Name);
                        return;
                    }
                }
                // at this point the address validated just fine.
            }

            int? ResCodeId = ResCode.Value.ToInt();
            if (ResCodeId == 0)
                ResCodeId = null;

            switch (Name)
            {
                case "FamilyAddr":
                    UpdateValue(f, "AddressLineOne", Address1);
                    UpdateValue(f, "AddressLineTwo", Address2);
                    UpdateValue(f, "AddressToDate", ToDt);
                    UpdateValue(f, "CityName", City);
                    UpdateValue(f, "StateCode", State.Value);
                    UpdateValue(f, "ResCodeId", ResCodeId);
                    UpdateValue(f, "ZipCode", Zip ?? "");
                    UpdateValue(f, "CountryName", Country.Value);
                    if (Preferred)
                        UpdateValue(p, "AddressTypeId", 10);
                    if (fsb.Length > 0)
                        BadAddress = false;
                    UpdateValue(f, "BadAddressFlag", BadAddress);
                    break;
                case "PersonalAddr":
                    UpdateValue(p, "AddressLineOne", Address1);
                    UpdateValue(p, "AddressLineTwo", Address2);
                    UpdateValue(p, "AddressToDate", ToDt);
                    UpdateValue(p, "CityName", City);
                    UpdateValue(p, "StateCode", State.Value);
                    UpdateValue(p, "ResCodeId", ResCodeId);
                    UpdateValue(p, "ZipCode", Zip ?? "");
                    UpdateValue(p, "CountryName", Country.Value);
                    if (Preferred)
                        UpdateValue(p, "AddressTypeId", 30);
                    if (psb.Length > 0)
                        BadAddress = false;
                    UpdateValue(p, "BadAddressFlag", BadAddress);
                    break;
            }
            if (psb.Length > 0)
            {
                var c = new ChangeLog
                {
                    UserPeopleId = Util.UserPeopleId.Value,
                    PeopleId = PeopleId,
                    Field = Name,
                    Data = "<table>\n" + psb + "</table>",
                    Created = Util.Now
                };
                DbUtil.Db.ChangeLogs.InsertOnSubmit(c);
            }
            if (fsb.Length > 0)
            {
                var c = new ChangeLog
                {
                    FamilyId = p.FamilyId,
                    UserPeopleId = Util.UserPeopleId.Value,
                    PeopleId = PeopleId,
                    Field = Name,
                    Data = "<table>\n" + fsb + "</table>",
                    Created = Util.Now
                };
                DbUtil.Db.ChangeLogs.InsertOnSubmit(c);
            }
            try
            {
                DbUtil.Db.SubmitChanges();
                DbUtil.LogActivity("Update Address for: {0}".Fmt(person.Name));
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
                return;
            }
            saved = true;

            if (!HttpContext.Current.User.IsInRole("Access"))
                if (psb.Length > 0 || fsb.Length > 0)
                {
                    DbUtil.Db.EmailRedacted(p.FromEmail, DbUtil.Db.GetNewPeopleManagers(),
                        "Address Info Changed",
                        "{0} changed the following information:<br />\n<table>{1}{2}</table>"
                        .Fmt(Util.UserName, psb.ToString(), fsb.ToString()));
                }
        }
        private StringBuilder fsb = new StringBuilder();
        private void UpdateValue(Family f, string field, object value)
        {
            var o = Util.GetProperty(f, field);
            if (o == null && value == null)
                return;
            if (o != null && o.Equals(value))
                return;
            fsb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>\n", field, o, value ?? "(null)");
            Util.SetProperty(f, field, value);
        }
        private StringBuilder psb = new StringBuilder();
        private void UpdateValue(CmsData.Person p, string field, object value)
        {
            var o = Util.GetProperty(p, field);
            if (o == null && value == null)
                return;
            if (o != null && o.Equals(value))
                return;
            psb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>\n", field, o, value ?? "(null)");
            Util.SetProperty(p, field, value);
        }

        public static IEnumerable<SelectListItem> StateCodes()
        {
            return CodeValueModel.ConvertToSelect(CodeValueModel.GetStateList(), "Code");
        }
    }
}