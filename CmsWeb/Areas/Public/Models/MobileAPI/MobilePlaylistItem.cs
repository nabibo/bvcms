using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsWeb.MobileAPI
{
	public class MobilePlaylistItem
	{
		public int type = 0;

		public DateTime date;

		public string name = "";
		public string url = "";
		public string thumb = "";

		public string speaker = "";
		public string reference = "";
	}
}