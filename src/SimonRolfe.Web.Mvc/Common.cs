using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace System.Web.Security
{
    public static class MembershipUserExtensions
    {
        public static string GetCommentValue(this MembershipUser user, string Key)
        {
            int indexOfKey = user.Comment.IndexOf(Key + "=");
            if (indexOfKey > -1)
            {
                int indexOfValue = indexOfKey + Key.Length + 1;
                int indexOfPipe = user.Comment.IndexOf('|', indexOfValue);
                if (indexOfPipe > -1)
                {
                    return user.Comment.Substring(indexOfValue, indexOfPipe - indexOfValue);
                }
                else
                {
                    return user.Comment.Substring(indexOfValue);
                }
            }
            else
            {
                throw new KeyNotFoundException("User comment does not contain key " + Key);
            }
        }

        public static void SetCommentValue(this MembershipUser user, string Key, string Value)
        {
            int indexOfKey = user.Comment.IndexOf(Key + "=");

            if (indexOfKey > -1)
            {
                int indexOfValue = indexOfKey + Key.Length + 1;
                string prefix = user.Comment.Substring(0, indexOfValue);

                int indexOfPipe = user.Comment.IndexOf('|', indexOfValue);
                if (indexOfPipe > -1)
                {
                    if (user.Comment.Substring(indexOfValue, indexOfPipe - indexOfValue) != Value)
                    {
                        string suffix = user.Comment.Substring(indexOfPipe);

                        user.Comment = prefix + Value + suffix;
                        Membership.UpdateUser(user);
                    }
                }
                else
                {
                    if (user.Comment.Substring(indexOfValue) != Value)
                    {
                        user.Comment.Substring(indexOfValue);
                        Membership.UpdateUser(user);
                    }
                }
            }
            else
            {
                user.Comment = user.Comment + "|" + Key + "=" + Value;
                Membership.UpdateUser(user);
            }
        }
    }
}

namespace SimonRolfe.Web.Mvc
{
    public static class URLExtensions
    {
        /// <summary>
        /// Adds a query parameter to a URL string. URL encodes the key and value, and prepends the correct parameter delimiter for the string.
        /// </summary>
        /// <param name="source">The string to add a query parameter to.</param>
        /// <param name="key">The key of the query parameter.</param>
        /// <param name="value">The value of the query parameter.</param>
        /// <returns>A string with a correctly-formatted, URL-encoded query parameter added.</returns>
        public static string AddQueryParam(this string source, string key, dynamic value)
        {
            string delim;
            string RetString;
            if ((source == null) || !source.Contains("?"))
            {
                delim = "?";
            }
            else if (source.EndsWith("?") || source.EndsWith("&"))
            {
                delim = string.Empty;
            }
            else
            {
                delim = "&";
            }
            if (value != null)
            {
                RetString = source + delim + System.Web.HttpUtility.UrlEncode(key) + "=" + System.Web.HttpUtility.UrlEncode(value.ToString());
            }
            else
            {
                RetString = source + delim + System.Web.HttpUtility.UrlEncode(key) + "=";
            }
            return RetString;
        }
    }

    public class Common 
    {
        /// <summary>
        /// Gets URL from an executing context for redirect purposes - i.e. logging in, selecting a user etc.
        /// </summary>
        /// <param name="filterContext">The Context that is executing.</param>
        /// <returns>A URL representing the current Context.</returns>
        public static string GetReturnURLFromActionExecutingContext(ActionExecutingContext filterContext, bool UrlEncodeResult = true, bool IncludeQueryString = true)
        {
            //..gather redirect info..
            string ReturnUrl = "/";

            if (filterContext.RouteData.DataTokens.ContainsKey("Area"))
            {
                ReturnUrl += (string)filterContext.RouteData.DataTokens["Area"] + "/";
            }

            ReturnUrl += filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + filterContext.ActionDescriptor.ActionName;

            if (IncludeQueryString)
            {
                foreach (KeyValuePair<string, object> Param in filterContext.ActionParameters)
                {
                    if (Param.Value != null && !string.IsNullOrWhiteSpace(Param.Value.ToString())) //only append if we have params with values
                    {
                        ReturnUrl = ReturnUrl.AddQueryParam(Param.Key, Param.Value.ToString());
                    }
                }
            }

            if (UrlEncodeResult)
            {
                return System.Web.HttpUtility.UrlEncode(ReturnUrl);
            }
            else
            {
                return ReturnUrl;
            }
        }

        public static string GetReturnURLFromActionExecutedContext(ActionExecutedContext filterContext, bool UrlEncodeResult = true)
        {

            string ReturnUrl = "/";

            if (filterContext.RouteData.DataTokens.ContainsKey("Area"))
            {
                ReturnUrl += (string)filterContext.RouteData.DataTokens["Area"] + "/";
            }

            ReturnUrl += filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + filterContext.ActionDescriptor.ActionName;

            if (UrlEncodeResult)
            {
                return System.Web.HttpUtility.UrlEncode(ReturnUrl);
            }
            else
            {
                return ReturnUrl;
            }
        }

        public class TinyListItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
            public bool Selected { get; set; }

            public TinyListItem(string Value, string Text, bool Selected = false)
            {
                this.Value = Value;
                if (String.IsNullOrEmpty(Value) && string.IsNullOrEmpty(Text)) //shouldn't change behaviour in existing listboxen, but will allow for custom item text for blank values
                {
                    this.Text = MvcConsts.Blank_Item_Text;
                }
                else
                {
                    this.Text = Text;
                }
                this.Selected = Selected;
            }
        }

        public static SelectList ListToSelectList(IEnumerable<TinyListItem> InputList, string SelectedItem = "")
        {
            if (!string.IsNullOrEmpty(SelectedItem))
            {
                foreach (TinyListItem item in InputList)
                {
                    if (item.Value == SelectedItem)
                    {
                        item.Selected = true;
                    }
                }
            }
            return new SelectList(InputList, "Value", "Text");
        }

        public static SelectList ListToSelectList(IEnumerable<string> InputList, string SelectedItem = "")
        {
            List<TinyListItem> ListItems = new List<TinyListItem>();
            foreach (string item in InputList)
            {
                ListItems.Add(new TinyListItem(item, item));
            }

            return ListToSelectList(ListItems, SelectedItem);
        }

    }
}
