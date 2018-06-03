using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using Oracle.DataAccess.Client;

namespace SimonRolfe.Web.Mvc
{
    public class DataAccess : Data.Oracle.DataAccess
    {
        /// <summary>
        /// Executes the specified Oracle command, returning an MVC SelectList with all rows of the data for use in a drop-down list.
        /// </summary>
        /// <param name="cmd">The Oracle command to execute, including all parameters. If the command name does not include a package specifier, the value of the public constant Package_Name_Prefix will be used.</param>
        /// <param name="SelectedValue">The Selected value to return from the list, or blank/null if no value is selected.</param>
        /// <param name="IncludeBlankItem">If true (default), prepend a blank item into the SelectList, with a value of null and text according to the BlankItemText constant in _Common.</param>
        /// <param name="valueColumnName">The column name to use for the value part of the SelectList item. If no column name is specified, the <paramref name="valueColumnNumber">value column number</paramref> will be used. If neither are specified, the first column will be used.</param>
        /// <param name="textColumnName">The column name to use for the text part of the SelectList item. If no column name is specified, the <paramref name="textColumnNumber">text column number</paramref> will be used. If neither are specified, the first column will be used.</param>
        /// <param name="BlankItemOverrideText">If you wish to override the default text for a blank item, do so here.</param>
        /// <param name="formatString">If you need to override the standard number or date formatting, do so with this.</param>
        /// <param name="ConnectionStringName">An optional ConnectionStrings web.config entry to use. Defaults to "Main".</param>
        /// <returns>A SelectList containing the specified data, for use within MVC views.</returns>
        public static SelectList FetchSelectList(OracleCommand cmd, string valueColumnName, string textColumnName, string SelectedValue = "", bool IncludeBlankItem = true, string BlankItemOverrideText = "", string formatString = "{1}", string ConnectionStringName = "Main")
        {
            using (DataSet ds = FetchDataSet(cmd, ConnectionStringName))
            {
                List<SelectListItem> Items = new List<SelectListItem>();
                if (IncludeBlankItem)
                {
                    SelectListItem BlankItem = new SelectListItem();
                    if (string.IsNullOrEmpty(BlankItemOverrideText))
                    {
                        BlankItem.Text = MvcConsts.Blank_Item_Text;
                    }
                    else
                    {
                        BlankItem.Text = BlankItemOverrideText;
                    }
                    BlankItem.Value = null;
                    Items.Add(BlankItem);
                }

                using (DataTable dt = ds.Tables[0])
                {
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            SelectListItem Item = new SelectListItem();
                            Item.Text = string.Format(formatString, row.Field<dynamic>(valueColumnName), row.Field<dynamic>(textColumnName));
                            Item.Value = row.Field<dynamic>(valueColumnName).ToString();

                            Item.Selected = (!string.IsNullOrEmpty(SelectedValue) && Item.Value == SelectedValue);

                            Items.Add(Item);
                        }
                    }
                }
                return new SelectList(Items, "Value", "Text");
            }
        }
    }
}
