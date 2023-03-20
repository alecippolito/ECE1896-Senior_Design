using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Communication
{
    public class PortComm
    {
        public const int InfiniteTimeout = -1;
        private SerialPort _serialPort;
        private int outputFileNum = 0;

        public PortComm()
        {
            init();

        }
        /*
         * init(): init serial port wth certain parameters
        */

        public void init()
        {
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
        }

        /*
        */
        public void sendData(string data)
        {
            startTransmission();
            _serialPort.Write(data);
            // for debug:
            // System.Console.WriteLine(data);
            endTransmission();
        }

        /*
         * startTransmisson(): writes SOH character
        */
        public void startTransmission()
        {
            // for debug:
            // String startVal = ((char)1).ToString();
            // System.Console.WriteLine(startVal);
            _serialPort.Write(((char)1).ToString());
        }

        /*
         * endTransmission(): writes EOF character
        */
        public void endTransmission()
        {
            // for debug:
            // String endVal = ((char)4).ToString();
            // System.Console.WriteLine(endVal);
            _serialPort.Write(((char)4).ToString());
        }
        /*
        */
        public string readFile(string filename)
        {
            if (File.Exists(filename) == true)
            {
                return System.IO.File.ReadAllText(filename);
            }
            else
            {
                return null;
            }


        }

        
        /*
        public void Read()
        {
            startReading();
            // TODO: change name!
            String fullPath = @"C:/Users/ajipp/Desktop/transmittedData_" + outputFileNum + ".txt";
            Int32 offset = 0;
            Int32 count = 64;
            byte[] bytes = new Byte[count];
            _serialPort.Read(bytes, offset, count);
            System.Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytes.Length));
            File.WriteAllBytes(fullPath, bytes);

            Process.Start(fullPath);

            outputFileNum++;

            
        }
        public void startReading()
        {
            
            bool start = true;
            String startVal = ((char)1).ToString();
            byte cByte = Convert.ToByte(startVal);
            while (start)
            {
                Int32 sByteI = _serialPort.ReadByte();
                byte sByte = Convert.ToByte(sByteI);
                if (sByte.Equals(cByte))
                start = false;
            }
            
        }
        */
        
    }
}



