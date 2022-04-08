using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessageBoxCreator
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            label1.Text += ProductVersion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c start https://github.com/ILoveAndLikePizza/MessageBoxCreator");
        }
        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c start https://discord.gg/CaMaGRXDqB");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c start https://nm-games.eu");
        }
    }
}
