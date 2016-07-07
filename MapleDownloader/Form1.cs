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
        private readonly string baseUrl = "http://download2.nexon.net/Game/MapleStory/FullVersion/";
        private readonly string baseName = "MSSetupv";
        private string savePath, fullFilePath;
        private bool finishedDownload;
        private int partNum, seconds;
        private WebClient c;
        private System.Timers.Timer speed;

        public Form1()
        {
            InitializeComponent();

            finishedDownload = true;

            partNum = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            addSpecialVersions();
            addVersions();

            versions.Text = Properties.Settings.Default.Version;
            textBox1.Text = Properties.Settings.Default.Destination;
            savePath = textBox1.Text;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Version = versions.Text;
            Properties.Settings.Default.Destination = savePath;
            Properties.Settings.Default.Save();
        }

        private void PickPath(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();

            if (folder.ShowDialog() == DialogResult.OK)
            {
                savePath = folder.SelectedPath;
                textBox1.Text = savePath;
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

            string fileName;

            if (versions.Text == "62") // special case
            {
                fileName = "MapleStory_V62.rar";
                fullFilePath = savePath + @"\" + fileName;
                c.DownloadFileAsync(new Uri("https://googledrive.com/host/0B8bVK378Rj_eZlp4X1A3Rllhblk/MapleStory_V62.rar"), fullFilePath);
                finishedDownload = false;
            }
            else if(versions.Text == "83")
            {
                fileName = "MapleStory_V83.rar";
                fullFilePath = savePath + @"\" + fileName;
                c.DownloadFileAsync(new Uri("https://googledrive.com/host/0B8bVK378Rj_eZlp4X1A3Rllhblk/MapleStory_V83.rar"), fullFilePath);
                finishedDownload = false;
            }
            else
            {
                fileName = baseName + versions.Text;
                int numFiles = Directory.GetFiles(savePath, fileName + "*").Length;
                string fullUrl;

                if (numFiles == 0)
                    fileName += ".exe";
                else
                {
                    partNum = numFiles;
                    fileName += ".z0" + partNum++;
                }

                fullUrl = baseUrl + versions.Text + "/" + fileName;
                fullFilePath = savePath + @"\" + fileName;

                finishedDownload = false;

                downloadFile(fullUrl, fileName);
            }

            if (!finishedDownload)
            {
                speed = new System.Timers.Timer(1000);
                speed.Elapsed += Speed_Elapsed;

                seconds = 1;
                speed.Start();

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
                c.DownloadFileAsync(new Uri(url), fullFilePath);

                label4.Text = fileName;
            }
        }

        private void C_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double fileSize = ((int)(e.TotalBytesToReceive / 10000.0)) / 100.0;
            int progress = e.ProgressPercentage;

            if (versions.Text == "62")
            {
                fileSize = 979.5;
                progress = (int)(e.BytesReceived / 1000000 / fileSize * 100);
                progressBar1.Value = progress;
            }
            else if (versions.Text == "83")
            {
                fileSize = 1594.67;
                progress = (int)(e.BytesReceived / 1000000 / fileSize * 100 );
                progressBar1.Value = progress;
            }
            else
            {
                progressBar1.Value = e.ProgressPercentage;
            }

            label6.Text = ((int)(e.BytesReceived / 10000.0)) / 100.0 + "  מתוך  " + fileSize + "  מגה בייט";
            label7.Text = progress + "%";
            label9.Text = e.BytesReceived / seconds / 1000 + " קילו בייט / שנייה";
        }

        private void C_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                c.Dispose();

                if (File.Exists(fullFilePath))
                {
                    this.ControlBox = false;
                    File.Delete(fullFilePath);
                    this.ControlBox = true;
                }
            }
            else if(versions.Text == "62" || versions.Text == "83")
            {
                finishedDownload = true;
                MessageBox.Show("ההורדה הושלמה !", "", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
            }
            else if (!finishedDownload)
            {
                string fileName = baseName + versions.Text + ".z0" + partNum++;
                string fullUrl = baseUrl + versions.Text + "/" + fileName;

                fullFilePath = savePath + @"\" + fileName;

                downloadFile(fullUrl, fileName);
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
            versions.Enabled = true;
            selectPath.Enabled = true;
            speed.Stop();
            finishedDownload = true;
        }

        private void enableDownloadButton(object sender, EventArgs e)
        {
            if (versions.Text == "" || savePath == "")
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

        private void addSpecialVersions()
        {
            string[,] arr =
            {
                { "62", "https://googledrive.com/host/0B8bVK378Rj_eZlp4X1A3Rllhblk/MapleStory_V62.rar" },
                { "83", "https://googledrive.com/host/0B8bVK378Rj_eZlp4X1A3Rllhblk/MapleStory_V83.rar" }
            };

            HttpWebRequest request;
            HttpWebResponse response;

            for (int i = 0; i < arr.Length / 2; i++)
            {
                request = (HttpWebRequest)WebRequest.Create(arr[i, 1]);
                request.Method = "HEAD";
                request.Proxy = null;

                response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                }

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    response.Close();
                    versions.Items.Add(arr[i, 0]);
                }
            }
        }

        private void Speed_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            seconds++;
        }
    }
}
