using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsWeb.Models;

namespace CmsWeb.Code
{
    public class CodeInfo
    {
        private string _value;

        public CodeInfo(object value, IEnumerable<CodeValueItem> items, string valuefield = "Id")
        {
            if (value != null)
                Value = value.ToString();
            Items = new SelectList(items, valuefield, "Value", value);
        }
        public CodeInfo(object value, IEnumerable<SelectListItem> items, string valuefield = "Id")
        {
            if (value != null)
                Value = value.ToString();
            Items = items;
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public IEnumerable<SelectListItem> Items { get; set; }
        public override string ToString()
        {
            var i = Items.SingleOrDefault(ii => ii.Value == Value);
            if (i == null)
                return "";
            return i.Text;
        }
    }
}