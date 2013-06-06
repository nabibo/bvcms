using System.Linq;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Code;
using CmsWeb.Models;
using UtilityExtensions;

namespace CmsWeb.Areas.People.Models.Person
{
    public class PersonInfo
    {
        public BasicPersonInfo basic { get; set; }
        public MemberInfo member { get; set; }
        public GrowthInfo growth { get; set; }
        public MemberNotesInfo membernotes { get; set; }

        public int PeopleId { get; set; }
        public int FamilyId { get; set; }
        public int? SpouseId { get; set; }
        public string[] StatusFlags { get; set; }

        public bool Deceased { get; set; }
        public string Name { get; set; }
        public Picture Picture
        {
            get { return picture ?? new Picture(); }
            set { picture = value; }
        }

        public int AddressTypeId { get; set; }
        private AddressInfo _PrimaryAddr;
        private Picture picture;

        public AddressInfo PrimaryAddr
        {
            get
            {
                if (_PrimaryAddr == null)
                    if (FamilyAddr.Preferred)
                        _PrimaryAddr = FamilyAddr;
                    else if (PersonalAddr.Preferred)
                        _PrimaryAddr = PersonalAddr;
                return _PrimaryAddr;
            }
        }
        public AddressInfo FamilyAddr { get; set; }
        public AddressInfo PersonalAddr { get; set; }
        public static PersonInfo GetPersonInfo(int? id)
        {
            var cv = new CodeValueModel();
            var flags = DbUtil.Db.Setting("StatusFlags", "F04,F01,F02,F03");
			var i = (from pp in DbUtil.Db.People
					 let spouse = (from sp in pp.Family.People where sp.PeopleId == pp.SpouseId select sp.Name).SingleOrDefault()
                     let statusflags = DbUtil.Db.StatusFlags(flags).Single(sf => sf.PeopleId == id).StatusFlags
					 where pp.PeopleId == id
					 select new
					 {
						 pp,
						 f = pp.Family,
						 spouse = spouse,
						 pp.Picture,
                         statusflags, 
					 }).FirstOrDefault();
            if (i == null)
                return null;
			var p = i.pp;
			var fam = i.f;

            var pi = new PersonInfo
            {
                PeopleId = p.PeopleId,
                AddressTypeId = p.AddressTypeId,
                Deceased = p.IsDeceased ?? false,
                FamilyId = p.FamilyId,
                Name = p.Name,
                Picture = i.Picture,
                SpouseId = p.SpouseId,
                StatusFlags = (i.statusflags ?? "").Split(','),

                member = new MemberInfo
                {
                    PeopleId = p.PeopleId,
                    BaptismSchedDate = p.BaptismSchedDate,
                    BaptismTypeId = p.BaptismTypeId ?? 0,
                    BaptismStatusId = p.BaptismStatusId ?? 0,
                    BaptismDate = p.BaptismDate,
                    DecisionDate = p.DecisionDate,
                    DecisionTypeId = p.DecisionTypeId ?? 0,
                    DropDate = p.DropDate,
                    DropTypeId = p.DropCodeId,
                    EnvelopeOptionId = p.EnvelopeOptionsId ?? 0,
                    StatementOptionId = p.ContributionOptionsId ?? 0,
                    JoinTypeId = p.JoinCodeId,
                    NewChurch = p.OtherNewChurch,
                    PrevChurch = p.OtherPreviousChurch,
                    NewMemberClassDate = p.NewMemberClassDate,
                    NewMemberClassStatusId = p.NewMemberClassStatusId ?? 0,
                    MemberStatusId = p.MemberStatusId,
                    JoinDate = p.JoinDate,
                },
                basic = new BasicPersonInfo
                {
                    PeopleId = p.PeopleId,
                    person = p,
                    Age = p.Age.ToString(),
                    Birthday = p.DOB,
                    CampusId = p.CampusId ?? 0,
                    Mobile = new CellPhoneInfo(p.CellPhone.FmtFone(), p.ReceiveSMS),
                    DeceasedDate = p.DeceasedDate,
                    DoNotCallFlag = p.DoNotCallFlag,
                    DoNotMailFlag = p.DoNotMailFlag,
                    DoNotVisitFlag = p.DoNotVisitFlag,
                    PrimaryEmail = new EmailInfo(p.EmailAddress, p.SendEmailAddress1 ?? true),
                    AltEmail = new EmailInfo(p.EmailAddress2, p.SendEmailAddress2 ?? false),
                    Gender = new CodeInfo(p.GenderId, "Gender"),
                    Marital = new CodeInfo(p.MaritalStatusId, "Marital"),
                    MemberStatus = new CodeInfo(p.MemberStatusId, "MemberStatus"),
                    FamilyPosition = new CodeInfo(p.PositionInFamilyId, "FamilyPosition"),
                    Employer = p.EmployerOther,
                    FirstName = p.FirstName,
                    Created = p.CreatedDate,
                    Grade = p.Grade.ToString(),
                    HomePhone = p.Family.HomePhone,
                    JoinDate = p.JoinDate,
                    LastName = p.LastName,
                    AltName = p.AltName,
                    FormerName = p.MaidenName,
                    MemberStatusId = p.MemberStatusId,
                    MiddleName = p.MiddleName,
                    GoesBy = p.NickName,
                    Occupation = p.OccupationOther,
                    School = p.SchoolOther,
                    Spouse = i.spouse,
                    Suffix = p.SuffixCode,
                    Title = new CodeInfo(p.TitleCode, "Title"),
                    WeddingDate = p.WeddingDate.FormatDate(),
                    Work = p.WorkPhone.FmtFone(),
                    ReceiveSMS = p.ReceiveSMS,
                },
                growth = new GrowthInfo
                {
                    PeopleId = p.PeopleId,
                    InterestPointId = p.InterestPointId ?? 0,
                    OriginId = p.OriginId ?? 0,
                    EntryPointId = p.EntryPointId ?? 0,
                    ChristAsSavior = p.ChristAsSavior,
                    Comments = p.Comments,
                    InterestedInJoining = p.InterestedInJoining,
                    MemberAnyChurch = p.MemberAnyChurch,
                    PleaseVisit = p.PleaseVisit,
                    SendInfo = p.InfoBecomeAChristian,
                },
                membernotes = new MemberNotesInfo
                {
                    PeopleId = p.PeopleId,
                    LetterStatusId = p.LetterStatusId ?? 0,
                    LetterReceived = p.LetterDateReceived,
                    LetterRequested = p.LetterDateRequested,
                    LetterNotes = p.LetterStatusNotes,
                },
                FamilyAddr = new AddressInfo
                {
                    Name = "FamilyAddr",
                    PeopleId = p.PeopleId,
                    person = p,
                    Address1 = fam.AddressLineOne,
                    Address2 = fam.AddressLineTwo,
                    City = fam.CityName,
                    Zip = fam.ZipCode,
                    BadAddress = fam.BadAddressFlag,
                    State = new CodeInfo(fam.StateCode, "State"),
                    Country = new CodeInfo(fam.CountryName, "Country"),
                    ResCode = new CodeInfo(fam.ResCodeId, "ResCode"),
                    FromDt = fam.AddressFromDate,
                    ToDt = fam.AddressToDate,
                    Preferred = p.AddressTypeId == 10,
                },
                PersonalAddr = new AddressInfo
                {
                    Name = "PersonalAddr",
                    PeopleId = p.PeopleId,
                    person = p,
                    Address1 = p.AddressLineOne,
                    Address2 = p.AddressLineTwo,
                    City = p.CityName,
                    State = new CodeInfo(p.StateCode, "State"),
                    Country = new CodeInfo(p.CountryName, "Country"),
                    ResCode = new CodeInfo(p.ResCodeId, "ResCode"),
                    Zip = p.ZipCode,
                    BadAddress = p.BadAddressFlag,
                    FromDt = p.AddressFromDate,
                    ToDt = p.AddressToDate,
                    Preferred = p.AddressTypeId == 30,
                },
                //familymembers = i.familymembers
            };
            return pi;
        }
    }
}
