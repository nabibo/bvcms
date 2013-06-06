/* Author: David Carroll
 * Copyright (c) 2008, 2009 Bellevue Baptist Church 
 * Licensed under the GNU General Public License (GPL v2)
 * you may not use this code except in compliance with the License.
 * You may obtain a copy of the License at http://bvcms.codeplex.com/license 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using UtilityExtensions;
using System.Text;
using CmsData;

namespace CmsWeb.Models
{
	public class PagerModel2
	{
		public PagerModel2(CountDelegate count) : this()
		{
			GetCount = new CountDelegate(count);
		}

		public PagerModel2()
		{
			ShowPageSize = true;
		}

		public string Sort { get; set; }
		public string Direction { get; set; }

		public string SortExpression
		{
			get
			{
				if (Direction == "asc")
					return Sort;
				return Sort + " " + Direction;
			}
		}

		public delegate int CountDelegate();
		public CountDelegate GetCount;
		private int? _count;

		private int count
		{
			get
			{
				if (!_count.HasValue)
				{
					_count = GetCount();
					if (StartRow >= _count)
						_Page = null;
				}
				return _count.Value;
			}
		}

		public void setCountDelegate(CountDelegate count)
		{
			GetCount = new CountDelegate(count);
		}

		public bool ShowPageSize { get; set; }
		public int? pagesize;

		public int PageSize
		{
			get
			{
				if (pagesize.HasValue)
					return pagesize.Value;
				return DbUtil.Db.UserPreference("PageSize", "10").ToInt();
			}
			set
			{
				DbUtil.Db.SetUserPreference("PageSize", value);
				pagesize = value;
			}
		}
		private int? _Page;
		public int? Page
		{
			get { return _Page ?? 1; }
			set { _Page = value; }
		}
		public int LastPage()
		{
			return (int)Math.Ceiling(count / (double)PageSize);
		}
		public int StartRow
		{
			get { return (Page.Value - 1) * PageSize; }
		}
		public IEnumerable<SelectListItem> PageSizeList()
		{
			int[] pagesizes = { 10, 25, 50, 100, 200 };
			return pagesizes.Select(i => new SelectListItem { Text = i.ToString(), Selected = PageSize == i });
		}
		public IEnumerable<int> PageList()
		{
			for (var i = 1; i <= LastPage(); i++)
			{
				if (i > 1 && i < Page - 2)
				{
					i = Page.Value - 3;
					yield return 0;
				}
				else if (i < LastPage() && i > Page + 2)
				{
					i = LastPage() - 1;
					yield return 0;
				}
				else
					yield return i;
			}
		}
	}
}
