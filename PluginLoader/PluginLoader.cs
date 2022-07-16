﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoader
{
    /// <summary>
    /// 插件加载器
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// 插件默认路径
        /// </summary>
        public static string PluginPath = Environment.CurrentDirectory + "\\Plugins";
        /// <summary>
        /// 全局插件列表
        /// </summary>
        public static List<PluginInfo> PluginInfos = new List<PluginInfo>();
        /// <summary>
        /// 获取插件信息
        /// </summary>
        /// <param name="t">
        /// 需要获取的插件主类
        /// </param>
        /// <returns>
        /// 插件主类对应的插件信息
        /// </returns>
        public static PluginInfo? GetPluginInfo(Type t)
        {
            Attribute? attribute = Attribute.GetCustomAttribute(t, typeof(PluginHandle));
            PluginHandle handle;
            if (attribute != null)
            {
                handle = (PluginHandle)attribute;
            }
            else { return null; }
            if (handle != null)
            {
                PluginInfo info = new PluginInfo();
                info.Name = handle.Name;
                info.Description = handle.Description;
                info.Version = handle.Version;
                info.Guid = handle.Guid;
                info.MainType = GetMainPluginType(t.Assembly.Location);
                info.Path = t.Assembly.Location;
                return info;
            }
            return null;
             
        }
        public static PluginInfo? GetPluginInfo(Plugin Plugin)
        {
            Type t = Plugin.GetType();
            Attribute? attribute = Attribute.GetCustomAttribute(t, typeof(PluginHandle));
            PluginHandle handle;
            if (attribute != null)
            {
                handle = (PluginHandle)attribute;
            }
            else { return null; }
            if (handle != null)
            {
                PluginInfo info = new PluginInfo();
                info.Name = handle.Name;
                info.Description = handle.Description;
                info.Version = handle.Version;
                info.Guid = handle.Guid;
                info.MainType = GetMainPluginType(t.Assembly.Location);
                info.Path = t.Assembly.Location;
                return info;
            }
            return null;

        }
        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="Path">
        /// 插件文件路径
        /// </param>
        public static void Load(string Path)
        {
            Type type = GetMainPluginType(Path);
            Type[] types = GetPluginTypes(Path);
            PluginInfo? plugin = (PluginInfo)(Plugin)Activator.CreateInstance(type, Array.Empty<object>());
            if(plugin == null)
            {
                return;
            }
            foreach(PluginInfo i in PluginInfos)
            {
                if(i.Guid == plugin.Guid)
                {
                    return;
                }
            }
            foreach (Type t in types)
            {
                Attribute[] att = t.GetCustomAttributes().ToArray();
                foreach (Attribute attr in att)
                {
                    if (attr.GetType().Name == "ListenerHandle")
                    {
                        var tmp = Activator.CreateInstance(t, new object[] { });
                        if (tmp != null) {
                            ((Listener)tmp).Register();
                        }
                    }
                    if(attr.GetType().Name == "RunnableHandle")
                    {
                        var tmp = Activator.CreateInstance(t, new object[] { });
                        if (tmp != null)
                        {
                            ((Runnable)tmp).Register();
                        }
                    }
                }
            }
            if (type != null)
            {
                try
                {
                    MethodInfo method = type.GetMethod("onPluginLoad");
                    object obj = Activator.CreateInstance(type);
                    method.Invoke(obj, new object[] { });
                    PluginInfo pluginInfo = GetPluginInfo(type);
                    pluginInfo.Path = Path;
                    pluginInfo.MainType = type;
                    PluginInfos.Add(pluginInfo);
                    PluginLoadEvent e = new PluginLoadEvent();
                    e.PluginInfo = pluginInfo;
                    Event.SetEvent(e);
                }
                catch { }
            }

        }
        public static void Load(PluginInfo pluginInfo)
        {
            if (pluginInfo != null)
            {
                Load(pluginInfo.Path);
            }
        }
        public static Type GetMainPluginType(string Path)
        {
            Assembly dllFromPlugin = Assembly.LoadFile(Path);
            bool IsPlugin = false;
            string MainClassLocation = null;
            try
            {
                string name = System.IO.Path.GetDirectoryName(Path);
                ResourceManager resourceManager = new ResourceManager(name + ".Properties.Resources", dllFromPlugin);
                MainClassLocation = resourceManager.GetString("MainClass");
            }
            catch { }
            if (MainClassLocation != null)
            {
                IsPlugin = true;
            }
            else
            {
                foreach (Type t in dllFromPlugin.GetTypes())
                {
                    Attribute[] att = t.GetCustomAttributes().ToArray();
                    foreach (Attribute attr in att)
                    {
                        if (attr.GetType().Name == "PluginHandle")
                        {
                            IsPlugin = true;
                            MainClassLocation = t.FullName;
                            break;
                        }
                    }
                }
            }
            if (!IsPlugin)
            {
                return null;
            }
            Type type = dllFromPlugin.GetType(MainClassLocation);
            return type;
        }
        public static Type[] GetPluginTypes(string Path)
        {
            Assembly dllFromPlugin = Assembly.LoadFile(Path);
            return dllFromPlugin.GetTypes();
        }
        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="Path">
        /// 已加载插件路径
        /// </param>
        public static void UnLoad(string Path)
        {
            for (int i = 0; i < PluginInfos.Count; i++)
            {
                if (PluginInfos[i].Path == Path)
                {
                    Type type = PluginInfos[i].MainType;
                    if (type != null)
                    {
                        try
                        {
                            MethodInfo method = type.GetMethod("onPluginUnLoad");
                            object obj = Activator.CreateInstance(type);
                            method.Invoke(obj, new object[] { });
                        }
                        catch (Exception) { }
                    }
                    PluginUnLoadEvent e = new PluginUnLoadEvent();
                    e.PluginInfo = PluginInfos[i];
                    Event.SetEvent(e);
                    for(int j = 0; j < Event.Listeners.Count; j++)
                    {
                        if(Event.Listeners[j].PluginInfo.Guid == PluginInfos[i].Guid)
                        {
                            
                            Event.Listeners.RemoveAt(j);
                            j--;
                        }
                    }
                    for (int j = 0; j < Runnable.Runnables.Count; j++)
                    {
                        if (Runnable.Runnables[j].PluginInfo.Guid == PluginInfos[i].Guid)
                        {
                            Runnable.Runnables[j].Stop();
                            Runnable.Runnables.RemoveAt(j);
                            j--;
                        }
                    }
                    PluginInfos.RemoveAt(i); return;
                }
            }
        }
        public static void UnLoad(PluginInfo pluginInfo)
        {
            if(pluginInfo != null)
            {
                UnLoad(pluginInfo.Path);
            }
        }
        public static void LoadAllFromPluginDir()
        {
            DirectoryInfo dir = new DirectoryInfo(PluginPath);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                Load(file.FullName);
            }
        }
        public static void LoadAllFromPlugin()
        {
            ConfigManager configManager = new ConfigManager(PluginPath + "\\Plugins.json");
            DirectoryInfo dir = new DirectoryInfo(PluginPath);
            foreach(DirectoryInfo PluginDir in dir.GetDirectories())
            {
                if(File.Exists(PluginDir.FullName + "\\Plugin.dll"))
                {
                    bool isEnable = true;
                    try
                    {
                        isEnable = configManager.GetBool(((PluginInfo)((Plugin)Activator.CreateInstance(GetMainPluginType(PluginDir.FullName + "\\Plugin.dll"), new object[] { }))).Guid);
                    }
                    catch { }
                    if (isEnable)
                    {
                        Load(PluginDir.FullName + "\\Plugin.dll");
                    }
                }
            }
        }
        public static void SetDisenable(string PluginGuid)
        {
            ConfigManager configManager = new ConfigManager(PluginPath + "\\Plugins.json");
            configManager.SetBool(PluginGuid, false);
            configManager.SaveConfig();
        }
        public static void SetEnable(string PluginGuid)
        {
            ConfigManager configManager = new ConfigManager(PluginPath + "\\Plugins.json");
            configManager.SetBool(PluginGuid, false);
            configManager.SaveConfig();
        }
        public static void SetDisenable(Plugin plugin)
        {
            ConfigManager configManager = new ConfigManager(PluginPath + "\\Plugins.json");
            configManager.SetBool(((PluginInfo)plugin).Guid, false);
            configManager.SaveConfig();
        }
        public static void SetEnable(Plugin plugin)
        {
            ConfigManager configManager = new ConfigManager(PluginPath + "\\Plugins.json");
            configManager.SetBool(((PluginInfo)plugin).Guid, false);
            configManager.SaveConfig();
        }
        public static void UnloadAll()
        {
            foreach(PluginInfo pluginInfo in PluginInfos.ToArray())
            {
                UnLoad(pluginInfo);
            }
        }
    }
}
