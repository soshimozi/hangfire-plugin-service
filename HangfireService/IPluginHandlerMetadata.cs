﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService
{
    public interface IPluginHandlerMetadata
    {
        string Name { get; }
        string CronExpression { get; }
    }
}
