using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

/*
 * Author: AIP
 */

namespace MapleDownloader
{
    public partial class Form1 : Form
    {
        private string filePath, baseUrl, baseName, fullName;
        private bool finishedDownload;
        private int partNum, seconds;
        private WebClient c;
        private System.Timers.Timer speed;

        public Form1()
        {
            InitializeComponent();

            baseUrl = "http://download2.nexon.net/Game/MapleStory/FullVersion/";

            finishedDownload = true;

            baseName = "MSSetupv";
            partNum = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            versions.Items.Add("62");
            addVersions();

            versions.Text = Properties.Settings.Default.Version;
            textBox1.Text = Properties.Settings.Default.Destination;
            filePath = textBox1.Text;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Version = versions.Text;
            Properties.Settings.Default.Destination = filePath;
            Properties.Settings.Default.Save();
        }

        private void PickPath(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();

            if (folder.ShowDialog() == DialogResult.OK)
            {
                filePath = folder.SelectedPath;
                textBox1.Text = filePath;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Download(object sender, EventArgs e)
        {
            c = new WebClient();
            c.DownloadProgressChanged += C_DownloadProgressChanged;
            c.DownloadFileCompleted += C_DownloadFileCompleted;

            if (versions.Text == "62") // special case
            {
                fullName = "MSv62.rar";
                c.DownloadFileAsync(new Uri("http://www.webgame.co.il/downloads/MapleStory_0.62.rar"), filePath + @"\" + fullName);
                finishedDownload = true;
            }
            else
            {
                string fileName = baseName + versions.Text;
                int numFiles = Directory.GetFiles(filePath, fileName + "*").Length;

                if (numFiles == 0)
                    fullName = fileName + ".exe";
                else
                {
                    partNum = numFiles;
                    fullName = fileName + ".z0" + partNum++;
                }

                speed = new System.Timers.Timer(1000);
                speed.Elapsed += Speed_Elapsed;

                seconds = 1;
                speed.Start();

                finishedDownload = false;

                downloadFile(baseUrl + versions.Text + "/" + fullName, fullName);
            }
        }

        private void downloadFile(string url, string fileName)
        {
            HttpWebRequest request;
            HttpWebResponse response;

            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.Proxy = null;

            response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                finishedDownload = true;
                MessageBox.Show("ההורדה הושלמה !", "", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }

            if (response != null && response.StatusCode == HttpStatusCode.OK)
            {
                response.Close();
                c.DownloadFileAsync(new Uri(url), filePath + @"\" + fileName);
                

                button1.Enabled = false;
                button2.Enabled = true;
                versions.Enabled = false;
                selectPath.Enabled = false;

                progressBar1.Show();

                label3.Show();
                label4.Text = fileName;
                label4.Show();
                label5.Show();
                label6.Show();
                label7.Show();
                label8.Show();
                label9.Show();
            }
        }

        private void C_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label6.Text = ((int)(e.BytesReceived / 10000.0)) / 100.0 + "  מתוך  " + ((int)(e.TotalBytesToReceive / 10000.0)) / 100.0 + "  מגה בייט";
            label7.Text = e.ProgressPercentage + "%";
            label9.Text = e.BytesReceived / seconds / 1000 + " קילו בייט / שנייה";
        }

        private void C_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                c.Dispose();

                if (File.Exists(filePath + @"\" + fullName))
                {
                    this.ControlBox = false;
                    File.Delete(filePath + @"\" + fullName);
                    this.ControlBox = true;
                }
            }
            else if (!finishedDownload)
            {
                string fileName = baseName + versions.Text;

                fullName = fileName + ".z0" + partNum++;

                string url = baseUrl + versions.Text + "/" + fullName;

                downloadFile(url, fullName);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!finishedDownload)
            {
                DialogResult res = MessageBox.Show("ההורדה עדיין לא הושלמה, האם אתה בטוח שברצונך לצאת?", "יציאה לא מתוכננת", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

                if (res == DialogResult.Yes)
                {
                    c.CancelAsync();
                    Thread.Sleep(1000); // lol?
                    e.Cancel = false;
                }
                else
                    e.Cancel = true;
            }
        }

        private void cancelDownload(object sender, EventArgs e)
        {
            c.CancelAsync();
            progressBar1.Value = 0;
            label7.Text = "0%";
            button2.Enabled = false;
            button1.Enabled = true;
            speed.Stop();
            finishedDownload = true;
        }

        private void enableDownloadButton(object sender, EventArgs e)
        {
            if (versions.Text == "" || filePath == "")
                return;
            else if (!button1.Enabled)
                button1.Enabled = true;
        }

        private void addVersions()
        {
            int ver = 134;
            int minVer = 0;
            int maxVer = 0;
            string url;

            HttpWebRequest request;
            HttpWebResponse response;


            while (minVer == 0 || maxVer == 0)
            {
                url = baseUrl + ver + "/MSSetupv" + ver + ".exe";
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Proxy = null;

                response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    if (minVer != 0 && ver != 136) //why would they delete only ver 136? 0.0
                        maxVer = ver;
                }

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    response.Close();
                    if (minVer == 0)
                        minVer = ver;
                    versions.Items.Add(ver);
                }
                ver++;
            }
        }

        private void Speed_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            seconds++;
        }
    }
}
