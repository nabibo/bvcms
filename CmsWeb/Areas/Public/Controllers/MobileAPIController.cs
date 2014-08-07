﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsData;
using CmsWeb.Areas.Reports.Models;
using CmsWeb.Models.iPhone;
using CmsWeb.MobileAPI;
using UtilityExtensions;
using Newtonsoft.Json;
using CmsWeb.Models;
using CmsData.Codes;
using System.Text.RegularExpressions;
using System.Text;
using CmsData.Registration;

namespace CmsWeb.Areas.Public.Controllers
{
	public class MobileAPIController : Controller
	{
		public ActionResult Authenticate()
		{
			if (CmsWeb.Models.AccountModel.AuthenticateMobile()) return null;
			else
			{
				return BaseMessage.createErrorReturn("You are not authorized!");
			}
		}

		public ActionResult Exists()
		{
			return Content("1");
		}

		public ActionResult CheckLogin(string data)
		{
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_LOGIN)
				return BaseMessage.createTypeErrorReturn();

			var givingOrg = (from e in DbUtil.Db.Organizations
								  where e.RegistrationTypeId == 8
								  select e).FirstOrDefault();

			var roles = from r in DbUtil.Db.UserRoles
							where r.UserId == Util.UserId
							orderby r.Role.RoleName
							select new MobileRole
							{
								id = r.RoleId,
								name = r.Role.RoleName
							};

			MobileSettings ms = new MobileSettings();

			ms.peopleID = Util.UserPeopleId ?? 0;
			ms.userID = Util.UserId;

			ms.givingEnabled = DbUtil.Db.Setting("EnableMobileGiving", "true") == "true" ? 1 : 0;
			ms.givingAllowCC = DbUtil.Db.Setting("NoCreditCardGiving", "false") == "false" ? 1 : 0;
			ms.givingOrgID = givingOrg != null ? givingOrg.OrganizationId : 0;
			ms.roles = roles.ToList();

			// Everything is in order, start the return
			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.id = Util.UserPeopleId ?? 0;
			br.data = JsonConvert.SerializeObject(ms);

			return br;
		}

		public ActionResult FetchPeople(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_PEOPLE_SEARCH)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostSearch mps = JsonConvert.DeserializeObject<MobilePostSearch>(dataIn.data);

			BaseMessage br = new BaseMessage();

			var m = new CmsWeb.Models.iPhone.SearchModel(mps.name, mps.comm, mps.addr);

			br.error = 0;
			br.type = BaseMessage.API_TYPE_PEOPLE_SEARCH;
			br.count = m.Count;

			switch (dataIn.device)
			{
				case BaseMessage.API_DEVICE_ANDROID:
					{
						Dictionary<int, MobilePerson> mpl = new Dictionary<int, MobilePerson>();

						MobilePerson mp;

						foreach (var item in m.ApplySearch().OrderBy(p => p.Name2).Take(20))
						{
							mp = new MobilePerson().populate(item);
							mpl.Add(mp.id, mp);
						}

						br.data = JsonConvert.SerializeObject(mpl);
						break;
					}

				case BaseMessage.API_DEVICE_IOS:
					{
						List<MobilePerson> mp = new List<MobilePerson>();

						foreach (var item in m.ApplySearch().OrderBy(p => p.Name2).Take(20))
						{
							mp.Add(new MobilePerson().populate(item));
						}

						br.data = JsonConvert.SerializeObject(mp);
						break;
					}

				default: break;
			}

			return br;
		}

		public ActionResult FetchPerson(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_PERSON_REFRESH)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostFetchPerson mpfs = JsonConvert.DeserializeObject<MobilePostFetchPerson>(dataIn.data);

			BaseMessage br = new BaseMessage();

			var person = DbUtil.Db.People.SingleOrDefault(p => p.PeopleId == mpfs.id);

			if (person == null)
			{
				br.error = 1;
				br.data = "Person not found.";
				return br;
			}

			br.error = 0;
			br.type = BaseMessage.API_TYPE_PERSON_REFRESH;
			br.count = 1;

			if (dataIn.device == BaseMessage.API_DEVICE_ANDROID)
			{
				br.data = JsonConvert.SerializeObject(new MobilePerson().populate(person));
			}
			else
			{
				List<MobilePerson> mp = new List<MobilePerson>();
				mp.Add(new MobilePerson().populate(person));
				br.data = JsonConvert.SerializeObject(mp);
			}

			return br;
		}

		public ActionResult FetchImage(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_PERSON_IMAGE_REFRESH)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostFetchImage mpfi = JsonConvert.DeserializeObject<MobilePostFetchImage>(dataIn.data);

			BaseMessage br = new BaseMessage();
			if (mpfi.id == 0) return br.setData("The ID for the person cannot be set to zero");

			br.data = "The picture was not found.";
			br.type = BaseMessage.API_TYPE_PERSON_IMAGE_REFRESH;

			var person = DbUtil.Db.People.SingleOrDefault(pp => pp.PeopleId == mpfi.id);

			if (person.PictureId != null)
			{
				ImageData.Image image = null;

				switch (mpfi.size)
				{
					case 0: // 50 x 50
						image = ImageData.DbUtil.Db.Images.SingleOrDefault(i => i.Id == person.Picture.ThumbId);
						break;

					case 1: // 120 x 120
						image = ImageData.DbUtil.Db.Images.SingleOrDefault(i => i.Id == person.Picture.SmallId);
						break;

					case 2: // 320 x 400
						image = ImageData.DbUtil.Db.Images.SingleOrDefault(i => i.Id == person.Picture.MediumId);
						break;

					case 3: // 570 x 800
						image = ImageData.DbUtil.Db.Images.SingleOrDefault(i => i.Id == person.Picture.LargeId);
						break;

				}

				if (image != null)
				{
					br.data = Convert.ToBase64String(image.Bits);
					br.count = 1;
					br.error = 0;
				}
			}

			return br;
		}

		public ActionResult SaveImage(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_PERSON_IMAGE_ADD)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostSaveImage mpsi = JsonConvert.DeserializeObject<MobilePostSaveImage>(dataIn.data);

			BaseMessage br = new BaseMessage();

			var imageBytes = Convert.FromBase64String(mpsi.image);

			var person = DbUtil.Db.People.SingleOrDefault(pp => pp.PeopleId == mpsi.id);

			if (person.Picture != null)
			{
				ImageData.DbUtil.Db.Images.DeleteOnSubmit(ImageData.DbUtil.Db.Images.Where(i => i.Id == person.Picture.ThumbId).SingleOrDefault());
				ImageData.DbUtil.Db.Images.DeleteOnSubmit(ImageData.DbUtil.Db.Images.Where(i => i.Id == person.Picture.SmallId).SingleOrDefault());
				ImageData.DbUtil.Db.Images.DeleteOnSubmit(ImageData.DbUtil.Db.Images.Where(i => i.Id == person.Picture.MediumId).SingleOrDefault());
				ImageData.DbUtil.Db.Images.DeleteOnSubmit(ImageData.DbUtil.Db.Images.Where(i => i.Id == person.Picture.LargeId).SingleOrDefault());

				person.Picture.ThumbId = ImageData.Image.NewImageFromBits(imageBytes, 50, 50).Id;
				person.Picture.SmallId = ImageData.Image.NewImageFromBits(imageBytes, 120, 120).Id;
				person.Picture.MediumId = ImageData.Image.NewImageFromBits(imageBytes, 320, 400).Id;
				person.Picture.LargeId = ImageData.Image.NewImageFromBits(imageBytes).Id;
			}
			else
			{
				var newPicture = new Picture();

				newPicture.ThumbId = ImageData.Image.NewImageFromBits(imageBytes, 50, 50).Id;
				newPicture.SmallId = ImageData.Image.NewImageFromBits(imageBytes, 120, 120).Id;
				newPicture.MediumId = ImageData.Image.NewImageFromBits(imageBytes, 320, 400).Id;
				newPicture.LargeId = ImageData.Image.NewImageFromBits(imageBytes).Id;

				person.Picture = newPicture;
			}

			DbUtil.Db.SubmitChanges();

			br.error = 0;
			br.data = "Image updated.";
			br.id = mpsi.id;
			br.count = 1;

			return br;
		}

		public ActionResult FetchOrgs(string data)
		{
			var authError = Authenticate();
			if (authError != null) return authError;

			if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Attendance"))
				return BaseMessage.createErrorReturn("Attendance roles is required to take attendance for organizations");

			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_ORG_REFRESH)
				return BaseMessage.createTypeErrorReturn();

			var pid = Util.UserPeopleId;
			var oids = DbUtil.Db.GetLeaderOrgIds(pid);
			var dt = DateTime.Parse("8:00 AM");

			var roles = DbUtil.Db.CurrentRoles();

			IQueryable<Organization> q = null;

			if (Util2.OrgLeadersOnly)
			{
				q = from o in DbUtil.Db.Organizations
					 where o.LimitToRole == null || roles.Contains(o.LimitToRole)
					 where oids.Contains(o.OrganizationId)
					 where o.SecurityTypeId != 3
					 select o;
			}
			else
			{
				q = from o in DbUtil.Db.Organizations
					 where o.LimitToRole == null || roles.Contains(o.LimitToRole)
					 let sc = o.OrgSchedules.FirstOrDefault() // SCHED
					 where (o.OrganizationMembers.Any(om => om.PeopleId == pid // either a leader, who is not pending / inactive
								  && (om.Pending ?? false) == false
								  && (om.MemberTypeId != MemberTypeCode.InActive)
								  && (om.MemberType.AttendanceTypeId == AttendTypeCode.Leader)
								  )
							 || oids.Contains(o.OrganizationId)) // or a leader of a parent org
					 where o.SecurityTypeId != 3
					 select o;
			}

			var orgs = from o in q
						  let sc = o.OrgSchedules.FirstOrDefault() // SCHED
						  select new OrganizationInfo
						  {
							  id = o.OrganizationId,
							  name = o.OrganizationName,
							  time = sc.SchedTime ?? dt,
							  day = sc.SchedDay ?? 0
						  };

			BaseMessage br = new BaseMessage();
			List<MobileOrganization> mo = new List<MobileOrganization>();

			br.error = 0;
			br.type = BaseMessage.API_TYPE_ORG_REFRESH;
			br.count = orgs.Count();

			foreach (var item in orgs)
			{
				mo.Add(new MobileOrganization().populate(item));
			}

			br.data = JsonConvert.SerializeObject(mo);
			return br;
		}

		public ActionResult FetchOrgRollList(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check Role
			if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Attendance"))
				return BaseMessage.createErrorReturn("Attendance roles is required to take attendance for organizations");

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_ORG_ROLL_REFRESH)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostRollList mprl = JsonConvert.DeserializeObject<MobilePostRollList>(dataIn.data);

			var meeting = Meeting.FetchOrCreateMeeting(DbUtil.Db, mprl.id, mprl.datetime);
			var people = RollsheetModel.RollList(meeting.MeetingId, meeting.OrganizationId, meeting.MeetingDate.Value);

			BaseMessage br = new BaseMessage();
			List<MobileAttendee> ma = new List<MobileAttendee>();

			br.id = meeting.MeetingId;
			br.error = 0;
			br.type = BaseMessage.API_TYPE_ORG_ROLL_REFRESH;
			br.count = people.Count();

			foreach (var person in people)
			{
				ma.Add(new MobileAttendee().populate(person));
			}

			br.data = JsonConvert.SerializeObject(ma);
			return br;
		}

		public ActionResult RecordAttend(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check Role
			if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Attendance"))
				return BaseMessage.createErrorReturn("Attendance roles is required to take attendance for organizations");

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_ORG_RECORD_ATTEND)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostAttend mpa = JsonConvert.DeserializeObject<MobilePostAttend>(dataIn.data);

			var meeting = DbUtil.Db.Meetings.SingleOrDefault(m => m.OrganizationId == mpa.orgID && m.MeetingDate == mpa.datetime);

			if (meeting == null)
			{
				meeting = new CmsData.Meeting
				{
					OrganizationId = mpa.orgID,
					MeetingDate = mpa.datetime,
					CreatedDate = Util.Now,
					CreatedBy = Util.UserPeopleId ?? 0,
					GroupMeetingFlag = false,
				};
				DbUtil.Db.Meetings.InsertOnSubmit(meeting);
				DbUtil.Db.SubmitChanges();
				var acr = (from s in DbUtil.Db.OrgSchedules
							  where s.OrganizationId == mpa.orgID
							  where s.SchedTime.Value.TimeOfDay == mpa.datetime.TimeOfDay
							  where s.SchedDay == (int)mpa.datetime.DayOfWeek
							  select s.AttendCreditId).SingleOrDefault();
				meeting.AttendCreditId = acr;
			}

			Attend.RecordAttendance(mpa.peopleID, meeting.MeetingId, mpa.present);

			DbUtil.Db.UpdateMeetingCounters(mpa.orgID);
			DbUtil.LogActivity("Mobile RecAtt o:{0} p:{1} u:{2} a:{3}".Fmt(meeting.OrganizationId, mpa.peopleID, Util.UserPeopleId, mpa.present));

			BaseMessage br = new BaseMessage();

			br.error = 0;
			br.count = 1;
			br.type = BaseMessage.API_TYPE_ORG_RECORD_ATTEND;

			return br;
		}

		public ActionResult AddPerson(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check Role
			if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Attendance"))
				return BaseMessage.createErrorReturn("Attendance role is required to take attendance for organizations.");

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_PERSON_ADD)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostAddPerson mpap = JsonConvert.DeserializeObject<MobilePostAddPerson>(dataIn.data);
			mpap.clean();

			var p = new Person();

			p.CreatedDate = Util.Now;
			p.CreatedBy = Util.UserId;

			p.MemberStatusId = MemberStatusCode.JustAdded;
			p.AddressTypeId = 10;

			p.FirstName = mpap.firstName;
			p.LastName = mpap.lastName;
			p.Name = "";

			if (mpap.goesBy.Length > 0)
				p.NickName = mpap.goesBy;

			if (mpap.birthday != null)
			{
				p.BirthDay = mpap.birthday.Value.Day;
				p.BirthMonth = mpap.birthday.Value.Month;
				p.BirthYear = mpap.birthday.Value.Year;
			}

			p.GenderId = mpap.genderID;
			p.MaritalStatusId = mpap.maritalStatusID;

			CmsData.Family f;

			if (mpap.familyID > 0)
			{
				f = DbUtil.Db.Families.First(fam => fam.FamilyId == mpap.familyID);
			}
			else
			{
				f = new Family();

				if (mpap.homePhone.Length > 0)
					f.HomePhone = mpap.homePhone;

				if (mpap.address.Length > 0)
					f.AddressLineOne = mpap.address;

				if (mpap.address2.Length > 0)
					f.AddressLineTwo = mpap.address2;

				if (mpap.city.Length > 0)
					f.CityName = mpap.city;

				if (mpap.state.Length > 0)
					f.StateCode = mpap.state;

				if (mpap.zipcode.Length > 0)
					f.ZipCode = mpap.zipcode;

				if (mpap.country.Length > 0)
					f.CountryName = mpap.country;

				DbUtil.Db.Families.InsertOnSubmit(f);
			}

			f.People.Add(p);

			p.PositionInFamilyId = PositionInFamily.Child;

			if (mpap.birthday != null && p.GetAge() >= 18)
			{
				if (f.People.Count(per => per.PositionInFamilyId == PositionInFamily.PrimaryAdult) < 2)
					p.PositionInFamilyId = PositionInFamily.PrimaryAdult;
				else
					p.PositionInFamilyId = PositionInFamily.SecondaryAdult;
			}

			p.OriginId = OriginCode.Visit;
			p.FixTitle();

			if (mpap.eMail.Length > 0)
				p.EmailAddress = mpap.eMail;

			if (mpap.cellPhone.Length > 0)
				p.CellPhone = mpap.cellPhone;

			if (mpap.homePhone.Length > 0)
				p.HomePhone = mpap.homePhone;

			p.MaritalStatusId = mpap.maritalStatusID;
			p.GenderId = mpap.genderID;

			DbUtil.Db.SubmitChanges();

			if (mpap.visitMeeting > 0)
			{
				var meeting = DbUtil.Db.Meetings.Single(mm => mm.MeetingId == mpap.visitMeeting);
				Attend.RecordAttendance(p.PeopleId, mpap.visitMeeting, true);
				DbUtil.Db.UpdateMeetingCounters(mpap.visitMeeting);
			}

			BaseMessage br = new BaseMessage();

			br.error = 0;
			br.id = p.PeopleId;
			br.count = 1;
			br.type = BaseMessage.API_TYPE_PERSON_ADD;

			return br;
		}

		public ActionResult JoinOrg(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check Role
			if (!CMSRoleProvider.provider.IsUserInRole(AccountModel.UserName2, "Attendance"))
				return BaseMessage.createErrorReturn("Attendance role is required to take attendance for organizations.");

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_ORG_JOIN)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostJoinOrg mpjo = JsonConvert.DeserializeObject<MobilePostJoinOrg>(dataIn.data);

			var om = DbUtil.Db.OrganizationMembers.SingleOrDefault(m => m.PeopleId == mpjo.peopleID && m.OrganizationId == mpjo.orgID);

			if (om != null)
			{
				if (mpjo.join)
					om = OrganizationMember.InsertOrgMembers(DbUtil.Db, mpjo.orgID, mpjo.peopleID, MemberTypeCode.Member, DateTime.Now, null, false);
				else
					om.Drop(DbUtil.Db, addToHistory: true);
			}

			DbUtil.Db.SubmitChanges();

			BaseMessage br = new BaseMessage();

			br.error = 0;
			br.count = 1;
			br.type = BaseMessage.API_TYPE_ORG_JOIN;

			return br;
		}


		public ActionResult FetchPaymentStatus(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_PERSON_PAYMENT_INFO)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_PERSON_PAYMENT_INFO;

			var payInfo = (from e in DbUtil.Db.PaymentInfos
								where e.PeopleId == Util.UserPeopleId
								select e).FirstOrDefault();

			if (payInfo != null)
			{
				List<MobilePaymentInfo> mpi = new List<MobilePaymentInfo>();

				if (payInfo.SageBankGuid != null)
				{
					mpi.Add(new MobilePaymentInfo().populate(payInfo, MobilePaymentInfo.TYPE_BANK_ACCOUNT));
					br.count++;
				}

				if (payInfo.SageCardGuid != null)
				{
					mpi.Add(new MobilePaymentInfo().populate(payInfo, MobilePaymentInfo.TYPE_CREDIT_CARD));
					br.count++;
				}

				br.data = JsonConvert.SerializeObject(mpi);
			}

			return br;
		}

		public ActionResult processGiving(string data)
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_GIVING_GIVE)
				return BaseMessage.createTypeErrorReturn();

			// Everything is in order, start the return
			MobilePostTransaction mpt = JsonConvert.DeserializeObject<MobilePostTransaction>(dataIn.data);

			if (mpt.amount == 0 || mpt.paymentType == 0 || mpt.peopleID == 0)
				return BaseMessage.createErrorReturn("Missing information!");

			Transaction ti = createTransaction(mpt);

			if (ti != null && ti.Id > 0)
			{
				SagePayments sp = new SagePayments(DbUtil.Db, false);
				TransactionResponse tr = null;

				switch (mpt.paymentType)
				{
					case MobilePostTransaction.TYPE_BANK_ACCOUNT:
						tr = sp.createVaultTransactionRequest(mpt.peopleID, mpt.amount, mpt.description, ti.Id, "B");
						break;

					case MobilePostTransaction.TYPE_CREDIT_CARD:
						tr = sp.createVaultTransactionRequest(mpt.peopleID, mpt.amount, mpt.description, ti.Id, "C");
						break;
				}

				// Build return
				BaseMessage br = new BaseMessage();
				br.type = BaseMessage.API_TYPE_GIVING_GIVE;

				if (tr != null && tr.Approved)
				{
					br.error = 0;
					br.id = mpt.transactionID = ti.Id;

					sendConfirmation(mpt);
				}
				else
				{
					DbUtil.Db.Transactions.DeleteOnSubmit(ti);
					DbUtil.Db.SubmitChanges();

					br.error = 1;
					br.data = tr.Message;
				}

				return br;
			}
			else
			{
				return BaseMessage.createErrorReturn("Transaction creation failed!");
			}
		}

		public Transaction createTransaction(MobilePostTransaction mpt)
		{
			var ti = new Transaction();

			ti.First = mpt.first;
			ti.MiddleInitial = mpt.middle;
			ti.Last = mpt.last;
			ti.Suffix = mpt.suffix;
			ti.Amt = mpt.amount;
			ti.Emails = mpt.email;
			ti.Description = mpt.description;
			ti.OrgId = mpt.orgID;
			ti.Address = mpt.address;
			ti.City = mpt.city;
			ti.State = mpt.state;
			ti.Zip = mpt.zip;
			ti.Phone = mpt.phone;
			ti.TransactionDate = mpt.date;

			DbUtil.Db.Transactions.InsertOnSubmit(ti);
			DbUtil.Db.SubmitChanges();

			ti.OriginalId = ti.Id;

			DbUtil.Db.SubmitChanges();

			return ti;
		}

		public void sendConfirmation(MobilePostTransaction mpt)
		{
			var person = DbUtil.Db.LoadPersonById(mpt.peopleID);
			var staff = DbUtil.Db.StaffPeopleForOrg(mpt.orgID)[0];

			var org = DbUtil.Db.LoadOrganizationById(mpt.orgID);
			var settings = new Settings(org.RegSetting, DbUtil.Db, mpt.orgID);

			var text = settings.Body.Replace("{church}", DbUtil.Db.Setting("NameOfChurch", "church"), ignoreCase: true);
			text = text.Replace("{amt}", (mpt.amount).ToString("N2"));
			text = text.Replace("{date}", DateTime.Today.ToShortDateString());
			text = text.Replace("{tranid}", mpt.transactionID.ToString());
			text = text.Replace("{name}", person.Name);
			text = text.Replace("{account}", "");
			text = text.Replace("{email}", person.EmailAddress);
			text = text.Replace("{phone}", person.HomePhone.FmtFone());
			text = text.Replace("{contact}", staff.Name);
			text = text.Replace("{contactemail}", staff.EmailAddress);
			text = text.Replace("{contactphone}", org.PhoneNumber.FmtFone());

			var re = new Regex(@"(?<b>.*?)<!--ITEM\sROW\sSTART-->(?<row>.*?)\s*<!--ITEM\sROW\sEND-->(?<e>.*)", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
			var match = re.Match(text);
			var b = match.Groups["b"].Value;
			var row = match.Groups["row"].Value.Replace("{funditem}", "{0}").Replace("{itemamt}", "{1:N2}");
			var e = match.Groups["e"].Value;
			var sb = new StringBuilder(b);

			var desc = "{0}; {1}; {2}".Fmt(person.Name, person.PrimaryAddress, person.PrimaryZip);

			foreach (var item in mpt.items)
			{
				sb.AppendFormat(row, item.name, item.amount);
				person.PostUnattendedContribution(DbUtil.Db, item.amount, item.id, desc);
			}

			var contributionemail = (from ex in person.PeopleExtras
											 where ex.Field == "ContributionEmail"
											 select ex.Data).SingleOrDefault();

			if (contributionemail.HasValue())
				contributionemail = (contributionemail ?? "").Trim();
			if (!Util.ValidEmail(contributionemail))
				contributionemail = person.FromEmail;

			Util.SendMsg(Util.SysFromEmail, Util.Host, Util.TryGetMailAddress(DbUtil.Db.StaffEmailForOrg(mpt.orgID)),
				 settings.Subject, sb.ToString(),
				 Util.EmailAddressListFromString(contributionemail), 0, person.PeopleId);

			DbUtil.Db.Email(contributionemail, DbUtil.Db.StaffPeopleForOrg(mpt.orgID),
				 "Online giving contribution received",
				 "See contribution records for {0} ({1})".Fmt(person.Name, person.PeopleId));

			/*
			t.Financeonly = true;
			if (t.Donate > 0)
			{
				var fundname = DbUtil.Db.ContributionFunds.Single(ff => ff.FundId == p.setting.DonationFundId).FundName;
				sb.AppendFormat(row, fundname, t.Donate);
				t.Fund = p.setting.DonationFund();
				p.person.PostUnattendedContribution(DbUtil.Db,
					 t.Donate.Value,
					 p.setting.DonationFundId,
					 desc);
			}
			sb.Append(e);
			if (!t.TransactionId.HasValue())
			{
				t.TransactionId = TransactionID;
				if (m.testing == true && !t.TransactionId.Contains("(testing)"))
					t.TransactionId += "(testing)";
			}
			p.CreateAccount();
			*/
		}
	}
}