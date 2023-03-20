using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Communication
{
    public partial class Form1 : Form
    {
        private PortComm _portComm;
        public Form1()
        {
            InitializeComponent();
            _portComm = new PortComm();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            var data = _portComm.readFile(dataTextbox.Text);
            if (string.IsNullOrEmpty(data))
            {
                warningLabel.Text = "Invalid Data: Try Again!";
                sentLabel.ForeColor = Color.Black;
            }
            sendTextFile(data);
        }
        public void sendTextFile(string data)
        {
            _portComm.startTransmission();
            warningLabel.Text = "";
            sentLabel.ForeColor = Color.Black;
            _portComm.sendData(data);
            _portComm.endTransmission();
            sentLabel.ForeColor = Color.Green;
        }

        public string getExtension()
        {
            FileInfo fi = new FileInfo(openFileDialog1.FileName);
            // Console.WriteLine("filename: " + fi.Extension);
            return fi.Extension;
        }

        private void browseDataButton_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                // DefaultExt = "txt",
                // Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dataTextbox.Text = openFileDialog1.FileName;
            }
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            // _portComm.Read();
        }

        
    }
}
