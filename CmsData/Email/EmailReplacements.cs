﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using CmsData.Codes;
using HtmlAgilityPack;
using UtilityExtensions;

namespace CmsData
{
    public class EmailReplacements
    {
        private const string RegisterLinkRe = "<a[^>]*?href=\"https{0,1}://registerlink2{0,1}/{0,1}\"[^>]*>.*?</a>";
        private readonly Regex registerLinkRe = new Regex(RegisterLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string RegisterTagRe = "(?:<|&lt;)registertag[^>]*(?:>|&gt;).+?(?:<|&lt;)/registertag(?:>|&gt;)";
        private readonly Regex registerTagRe = new Regex(RegisterTagRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string RsvpLinkRe = "<a[^>]*?href=\"https{0,1}://rsvplink/{0,1}\"[^>]*>.*?</a>";
        private readonly Regex rsvpLinkRe = new Regex(RsvpLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string SendLinkRe = "<a[^>]*?href=\"https{0,1}://sendlink2{0,1}/{0,1}\"[^>]*>.*?</a>";
        private readonly Regex sendLinkRe = new Regex(SendLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string SupportLinkRe = "<a[^>]*?href=\"https{0,1}://supportlink/{0,1}\"[^>]*>.*?</a>";
        private readonly Regex supportLinkRe = new Regex(SupportLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string VolReqLinkRe = "<a[^>]*?href=\"https{0,1}://volreqlink\"[^>]*>.*?</a>";
        private readonly Regex volReqLinkRe = new Regex(VolReqLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string VolSubLinkRe = "<a[^>]*?href=\"https{0,1}://volsublink\"[^>]*>.*?</a>";
        private readonly Regex volSubLinkRe = new Regex(VolSubLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private const string VoteLinkRe = "<a[^>]*?href=\"https{0,1}://votelink/{0,1}\"[^>]*>.*?</a>";
        private readonly Regex voteLinkRe = new Regex(VoteLinkRe, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly CMSDataContext db;

        private readonly string[] stringlist;
        private readonly MailAddress from;

        public EmailReplacements(CMSDataContext db, string text, MailAddress from)
        {
            this.db = db;
            this.from = from;
            if (text == null)
                text = "(no content)";
            string pattern = @"({{[^}}]*?}}|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8})".Fmt(RegisterLinkRe, RegisterTagRe, RsvpLinkRe, SendLinkRe, SupportLinkRe, VolReqLinkRe, VolReqLinkRe, VolSubLinkRe, VoteLinkRe);
            stringlist = Regex.Split(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        public List<MailAddress> ListAddresses { get; set; }

        public string DoReplacements(Person p, EmailQueueTo emailqueueto)
        {
            var pi = emailqueueto.OrgId.HasValue
                ? (from m in db.OrganizationMembers
                   let ts = db.ViewTransactionSummaries.SingleOrDefault(tt => tt.RegId == m.TranId && tt.PeopleId == m.PeopleId)
                   where m.PeopleId == emailqueueto.PeopleId && m.OrganizationId == emailqueueto.OrgId
                   select new PayInfo
                   {
                       PayLink = m.PayLink2(db),
                       Amount = ts.IndAmt,
                       AmountPaid = ts.IndPaid,
                       AmountDue = ts.IndDue,
                       RegisterMail = m.RegisterEmail
                   }).SingleOrDefault()
                : null;

            var aa = db.GetAddressList(p);

            if (emailqueueto.AddEmail.HasValue())
                foreach (string ad in emailqueueto.AddEmail.SplitStr(","))
                    Util.AddGoodAddress(aa, ad);

            if (emailqueueto.OrgId.HasValue && pi != null)
                Util.AddGoodAddress(aa, Util.FullEmail(pi.RegisterMail, p.Name));

            ListAddresses = aa.DistinctEmails();

            var texta = new List<string>(stringlist);
            for (var i = 1; i < texta.Count; i += 2)
                texta[i] = DoReplaceCode(texta[i], p, pi, emailqueueto);

            return string.Join("", texta);
        }

        private class PayInfo
        {
            public string PayLink { get; set; }
            public decimal? Amount { get; set; }
            public decimal? AmountPaid { get; set; }
            public decimal? AmountDue { get; set; }
            public string RegisterMail { get; set; }
        }

        private string DoReplaceCode(string code, Person p, PayInfo pi, EmailQueueTo emailqueueto)
        {
            switch (code.ToLower())
            {
                case "{address}":
                    return p.PrimaryAddress;

                case "{address2}":
                    if (p.PrimaryAddress2.HasValue())
                        return "<br>" + p.PrimaryAddress2;
                    return "";

                case "{amtdue}":
                    if (pi != null)
                        return pi.AmountDue.ToString2("c");
                    break;

                case "{amtpaid}":
                    if (pi != null)
                        return pi.AmountPaid.ToString2("c");
                    break;

                case "{amount}":
                    if (pi != null)
                        return pi.Amount.ToString2("c");
                    break;

                case "{barcode}":
                    return string.Format("<img src='{0}' />", Util.URLCombine(db.CmsHost, "/Track/Barcode/" + emailqueueto.PeopleId));

                case "{campus}":
                    return p.CampusId != null ? p.Campu.Description : "No Campus Specified";

                case "{cellphone}":
                    return p.CellPhone.HasValue() ? p.CellPhone.FmtFone() : "no cellphone on record";

                case "{city}":
                    return p.PrimaryCity;

                case "{csz}":
                    return Util.FormatCSZ(p.PrimaryCity, p.PrimaryState, p.PrimaryZip);

                case "{country}":
                    return p.PrimaryCountry;

                case "{createaccount}":
                    return CreateUserTag(emailqueueto);

                case "{cmshost}":
                    return db.CmsHost.TrimEnd('/');

                case "{emailhref}":
                    return Util.URLCombine(db.CmsHost, "/EmailView/" + emailqueueto.Id);

                case "{first}":
                    return p.PreferredName.Contains("?") || p.PreferredName.Contains("unknown", true) ? "" : p.PreferredName;

                case "{fromemail}":
                    return from.Address;

                case "{last}":
                    return p.LastName;

                case "{name}":
                    return p.Name.Contains("?") || p.Name.Contains("unknown", true) ? "" : p.Name;

                case "{nextmeetingtime}":
                    return NextMeetingDate(code, emailqueueto);

                case "{occupation}":
                    return p.OccupationOther;

                case "{orgname}":
                    return
                        db.Organizations.Where(oo => oo.OrganizationId == db.CurrentOrgId)
                            .Select(oo => oo.OrganizationName).SingleOrDefault();

                case "{orgmembercount}":
                    return
                        db.OrganizationMembers.Count(om => om.OrganizationId == db.CurrentOrgId).ToString();

                case "{paylink}":
                    if (pi != null && pi.PayLink.HasValue())
                        return "<a href=\"{0}\">Click this link to make a payment and view your balance.</a>".Fmt(pi.PayLink);
                    break;

                case "{peopleid}":
                    return p.PeopleId.ToString();

                case "{saluation}":
                    return db.GoerSupporters.Where(ee => ee.Id == emailqueueto.GoerSupportId).Select(ee => ee.Salutation).SingleOrDefault();

                case "{state}":
                    return p.PrimaryState;

                case "{toemail}":
                    if (ListAddresses.Count > 0)
                        return ListAddresses[0].Address;
                    break;

                case "{today}":
                    return DateTime.Today.ToShortDateString();

                case "{track}":
                    return emailqueueto.Guid.HasValue ? "<img src=\"{0}\" />".Fmt(Util.URLCombine(db.CmsHost, "/Track/Key/" + emailqueueto.Guid.Value.GuidToQuerystring())) : "";

                case "{unsubscribe}":
                    return UnSubscribeLink(emailqueueto);

                default:
                    if (code.StartsWith("{addsmallgroup:", StringComparison.OrdinalIgnoreCase))
                        return AddSmallGroup(code, emailqueueto);

                    if (code.StartsWith("{extra", StringComparison.OrdinalIgnoreCase))
                        return ExtraValue(code, emailqueueto);

                    if (registerLinkRe.IsMatch(code))
                        return RegisterLink(code, emailqueueto);

                    if (registerTagRe.IsMatch(code))
                        return RegisterTag(code, emailqueueto);

                    if (rsvpLinkRe.IsMatch(code))
                        return RsvpLink(code, emailqueueto);

                    if (sendLinkRe.IsMatch(code))
                        return SendLink(code, emailqueueto);

                    if (code.StartsWith("{orgextra:", StringComparison.OrdinalIgnoreCase))
                        return OrgExtra(code, emailqueueto);

                    if (code.StartsWith("{orgmember:", StringComparison.OrdinalIgnoreCase))
                        return OrgMember(code, emailqueueto);

                    if (code.StartsWith("{smallgroup:", StringComparison.OrdinalIgnoreCase))
                        return SmallGroup(code, emailqueueto);

                    if (code.StartsWith("{smallgroups", StringComparison.OrdinalIgnoreCase))
                        return SmallGroups(code, emailqueueto); ;

                    if (supportLinkRe.IsMatch(code))
                        return SupportLink(code, emailqueueto);

                    if (volReqLinkRe.IsMatch(code))
                        return VolReqLink(code, emailqueueto);

                    if (volSubLinkRe.IsMatch(code))
                        return VolSubLink(code, emailqueueto);

                    if (voteLinkRe.IsMatch(code))
                        return VoteLink(code, emailqueueto);

                    break;
            }

            return code;
        }

        const string AddSmallGroupRe = @"\{addsmallgroup:\[(?<group>[^\]]*)\]\}";
        readonly Regex addSmallGroupRe = new Regex(AddSmallGroupRe, RegexOptions.Singleline);
        private string AddSmallGroup(string code, EmailQueueTo emailqueueto)
        {
            var match = addSmallGroupRe.Match(code);
            if (!match.Success || !emailqueueto.OrgId.HasValue)
                return code;

            var group = match.Groups["group"].Value;
            var om = (from mm in db.OrganizationMembers
                      where mm.OrganizationId == emailqueueto.OrgId
                      where mm.PeopleId == emailqueueto.PeopleId
                      select mm).SingleOrDefault();
            if (om != null)
                om.AddToGroup(db, @group);
            return "";
        }

        const string OrgExtraRe = @"\{orgextra:(?<field>[^\]]*)\}";
        readonly Regex orgExtraRe = new Regex(OrgExtraRe, RegexOptions.Singleline);
        private string OrgExtra(string code, EmailQueueTo emailqueueto)
        {
            var match = orgExtraRe.Match(code);
            if (!match.Success || !emailqueueto.OrgId.HasValue)
                return code;
            var field = match.Groups["field"].Value;
            var ev = db.OrganizationExtras.SingleOrDefault(ee => ee.Field == field && ee.OrganizationId == db.CurrentOrgId);
            if (ev == null || !ev.Data.HasValue())
                return null;
            return ev.Data;
        }

        const string OrgMemberRe = @"{orgmember:(?<type>.*?),(?<divid>.*?)}";
        readonly Regex orgMemberRe = new Regex(OrgMemberRe, RegexOptions.Singleline);
        private string OrgMember(string code, EmailQueueTo emailqueueto)
        {
            var match = orgMemberRe.Match(code);
            if (!match.Success)
                return code;
            var divid = match.Groups["divid"].Value.ToInt();
            var type = match.Groups["type"].Value;
            var org = (from om in db.OrganizationMembers
                       where om.PeopleId == emailqueueto.PeopleId
                       where om.Organization.DivOrgs.Any(dd => dd.DivId == divid)
                       select om.Organization).FirstOrDefault();

            if (org == null)
                return "?";

            switch (type.ToLower())
            {
                case "location":
                    return org.Location;
                case "pendinglocation":
                case "pendingloc":
                    return org.PendingLoc;
                case "orgname":
                case "name":
                    return org.OrganizationName;
                case "leader":
                    return org.LeaderName;
            }
            return code;
        }

        private string CreateUserTag(EmailQueueTo emailqueueto)
        {
            User user = (from u in db.Users
                         where u.PeopleId == emailqueueto.PeopleId
                         select u).FirstOrDefault();
            if (user != null)
            {
                user.ResetPasswordCode = Guid.NewGuid();
                user.ResetPasswordExpires = DateTime.Now.AddHours(db.Setting("ResetPasswordExpiresHours", "24").ToInt());
                string link = Util.URLCombine(db.CmsHost, "/Account/SetPassword/" + user.ResetPasswordCode.ToString());
                db.SubmitChanges();
                return @"<a href=""{0}"">Set password for {1}</a>".Fmt(link, user.Username);
            }
            var ot = new OneTimeLink
                {
                    Id = Guid.NewGuid(),
                    Querystring = emailqueueto.PeopleId.ToString()
                };
            db.OneTimeLinks.InsertOnSubmit(ot);
            db.SubmitChanges();
            string url = Util.URLCombine(db.CmsHost, "/Account/CreateAccount/{0}".Fmt(ot.Id.ToCode()));
            return @"<a href=""{0}"">Create Account</a>".Fmt(url);
        }

        const string ExtraValueRe = @"{extra(?<type>.*?):(?<field>.*?)}";
        readonly Regex extraValueRe = new Regex(ExtraValueRe, RegexOptions.Singleline);
        private string ExtraValue(string code, EmailQueueTo emailqueueto)
        {
            var match = extraValueRe.Match(code);
            if (!match.Success)
                return code;
            var field = match.Groups["field"].Value;
            var type = match.Groups["type"].Value;
            var ev = db.PeopleExtras.SingleOrDefault(ee => ee.Field == field && emailqueueto.PeopleId == ee.PeopleId);
            if (ev == null)
                return "";

            switch (type)
            {
                case "value":
                case "code":
                    return ev.StrValue;
                case "data":
                case "text":
                    return ev.Data;
                case "date":
                    return ev.DateValue.FormatDate();
                case "int":
                    return ev.IntValue.ToString();
                case "bit":
                case "bool":
                    return ev.BitValue.ToString();
            }
            return code;
        }

        private string NextMeetingDate(string code, EmailQueueTo emailqueueto)
        {
            if (!emailqueueto.OrgId.HasValue)
                return code;

            var mt = (from aa in db.Attends
                      where aa.OrganizationId == emailqueueto.OrgId
                      where aa.PeopleId == emailqueueto.PeopleId
                      where aa.Commitment == AttendCommitmentCode.Attending
                      where aa.MeetingDate > DateTime.Now
                      orderby aa.MeetingDate
                      select aa.MeetingDate).FirstOrDefault();
            return mt == DateTime.MinValue ? "none" : mt.ToString("g");
        }

        private string RegisterLink(string code, EmailQueueTo emailqueueto)
        {
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            var id = GetId(d, "RegisterLink");

            var showfamily = code.Contains("registerlink2", ignoreCase: true);
            string qs = "{0},{1},{2}".Fmt(id, emailqueueto.PeopleId, emailqueueto.Id);
            OneTimeLink ot;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                    {
                        Id = Guid.NewGuid(),
                        Querystring = qs
                    };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }
            string url = Util.URLCombine(db.CmsHost, "/OnlineReg/RegisterLink/{0}".Fmt(ot.Id.ToCode()));
            if (showfamily)
                url += "?showfamily=true";
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        private string RegisterTag(string code, EmailQueueTo emailqueueto)
        {
            var doc = new HtmlDocument();
            if (code.Contains("&lt;"))
                code = HttpUtility.HtmlDecode(code);
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.FirstChild;
            string inside = ele.InnerHtml;
            var id = ele.Id.ToInt();
            string url = RegisterLinkUrl(db, id, emailqueueto.PeopleId, emailqueueto.Id, "registerlink");
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }
        private string RsvpLink(string code, EmailQueueTo emailqueueto)
        {
            //<a dir="ltr" href="http://rsvplink" id="798" rel="meetingid" title="This is a message">test</a>
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            string msg = "Thank you for responding.";
            if (d.ContainsKey("title"))
                msg = d["title"];

            string confirm = "false";
            if (d.ContainsKey("dir") && d["dir"] == "ltr")
                confirm = "true";

            string smallgroup = null;
            if (d.ContainsKey("rel"))
                smallgroup = d["rel"];

            var id = GetId(d, "RsvpLink");

            string qs = "{0},{1},{2},{3}".Fmt(id, emailqueueto.PeopleId, emailqueueto.Id, smallgroup);
            OneTimeLink ot;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                    {
                        Id = Guid.NewGuid(),
                        Querystring = qs
                    };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }
            string url = Util.URLCombine(db.CmsHost, "/OnlineReg/RsvpLinkSg/{0}?confirm={1}&message={2}"
                                                      .Fmt(ot.Id.ToCode(), confirm, HttpUtility.UrlEncode(msg)));
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        private string SendLink(string code, EmailQueueTo emailqueueto)
        {
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            var id = GetId(d, "SendLink");

            var showfamily = code.Contains("sendlink2", ignoreCase: true);
            string qs = "{0},{1},{2},{3}".Fmt(id, emailqueueto.PeopleId, emailqueueto.Id,
                showfamily ? "registerlink2" : "registerlink");

            OneTimeLink ot;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                    {
                        Id = Guid.NewGuid(),
                        Querystring = qs
                    };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }
            string url = Util.URLCombine(db.CmsHost, "/OnlineReg/SendLink/{0}".Fmt(ot.Id.ToCode()));
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        const string SmallGroupRe = @"\{smallgroup:\[(?<prefix>[^\]]*)\](?:,(?<def>[^}]*)){0,1}\}";
        readonly Regex smallGroupRe = new Regex(SmallGroupRe, RegexOptions.Singleline);
        private string SmallGroup(string code, EmailQueueTo emailqueueto)
        {
            var match = smallGroupRe.Match(code);
            if (!match.Success || !emailqueueto.OrgId.HasValue)
                return code;

            string prefix = match.Groups["prefix"].Value;
            string def = match.Groups["def"].Value;
            string sg = (from mm in db.OrgMemMemTags
                         where mm.OrgId == emailqueueto.OrgId
                         where mm.PeopleId == emailqueueto.PeopleId
                         where mm.MemberTag.Name.StartsWith(prefix)
                         select mm.MemberTag.Name).FirstOrDefault();
            if (!sg.HasValue())
                sg = def;
            return sg;
        }

        private string SmallGroups(string code, EmailQueueTo emailqueueto)
        {
            const string RE = @"\{smallgroups(:\[(?<prefix>[^\]]*)\]){0,1}\}";
            var re = new Regex(RE, RegexOptions.Singleline);
            Match match = re.Match(code);
            if (!match.Success || !emailqueueto.OrgId.HasValue)
                return code;

            string tag = match.Value;
            string prefix = match.Groups["prefix"].Value;
            var q = from mm in db.OrgMemMemTags
                    where mm.OrgId == emailqueueto.OrgId
                    where mm.PeopleId == emailqueueto.PeopleId
                    where mm.MemberTag.Name.StartsWith(prefix) || prefix == null || prefix == ""
                    orderby mm.MemberTag.Name
                    select mm.MemberTag.Name.Substring(prefix.Length);
            return string.Join("<br/>\n", q);
        }

        private string SupportLink(string code, EmailQueueTo emailqueueto)
        {
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            string qs = "{0},{1},{2},{3},{4}".Fmt(emailqueueto.OrgId, emailqueueto.PeopleId, emailqueueto.Id, "supportlink", emailqueueto.GoerSupportId);

            OneTimeLink ot;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                    {
                        Id = Guid.NewGuid(),
                        Querystring = qs
                    };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }
            string url = Util.URLCombine(db.CmsHost, "/OnlineReg/SendLink/{0}".Fmt(ot.Id.ToCode()));
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        private string UnSubscribeLink(EmailQueueTo emailqueueto)
        {
            var qs = "OptOut/UnSubscribe/?enc=" + Util.EncryptForUrl("{0}|{1}".Fmt(emailqueueto.PeopleId, from.Address));
            var url = Util.URLCombine(db.CmsHost, qs);
            return @"<a href=""{0}"">Unsubscribe</a>".Fmt(url);
        }

        private string VolReqLink(string code, EmailQueueTo emailqueueto)
        {
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            string qs = "{0},{1},{2},{3}"
                .Fmt(d["mid"], d["pid"], d["ticks"], emailqueueto.PeopleId);
            OneTimeLink ot = null;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                {
                    Id = Guid.NewGuid(),
                    Querystring = qs
                };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }

            var url = Util.URLCombine(db.CmsHost, "/OnlineReg/VolRequestResponse/{0}/{1}".Fmt(d["ans"], ot.Id.ToCode()));
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        private string VolSubLink(string code, EmailQueueTo emailqueueto)
        {
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            string qs = "{0},{1},{2},{3}"
                .Fmt(d["aid"], d["pid"], d["ticks"], emailqueueto.PeopleId);
            OneTimeLink ot = null;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                {
                    Id = Guid.NewGuid(),
                    Querystring = qs
                };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }

            var url = Util.URLCombine(db.CmsHost, "/OnlineReg/ClaimVolSub/{0}/{1}".Fmt(d["ans"], ot.Id.ToCode()));
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        private string VoteLink(string code, EmailQueueTo emailqueueto)
        {
            //<a dir="ltr" href="http://votelink" id="798" rel="smallgroup" title="This is a message">test</a>
            var list = new Dictionary<string, OneTimeLink>();

            var doc = new HtmlDocument();
            doc.LoadHtml(code);
            HtmlNode ele = doc.DocumentNode.Element("a");
            string inside = ele.InnerHtml;
            Dictionary<string, string> d = ele.Attributes.ToDictionary(aa => aa.Name.ToString(), aa => aa.Value);

            string msg = "Thank you for responding.";
            if (d.ContainsKey("title"))
                msg = d["title"];

            string confirm = "false";
            if (d.ContainsKey("dir") && d["dir"] == "ltr")
                confirm = "true";

            if (!d.ContainsKey("rel"))
                throw new Exception("Votelink: no smallgroup attribute");
            string smallgroup = d["rel"];
            string pre = "";
            string[] a = smallgroup.SplitStr(":");
            if (a.Length > 1)
                pre = a[0];

            var id = GetId(d, "VoteLink");

            string qs = "{0},{1},{2},{3},{4}".Fmt(id, emailqueueto.PeopleId, emailqueueto.Id, pre, smallgroup);
            OneTimeLink ot;
            if (list.ContainsKey(qs))
                ot = list[qs];
            else
            {
                ot = new OneTimeLink
                    {
                        Id = Guid.NewGuid(),
                        Querystring = qs
                    };
                db.OneTimeLinks.InsertOnSubmit(ot);
                db.SubmitChanges();
                list.Add(qs, ot);
            }
            string url = Util.URLCombine(db.CmsHost, "/OnlineReg/VoteLinkSg/{0}?confirm={1}&message={2}"
                                                      .Fmt(ot.Id.ToCode(), confirm, HttpUtility.UrlEncode(msg)));
            return @"<a href=""{0}"">{1}</a>".Fmt(url, inside);
        }

        private static string GetId(Dictionary<string, string> d, string from)
        {
            string id = null;
            if (d.ContainsKey("lang"))
                id = d["lang"];
            else if (d.ContainsKey("id"))
                id = d["id"];
            if (id == null)
                throw new Exception("{0}: no id attribute".Fmt(from));
            return id;
        }

        private static List<string> SPECIAL_FORMATS = new List<string>() 
        { 
            "http://votelink", 
            "http://registerlink", 
            "http://registerlink2", 
            "http://supportlink", 
            "http://rsvplink", 
            "http://volsublink", 
            "http://volreqlink", 
            "http://sendlink", 
            "http://sendlink2", 
            "https://votelink", 
            "https://registerlink", 
            "https://registerlink2", 
            "https://supportlink", 
            "https://rsvplink", 
            "https://volsublink", 
            "https://volreqlink", 
            "https://sendlink", 
            "https://sendlink2", 
            "{emailhref}" 
        };
        public static bool IsSpecialLink(string link)
        {
            return SPECIAL_FORMATS.Contains(link.ToLower());
        }

        public static string RegisterLinkUrl(CMSDataContext db, int orgid, int pid, int queueid, string linktype)
        {
            var showfamily = linktype == "registerlink2";
            string qs = "{0},{1},{2},{3}".Fmt(orgid, pid, queueid, linktype);
            var ot = new OneTimeLink
                {
                    Id = Guid.NewGuid(),
                    Querystring = qs
                };
            db.OneTimeLinks.InsertOnSubmit(ot);
            db.SubmitChanges();
            string url = Util.URLCombine(db.CmsHost, "/OnlineReg/RegisterLink/{0}".Fmt(ot.Id.ToCode()));
            if (showfamily)
                url += "?showfamily=true";
            return url;
        }
    }
}