﻿using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IScheduleRepository : IEntityRepository<Schedule>
    {
        public PaginatedList<ScheduleViewModel> FindAllView(Predicate<ScheduleViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);

    }
}
