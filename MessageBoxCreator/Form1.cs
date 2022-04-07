using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MessageBoxCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // load by argument passed configuration
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                string[] fragments = args[1].Split('.');
                string[] text = System.IO.File.ReadAllLines(args[1]);
                if (fragments[fragments.Length - 1] != "mbconfig")
                {
                    MessageBox.Show("You loaded an invalid file!", "Cannot load file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Environment.Exit(1);
                }
                int j = 0;
                int index = 0;
                foreach (string line in text)
                {
                    if (j % 4 == 0) messages[index] = line;
                    else if (j % 4 == 1) titles[index] = line;
                    else if (j % 4 == 2)
                    {
                        string[] stats = line.Split(';');
                        icons[index] = int.Parse(stats[0]);
                        buttons[index] = int.Parse(stats[1]);
                        answerRequirements[index] = int.Parse(stats[2]);
                    }
                    else index++;
                    j++;
                }
                for (int i = 0; i < index; i++) listBox1.Items.Add("Message box " + (i + 1));
                button3.Enabled = (listBox1.Items.Count < 10);
                button1.Enabled = button5.Enabled = true;
                listBox1.SelectedIndex = -1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // preview
            string content = textBox1.Text;
            string windowtitle = textBox2.Text;
            int icon = comboBox1.SelectedIndex;
            int options = comboBox2.SelectedIndex;
            if ((comboBox1.Enabled && icon == -1) || options == -1 || content.Length == 0 || windowtitle.Length == 0)
            {
                error("preview");
                return;
            }

            MessageBoxIcon mbi;
            if (icon == 1) {mbi = MessageBoxIcon.Error;}
            else if (icon == 2) {mbi = MessageBoxIcon.Question;}
            else if (icon == 3) {mbi = MessageBoxIcon.Warning;}
            else if (icon == 4) {mbi = MessageBoxIcon.Information;}
            else {mbi = MessageBoxIcon.None;}
            
            MessageBoxButtons mbb;
            if (options == 1) { mbb = MessageBoxButtons.OKCancel; }
            else if (options == 2) { mbb = MessageBoxButtons.AbortRetryIgnore; }
            else if (options == 3) { mbb = MessageBoxButtons.YesNoCancel; }
            else if (options == 4) { mbb = MessageBoxButtons.YesNo; }
            else if (options == 5) { mbb = MessageBoxButtons.RetryCancel; }
            else { mbb = MessageBoxButtons.OK; }

            MessageBox.Show(content, windowtitle, mbb, mbi);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // export
            for (int i=0; i<listBox1.Items.Count; i++)
            {
                if (messages[i] == null || titles[i] == null || messages[i] == "" || titles[i] == "" || icons[i] < 0 || buttons[i] < 0 || answerRequirements[Math.Max(i, 1)] == -1)
                {
                    error("message box", i + 1);
                    return;
                }
            }
            string filecontent = "";
            for (int i=0; i<listBox1.Items.Count; i++)
            {
                if (i > 0 && answerRequirements[i] > 0) filecontent += "If X=" + answerRequirements[i] + " Then\n\n";
                filecontent += "X=MsgBox(\"" + messages[i] + "\", " + buttons[i] + "+" + (icons[i] * 16) + ", \"" + titles[i] + "\")\n\n";
                if (i > 0 && answerRequirements[i] > 0) filecontent += "End If\n\n";
            }
            saveFileDialog1.Filter = "VBScript|*.vbs";
            saveFileDialog1.Title = "Export Message Box";
            saveFileDialog1.FileName = "messagebox.vbs";
            saveFileDialog1.InitialDirectory = Environment.GetEnvironmentVariable("userprofile") + "\\Downloads";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.OpenFile());
                writer.WriteLine(filecontent);
                writer.Dispose();
                writer.Close();
                success("Exported successfully!");
            }
        }

        private void error(string of_what, int where = 0)
        {
            string msg = "Check if everything is entered correctly";
            msg += (where > 0) ? " at message box " + where + "." : ".";
            MessageBox.Show(msg, "Cannot generate " + of_what, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private string[] messages = new string[10];
        private string[] titles = new string[10];
        private int[] icons = new int[10] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
        private int[] buttons = new int[10] {-1, -1, -1, -1, -1, -1, -1, -1, -1 ,-1};
        private int[] answerRequirements = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private void success(string content)
        {
            label8.Text = content;
            label8.Visible = true;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = (listBox1.SelectedIndex >= 0);
            button4.Enabled = (listBox1.SelectedIndex >= 0);
            groupBox1.Enabled = (listBox1.SelectedIndex >= 0);
            label10.Enabled = comboBox3.Enabled = button7.Enabled = (listBox1.SelectedIndex > 0);
            button8.Enabled = (listBox1.Items.Count > 1 && listBox1.SelectedIndex > -1 && listBox1.SelectedIndex < listBox1.Items.Count - 1);

            if (listBox1.SelectedIndex < 0) return;
            label6.Enabled = true;
            checkBox1.Checked = (icons[listBox1.SelectedIndex] == 256);
            textBox1.Text = messages[listBox1.SelectedIndex];
            textBox2.Text = titles[listBox1.SelectedIndex];
            comboBox1.Enabled = (icons[listBox1.SelectedIndex] != 256);
            comboBox1.SelectedIndex = (icons[listBox1.SelectedIndex] == 256) ? 0 : icons[listBox1.SelectedIndex];
            comboBox2.SelectedIndex = buttons[listBox1.SelectedIndex];
            comboBox3.SelectedIndex = (listBox1.SelectedIndex == 0) ? -1 : answerRequirements[listBox1.SelectedIndex];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // add
            if (listBox1.Items.Count >= 9) button3.Enabled = false;
            button1.Enabled = button5.Enabled = true;

            messages.Concat(new string[] { "" }).ToArray();
            titles.Concat(new string[] { "" }).ToArray();
            icons.Concat(new int[] { -1 }).ToArray();
            buttons.Concat(new int[] { -1 }).ToArray();
            answerRequirements.Concat(new int[] { 0 }).ToArray();
            int itemIndex = listBox1.Items.Count + 1;
            if (itemIndex == 2 && listBox1.SelectedIndex == 0) button8.Enabled = true;
            listBox1.Items.Add("Message box " + itemIndex);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // remove
            if (listBox1.SelectedIndex == -1) return;
            if (MessageBox.Show("Are you sure you want to remove box " + (listBox1.SelectedIndex + 1) + "?", "Please confirm:", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                textBox1.Text = textBox2.Text = "";
                comboBox1.SelectedIndex = comboBox2.SelectedIndex = comboBox3.SelectedIndex = -1;
                checkBox1.Checked = label6.Enabled = false;

                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                button3.Enabled = true;
                if (listBox1.Items.Count == 0) button1.Enabled = button5.Enabled = false;
                for (int i = 0; i < listBox1.Items.Count; i++) listBox1.Items[i] = "Message box " + (i + 1);
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            messages[listBox1.SelectedIndex] = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            titles[listBox1.SelectedIndex] = textBox2.Text;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            icons[listBox1.SelectedIndex] = comboBox1.SelectedIndex;
        }
        
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttons[listBox1.SelectedIndex] = comboBox2.SelectedIndex;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd.exe", "/c start https://nm-games.eu");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // save
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Message Box Configuration|*.mbconfig";
            fileDialog.Title = "Save Configuration";
            fileDialog.InitialDirectory = Environment.GetEnvironmentVariable("appdata") + "\\Message Box Creator\\Configurations";
            fileDialog.FileName = listBox1.Items.Count.ToString() + "_msg_config.mbconfig";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(fileDialog.OpenFile());
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    writer.WriteLine(messages[i]);
                    writer.WriteLine(titles[i]);
                    writer.WriteLine(icons[i] + ";" + buttons[i] + ";" + answerRequirements[i] + "\n");
                }
                writer.Dispose();
                writer.Close();
                success("Configuration saved!");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // load
            if (listBox1.Items.Count > 0 && MessageBox.Show("Loading a configuration overwrites the current one.\nDo you want to proceed?", "Please confirm:", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Message Box Configuration|*.mbconfig";
            fileDialog.InitialDirectory = Environment.GetEnvironmentVariable("appdata") + "\\Message Box Creator\\Configurations";
            fileDialog.Title = "Load Configuration";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] fragments = fileDialog.FileName.Split('.');
                if (fragments[fragments.Length - 1] != "mbconfig")
                {
                    MessageBox.Show("You loaded an invalid file!", "Cannot load file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                textBox1.Text = textBox2.Text = "";
                comboBox1.SelectedIndex = comboBox2.SelectedIndex = -1;

                StreamReader reader = new StreamReader(fileDialog.OpenFile());
                int j = 0;
                int index = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (j % 4 == 0) messages[index] = line;
                    else if (j % 4 == 1) titles[index] = line;
                    else if (j % 4 == 2)
                    {
                        string[] stats = line.Split(';');
                        icons[index] = int.Parse(stats[0]);
                        buttons[index] = int.Parse(stats[1]);
                        answerRequirements[index] = int.Parse(stats[2]);
                    }
                    else index++;
                    j++;
                }
                listBox1.Items.Clear();
                for (int i = 0; i < index; i++) listBox1.Items.Add("Message box " + (i + 1));
                button3.Enabled = (listBox1.Items.Count < 10);
                button1.Enabled = button5.Enabled = true;
                listBox1.SelectedIndex = -1;
                groupBox1.Enabled = button4.Enabled = button2.Enabled = button7.Enabled = button8.Enabled = false;
                success("Configuration loaded!");
            }
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            answerRequirements[listBox1.SelectedIndex] = comboBox3.SelectedIndex;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            label9.Visible = checkBox1.Checked;
            comboBox1.SelectedIndex = 0;
            comboBox1.Enabled = (!checkBox1.Checked);
            icons[listBox1.SelectedIndex] = (checkBox1.Checked) ? 256 : icons[listBox1.SelectedIndex]; // 256 * 16 = 4096 -> always on top number
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int fromIndex = listBox1.SelectedIndex;
            int toIndex = listBox1.SelectedIndex - 1;

            string currentContent = messages[fromIndex];
            string currentTitle = titles[fromIndex];
            int currentIcon = icons[fromIndex];
            int currentButtons = buttons[fromIndex];
            int currentAnswerReq = answerRequirements[fromIndex];

            messages[fromIndex] = messages[toIndex];
            titles[fromIndex] = titles[toIndex];
            icons[fromIndex] = icons[toIndex];
            buttons[fromIndex] = buttons[toIndex];
            answerRequirements[fromIndex] = answerRequirements[toIndex];
            messages[toIndex] = currentContent;
            titles[toIndex] = currentTitle;
            icons[toIndex] = currentIcon;
            buttons[toIndex] = currentButtons;
            answerRequirements[toIndex] = currentAnswerReq;

            listBox1.SelectedIndex = toIndex;
            if (answerRequirements[fromIndex] == -1) answerRequirements[fromIndex] = 0;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int fromIndex = listBox1.SelectedIndex;
            int toIndex = listBox1.SelectedIndex + 1;

            string currentContent = messages[fromIndex];
            string currentTitle = titles[fromIndex];
            int currentIcon = icons[fromIndex];
            int currentButtons = buttons[fromIndex];
            int currentAnswerReq = answerRequirements[fromIndex];

            messages[fromIndex] = messages[toIndex];
            titles[fromIndex] = titles[toIndex];
            icons[fromIndex] = icons[toIndex];
            buttons[fromIndex] = buttons[toIndex];
            answerRequirements[fromIndex] = answerRequirements[toIndex];
            messages[toIndex] = currentContent;
            titles[toIndex] = currentTitle;
            icons[toIndex] = currentIcon;
            buttons[toIndex] = currentButtons;
            answerRequirements[toIndex] = currentAnswerReq;

            listBox1.SelectedIndex = toIndex;
            if (comboBox3.SelectedIndex == -1) comboBox3.SelectedIndex = 0;
        }
    }
}
