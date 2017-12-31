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
        }
        

        private void AddLayerButton_Click(object sender, EventArgs e)
        {
            var moduleDialog = new ModulesListWindow();
            if (moduleDialog.ShowDialog() == DialogResult.Cancel || moduleDialog.SelectedModule == null)
            {
                return;
            }
            var module = moduleDialog.SelectedModule;

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

            LayerListView.Items.Add(newItem);

            LayerSettingsWindow window = new LayerSettingsWindow(settings, module);
            if (window.ShowDialog() == DialogResult.OK)
            {
                newItem.SubItems[2].Text = settings.Name;
                newItem.SubItems[3].Text = settings.Description;
                newItem.SubItems[4].Text = settings.Dimensions.MonitorName;
                newItem.SubItems[5].Text = settings.Enabled.ToString();
            }
            _lastId++;
        }

        private void RemoveLayerButton_Click(object sender, EventArgs e)
        {

        }

        private void LayerOptionsButton_Click(object sender, EventArgs e)
        {
            if (LayerListView.SelectedItems.Count == 0)
            {
                return;
            }
            var selectedItem = LayerListView.SelectedItems[0];
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

        private void OkButton_Click(object sender, EventArgs e)
        {
            LayerLayout.Layers.Clear();
            foreach (ListViewItem item in LayerListView.Items)
            {
                var tuple = ((Module, LayerSettings))item.Tag;
                LayerLayout.Layers.Add(tuple.Item2);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
