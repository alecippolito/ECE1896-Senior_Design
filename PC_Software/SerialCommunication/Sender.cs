
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

public static class Sender
{

    public enum SERIAL_CHARS : byte
    {
        MESSAGE_START = 1,
        CHUNK_START = 2,
        CHUNK_END = 3,
        MESSAGE_END = 4,
        EXT_START = 5,
        EXT_END = 6,
        UNKNOWN = 255
    }


    public const int senderDelay = 1000;
    public static void SendFile(String filePath, SerialPort serial, int maxBytesPerChunk)
    {
        Console.WriteLine("Sending Starting!");
        // Filename
        FileInfo fi = new FileInfo(filePath);
        byte[] fileName = Encoding.ASCII.GetBytes(fi.Name);


        var firstLoop = true;
        while (true)
        {
            // Wait when Error
            if (!firstLoop)
            {
                Thread.Sleep(senderDelay);
            }
            else
            {
                firstLoop = false;
            }
            if (!MessageStartSendS(serial))
            {
                continue;
            }
            if (!MessageStartWaitS(serial))
            {
                continue;
            }

            if (!ExtStartSendS(serial))
            {
                continue;
            }
            if (!ExtStartWaitS(serial))
            {
                continue;
            }

            if (!LengthSendS(serial, fileName.Length))
            {
                continue;
            }

            if (!LengthWaitS(serial, fileName.Length))
            {
                continue;
            }

            SendDataS(serial, fileName);

            if (!DataWaitS(serial, fileName))
            {
                continue;
            }

            if (!ExtEndSendS(serial))
            {
                continue;
            }
            if (!ExtEndWaitS(serial))
            {
                continue;
            }


            using (var stream = File.Open(filePath, FileMode.Open))
            {
                // FILE: create binary reader to read from stream
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    while (true)
                    {
                        var bytes = reader.ReadBytes(maxBytesPerChunk);
                        // end loop if no more data in file
                        if (bytes.Length == 0)
                        {
                            break;
                        }

                        if (!ChunkStartSendS(serial))
                        {
                            continue;
                        }
                        if (!ChunkStartWaitS(serial))
                        {
                            continue;
                        }

                        if (!LengthSendS(serial, bytes.Length))
                        {
                            continue;
                        }
                        if (!LengthWaitS(serial, bytes.Length))
                        {
                            continue;
                        }

                        SendDataS(serial, bytes);

                        if (!ChunkEndSendS(serial))
                        {
                            continue;
                        }

                        if (!ChunkEndWaitS(serial))
                        {
                            continue;
                        }
                    }
                }
            }
            if (!MessageEndSendS(serial))
            {
                continue;
            }
            if (!MessageEndWaitS(serial))
            {
                continue;
            }

            Console.WriteLine("SENDER: Send Done.");
            break;
        }
    }


    public static bool MessageStartSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Message Start Byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_START }, 0, 1);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for sending Message Start byte.");
            return false;
        }
        return true;
    }
    public static bool MessageStartWaitS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Waiting for Message Start Echo");
            var data = serial.ReadByte();
            // Return True if Echo Recived
            if (data == 1)
            {
                Console.WriteLine("SENDER: Corect Message Start value recieved");
                return true;
            }
            else
            {
                Console.WriteLine("SENDER: Incorrect Message Start value recieved");
                return false;
            }
        }
        catch
        {
            Console.WriteLine("SENDER: Timeout Error for Message Start recieved");
            return false;
        }

    }

    public static bool ChunkStartSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Chunk Start byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for sending Chunk Start byte.");
            return false;
        }
        return true;
    }
    public static bool ChunkStartWaitS(SerialPort serial)
    {
        Console.WriteLine("SENDER: Waiting for Chunk Start Echo!");
        var data = serial.ReadByte();
        if (data == 2)
        {
            Console.WriteLine("SENDER: Correct Chunk Start byte recieved.");
            return true;
        }
        else
        {
            Console.WriteLine("SENDER: Incorrect Chunk Start byte recieved.");
            return false;
        }
    }
    public static bool LengthSendS(SerialPort serial, int dataLength)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Length bytes!");
            serial.Write(BitConverter.GetBytes(dataLength), 0, 4);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for sending Length bytes.");
            return false;
        }
        return true;

    }
    public static bool LengthWaitS(SerialPort serial, int dataLength)
    {
        var lenData = new byte[4];
        Console.WriteLine("SENDER: Waiting for Length bytes Echo!");
        var bytesRead = SerialRead(serial, lenData, 4);
        var chunkLength = BitConverter.ToInt32(lenData, 0);
        if (chunkLength == dataLength)
        {
            Console.WriteLine("SENDER: Correct Length bytes recieved");
            return true;
        }
        else
        {
            Console.WriteLine("SENDER: Incorrect Length bytes recieved, got " + chunkLength + ", when  " + dataLength + " was expected.");
            return false;
        }
    }


    public static void SendDataS(SerialPort serial, byte[] data)
    {
        Console.WriteLine("SENDER: Sending Data packet!");
        serial.Write(data, 0, data.Length);
    }
    private static bool DataWaitS(SerialPort serial, byte[] data)
    {
        Console.WriteLine("SENDER: Reading in Data Packet  (for filename)!");
        var inData = new byte[data.Length];
        SerialRead(serial, inData, data.Length);
        if (inData == data)
        {
            Console.WriteLine("SENDER: Corret Filename recieved!");
            return true;
        }
        Console.WriteLine("SENDER: Inorrect Filename data recieved.");
        return true;
    }
    public static bool ChunkEndSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Chunk End!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_END }, 0, 1);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for sending Chunk End.");
            return false;
        }
        return true;
    }
    public static bool ChunkEndWaitS(SerialPort serial)
    {
        Console.WriteLine("SENDER: Waiting for Chunk End byte!");
        var data = serial.ReadByte();
        if (data == 3)
        {
            Console.WriteLine("SENDER: Correct Chunk End byte recieved!");
            return true;
        }
        else
        {
            Console.WriteLine("SENDER: Inorrect Chunk End byte recieved.");
            return false;
        }
    }
    public static bool MessageEndSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Writing Message End Byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_END }, 0, 1);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for writing Message End byte.");
            return false;
        }
        return true;
    }
    public static bool MessageEndWaitS(SerialPort serial)
    {
        Console.WriteLine("SENDER: Waiting for Message End byte!");
        try
        {
            var data = serial.ReadByte();

            if (data == 4)
            {
                Console.WriteLine("SENDER: Correct Message End byte recieved!");
                return true;
            }
            else
            {
                Console.WriteLine("SENDER: Inorrect Message End byte recieved.");
                return false;
            }
        }
        catch
        {
            Console.WriteLine("SENDER: Timeout Occur from Message End.");
            return false;
        }
    }

    // Send Extension Start
    public static bool ExtStartSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Extension Start byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.EXT_START }, 0, 1);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for sending Extension Start byte.");
            return false;
        }
        return true;
    }
    // Wait Extension Start
    public static bool ExtStartWaitS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Waiting for Extension Start Echo!");
            var data = serial.ReadByte();
            if (data == 5)
            {
                Console.WriteLine("SENDER: Correct Extension Start byte recieved.");
                return true;
            }
            else
            {
                Console.WriteLine("SENDER: Incorrect Extension Start byte recieved.");
                return false;
            }
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for Extension Start Wait.");
            return false;
        }
    }

    // Send Extension Start
    public static bool ExtEndSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Extension End byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.EXT_END }, 0, 1);
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for sending Extension End byte.");
            return false;
        }
        return true;
    }
    // Wait Extension Start
    public static bool ExtEndWaitS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Waiting for Extension End Echo!");
            var data = serial.ReadByte();
            if (data == 6)
            {
                Console.WriteLine("SENDER: Correct Extension End byte recieved.");
                return true;
            }
            else
            {
                Console.WriteLine("SENDER: Incorrect Extension End byte recieved.");
                return false;
            }
        }
        catch
        {
            Console.WriteLine("SENDER: Exception for Extension End byte.");
            return false;
        }
    }
    // Reading Bytes from Serial Port
    private static int SerialRead(SerialPort serial, byte[] data, int dataLength)
    {
        var bytesRead = 0;
        while (bytesRead < dataLength)
        {
            var b = serial.Read(data, bytesRead, dataLength - bytesRead);
            if (b < 0)
            {
                return b;
            }
            bytesRead += b;
        }
        return bytesRead;
    }
    public enum STATE : int
    {
        Delay = -1,
        MessageStartSend = 0,
        MessageStartWait,
        ExtStartSend,
        ExtStartWait,
        ExtLengthSend,
        ExtLengthWait,
        ExtDataSend,
        ExtDataWait,
        ExtEndSend,
        ExtEndWait,
        ReadFile,
        ChunkStartSend,
        ChunkStartWait,
        LengthSend,
        LengthWait,
        DataSend,
        ChunkEndSend,
        ChunkEndWait,
        MessageEndSend,
        MessageEndWait,
        Done
    }
    public static void SendFileN(String filePath, SerialPort serial, int maxBytesPerChunk)
    {
        Console.WriteLine("Sending Starting!");
        FileInfo fi = new FileInfo(filePath);
        byte[] fileName = Encoding.ASCII.GetBytes(fi.Name);
        STATE state = STATE.MessageStartSend;
        var nextStateAfterDelay = STATE.MessageStartSend;
        var stream = File.Open(filePath, FileMode.Open);
        var reader = new BinaryReader(stream, Encoding.UTF8, false);
        byte[] bytes = null;

        while (true)
        {
            Console.WriteLine("SENDER: Entering: " + state.ToString());
            switch (state)
            {
                case STATE.Delay:
                    Thread.Sleep(senderDelay);
                    state = nextStateAfterDelay;
                    continue;
                case STATE.MessageStartSend:
                    if (!MessageStartSendS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.MessageStartWait:
                    if (!MessageStartWaitS(serial))
                    {
                        //nextStateAfterelay = xxx
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtStartSend:
                    if (!ExtStartSendS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtStartWait:
                    if (!ExtStartWaitS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtLengthSend:
                    if (!LengthSendS(serial, fileName.Length))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtLengthWait:
                    if (!LengthWaitS(serial, fileName.Length))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtDataSend:
                    SendDataS(serial, fileName);
                    break;
                case STATE.ExtDataWait:
                    if (!DataWaitS(serial, fileName))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtEndSend:
                    if (!ExtEndSendS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtEndWait:
                    if (!ExtEndWaitS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ReadFile:
                    bytes = reader.ReadBytes(maxBytesPerChunk);
                    if (bytes.Length == 0)
                    {
                        state = STATE.MessageEndSend;
                        continue;
                    }
                    break;
                case STATE.ChunkStartSend:
                    if (!ChunkStartSendS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ChunkStartWait:
                    if (!ChunkStartWaitS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.LengthSend:
                    if (!LengthSendS(serial, bytes.Length))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.LengthWait:
                    if (!LengthWaitS(serial, bytes.Length))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.DataSend:
                    SendDataS(serial, bytes);
                    break;
                case STATE.ChunkEndSend:
                    if (!ChunkEndSendS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ChunkEndWait:
                    if (!ChunkEndWaitS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    state = STATE.ReadFile;
                    continue;
                case STATE.MessageEndSend:
                    if (!MessageEndSendS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.MessageEndWait:
                    if (!MessageEndWaitS(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.Done:
                    Console.WriteLine("SENDER: Send Done.");
                    return;
                default:
                    break;
            }
            Console.WriteLine("SENDER: Completed: " + state.ToString());
            state++;

        }
    }
}

