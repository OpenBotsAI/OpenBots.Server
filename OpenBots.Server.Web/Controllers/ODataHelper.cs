using OpenBots.Server.Model.Core;
using StringToExpression.LanguageDefinitions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace OpenBots.Server.WebAPI.Controllers
{
    public class ODataHelper<T> where T : class
    {
        public int Skip { get; private set; }

        public int Top { get; set; }

        public Func<T, bool> Filter { get; private set; }

        public Func<T, object> Sort { get; private set; }
        public OrderByDirectionType SortDirection { get; internal set; }

        public void Parse(string queryString)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                var queryStrings = HttpUtility.ParseQueryString(queryString);
                if (queryStrings.HasKeys() && queryStrings.AllKeys.Contains("$filter"))
                {
                    string filter = queryStrings["$filter"];
                    var language = new ODataFilterLanguage();
                    Expression<Func<T, bool>> predicateExpression = language.Parse<T>(filter);
                    Filter = predicateExpression.Compile();
                }
                if (queryStrings.HasKeys() && queryStrings.AllKeys.Contains("$top"))
                {
                    string top = queryStrings["$top"];
                    ParseTop(top);
                }
                if (queryStrings.HasKeys() && queryStrings.AllKeys.Contains("$skip"))
                {
                    string skip = queryStrings["$skip"];
                    ParseSkip(skip);
                }

                if (queryStrings.HasKeys() && queryStrings.AllKeys.Contains("$orderby"))
                {
                    string orderby = queryStrings["$orderby"];
                    OrderByClause<T> orderbyClause = new OrderByClause<T>();
                    orderbyClause.Parse(orderby);
                    Sort = orderbyClause.RootExpression;
                    SortDirection = orderbyClause.Direction;
                }
            }
        }

        protected void ParseSkip(string skip)
        {
            if (string.IsNullOrEmpty(skip))
            {
                Skip = 0;
                return;
            }
            if (int.TryParse(skip, out int val))
                Skip = val;
        }

        protected void ParseTop(string top)
        {
            if (string.IsNullOrEmpty(top))
            {
                Top = 0;
                return;
            }
            if (int.TryParse(top, out int val))
                Top = val;
        }

        public OrderByNode<T> ParseOrderByQuerry(string queryString)
        {
            try
            {
                OrderByNode<T> response = null;
                if (!string.IsNullOrEmpty(queryString))
                {
                    var queryStrings = HttpUtility.ParseQueryString(queryString);
                    if (queryStrings.HasKeys() && queryStrings.AllKeys.Contains("$orderby"))
                    {
                        string orderby = queryStrings["$orderby"];
                        OrderByClause<T> orderbyClause = new OrderByClause<T>();

                        response = orderbyClause.ParseOrderBy(orderby);
                    }
                }
                return response;
            }
            catch
            {
                return null;
            }
        }
    }
}
