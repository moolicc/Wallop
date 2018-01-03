using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallApp.Scripting;

namespace WallApp.Windows
{
    internal partial class SettingsWindow : Form
    {
        private int _lastId;

        public SettingsWindow()
        {
            InitializeComponent();
            _lastId = 0;
        }

        private void SettingsWindow_Load(object sender, EventArgs e)
        {
            LoadSettings();
            LoadLayout();
        }

        private void LoadSettings()
        {
            FpsNumericUpDown.Value = Settings.Instance.FrameRate;
            BackBufferFactorUpDown.Value = (decimal)Settings.Instance.BackBufferWidthFactor;
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
                Text = module.Name,
                Tag = (module, settings)
            };
            newItem.SubItems.Add(_lastId.ToString());
            newItem.SubItems.Add(settings.Name);
            newItem.SubItems.Add(settings.Description);
            newItem.SubItems.Add(settings.Dimensions.MonitorName);
            newItem.SubItems.Add(settings.Enabled.ToString());


            LayerSettingsWindow window = new LayerSettingsWindow(settings, module);
            if (window.ShowDialog() == DialogResult.OK)
            {
                newItem.SubItems[2].Text = settings.Name;
                newItem.SubItems[3].Text = settings.Description;
                newItem.SubItems[4].Text = settings.Dimensions.MonitorName;
                newItem.SubItems[5].Text = settings.Enabled.ToString();
            }
            else
            {
                return;
            }

            LayerListView.Items.Add(newItem);
            _lastId++;
        }

        private void RemoveLayerButton_Click(object sender, EventArgs e)
        {
            if (LayerListView.SelectedItems.Count == 0)
            {
                return;
            }
            LayerListView.Items.RemoveAt(LayerListView.SelectedIndices[0]);

            //Recursively call this function to remove all selected items.
            RemoveLayerButton_Click(sender, e);
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
            SaveSettings();
            SaveLayout();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void SaveSettings()
        {
            Settings.Instance.FrameRate = (int) FpsNumericUpDown.Value;
            Settings.Instance.BackBufferWidthFactor = (float) BackBufferFactorUpDown.Value;
            Settings.Instance.BackBufferHeightFactor = (float) BackBufferFactorUpDown.Value;
        }

        private void SaveLayout()
        {
            WallApp.Layout.Layers.Clear();
            foreach (ListViewItem item in LayerListView.Items)
            {
                var tuple = ((Module, LayerSettings))item.Tag;
                WallApp.Layout.Layers.Add(tuple.Item2);
            }
            WallApp.Layout.Save("layout.json");
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            WallApp.Layout.Load("layout.json");
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoadLayout()
        {
            //TODO: Work some magic, _lastId should be the lowest possible (not taken) number that is greater than 0

            LayerListView.Items.Clear();
            foreach (var settings in WallApp.Layout.Layers)
            {
                var module = Scripting.Resolver.Cache[settings.Module];
                
                var newItem = new ListViewItem()
                {
                    Text = module.Name,
                    Tag = (module, settings)
                };
                newItem.SubItems.Add(settings.LayerId.ToString());
                newItem.SubItems.Add(settings.Name);
                newItem.SubItems.Add(settings.Description);
                newItem.SubItems.Add(settings.Dimensions.MonitorName);
                newItem.SubItems.Add(settings.Enabled.ToString());

                LayerListView.Items.Add(newItem);
                
                newItem.SubItems[2].Text = settings.Name;
                newItem.SubItems[3].Text = settings.Description;
                newItem.SubItems[4].Text = settings.Dimensions.MonitorName;
                newItem.SubItems[5].Text = settings.Enabled.ToString();

                _lastId++;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WallApp.Layout.New();
            LayerListView.Items.Clear();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select Layout";
            dialog.Filter = "JSON files (*.json)|*.json";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                WallApp.Layout.Load(dialog.FileName);
                LoadLayout();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "Save Layout";
            dialog.Filter = "JSON files (*.json)|*.json";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                WallApp.Layout.Layers.Clear();
                foreach (ListViewItem item in LayerListView.Items)
                {
                    var tuple = ((Module, LayerSettings))item.Tag;
                    WallApp.Layout.Layers.Add(tuple.Item2);
                }
                WallApp.Layout.Save("layout.json");
                if (File.Exists(dialog.FileName))
                {
                    File.Delete(dialog.FileName);
                }
                File.Copy("layout.json", dialog.FileName);
            }
        }

        private void LayerUpButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in LayerListView.SelectedItems)
            {
                if (item.Index > 0)
                {
                    int newIndex = item.Index - 1;
                    LayerListView.Items.RemoveAt(item.Index);
                    LayerListView.Items.Insert(newIndex, item);
                }
            }
        }

        private void LayerDownButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in LayerListView.SelectedItems)
            {
                if (item.Index < LayerListView.Items.Count - 1)
                {
                    int newIndex = item.Index + 1;
                    LayerListView.Items.RemoveAt(item.Index);
                    LayerListView.Items.Insert(newIndex, item);
                }
            }
        }

        private void CloneLayerButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in LayerListView.SelectedItems)
            {
                ListViewItem clonedItem = (ListViewItem)item.Clone();

                (var module, var settings) = ((Module, LayerSettings))clonedItem.Tag;
                settings = (LayerSettings)settings.Clone();

                settings.LayerId = _lastId;
                clonedItem.SubItems[1].Text = _lastId.ToString();
                _lastId++;

                settings.Name = "Clone of " + settings.Name;
                clonedItem.SubItems[2].Text = settings.Name;

                clonedItem.Tag = (module, settings);

                LayerListView.Items.Add(clonedItem);
            }
        }
    }
}
