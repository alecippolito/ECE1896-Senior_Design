
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

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();
            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM9"; // change to whichever port is connecteed
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = InfiniteTimeout;
            _serialPort.WriteTimeout = InfiniteTimeout;
            // open port
            _serialPort.Open();
            // regular files (get to work on all of these for sure first **
            string filename = @"C:/Users/ajipp/Desktop/in.txt";
            // string filename = @"C:/Users/ajipp/Desktop/interface.py";
            // string filename = @"C:/Users/ajipp/Desktop/text_only.docx";
            // string filename = @"C:/Users/ajipp/Desktop/Cyber_Notes.docx";
            // audio/video/picture
            // string filename = @"C:\Users\ajipp\Desktop\file_example_MP3_700KB.mp3";
            // string filename = @"C:\Users\ajipp\Desktop\video.mp4.crdownload"
            int maxBytesPerChunk = 1024;
            /*
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            SerialComms.SendFile(filename, _serialPort, maxBytesPerChunk);
            // SerialComms.ReceiveFile(filename, _serialPort);
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Elapsed Time: " + elapsedTime);
            */
            SerialComms.SendFile(filename, _serialPort, maxBytesPerChunk);

        }
    }
}



