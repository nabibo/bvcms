using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CmsData;
using CmsWeb.Models;

namespace CmsWeb.Areas.Org.Models
{
    public class OrgMemberTransactionModel
    {
        private int? orgId;
        private int? peopleId;
        private OrganizationMember om;
        private bool isMissionTrip;
        public CmsData.View.TransactionSummary TransactionSummary;
        public decimal Due;
        private void Populate()
        {
            var q = from mm in DbUtil.Db.OrganizationMembers
                    let ts = DbUtil.Db.ViewTransactionSummaries.SingleOrDefault(tt => tt.RegId == mm.TranId && tt.PeopleId == mm.PeopleId)
                    where mm.OrganizationId == OrgId && mm.PeopleId == PeopleId
                    select new
                    {
                        mm.Person.Name,
                        mm.Organization.OrganizationName,
                        om = mm,
                        mt = mm.Organization.IsMissionTrip == true,
                        ts
                    };
            var i = q.SingleOrDefault();
            if (i == null)
                return;
            Name = i.Name;
            OrgName = i.OrganizationName;
            om = i.om;
            isMissionTrip = i.mt;
            TransactionSummary = i.ts;
            Due = isMissionTrip 
                ? MissionTripFundingModel.TotalDue(peopleId, orgId)
                : i.ts != null ? i.ts.TotDue ?? 0 : 0;
        }

        public int? OrgId
        {
            get { return orgId; }
            set
            {
                orgId = value;
                if (peopleId.HasValue)
                    Populate();
            }
        }
        public int? PeopleId
        {
            get { return peopleId; }
            set
            {
                peopleId = value;
                if (orgId.HasValue)
                    Populate();
            }
        }
        public string Name { get; set; }
        public string OrgName { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Payment { get; set; }

        internal void PostTransaction()
        {
            var reason = TransactionSummary == null
                ? "Inital Tran"
                : "Adjustment";
            if (isMissionTrip)
            {
                if (TransactionSummary == null)
                {
                    om.AddToGroup(DbUtil.Db, "Goer");
                    om.Amount = Amount;
                }
                var gs = new GoerSenderAmount
                {
                    GoerId = om.PeopleId,
                    SupporterId = om.PeopleId,
                    Amount = Payment,
                    OrgId = om.OrganizationId,
                    Created = DateTime.Now,
                };
                DbUtil.Db.GoerSenderAmounts.InsertOnSubmit(gs);
            }
            om.AddTransaction(DbUtil.Db, reason, Payment ?? 0, Amount);
        }
    }
}
