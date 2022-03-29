using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiAFK
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Sets the time (in seconds) that players are allowed to be AFK before getting booted")]
        public int AFK_Time = 10;
    }
}
