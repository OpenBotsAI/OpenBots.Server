using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Core
{
    public class PaginatedList<T>
    {
        public PaginatedList()
        {
            Items = new List<T>();
        }

        public PaginatedList(IEnumerable<T> items)
        {
            Items = new List<T>();
            Add(items);
            TotalCount = Items.Count;
        }

        [Display(Name = "PageNumber")]
        public int? PageNumber { get; set; }

        [Display(Name = "PageSize")]
        public int? PageSize { get; set; }

        [Display(Name = "TotalCount")]
        public int? TotalCount { get; set; }

        [Display(Name = "Items")]
        public List<T> Items { get; set; }

        [Display(Name = "Completed")]
        public int? Completed { get; set; }

        [Display(Name = "Started")]
        public int? Started { get; set; }

        [Display(Name = "Impediments")]
        public int? Impediments { get; set; }

        public Guid? ParentId { get; set; }

        public void Add(T item)
        {
            Items.Add(item);
        }

        public void Add(IEnumerable<T> items)
        {
            Items.AddRange(items);          
        }
    }
}
