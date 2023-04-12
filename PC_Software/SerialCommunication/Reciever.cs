
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

public static class Reciever
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



    public const int recieverDelay = 1000;
    
    

    public static void RecieveFiles(string filePath, SerialPort serial, CancellationToken token)
    {
        Console.WriteLine("File Recieving Starting!");
        while (true)
        {
            RecieveFileN(filePath, serial);

            if (token.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public static bool RecieveFile(string filePath, SerialPort serial)
    {
        Console.WriteLine("Starting Receieving!");

        var lengthData = 0;
        var lengthDataFilename = 0;
        STATE state = STATE.MessageStartWait;
        var nextStateAfterDelay = STATE.MessageStartWait;
        byte[] dataFilename = null;
        FileStream stream = null;
        BinaryWriter writer = null;
        var openFile = true;
        byte[] data = null;
        // stream = File.Create(@"C:\Users\ajipp\Desktop\Downloads\in.txt");



        while (true)
        {
            Console.WriteLine("RECIEVER: Entering " + state.ToString());
            switch (state)
            {
                case STATE.Delay:
                    Thread.Sleep(recieverDelay);
                    state = nextStateAfterDelay;
                    continue;
                case STATE.MessageStartWait:
                    if (!MessageStartWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.MessageStartEcho:
                    if (!MessageStartEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtStartWait:
                    if (!ExtStartWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtStartEcho:
                    if (!ExtStartEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtLengthWait:
                    if (!LengthWaitR(serial, out lengthDataFilename))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtLengthEcho:
                    if (!LengthEchoR(serial, lengthDataFilename))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtDataWait:
                    if (!RecieveDataR(serial, lengthDataFilename, out dataFilename))
                    {
                        Console.WriteLine("LOOK HERE: " + Encoding.UTF8.GetString(dataFilename, 0, dataFilename.Length));
                        state = STATE.Delay;
                        continue;
                    }
                    Console.WriteLine("LOOK HERE: " + Encoding.UTF8.GetString(dataFilename, 0, dataFilename.Length));

                    break;
                case STATE.ExtDataEcho:
                    if (!EchoDataR(serial, dataFilename))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.OpenStream:
                    if (openFile)
                    {
                        stream = File.Create(filePath + Encoding.UTF8.GetString(dataFilename, 0, dataFilename.Length));
                        writer = new BinaryWriter(stream, Encoding.UTF8, false);
                        openFile = false;
                        continue;
                    }
                    break;
                case STATE.ExtEndWait:
                    if (!ExtEndtWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtEndEcho:
                    if (!ExtEndEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ChunkStartWait:
                    if (!ChunkStartOrEndMessageWaitR(serial, out var isEnd))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    if (isEnd)
                    {
                        state = STATE.Done;
                        continue;
                    }
                    break;
                case STATE.ChunkStartEcho:
                    if (!ChunkStartEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }

                    break;
                case STATE.LengthWait:
                    if (!LengthWaitR(serial, out lengthData))
                    {
                        state = STATE.Delay;
                        continue; // If wrong message, start over
                    }
                    break;
                case STATE.LengthEcho:
                    if (!LengthEchoR(serial, lengthData))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.DataSend:
                    // Recieve data and write to file
                    if (!RecieveDataR(serial, lengthData, out data))
                    {
                        continue;
                    }
                    writer.Write(data, 0, lengthData);
                    break;
                case STATE.ChunkEndWait:
                    if (!ChunkEndWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue; // If wrong message, start over
                    }
                    break;
                case STATE.ChunkEndEcho:
                    if (!ChunkEndEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.MessageEndWait:
                    break;
                case STATE.MessagEndEcho:
                    if (!MessageEndEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;

                case STATE.Done:
                    Console.WriteLine("RECIVER: Recieving Done.");
                    return true;
                default:
                    break;
            }
            Console.WriteLine("RECIEVER: Completed " + state.ToString());
            state++;
        }
    }


    // Message Start Function
    public static bool MessageStartWaitR(SerialPort serial)
    {
        Console.WriteLine("RECIVER: Waiting for Message Start byte!");
        var data = serial.ReadByte();
        if (data == (int)SERIAL_CHARS.MESSAGE_START)
        {
            Console.WriteLine("RECIVER: Correct Message Start byte recieved!");
            return true;
        }
        Console.WriteLine("RECIVER: Incorrect Message Start byte recieved, recieved " + data + " instead.");
        return false;
    }
    // Message Echo Function
    public static bool MessageStartEchoR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Message Start!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_START }, 0, 1);
        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Echoing Message Start.");
            return false;
        }
        return true;
    }
    // Chunk Start OR End Message Wait Function
    public static bool ChunkStartOrEndMessageWaitR(SerialPort serial, out bool isEnd)
    {
        var data = serial.ReadByte();
        if (data == (int)SERIAL_CHARS.CHUNK_START)
        {
            Console.WriteLine("RECIVER: Waiting for byte and recieved Chunk Start byte!");
            isEnd = false;
            return true;
        }
        if (data == (int)SERIAL_CHARS.MESSAGE_END)
        {
            Console.WriteLine("RECIVER: Waiting for byte and recieved Message End byte!");
            isEnd = true;
            return true;
        }
        else
        {
            Console.WriteLine("RECIVER: Incorrect byte, got " + data + " instead of expected Chunk Start or Message End.");
            isEnd = false;
            return false;
        }
    }
    // Chunk Start Echo
    public static bool ChunkStartEchoR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Chunk Start byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);
        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Echoing Chunk Start.");
            return false;
        }
        return true;
    }
    // Ext Start Echo
    public static bool ExtStartEchoR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Extension Start byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.EXT_START }, 0, 1);
        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Extension Start Start.");
            return false;
        }
        return true;
    }

    // Extension End Function
    public static bool ExtEndtWaitR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Waiting for Extension End byte!");
            var data = serial.ReadByte();
            if (data == (int)SERIAL_CHARS.EXT_END)
            {
                Console.WriteLine("RECIVER: Correct Extension End byte recieved!");
                return true;
            }
            Console.WriteLine("RECIVER: Incorrect Extension End byte recieved, recieved " + data + " instead.");
            return false;
        }
        catch
        {
            Console.WriteLine("RECIEVER: Excepton for Extension End byte.");
            return false;
        }
    }
    // Extension End Echo
    public static bool ExtEndEchoR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Chunk End byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.EXT_END }, 0, 1);
        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Echoing Extension End.");
            return false;
        }
        return true;
    }

    // Length Function
    public static bool LengthWaitR(SerialPort serial, out int lengthData)
    {
        var data = new byte[4];
        Console.WriteLine("RECIVER: Waiting for Length bytes!");
        var numBytes = SerialRead(serial, data, 4);
        if (numBytes != 4)
        {
            Console.WriteLine("RECIVER: Incorrect Length bytes size recieved, recieved " + numBytes + " instead.");
            lengthData = 0;
            return false;
        }
        lengthData = BitConverter.ToInt32(data, 0);
        Console.WriteLine("RECIVER: Correct Length bytes recieved!");
        return true;
    }
    // Length Echo
    public static bool LengthEchoR(SerialPort serial, Int32 lengthData)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Length bytes!");
            serial.Write(BitConverter.GetBytes(lengthData), 0, 4);

        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Echoing Length.");
            return false;
        }
        return true;
    }
    // Recieve Data Function
    public static bool RecieveDataR(SerialPort serial, int dataLength, out byte[] data)
    {
        Console.WriteLine("RECIVER: Reading in Data Packet!");
        data = new byte[dataLength];
        SerialRead(serial, data, dataLength);
        return true;
    }
    private static bool EchoDataR(SerialPort serial, byte[] data)
    {
        Console.WriteLine("RECIVER: EChoing Data Paket (for filename)!");
        serial.Write(data, 0, data.Length);
        return true;
    }

    // Chunk End Function
    public static bool ChunkEndWaitR(SerialPort serial)
    {
        Console.WriteLine("RECIVER: Waiting for Chunk End byte!");
        var data = serial.ReadByte();
        if (data == (int)SERIAL_CHARS.CHUNK_END)
        {
            Console.WriteLine("RECIVER: Correct Chunk End byte recieved!");
            return true;
        }
        Console.WriteLine("RECIVER: Incorrect Chunk End byte recieved, recieved " + data + " instead.");
        return false;
    }
    // Chunk End Echo
    public static bool ChunkEndEchoR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Chunk End byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_END }, 0, 1);
        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Echoing Chunk End.");
            return false;
        }
        return true;
    }
    // Message End Echo
    public static bool MessageEndEchoR(SerialPort serial)
    {
        try
        {
            Console.WriteLine("RECIVER: Echoing Message End byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_END }, 0, 1);

        }
        catch
        {
            Console.WriteLine("RECIVER: Exception for Echoing Message End.");
            return false;
        }
        return true;
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

    /// 
    // Extension Start Function
    public static bool ExtStartWaitR(SerialPort serial)
    {
        Console.WriteLine("RECIVER: Waiting for Extension Start byte!");
        var data = serial.ReadByte();
        if (data == (int)SERIAL_CHARS.EXT_START)
        {
            Console.WriteLine("RECIVER: Coasdrrect Extension Start byte recieved!");
            return true;
        }
        Console.WriteLine("RECIVER: Incorrect Extension Start byte recieved, recieved " + data + " instead.");
        return false;
    }
    public enum STATE : int
    {
        Delay = -1,
        MessageStartWait = 0,
        MessageStartEcho,
        ExtStartWait,
        ExtStartEcho,
        ExtLengthWait,
        ExtLengthEcho,
        ExtDataWait,
        ExtDataEcho,
        ExtEndWait,
        ExtEndEcho,
        OpenStream,
        ChunkStartWait,
        ChunkStartEcho,
        LengthWait,
        LengthEcho,
        DataSend,
        ChunkEndWait,
        ChunkEndEcho,
        MessageEndWait,
        MessagEndEcho,
        Done
    }

    public static bool RecieveFileN(string filePath, SerialPort serial)
    {
        Console.WriteLine("Starting Receieving!");
        
        var lengthData = 0;
        var lengthDataFilename = 0;
        STATE state = STATE.MessageStartWait;
        var nextStateAfterDelay = STATE.MessageStartWait;
        byte[] dataFilename = null;
        FileStream stream = null;
        BinaryWriter writer = null;
        var openFile = true;
        byte[] data = null;
        // stream = File.Create(@"C:\Users\ajipp\Desktop\Downloads\in.txt");
        


        while (true)
        {
            Console.WriteLine("RECIEVER: Entering " + state.ToString());
            switch (state)
            {
                case STATE.Delay:
                    Thread.Sleep(recieverDelay);
                    state = nextStateAfterDelay;
                    continue;
                case STATE.MessageStartWait:
                    if (!MessageStartWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.MessageStartEcho:
                    if (!MessageStartEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtStartWait:
                    if (!ExtStartWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtStartEcho:
                    if (!ExtStartEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtLengthWait:
                    if (!LengthWaitR(serial, out lengthDataFilename))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtLengthEcho:
                    if (!LengthEchoR(serial, lengthDataFilename))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtDataWait:
                    if (!RecieveDataR(serial, lengthDataFilename, out dataFilename))
                    {
                        Console.WriteLine("LOOK HERE: " + Encoding.UTF8.GetString(dataFilename, 0, dataFilename.Length));
                        state = STATE.Delay;
                        continue;
                    }
                    Console.WriteLine("LOOK HERE: " + Encoding.UTF8.GetString(dataFilename, 0, dataFilename.Length));

                    break;
                case STATE.ExtDataEcho:
                    if (!EchoDataR(serial, dataFilename))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.OpenStream:
                    if (openFile)
                    {
                        stream = File.Create(filePath + Encoding.UTF8.GetString(dataFilename, 0, dataFilename.Length));
                        writer = new BinaryWriter(stream, Encoding.UTF8, false);
                        openFile = false;
                        continue;
                    }
                    break;
                case STATE.ExtEndWait:
                    if (!ExtEndtWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ExtEndEcho:
                    if (!ExtEndEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.ChunkStartWait:
                    if (!ChunkStartOrEndMessageWaitR(serial, out var isEnd))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    if (isEnd)
                    {
                        state = STATE.Done;
                        continue;
                    }
                    break;                    
                case STATE.ChunkStartEcho:
                    if (!ChunkStartEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    
                    break;
                case STATE.LengthWait:
                    if (!LengthWaitR(serial, out lengthData))
                    {
                        state = STATE.Delay;
                        continue; // If wrong message, start over
                    }
                    break;
                case STATE.LengthEcho:
                    if (!LengthEchoR(serial, lengthData))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.DataSend:
                    // Recieve data and write to file
                    if (!RecieveDataR(serial, lengthData, out data))
                    {
                        continue;
                    }
                    writer.Write(data, 0, lengthData);
                    break;
                case STATE.ChunkEndWait:
                    if (!ChunkEndWaitR(serial))
                    {
                        state = STATE.Delay;
                        continue; // If wrong message, start over
                    }
                    break;
                case STATE.ChunkEndEcho:
                    if (!ChunkEndEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                case STATE.MessageEndWait:
                    break;
                case STATE.MessagEndEcho:
                    if (!MessageEndEchoR(serial))
                    {
                        state = STATE.Delay;
                        continue;
                    }
                    break;
                    
                case STATE.Done:
                    Console.WriteLine("RECIVER: Recieving Done.");
                    return true;
                default:
                    break;
            }
            Console.WriteLine("RECIEVER: Completed " + state.ToString());
            state++;
        }
    }       
}

