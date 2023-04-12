
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

public static class SerialComms
{
   
    public enum SERIAL_CHARS : byte
    {
        MESSAGE_START = 1, // SOF
        CHUNK_START = 2, // STX
        CHUNK_END = 3, // ETX
        MESSAGE_END = 4, // EOT
        UNKNOWN = 255
    }

    public const int senderDelay = 1000;
    public const int recieverDelay = 1000;





    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                    RECIEVER HELPER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    public static string getFilename(SerialPort serial)
    {
        List<SERIAL_CHARS> expectedCodes = new List<SERIAL_CHARS>();
        expectedCodes.Add(SERIAL_CHARS.CHUNK_START);
        expectedCodes.Add(SERIAL_CHARS.MESSAGE_END);
        // ReceiveCode(serial, expectedCodes, out var receivedcode);
        var lenBytes = new Byte[4];
        var bytesRead = SerialRead(serial, lenBytes, 4);
        var chunkLength = BitConverter.ToInt32(lenBytes, 0);
        var chunkData = new byte[chunkLength];
        bytesRead = SerialRead(serial, chunkData, chunkLength);
        // ReceiveCode(serial, SERIAL_CHARS.CHUNK_END);
        string result = System.Text.Encoding.UTF8.GetString(chunkData);
        return result;
    }


   

   


    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ////////////////////////////////////////////////////////////////////////////////////////////////// SENDER ///////////////////////////////////////////////////////////////////////////////////////////////////
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    
   
    private enum ProcessStateSender
    {
        SendMessageStart,
        WaitMessageStart,
        SendChunkStart,
        WaitChunkStart,
        SendLength,
        WaitLength,
        SendData,
        SendEnd,
        SendChunkEnd,
        WaitChunkEnd,
        SendMessageEnd,
        Done
    }


    public static void SendMessage(String filePath, SerialPort serial, int maxBytesPerChunk)
    {
        Console.WriteLine("Sending Starting!");

        var firstLoop = true;
        while (true) 
        {
            // Wait when Error
            if (!firstLoop)
            {
                Thread.Sleep(senderDelay);
            } else
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
    {Console.WriteLine("");
        try
        {
            Console.WriteLine("SENDER: Sending Message Start Byte!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_START }, 0, 1);
        }
        catch (TimeoutException)
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
        catch (TimeoutException)
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
        catch (TimeoutException)
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
            Console.WriteLine("SENDER: Incorrect Chunk Start byte recieved.");
            return false; 
        }
    }


    public static void SendDataS(SerialPort serial, byte[] data)
    {
        Console.WriteLine("SENDER: Sending Data packet!");
        serial.Write(data, 0, data.Length);
    }
    public static bool ChunkEndSendS(SerialPort serial)
    {
        try
        {
            Console.WriteLine("SENDER: Sending Chunk End!");
            serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_END }, 0, 1);
        }
        catch (TimeoutException)
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
            Console.WriteLine("SENDER: Correct Chunk End byte recieved.");
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
        catch (TimeoutException)
        {
            Console.WriteLine("SENDER: Exception for writing Message End byte.");
            return false;
        }
        return true;
    }
    public static bool MessageEndWaitS(SerialPort serial)
    {
        Console.WriteLine("SENDER: Waiting for Message End byte!");
        try {        
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



    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// ////////////////////////////////////////////////////////////////////////////////////////////////// RECIEVER ///////////////////////////////////////////////////////////////////////////////////////////////////
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    private enum ProcessStateReciever
    {
        WaitMessageStart,
        EchoMessageStart,
        WaitChunkStart,
        EchoChunkStart,
        WaitChunkLength,
        EchoChunkLegth,
        WaitLength,
        EchoLength,
        RecieveData,
        WaitChunkEnd,
        EchoChunkEnd,
        WaitMessageEnd,
        EchoMessageEnd,
        Done
    }

    public static void RecieveMessages(string filePath, SerialPort serial, CancellationToken token)
    {
        Console.WriteLine("File Recieving Starting!");
        int i = 1;

        while (true)
        {
            var filePath2 = filePath + i.ToString() + ".txt"; 
            RecieveData(filePath2, serial);
            i++;
            
            if (token.IsCancellationRequested)
            {
                break;
            }
        }
    }
    public static bool RecieveData(string filePath, SerialPort serial)
    {
        var lengthData = 0;       
        bool firstLoop = true;

        while (true)
        {
            if (!firstLoop)
            {
                Thread.Sleep(recieverDelay);
            }
            else
            {
                firstLoop = false;
            }
            // Wait for Message Start
            if (!MessageStartWaitR(serial))
            {
                continue; // If wrong message, start over
            }
            // Send Echo if correct
            if (!MessageStartEchoR(serial))
            {
                continue; 
            }
            // Create FileStream for writing
            using (var stream = File.Create(filePath))
            {
                // Create Binary Writer to write to stream
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    while (true)
                    {
                        // Wait for Chunk Start
                        if (!ChunkStartOrEndMessageWaitR(serial, out var isEnd))
                        {
                            continue; 
                        }
                        if (isEnd)
                        {
                            break;
                        }
                        // Send echo if correct
                        if (!ChunkStartEchoR(serial))
                        {
                            continue;
                        }                     
                        // Wait for Length
                        if (!LengthWaitR(serial, out lengthData))
                        {
                            continue; // If wrong message, start over
                        }
                        // Send Echo if correct
                        if (!LengthEchoR(serial, lengthData))
                        {
                            continue;
                        }
                        // Recieve data and write to file
                        if (!RecieveDataR(serial, lengthData, out var data))
                        {
                            continue;
                        }
                        writer.Write(data, 0, lengthData);
                        // Wait for Chunk End
                        if (!ChunkEndWaitR(serial))
                        {
                            continue; // If wrong message, start over
                        }
                        // Send Echo if correct
                        if (!ChunkEndEchoR(serial))
                        {
                            continue;
                        }
                    }
                }                       
            }       
            // Echo if correct
            if (!MessageEndEchoR(serial))
            {
                continue;
            }            
            Console.WriteLine("RECIVER: Recieving Done.");
            break;
        }
        return true;          
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
        catch (TimeoutException)
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
        if (data == (int) SERIAL_CHARS.CHUNK_START)
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
        catch (TimeoutException)
        {            
            Console.WriteLine("RECIVER: Exception for Echoing Chunk Start.");            
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
        catch (TimeoutException)
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
        catch (TimeoutException)
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
        catch (TimeoutException)
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




}