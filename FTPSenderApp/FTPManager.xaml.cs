using System;
using System.Windows;
using Microsoft.Win32; // For the OpenFileDialog
using System.IO;
using System.Net;
using System.ComponentModel; // BackgroundWorker

namespace FTPSenderApp
{

    struct FTPData
    {
        public String IpAddress;
        public String FilePath;
        public String FileName;
        public String Password;
        public String Username;

        public void Initialize(String IpAddress, String FilePath, String FileName, String Username, String Password)
        {
            this.IpAddress = IpAddress;
            this.FilePath = FilePath;
            this.FileName = FileName;
            this.Password = Password;
            this.Username = Username;
        }
    };


    /// <summary>
    /// Interaction logic for FTPManager.xaml
    /// </summary>
    public partial class FTPManager : Window
    {
        public FTPManager()
        {
            InitializeComponent();
        }

        private void button_send_click(object sender, RoutedEventArgs e)
        {

            button_send.IsEnabled = false;
            FTPData Data = new FTPData();

            Data.Initialize(textBox_ip_address.Text, textBox_ftp_path.Text, System.IO.Path.GetFileName(textBox_ftp_path.Text), textBox_username.Text, password.Password);

            BackgroundWorker bw = new BackgroundWorker();

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync(Data);
        }

        private void button_open_file_click(object sender, RoutedEventArgs e)
        {
            //textBox_ftp_path
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select a File";
            // Show open file dialog box
            dlg.ShowDialog();

            textBox_ftp_path.Text = dlg.FileName;

        }



        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            FTPData Data = (FTPData)e.Argument;

            String fileName = System.IO.Path.GetFileName(Data.FilePath);


            FileStream stream = File.OpenRead(Data.FilePath);
            //ftp:// means that the protocol will be fpt. 
            //  /fpt/ means that the files will go to the homedirectory/ftp
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create("ftp://" + Data.IpAddress + "/ftp/" + Data.FileName);
            req.Method = WebRequestMethods.Ftp.UploadFile;
            req.Credentials = new NetworkCredential(Data.Username, Data.Password);
            req.UsePassive = true;
            req.KeepAlive = true;
            req.UseBinary = true;



            try
            {
                Stream reqstrm = req.GetRequestStream();

                int count = 0;
                byte[] buffer = new byte[4096];
                long length = 0;

                long lenghtFile = stream.Length;

                while ((count = stream.Read(buffer, 0, 4096)) != 0)
                {
                    length += count;
                    double value = (length * 1.0 / lenghtFile * 1.0) * 100;

                    Int32 percentage = (Convert.ToInt32(value));
                    worker.ReportProgress(percentage);

                    Console.WriteLine(value.ToString());
                    reqstrm.Write(buffer, 0, count);
                }

                reqstrm.Close();

                stream.Close();
            }
            catch
            {
                MessageBox.Show("A problem ocurred!");
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progress_uploading.Value = (e.ProgressPercentage);
            this.progress_uploading_percentage.Content = (e.ProgressPercentage);
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button_send.IsEnabled = true;
            MessageBox.Show("Work Done.");

        }

    }
}