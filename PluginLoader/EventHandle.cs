using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandle : Attribute
    {
    }
}
