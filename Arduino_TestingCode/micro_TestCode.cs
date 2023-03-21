
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
            _serialPort.PortName = "COM9"; 

            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = InfiniteTimeout;
            _serialPort.WriteTimeout = InfiniteTimeout;
            _serialPort.Open();

            // change filename to whatever you want to send over the port
            string filename = @"C:/Users/ajipp/Desktop/in.txt";


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            int maxBytesPerChunk = 1024;
            sendFile(filename, _serialPort, maxBytesPerChunk);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Elapsed Time: " + elapsedTime);

        }


        // sendFile() does this:

        // First, it sends:
        //      [MESSAGE_START] (1 byte)
        // For each chunk (of arbitrary size), it sends:
        //      [CHUNK_START] (1 byte)
        //      <CHUNK_LENGTH> (4 bytes)
        //      <Data>
        //      [CHUNK_END] (1 byte)
        // After all chunks complete, it sends:
        //      [MESSAGE_END] (1 byte)
        public static void sendFile(string fileName, SerialPort serial, int maxBytesPerChunk)
        {
            // SERIAL: send [MESSAGE_START] byte
            serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_START }, 0, 1);
            // FILE: open stream for reading
            using (var stream = File.Open(fileName, FileMode.Open))
            {
                // FILE: create binary reader to read from stream
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {

                    while (true)
                    {

                        // FILE: read next chunk of data
                        var bytes = reader.ReadBytes(maxBytesPerChunk);
                        // end loop if no more data in file
                        if (bytes.Length == 0)
                        {
                            break;
                        }

                        // SERIAL: send [CHUNK_START] byte
                        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);

                        // SERIAL: send length of chunk (as 4 byte integer)             
                        serial.Write(BitConverter.GetBytes(bytes.Length), 0, 4);

                        // SERIAL: send data
                        serial.Write(bytes, 0, bytes.Length);

                        // SERIAL: send [CHUNK_END] byte
                        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);
                    }
                }
            }

        }
    }
}





