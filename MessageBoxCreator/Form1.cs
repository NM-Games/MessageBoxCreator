using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MessageBoxCreator
{
    public partial class Form1 : Form
    {
        private MsgBox[] boxes = new MsgBox[10];
        private string LoadedConfig = "";
        private int currentDefaultButton = 0;

        public Form1()
        {
            for (int i = 0; i < 10; i++) boxes[i] = new MsgBox();
            InitializeComponent();
            log("INFO", "Message Box Creator started successfully.");
            // load by argument passed configuration
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                try
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
                        if (j % 4 == 0) boxes[index].Content = line;
                        else if (j % 4 == 1) boxes[index].Title = line;
                        else if (j % 4 == 2)
                        {
                            string[] stats = line.Split(';');
                            boxes[index].Icon = int.Parse(stats[0]);
                            boxes[index].Button = int.Parse(stats[1]);
                            boxes[index].AnswerRequirement = int.Parse(stats[2]);
                            boxes[index].DefaultButton = int.Parse(stats[3]);
                            boxes[index].AlwaysOnTop = bool.Parse(stats[4]);
                            boxes[index].RightAligned = bool.Parse(stats[5]);
                        }
                        else index++;
                        j++;
                    }
                    for (int i = 0; i < index; i++) listBox1.Items.Add("Message box " + (i + 1));
                    button3.Enabled = (listBox1.Items.Count < 10);
                    button1.Enabled = button5.Enabled = true;
                    string[] directories = fragments[0].Split('\\');
                    SetLoadedConfig(directories[directories.Length - 1]);
                    listBox1.SelectedIndex = -1;
                } catch
                {
                    MessageBox.Show("Something went wrong with loading that file.", "Cannot load file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void SetLoadedConfig(string name)
        {
            Text = name + " | Message Box Creator";
            LoadedConfig = name;
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            // preview
            string content = textBox1.Text;
            string windowtitle = textBox2.Text;
            int icon = comboBox1.SelectedIndex;
            int options = comboBox2.SelectedIndex;
            int defaultButton = comboBox4.SelectedIndex;
            bool alwaysOnTop = checkBox1.Checked;
            bool rightAlign = checkBox2.Checked;
            if ((comboBox1.Enabled && icon == -1) || options == -1 || content.Length == 0 || windowtitle.Length == 0)
            {
                error("preview");
                return;
            }
            
            MessageBoxIcon mbi;
            if (icon == 1) mbi = MessageBoxIcon.Error;
            else if (icon == 2) mbi = MessageBoxIcon.Question;
            else if (icon == 3) mbi = MessageBoxIcon.Warning;
            else if (icon == 4) mbi = MessageBoxIcon.Information;
            else mbi = MessageBoxIcon.None;
            
            MessageBoxButtons mbb;
            if (options == 1) mbb = MessageBoxButtons.OKCancel;
            else if (options == 2) mbb = MessageBoxButtons.AbortRetryIgnore;
            else if (options == 3) mbb = MessageBoxButtons.YesNoCancel;
            else if (options == 4) mbb = MessageBoxButtons.YesNo;
            else if (options == 5) mbb = MessageBoxButtons.RetryCancel;
            else mbb = MessageBoxButtons.OK;

            MessageBoxDefaultButton mbdb;
            if (defaultButton == 0) mbdb = MessageBoxDefaultButton.Button1;
            else if (defaultButton == 1) mbdb = MessageBoxDefaultButton.Button2;
            else mbdb = MessageBoxDefaultButton.Button3;

            MessageBoxOptions mbo;
            if (alwaysOnTop && rightAlign) mbo = MessageBoxOptions.ServiceNotification | MessageBoxOptions.RightAlign;
            else if (alwaysOnTop) mbo = MessageBoxOptions.ServiceNotification;
            else if (rightAlign) mbo = MessageBoxOptions.RightAlign;
            else mbo = 0;

            MessageBox.Show(content, windowtitle, mbb, mbi, mbdb, mbo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // export
            for (int i=0; i<listBox1.Items.Count; i++)
            {
                if (boxes[i].Content == null || boxes[i].Title == null || boxes[i].Content == "" || boxes[i].Title == "" || !boxes[i].IsIconSet() || !boxes[i].IsButtonSet() || boxes[Math.Max(i, 1)].AnswerRequirement == -1)
                {
                    error("message box", i + 1);
                    return;
                }
            }
            string filecontent = "";
            for (int i=0; i<listBox1.Items.Count; i++)
            {
                string[] buttonConfig = new string[3] { boxes[i].GetIconName(), boxes[i].GetButtonName(), "vbDefaultButton" + (boxes[i].DefaultButton + 1).ToString() };
                if (boxes[i].AlwaysOnTop) buttonConfig = buttonConfig.Concat(new string[] { "vbSystemModal" }).ToArray();
                if (boxes[i].RightAligned) buttonConfig = buttonConfig.Concat(new string[] { "vbMsgBoxRight" }).ToArray();
                string buttons = String.Join(" + ", buttonConfig);

                Regex escapeQuotes = new Regex("[\"]");
                if (i > 0 && boxes[i].AnswerRequirement > 0) filecontent += "If X=" + boxes[i].AnswerRequirement + " Then\n\n";
                filecontent += "X=MsgBox(\"" + escapeQuotes.Replace(boxes[i].Content, "\"\"") + "\", " + buttons + ", \"" + escapeQuotes.Replace(boxes[i].Title, "\\\"") + "\")\n\n";
                if (i > 0 && boxes[i].AnswerRequirement > 0) filecontent += "End If\n\n";
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "VBScript|*.vbs";
            saveFileDialog.Title = "Export Message Box";
            saveFileDialog.FileName = (LoadedConfig.Length > 0) ? LoadedConfig + ".vbs" : "messagebox.vbs";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog.OpenFile());
                writer.WriteLine(filecontent);
                writer.Dispose();
                writer.Close();
                log("SUCCESS", "Configuration exported successfully as " + saveFileDialog.FileName);
            }
        }

        private void error(string of_what, int where = 0)
        {
            string msg = "Can't generate " + of_what + ", is everything entered correctly";
            msg += (where > 0) ? " at message box " + where + "?" : "?";
            log("ERROR", msg);
        }

        private void log(string type, string content)
        {
            listBox2.Items.Add("[" + DateTime.Now.ToString() + "] (" + type + ") " + content);
            listBox2.TopIndex = Math.Max(0, listBox2.Items.Count - 6); // 6 = lines fitting in box with scrollbar
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
            textBox1.Text = boxes[listBox1.SelectedIndex].Content;
            textBox2.Text = boxes[listBox1.SelectedIndex].Title;
            comboBox1.SelectedIndex = boxes[listBox1.SelectedIndex].Icon;
            comboBox2.SelectedIndex = boxes[listBox1.SelectedIndex].Button;
            comboBox3.SelectedIndex = (listBox1.SelectedIndex == 0) ? -1 : boxes[listBox1.SelectedIndex].AnswerRequirement;
            comboBox4.SelectedIndex = boxes[listBox1.SelectedIndex].DefaultButton;
            checkBox1.Checked = boxes[listBox1.SelectedIndex].AlwaysOnTop;
            checkBox2.Checked = boxes[listBox1.SelectedIndex].RightAligned;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // add
            if (listBox1.Items.Count >= 9) button3.Enabled = false;
            button1.Enabled = button5.Enabled = true;

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
            boxes[listBox1.SelectedIndex].Content = textBox1.Text;            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            boxes[listBox1.SelectedIndex].Title = textBox2.Text;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            boxes[listBox1.SelectedIndex].Icon = comboBox1.SelectedIndex;
        }
        
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentDefaultButton = boxes[listBox1.SelectedIndex].DefaultButton;
            boxes[listBox1.SelectedIndex].Button = comboBox2.SelectedIndex;
            string[] newItems = comboBox2.Text.Split(new[] { ", " }, StringSplitOptions.None);
            comboBox4.Items.Clear();
            comboBox4.Items.AddRange(newItems);
            comboBox4.SelectedIndex = currentDefaultButton;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            saveConfiguration();
        }

        private void saveConfiguration(bool close = false)
        {
            // save
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Message Box Configuration|*.mbconfig";
            fileDialog.Title = "Save Configuration";
            fileDialog.InitialDirectory = Environment.GetEnvironmentVariable("appdata") + "\\Message Box Creator\\Configurations";
            fileDialog.FileName = (LoadedConfig.Length > 0) ? LoadedConfig + ".mbconfig" : listBox1.Items.Count.ToString() + "_msg_config.mbconfig";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(fileDialog.OpenFile());
                string[] directories = fileDialog.FileName.Split('.')[0].Split('\\');
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    writer.WriteLine(boxes[i].Content);
                    writer.WriteLine(boxes[i].Title);
                    writer.WriteLine(boxes[i].Icon + ";" + boxes[i].Button + ";" + boxes[i].AnswerRequirement + ";" + boxes[i].DefaultButton + ";" + boxes[i].AlwaysOnTop + ";" + boxes[i].RightAligned + "\n");
                }
                writer.Dispose();
                writer.Close();
                SetLoadedConfig(directories[directories.Length - 1]);
                log("SUCCESS", "Saved configuration successfully as " + fileDialog.FileName);
                if (close) System.Environment.Exit(1);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // load
            if (listBox1.Items.Count > 0 && MessageBox.Show("When loading a new configuration, the current one will not be saved. Do you want to proceed?", "Please confirm:", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Message Box Configuration|*.mbconfig";
            fileDialog.InitialDirectory = Environment.GetEnvironmentVariable("appdata") + "\\Message Box Creator\\Configurations";
            fileDialog.Title = "Load Configuration";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] fragments = fileDialog.FileName.Split('.');
                    string[] directories = fragments[0].Split('\\');
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
                        if (j % 4 == 0) boxes[index].Content = line;
                        else if (j % 4 == 1) boxes[index].Title = line;
                        else if (j % 4 == 2)
                        {
                            string[] stats = line.Split(';');
                            boxes[index].Icon = int.Parse(stats[0]);
                            boxes[index].Button = int.Parse(stats[1]);
                            boxes[index].AnswerRequirement = int.Parse(stats[2]);
                            boxes[index].DefaultButton = int.Parse(stats[3]);
                            boxes[index].AlwaysOnTop = bool.Parse(stats[4]);
                            boxes[index].RightAligned = bool.Parse(stats[5]);
                        }
                        else index++;
                        j++;
                    }
                    reader.Close();
                    listBox1.Items.Clear();
                    for (int i = 0; i < index; i++) listBox1.Items.Add("Message box " + (i + 1));
                    button3.Enabled = (listBox1.Items.Count < 10);
                    button1.Enabled = button5.Enabled = true;
                    listBox1.SelectedIndex = -1;
                    groupBox1.Enabled = button4.Enabled = button2.Enabled = button7.Enabled = button8.Enabled = false;
                    SetLoadedConfig(directories[directories.Length - 1]);
                    log("SUCCESS", "The configuration is successfully loaded.");
                } catch
                {
                    log("ERROR", "Something went wrong with loading that file. Are you using the correct version?");
                }
            }
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            boxes[listBox1.SelectedIndex].AnswerRequirement = comboBox3.SelectedIndex;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            boxes[listBox1.SelectedIndex].AlwaysOnTop = checkBox1.Checked;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int fromIndex = listBox1.SelectedIndex;
            int toIndex = listBox1.SelectedIndex - 1;

            MsgBox current = boxes[fromIndex];
            boxes[fromIndex] = boxes[toIndex];
            boxes[toIndex] = current;

            listBox1.SelectedIndex = toIndex;
            if (boxes[fromIndex].AnswerRequirement == -1) boxes[fromIndex].AnswerRequirement = 0;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int fromIndex = listBox1.SelectedIndex;
            int toIndex = listBox1.SelectedIndex + 1;

            MsgBox current = boxes[fromIndex];
            boxes[fromIndex] = boxes[toIndex];
            boxes[toIndex] = current;

            listBox1.SelectedIndex = toIndex;
            if (comboBox3.SelectedIndex == -1) comboBox3.SelectedIndex = 0;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void quitAttempt(object sender, FormClosingEventArgs e)
        {
            if (!button5.Enabled) return;
            DialogResult prompt = MessageBox.Show("Do you want to save your changes?", "Please confirm:", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (prompt == DialogResult.Yes || prompt == DialogResult.Cancel) e.Cancel = true;
            if (prompt == DialogResult.Yes) saveConfiguration(true);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            boxes[listBox1.SelectedIndex].DefaultButton = comboBox4.SelectedIndex;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            boxes[listBox1.SelectedIndex].RightAligned = checkBox2.Checked;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "The Message Box Creator Application");
                HttpResponseMessage response = client.GetAsync("https://api.github.com/repos/ILoveAndLikePizza/MessageBoxCreator/releases/latest").Result;
                response.EnsureSuccessStatusCode();
                string responseText = await response.Content.ReadAsStringAsync();
                if (!responseText.Contains("\"name\":\"Message Box Creator v" + ProductVersion + "\""))
                {
                    if (MessageBox.Show("There is a new update available for Message Box Creator.\nWould you like to visit the download page?", "Update available", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) System.Diagnostics.Process.Start("cmd.exe", "/c start https://github.com/ILoveAndLikePizza/MessageBoxCreator/releases");
                }
            }
            catch (Exception)
            {
                log("INFO", "Unable to search for new updates right now.");
            }
        }
    }

    public class MsgBox
    {
        public string Content { get; set; }
        public string Title { get; set; }
        public int Icon { get; set; }
        public int Button { get; set; }
        public int AnswerRequirement { get; set; }
        public int DefaultButton { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool RightAligned { get; set; }

        public MsgBox()
        {
            Content = Title = "";
            Icon = Button = AnswerRequirement = DefaultButton = 0;
            AlwaysOnTop = RightAligned = false;
        }
        public string GetIconName()
        {
            string[] names = new string[5] { "vbOKOnly", "vbCritical", "vbQuestion", "vbExclamation", "vbInformation" };
            return names[this.Icon];
        }
        public string GetButtonName()
        {
            string[] names = new string[6] { "vbOKOnly", "vbOKCancel", "vbAbortRetryIgnore", "vbYesNoCancel", "vbYesNo", "vbRetryCancel" };
            return names[this.Button];
        }
        public bool IsIconSet()
        {
            return (this.Icon >= 0);
        }
        public bool IsButtonSet()
        {
            return (this.Button >= 0);
        }
    }
}
