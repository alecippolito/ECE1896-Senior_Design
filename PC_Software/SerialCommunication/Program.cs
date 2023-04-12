
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
        static SerialPort _serialPort0;
        public const int InfiniteTimeout = -1;
        public enum SERIAL_CHARS : byte
        {
            MESSAGE_START = 1, // SOF
            CHUNK_START = 2, // STX
            CHUNK_END = 3, // ETX
            MESSAGE_END = 4, // EOT
            UNKNOWN = 255
        }
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
            // Sending Port
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM11";
            _serialPort.BaudRate = 1200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.Open();


            // Recieving Port
            _serialPort0 = new SerialPort();
            _serialPort0.PortName = "COM12";
            _serialPort0.BaudRate = 1200;
            _serialPort0.Parity = Parity.None;
            _serialPort0.DataBits = 8;
            _serialPort0.StopBits = StopBits.One;
            _serialPort0.Handshake = Handshake.None;
            _serialPort0.ReadTimeout = 10000;
            _serialPort0.WriteTimeout = 10000;
            _serialPort0.Open();

            string inFileName = @"C:\Users\ajipp\Desktop\in.txt";
            string inFileName1 = @"C:\Users\ajipp\Desktop\notes.txt";
            string outFile = @"C:\Users\ajipp\Desktop\Downloads\";
            int maxBytesPerChunk = 1024;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            CancellationTokenSource tokenS = new CancellationTokenSource();
            Task recieveTask = Task.Factory.StartNew(() => Reciever.RecieveFiles(outFile, _serialPort0, tokenS.Token), tokenS.Token);
            Sender.SendFileN(inFileName, _serialPort, maxBytesPerChunk);
            // Sender.SendFileN(inFileName1, _serialPort, maxBytesPerChunk);
            tokenS.Cancel();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
            Console.WriteLine("Time Stamp for Program: " + elapsedTime);

        }

    }
}



