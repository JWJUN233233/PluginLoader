using PluginLoader;
using System.Reflection;

namespace PluginsLoader_Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PluginLoader.PluginLoader.Load(Environment.CurrentDirectory + "\\Plugin.dll");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PluginLoader.Event.SetEvent(new PluginLoader.EmptyEvent());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PluginLoader.PluginLoader.UnLoad(Environment.CurrentDirectory + "\\Plugin.dll");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}