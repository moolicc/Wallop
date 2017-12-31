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
    public partial class ModulesListWindow : Form
    {
        public Module SelectedModule { get; private set; }

        public ModulesListWindow()
        {
            InitializeComponent();
        }

        private void ModulesListWindow_Load(object sender, EventArgs e)
        {
            Module[] modules = Resolver.Cache.Values.ToArray();
            foreach (var module in modules)
            {
                ListViewItem item = new ListViewItem(module.Name);
                item.SubItems.Add(module.Description);
                item.SubItems.Add(Path.GetFileName(module.SourceFile));
                item.Tag = module;
                listView1.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                SelectedModule = (Module)listView1.SelectedItems[0].Tag;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
