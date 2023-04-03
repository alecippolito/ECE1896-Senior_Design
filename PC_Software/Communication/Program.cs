
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Communication
{
    static class PortChat
    {
        static SerialPort _serialPort;
        public const int InfiniteTimeout = -1;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            */
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM9"; // CHANGE: to current com port
            _serialPort.BaudRate = 1200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = InfiniteTimeout;
            _serialPort.WriteTimeout = InfiniteTimeout;
            _serialPort.Open();
            string inFileName = @"C:\Users\ajipp\Desktop\in.txt"; // change to desired data to ssends
            string outFileName = @"C:\Users\ajipp\Desktop\Downloads\"; // simulates the download folder on computer


            // string inFileName = @"C:\Z\in.txt"; // change to desired data to ssends
            // string outFileName = @"C:\Z\Downloads\"; // simulates the download folder on computer

            int maxBytesPerChunk = 1024; // change to chunk size you want

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Task sendTask = Task.Factory.StartNew(() => SerialComms.Send(inFileName, _serialPort, maxBytesPerChunk));
            
            Task receiveTask = Task.Factory.StartNew(() => SerialComms.Receive(outFileName, _serialPort));
            Task.WaitAll(new[] { sendTask, receiveTask });
            Task.WaitAll(new[] {receiveTask });
            
            
            /*
            
            while (true) {
                var val = _serialPort.ReadByte();
                byte[] intBytes = BitConverter.GetBytes(val);
                byte[] res = intBytes;
                string result = System.Text.Encoding.UTF8.GetString(res);
                Console.WriteLine(result);
            }
                 
            */
            

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Elapsed Time: " + elapsedTime);

        }

    }
}



