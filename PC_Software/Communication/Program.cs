
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
        public enum SERIAL_CHARS : byte
        {
            MESSAGE_START = 1, // SOF
            CHUNK_START = 2, // STX
            CHUNK_END = 3, // ETX
            MESSAGE_END = 4, // EOT
            UNKNOWN = 255
        }
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

            // Create a new SerialPort object and open it
            _serialPort = new SerialPort();

            // change to whichever port is connected on your computer or simulated port
            _serialPort.PortName = "COM8";

            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = InfiniteTimeout;
            _serialPort.WriteTimeout = InfiniteTimeout;
            _serialPort.Open();

            string inFileName = @"C:/Users/ajipp/Desktop/in.txt";
            string outFileName = @"C:/Users/ajipp/Desktop/Downloads/"; // simulates the download folder on computer

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int maxBytesPerChunk = 1024;

            Task sendTask = Task.Factory.StartNew(() => SerialComms.SendFileEC(inFileName, _serialPort, maxBytesPerChunk));
            Task receiveTask = Task.Factory.StartNew(() => SerialComms.ReceiveFileEC(outFileName, _serialPort));


            Task.WaitAll(new[] { sendTask, receiveTask });


            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Elapsed Time: " + elapsedTime);

        }

    }
}



