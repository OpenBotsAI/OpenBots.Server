using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel.Core
{
    public class ODataHelperViewModel<T> where T : class
    {
        public Predicate<T> Predicate { get; set; }
        public string PropertyName { get; set; }
        public OrderByDirectionType Direction { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Top { get; set; }
        public Func<T, bool> Filter { get; set; }
        public Func<T, object> Sort { get; set; }
        public OrderByDirectionType SortDirection { get; set; }
    }
}
