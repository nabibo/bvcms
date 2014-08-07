using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CmsData;
using CmsWeb.MobileAPI;
using Newtonsoft.Json;
using System;

namespace CmsWeb.Areas.Public.Controllers
{
	public class MobileAPIListController : Controller
	{
		public ActionResult Authenticate()
		{
			if (CmsWeb.Models.AccountModel.AuthenticateMobile()) return null;
			else
			{
				return BaseMessage.createErrorReturn("You are not authorized!");
			}
		}

		public ActionResult Countries()
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			var countries = from e in DbUtil.Db.Countries
								 orderby e.Id
								 select new MobileCountry
								 {
									 id = e.Id,
									 code = e.Code,
									 description = e.Description
								 };

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_SYSTEM_COUNTRIES;
			br.count = countries.Count();
			br.data = JsonConvert.SerializeObject(countries.ToList());

			return br;
		}

		public ActionResult States()
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			var states = from e in DbUtil.Db.StateLookups
							 orderby e.StateCode
							 select new MobileState
							 {
								 code = e.StateCode,
								 name = e.StateName
							 };

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_SYSTEM_STATES;
			br.count = states.Count();
			br.data = JsonConvert.SerializeObject(states.ToList());

			return br;
		}

		public ActionResult MaritalStatuses()
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			var statuses = from e in DbUtil.Db.MaritalStatuses
								orderby e.Id
								select new MobileMaritalStatus
								{
									id = e.Id,
									code = e.Code,
									description = e.Description
								};

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_SYSTEM_MARITAL_STATUSES;
			br.count = statuses.Count();
			br.data = JsonConvert.SerializeObject(statuses.ToList());

			return br;
		}

		public ActionResult GivingFunds()
		{
			// Authenticate first
			var authError = Authenticate();
			if (authError != null) return authError;

			var funds = from f in DbUtil.Db.ContributionFunds
							where f.FundStatusId == 1
							where f.OnlineSort > 0
							orderby f.OnlineSort
							select new MobileFund
							{
								id = f.FundId,
								name = f.FundName
							};

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_SYSTEM_GIVING_FUNDS;
			br.count = funds.Count();
			br.data = JsonConvert.SerializeObject(funds.ToList());

			return br;
		}

		public ActionResult Playlists(string data)
		{
			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_MEDIA_PLAYLIST)
				return BaseMessage.createTypeErrorReturn();

			var playlists = from p in DbUtil.Db.MobileAppPlaylists
								 where p.Type == dataIn.argInt
								 where p.Enabled == true
								 select new MobilePlaylist
								 {
									 id = p.Id,
									 type = p.Type,
									 name = p.Name,
									 url = p.Url,
									 thumb = p.Thumb
								 };

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_MEDIA_PLAYLIST;
			br.count = playlists.Count();
			br.data = JsonConvert.SerializeObject(playlists.ToList());

			return br;
		}

		public ActionResult PlaylistItems(string data)
		{
			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_MEDIA_PLAYLIST_ITEM)
				return BaseMessage.createTypeErrorReturn();

			var playlistItems = from p in DbUtil.Db.MobileAppPlaylistItems
									  where p.PlaylistID == dataIn.argInt
									  where p.Enabled == true
									  orderby p.DateX descending
									  select new MobilePlaylistItem
									  {
										  type = p.Type,
										  date = p.DateX ?? DateTime.Now,
										  name = p.Name,
										  url = p.Url,
										  thumb = p.Thumb,
										  speaker = p.Speaker,
										  reference = p.Reference
									  };

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_MEDIA_PLAYLIST_ITEM;
			br.count = playlistItems.Count();
			br.data = JsonConvert.SerializeObject(playlistItems.ToList());

			return br;
		}

		public ActionResult HomeActions(string data)
		{
			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_SYSTEM_HOME_ACTIONS)
				return BaseMessage.createTypeErrorReturn();

			var actions = from p in DbUtil.Db.MobileAppActions
							  where p.Enabled == true
							  orderby p.Section, p.Order
							  select new MobileHomeAction
							  {
								  section = p.Section,
								  type = p.Type,
								  title = p.Title,
								  url = p.Url,
								  custom = p.Custom
							  };

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_SYSTEM_HOME_ACTIONS;
			br.count = actions.Count();
			br.data = JsonConvert.SerializeObject(actions.ToList());

			return br;
		}

		public ActionResult IconSet(string data)
		{
			// Check to see if type matches
			BaseMessage dataIn = BaseMessage.createFromString(data);
			if (dataIn.type != BaseMessage.API_TYPE_SYSTEM_ICONS)
				return BaseMessage.createTypeErrorReturn();

			var actions = from p in DbUtil.Db.MobileAppIconSets
							  join i in DbUtil.Db.MobileAppIcons on p.Id equals i.SetID
							  where p.Active
							  select new MobileIcon
							  {
								  type = i.Type,
								  url = i.Url
							  };

			BaseMessage br = new BaseMessage();
			br.error = 0;
			br.type = BaseMessage.API_TYPE_SYSTEM_ICONS;
			br.count = actions.Count();
			br.data = JsonConvert.SerializeObject(actions.ToList().ToDictionary( k => k.type, v => v.url ));

			return br;
		}
	}
}
