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
    internal partial class LayerSettingsWindow : Form
    {
        public LayerSettings LayerSettings { get; private set; }

        private Module _module;
        private bool _absFlipped;
        private SettingsController _settingsController;

        public LayerSettingsWindow(LayerSettings layerSettings, Module module)
        {
            _absFlipped = false;
            InitializeComponent();

            LayerSettings = layerSettings;
            _module = module;
            _settingsController = _module.CreateSettingsController();
            if (_settingsController != null)
            {
                _settingsController.Settings = layerSettings;
            }

            textBox1.Text = layerSettings.Name;
            textBox2.Text = layerSettings.Description;
            checkBox3.Checked = layerSettings.Enabled;
            pictureBox1.BackColor = Color.FromArgb(layerSettings.TintColor.R, layerSettings.TintColor.G,
                layerSettings.TintColor.B);

            //Load screens into the combobox.
            //This will cause the CalculateNumericValues function to be called as well.
            comboBox1.Items.AddRange(Screen.AllScreens.Select(s => s.DeviceName).ToArray());
            if (!string.IsNullOrEmpty(layerSettings.Dimensions.MonitorName))
            {
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    if (Screen.AllScreens[i].DeviceName == layerSettings.Dimensions.MonitorName)
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }
            }


            checkBox1.Checked = layerSettings.Dimensions.AbsoluteValues;
            checkBox2.Checked = layerSettings.Dimensions.MarginValues;

            numericUpDown1.Value = layerSettings.Dimensions.XValue;
            numericUpDown2.Value = layerSettings.Dimensions.YValue;
            numericUpDown3.Value = layerSettings.Dimensions.ZValue;
            numericUpDown4.Value = layerSettings.Dimensions.WValue;

            label8.Text = $"Layer ID: {layerSettings.LayerId}";

            groupBox1.Controls.Add(new Label { Text = "There are no options specific to this controller.", Location = new Point(10, 32), AutoSize = true });
            if (_settingsController != null)
            {
                var control = _settingsController.GetSettingsControl();
                if (control != null)
                {
                    groupBox1.Controls.Clear();
                    groupBox1.Controls.Add(control);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_settingsController != null)
            {
                var result = _settingsController.ApplyClicked();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    MessageBox.Show(result, "WallApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            LayerSettings.Name = textBox1.Text;
            LayerSettings.Description = textBox2.Text;
            LayerSettings.Enabled = checkBox3.Checked;

            LayerSettings.Dimensions.MonitorName = comboBox1.SelectedItem.ToString();
            LayerSettings.Dimensions.AbsoluteValues = checkBox1.Checked;
            LayerSettings.Dimensions.MarginValues = checkBox2.Checked;

            LayerSettings.Dimensions.XValue = (int)numericUpDown1.Value;
            LayerSettings.Dimensions.YValue = (int)numericUpDown2.Value;
            LayerSettings.Dimensions.ZValue = (int)numericUpDown3.Value;
            LayerSettings.Dimensions.WValue = (int)numericUpDown4.Value;

            LayerSettings.TintColor = new Microsoft.Xna.Framework.Color(pictureBox1.BackColor.R, pictureBox1.BackColor.G, pictureBox1.BackColor.B);
            
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _settingsController?.CancelClicked();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateNumericValues();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _absFlipped = true;
            CalculateNumericValues();
            _absFlipped = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                label3.Text = "Right";
                label4.Text = "Bottom";
            }
            else
            {
                label3.Text = "Width";
                label4.Text = "Height";
            }
            CalculateNumericValues();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            CalculateNumericValues();
        }

        private void CalculateNumericValues()
        {
            if (comboBox1.SelectedItem == null)
            {
                return;
            }

            var screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == comboBox1.SelectedItem.ToString());
            int width = 0;
            int height = 0;
            if (screen != null)
            {
                width = screen.WorkingArea.Width;
                height = screen.WorkingArea.Height;
            }

            double curX = (int)numericUpDown1.Value;
            double curY = (int)numericUpDown2.Value;
            double curWidth = (int)numericUpDown3.Value;
            double curHeight = (int)numericUpDown4.Value;

            if (checkBox1.Checked && _absFlipped)
            {
                curX = (int)(width * ((float)curX / 100.0F));
                curY = (int)(height * ((float)curY / 100.0F));
                curWidth = (int)(width * ((float)curWidth / 100.0F));
                curHeight = (int)(height * ((float)curHeight / 100.0F));

                numericUpDown1.DecimalPlaces = 0;
                numericUpDown2.DecimalPlaces = 0;
                numericUpDown3.DecimalPlaces = 0;
                numericUpDown4.DecimalPlaces = 0;

                numericUpDown1.Maximum = width - 1;
                numericUpDown2.Maximum = height - 1;
                numericUpDown3.Maximum = width;
                numericUpDown4.Maximum = height;

                if (curWidth < 1)
                {
                    curWidth = 1;
                }
                if (curHeight < 1)
                {
                    curHeight = 1;
                }

                numericUpDown3.Minimum = 1;
                numericUpDown4.Minimum = 1;
            }
            else if (!checkBox1.Checked && _absFlipped)
            {
                curX = ((float)curX / (float)width) * 100.0F;
                curY = ((float)curY / (float)height) * 100.0F;
                curWidth = ((float)curWidth / (float)width) * 100.0F;
                curHeight = ((float)curHeight / (float)height) * 100.0F;

                numericUpDown1.DecimalPlaces = 2;
                numericUpDown2.DecimalPlaces = 2;
                numericUpDown3.DecimalPlaces = 2;
                numericUpDown4.DecimalPlaces = 2;

                numericUpDown1.Maximum = 99.99M;
                numericUpDown2.Maximum = 99.99M;
                numericUpDown3.Maximum = 100;
                numericUpDown4.Maximum = 100;

                numericUpDown3.Minimum = 0.01M;
                numericUpDown4.Minimum = 0.01M;

                if (curWidth < 0.01D)
                {
                    curWidth = 0.01D;
                }
                if (curHeight < 0.01D)
                {
                    curHeight = 0.01D;
                }
            }
            
            numericUpDown1.Value = (decimal)curX;
            numericUpDown2.Value = (decimal)curY;
            numericUpDown3.Value = (decimal)curWidth;
            numericUpDown4.Value = (decimal)curHeight;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var colorPicker = new ColorDialog
            {
                Color = pictureBox1.BackColor,
                AllowFullOpen = true,
                AnyColor = true,
                SolidColorOnly = true
            };

            if (colorPicker.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.BackColor = colorPicker.Color;
            }
        }
    }
}
