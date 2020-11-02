using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Core
{
    /// <summary>
    /// Represents Order by clause provided in OData Query Expression
    /// </summary>
    public class OrderByClause<T> where T : class
    {
        const string fieldSeparator = @"\,";
        const string orderBySeparator = @"\s+";

        /// <summary>
        /// Collection of order by node 
        /// </summary>

        [Display(Name = "OrderByNodes")]
        public List<OrderByNode<T>> OrderByNodes { get; protected set; }

        [Display(Name = "RootExpression")]
        public Func<T, object> RootExpression { get; protected set; }

        [Display(Name = "Direction")]
        public OrderByDirectionType  Direction { get; protected set; }

        /// <summary>
        /// Creates new instance of OrderByClause
        /// </summary>
        public OrderByClause()
        {
            OrderByNodes = new List<OrderByNode<T>>();
            Direction = OrderByDirectionType.Ascending;
        }

        /// <summary>
        /// Try and Parse Orderby expression from OData Query
        /// </summary>
        /// <exception cref="ArgumentNullException">If <paramref name="expression"/> is not null or empty.</exception>
        /// <exception cref="ArgumentException">property name provided in field does not belong to <typeparamref name="T"/>></exception>
        /// <exception cref="InvalidOperationException">property name provided in field does not belong to <typeparamref name="T"/>></exception>
        /// <param name="expression">order by expression</param>
        [Display(Name ="Parse")]
        public void Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentNullException(expression);
            }

            List<string> fields = Regex.Split(expression, fieldSeparator)?.ToList();

            if (fields?.Any() == true)
            {
                int seq = 1;
                fields.ForEach(o =>
                {
                    var parts = Regex.Split(o, orderBySeparator);
                    if (parts.Length <= 2)
                    {
                        string field = parts[0];
                        string direction = (parts.Length == 2) ? parts[1] : "asc";
                        Type t = typeof(T);
                        var propInfo = t.GetProperty(field, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (propInfo == null)
                        {
                            throw new ArgumentException($"Property <{field}> not found for <{t.Name}> in $orderby.");
                        }

                        var newNode = new OrderByNode<T>
                        {
                            Sequence = seq,
                            PropertyName = propInfo.Name,
                            Direction = (string.Compare(direction, "asc", true) == 0) ? OrderByDirectionType.Ascending : OrderByDirectionType.Descending
                        };

                        CreateExpression(newNode);
                        OrderByNodes.Add(newNode);
                        seq++;
                    }
                });
            }

            var rootNode = OrderByNodes.FirstOrDefault();
            if (rootNode != null)
            {
                RootExpression = rootNode.Expression.Compile();
                Direction = rootNode.Direction;
            }
        }

        /// <summary>
        /// Creates Expression for OrderBy/OrderByDescending
        /// </summary>
        /// <param name="node">Order by node to create expression</param>
        [Display(Name = "CreateExpression")]
        private void CreateExpression(OrderByNode<T> node)
        {
            if (!string.IsNullOrWhiteSpace(node?.PropertyName))
            {
                var param = Expression.Parameter(typeof(T), "p");
                var parts = node.PropertyName.Split('.');
                Expression parent = param;
                foreach (var part in parts)
                {
                    parent = Expression.Property(parent, part);
                }

                if (parent.Type.IsValueType)
                {
                    var converted = Expression.Convert(parent, typeof(object));
                    node.Expression = Expression.Lambda<Func<T, object>>(converted, param);
                }
                else
                {
                    node.Expression = Expression.Lambda<Func<T, object>>(parent, param);
                }
            }
        }

        public OrderByNode<T> ParseOrderBy(string expression)
        {
            OrderByNode<T> newNode = null;
            List<string> fields = Regex.Split(expression, fieldSeparator)?.ToList();
            if (fields?.Any() == true)
            {
                int seq = 1;
                fields.ForEach(o =>
                {
                    var parts = Regex.Split(o, orderBySeparator);
                    if (parts.Length <= 2)
                    {
                        string field = parts[0];
                        string direction = (parts.Length == 2) ? parts[1] : "asc";
                        Type t = typeof(T);
                        var propInfo = t.GetProperty(field, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (propInfo == null)
                        {
                            throw new ArgumentException($"Property <{field}> not found for <{t.Name}> in $orderby.");
                        }

                        newNode = new OrderByNode<T>
                        {
                            Sequence = seq,
                            PropertyName = propInfo.Name,
                            Direction = (string.Compare(direction, "asc", true) == 0) ? OrderByDirectionType.Ascending : OrderByDirectionType.Descending
                        };
                    }
                });
            }
            return newNode;
        }
    }

    /// <summary>
    /// Represents single order by field node
    /// </summary>
    public class OrderByNode<T>
    {
        /// <summary>
        /// Field Sequence
        /// </summary>
        [Display(Name = "Sequence")]
        public int Sequence { get; set; }

        /// <summary>
        /// Property Name
        /// </summary>
        [Display(Name = "PropertyName")]
        public string PropertyName { get; set; }

        /// <summary>
        /// Order by direction (Asc or Dsc)
        /// </summary>
        [Display(Name = "Direction")]
        public OrderByDirectionType Direction { get; set; }

        /// <summary>
        /// Gets or sets Order by expression to use in LINQ
        /// </summary>
        [Display(Name = "Expression")]
        public Expression<Func<T, object>> Expression { get; set; }
    }

    /// <summary>
    /// Order by direction type
    /// </summary>
    public enum OrderByDirectionType
    {
        /// <summary>
        /// sort data in ascending order
        /// </summary>
        Ascending,

        /// <summary>
        /// sort data in descending order
        /// </summary>
        Descending
    }
}