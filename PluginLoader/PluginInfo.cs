using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    /// <summary>
    /// 表示一个插件的信息
    /// </summary>
    public class PluginInfo
    {
        public string Guid;
        public string Name;
        public string? Description;

        public static explicit operator PluginInfo(Plugin v)
        {
            PluginInfo? pluginInfo = PluginLoader.GetPluginInfo(v);
            if(pluginInfo != null)
            {
                if(pluginInfo.MainType == new EmptyPlugin(null).GetType())
                {
                    pluginInfo.Guid = ((EmptyPlugin)v).Flag;
                }
            }
            return pluginInfo;
        }

        public string? Version;
        public string Path;
        public Type? MainType;
    }
}
