using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallApp.Scripting;

namespace WallApp.Windows
{
    internal partial class SettingsWindow : Form
    {
        public Layout LayerLayout { get; set; }

        private int _lastId;

        public SettingsWindow()
        {
            InitializeComponent();
            _lastId = 0;
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            Module[] modules = Resolver.Cache.Values.ToArray();
            foreach (var module in modules)
            {
                ListViewItem item = new ListViewItem(module.GetName());
                item.SubItems.Add(module.GetDescription());
                item.SubItems.Add(module.File);
                item.Tag = module;
                listView1.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var module = (Module) listView1.SelectedItems[0].Tag;

            LayerSettings settings = new LayerSettings();
            settings.LayerId = _lastId;
            settings.Module = module.File;
            settings.Dimensions = new LayerDimensions()
            {
                AbsoluteValues = false,
                MarginValues = false,
                XValue = 0,
                YValue = 0,
                WValue = 100,
                ZValue = 100
            };

            var newItem = new ListViewItem()
            {
                Text = module.GetName(),
                Tag = (module, settings)
            };
            newItem.SubItems.Add(_lastId.ToString());
            newItem.SubItems.Add(settings.Name);
            newItem.SubItems.Add(settings.Description);
            newItem.SubItems.Add(settings.Dimensions.MonitorName);
            newItem.SubItems.Add(settings.Enabled.ToString());

            listView2.Items.Add(newItem);
            _lastId++;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
            {
                return;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
            {
                return;
            }
            var selectedItem = listView2.SelectedItems[0];
            var tuple = ((Module, LayerSettings))selectedItem.Tag;
            var settings = tuple.Item2;
            LayerSettingsWindow window = new LayerSettingsWindow(tuple.Item2, tuple.Item1);
            if (window.ShowDialog() == DialogResult.OK)
            {
                selectedItem.SubItems[2].Text = settings.Name;
                selectedItem.SubItems[3].Text = settings.Description;
                selectedItem.SubItems[4].Text = settings.Dimensions.MonitorName;
                selectedItem.SubItems[5].Text = settings.Enabled.ToString();
            }
        }
        
        private void button7_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            LayerLayout.Layers.Clear();
            foreach (ListViewItem item in listView2.Items)
            {
                var tuple = ((Module, LayerSettings))item.Tag;
                LayerLayout.Layers.Add(tuple.Item2);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
