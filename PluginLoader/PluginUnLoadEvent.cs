using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    public class PluginUnLoadEvent : Event
    {
        public override string Name { get { return "PluginUnLoadEvent"; } }
        public PluginInfo PluginInfo { get; set; }
        public override void Do(bool IsCancel)
        {
        }
    }
}
