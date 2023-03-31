
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

public static class SerialComms
{
    /*
     * How sending works:
     *      Writes [MESSAGE START] (1 bytes)
     *          Sends filename and extension
     *         * Opens up File and Binary Reader *
     *          For each chunk (of maxBytesPerChunk size), it sends:
     *              [CHUNK_START] (1 byte)
     *              <CHUNK_LENGTH> (4 bytes)
     *              <CHUNK DATA>
     *              [CHUNK_END] (1 byte)
     *      After all chunks are sent,
     *          [MESSAGE_EN] (1 byte)
     *          
     *          
     *          
     * How recieving works:
     *      Checks first thing sent is [MESSAGE_START]
     *      * Opens up File and Binary Reader *
     *      Makes sure code is [CHUNK_START] or [MESSAGE_END]
     *      If [MESSAGE_END], break loop
     *      Does Error Handling: if bytesRead != 4, if bytesRead != chunkLength, if last byte = [CHUNK_END]
     *      Writes chunk data to fileName      
     * 
     */

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                        SEND AND RECIEVE FUNCTIONS (INCLUDING ERROR DETECTION)
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    public enum SERIAL_CHARS : byte
    {
        MESSAGE_START = 1, // SOF
        CHUNK_START = 2, // STX
        CHUNK_END = 3, // ETX
        MESSAGE_END = 4, // EOT
        UNKNOWN = 255
    }

    

    public static void Send(string filePath, SerialPort serial, int maxBytesPerChunk)
    {
        var totalBytes = 0;
        // Send Start Byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_START }, 0, 1);

        sendFileName(filePath, serial);
        // FILE: open stream for reading
        using (var stream = File.Open(filePath, FileMode.Open))
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
                    totalBytes += sendBytes(serial, bytes);

                    

                    Console.WriteLine("Send TotalBytes: " + totalBytes);
                }
            }
        }
        Console.WriteLine("Send done.");

        // SERIAL: send [MESSAGE_END] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_END }, 0, 1);
    }

    public static bool Receive(string filePath, SerialPort serial)
    {
        var totalBytes = 0;

        // SERIAL: receive [MESSAGE_START] byte
        if (!ReceiveCode(serial, SERIAL_CHARS.MESSAGE_START))
        {
            return false;
        }

        // Get Filename and add to path
        string fileName = getFilename(serial);
        filePath = filePath + fileName;
        bool func;
        // FILE: create stream for writing
        using (var stream = File.Create(filePath))
        {
            // FILE: create binary writer to write to stream
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                func = readBytes(serial, writer, totalBytes);
            }
        }
        Console.WriteLine("Receive Done.");
        return func;
    }


    public static void SendWithErrorCorrection(string filePath, SerialPort serial, int maxBytesPerChunk)
    {
        var totalBytes = 0;
        // Seend start Byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_START }, 0, 1);

        // Read in Filename with ECs
        sendFileNameeEC(filePath, serial);

        // FILE: open stream for reading
        using (var stream = File.Open(filePath, FileMode.Open))
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
                    totalBytes += sendBytesEC(serial, bytes);

                    Console.WriteLine("Send TotalBytes: " + totalBytes);
                }
            }
        }
        Console.WriteLine("Send done.");

        // SERIAL: send [MESSAGE_END] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_END }, 0, 1);
    }

    public static bool ReceiveWithErrrorCorrection(string filePath, SerialPort serial)
    {
        var totalBytes = 0;
        // SERIAL: receive [MESSAGE_START] byte
        if (!ReceiveCode(serial, SERIAL_CHARS.MESSAGE_START))
        {
            return false;
        }
        // Get Filename and add to path
        string fileName = getFilenameEC(serial);
        filePath = filePath + fileName;
        bool res;
        // FILE: create stream for writing
        using (var stream = File.Create(filePath))
        {
            // FILE: create binary writer to write to stream
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                res = readBytesEC(serial, writer, totalBytes);
            }
        }
        Console.WriteLine("Receive Done.");
        return res;
    }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                    SENDER HELPER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    public static void sendFileName(string filePath, SerialPort serial)
    {
        string fileName = Path.GetFileName(filePath);
        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);
        serial.Write(BitConverter.GetBytes(Encoding.ASCII.GetBytes(fileName).Length), 0, 4);
        serial.Write(Encoding.ASCII.GetBytes(fileName), 0, Encoding.ASCII.GetBytes(fileName).Length);
        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_END }, 0, 1);
    }
    public static int sendBytes(SerialPort serial, byte[] bytes)
    {
        // SERIAL: send [CHUNK_START] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);

        // SERIAL: send length of chunk (as 4 byte integer)             
        serial.Write(BitConverter.GetBytes(bytes.Length), 0, 4);

        // SERIAL: send data
        serial.Write(bytes, 0, bytes.Length);

        // SERIAL: send [CHUNK_END] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_END }, 0, 1);

        return bytes.Length;
    }


    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                    RECIEVER HELPER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    public static string getFilename(SerialPort serial)
    {
        List<SERIAL_CHARS> expectedCodes = new List<SERIAL_CHARS>();
        expectedCodes.Add(SERIAL_CHARS.CHUNK_START);
        expectedCodes.Add(SERIAL_CHARS.MESSAGE_END);
        ReceiveCode(serial, expectedCodes, out var receivedcode);
        var lenBytes = new Byte[4];
        var bytesRead = SerialRead(serial, lenBytes, 4);
        var chunkLength = BitConverter.ToInt32(lenBytes, 0);
        var chunkData = new byte[chunkLength];
        bytesRead = SerialRead(serial, chunkData, chunkLength);
        ReceiveCode(serial, SERIAL_CHARS.CHUNK_END);
        string result = System.Text.Encoding.UTF8.GetString(chunkData);
        return result;
    }
    public static bool readBytes(SerialPort serial, BinaryWriter writer, int totalBytes)
    {
        while (true)
        {
            // SERIAL: receive either [CHUNK_START] or [MESSAGE_END] byte
            // Add  expeted codes into list to pass in
            List<SERIAL_CHARS> expectedCodes = new List<SERIAL_CHARS>();
            expectedCodes.Add(SERIAL_CHARS.CHUNK_START);
            expectedCodes.Add(SERIAL_CHARS.MESSAGE_END);
            // MESSAGE END?
            if (!ReceiveCode(serial, expectedCodes, out var receivedcode))
            {
                return false;
            }

            if (receivedcode == SERIAL_CHARS.MESSAGE_END) // if no more chunks, exit loop
            {
                break;
            }

            // SERIAL: receive length of chunk (as 4 byte integer)
            var lenBytes = new Byte[4];
            var bytesRead = SerialRead(serial, lenBytes, 4);

            if (bytesRead != 4) // if error reading
            {
                Console.WriteLine("Error 8: Expecting 4 bytes for the chunk length, but got " + bytesRead.ToString() + " instead.");
                return false;
            }
            var chunkLength = BitConverter.ToInt32(lenBytes, 0);

            // SERIAL: read chunk
            var chunkData = new byte[chunkLength];

            bytesRead = SerialRead(serial, chunkData, chunkLength);
            if (bytesRead != chunkLength) // if error reading
            {
                Console.WriteLine("Error 7: Expecting " + chunkLength.ToString() + " bytes of data, but got " + bytesRead.ToString() + " instead.");
                return false;
            }
            totalBytes += bytesRead;

            // FILE: Write chunk
            writer.Write(chunkData, 0, chunkLength);

            Console.WriteLine("Receive TotalBytes: " + totalBytes);

            // SERIAL: read [CHUNK_END] byte
            if (!ReceiveCode(serial, SERIAL_CHARS.CHUNK_END))
            {
                return false;
            }

        }
        return true;
    }
    

    private static int SerialRead(SerialPort serial, byte[] data, int dataLength)
    {
        var bytesRead = 0;
        while (bytesRead < dataLength)
        {
            var b = serial.Read(data, bytesRead, dataLength - bytesRead);
            if ( b < 0)
            {
                return b;
            }
            bytesRead += b;
        }
        return bytesRead;
    }

    private static bool ReceiveCode(SerialPort serial, SERIAL_CHARS expectedCode)
    {
        var b = serial.ReadByte();
        if (b == -1) // stream was closed
        {
            Console.WriteLine("Error 1: Expecting code " + expectedCode.ToString() + ", but stream was closed.");
            return false;
        }
        if (!Enum.IsDefined(typeof(SERIAL_CHARS), (SERIAL_CHARS)b)) // if b isn't a valid code
        {
            Console.WriteLine("Error 2: Expecting code " + expectedCode.ToString() + ", but received a non-code value of " + b.ToString() + " instead.");
            return false;
        }
        var receivedcode = (SERIAL_CHARS)b;
        

        if (expectedCode != receivedcode) // if received code is not the expected code
        {
            Console.WriteLine("EXPECTED: " + expectedCode);
            Console.WriteLine("RECIEVED: " + receivedcode);
            Console.WriteLine("Error 3: Expecting code " + expectedCode.ToString() + ", but got " + receivedcode.ToString() + " instead.");
            return false;
        }
        return true;
    }

    private static bool ReceiveCode(SerialPort serial, List<SERIAL_CHARS> expectedCodes, out SERIAL_CHARS receivedCode)
    {
        var b = serial.ReadByte();
        if (b == -1) // stream was closed
        {
            Console.WriteLine("Error 4: Expecting code, but stream was closed.");
            receivedCode = SERIAL_CHARS.UNKNOWN;
            return false;
        }
        if (!Enum.IsDefined(typeof(SERIAL_CHARS), (SERIAL_CHARS)b)) // if b isn't a valid code
        {
            Console.WriteLine("Error 5: Expecting code, but received a non-code value of " + b.ToString() + " instead.");
            receivedCode = SERIAL_CHARS.UNKNOWN;
            return false;
        }
        receivedCode = (SERIAL_CHARS)b;
        if (!expectedCodes.Contains(receivedCode))  // if received code is not one of the expected codes
        {
            Console.WriteLine("Error 6: Received unexpected code of " + receivedCode.ToString() + ".");
            return false;
        }
        return true;
    }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                        SENDER ERROR CORRECTION HELPER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    public static void sendFileNameeEC(string filePath, SerialPort serial)
    {
        sendFileName(filePath, serial);
        sendFileName(filePath, serial);
        sendFileName(filePath, serial);
    }

    public static int sendBytesEC(SerialPort serial, byte[] bytes)
    {
        // SERIAL: send [CHUNK_START] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);

        // SERIAL: send length of chunk (as 4 byte integer)             
        serial.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
        for (int i = 0; i < bytes.Length; i++)
        {
            // SERIAL: send data
            serial.Write(bytes, i, 1);
            serial.Write(bytes, i, 1);
            serial.Write(bytes, i, 1);
        }

        // SERIAL: send [CHUNK_END] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_END }, 0, 1);

        return bytes.Length;
    }

    public static string getFilenameEC(SerialPort serial)
    {
        string fn1 = getFilename(serial);
        string fn2 = getFilename(serial);
        string fn3 = getFilename(serial);
        // all the same case
        if (String.Compare(fn1, fn2) == 0 && String.Compare(fn2, fn3) == 0)
        {
            return fn1;
        }
        // 1 and 2 are the same
        else if (String.Compare(fn1, fn2) == 0)
        {
            return fn1;
        }
        // 2 and 3 are the same
        else if (String.Compare(fn2, fn3) == 0)
        {
            return fn2;
        }
        // 1 and 3 are the same
        else if (String.Compare(fn1, fn3) == 0)
        {
            return fn1;
        }
        else
        {
            return fn1;
        }
    }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                        RECIEVER ERROR CORRECTION HELPER FUNCTIONS
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    private static byte chooseByteEC(char d1, char d2, char d3)
    {
        // all same
        if(d1.Equals(d2) == true && d2.Equals(d3) == true)
        {
            return Convert.ToByte(d1);
        } 
        // 1 and 2 are same
        else if (d1.Equals(d2) == true)
        {
            return Convert.ToByte(d1);
        }
        // 2 and 3 are the same
        else if (d2.Equals(d3) == true)
        {
            return Convert.ToByte(d2);
        }
        // 1 and 3 are the same
        else if (d1.Equals(d3) == true)
        {
            return Convert.ToByte(d1);
        }
        // none are are same
        else
        {
            return Convert.ToByte(d1);
        }
    }


    public static bool readBytesEC(SerialPort serial, BinaryWriter writer, int totalBytes)
    {
        var lenBytes = new Byte[4];

        while (true)
        {
            
            // SERIAL: receive either [CHUNK_START] or [MESSAGE_END] byte
            // Add  expeted codes into list to pass in
            List<SERIAL_CHARS> expectedCodes = new List<SERIAL_CHARS>();
            expectedCodes.Add(SERIAL_CHARS.CHUNK_START);
            expectedCodes.Add(SERIAL_CHARS.MESSAGE_END);

            // MESSAGE END?
            if (!ReceiveCode(serial, expectedCodes, out var receivedcode))
            {
                return false;
            }

            if (receivedcode == SERIAL_CHARS.MESSAGE_END) // if no more chunks, exit loop
            {
                break;
            }

            // SERIAL: receive length of chunk (as 4 byte integer)
            var bytesRead = SerialRead(serial, lenBytes, 4);


            if (bytesRead != 4) // if error reading
            {
                Console.WriteLine("Error 8: Expecting 4 bytes for the chunk length, but got " + bytesRead.ToString() + " instead.");
                return false;
            }
            var chunkLength = BitConverter.ToInt32(lenBytes, 0);

            // SERIAL: read chunk
            var chunkData = new byte[chunkLength];

            SerialReadEC(serial, chunkData, chunkLength);



            // SERIAL: read [CHUNK_END] byte
            if (!ReceiveCode(serial, SERIAL_CHARS.CHUNK_END))
            {
                return false;
            }

            totalBytes += bytesRead;
            // FILE: Write chunk
            writer.Write(chunkData, 0, chunkLength);

            Console.WriteLine("Receive TotalBytes: " + totalBytes);

        }
        return true;
    }

    private static void SerialReadEC(SerialPort serial, byte[] data, int dataLength)
    {
        for (int i = 0; i < dataLength; i++)
        {
            data[i] = SerialReadByteEC(serial);
        }
    }

    private static byte SerialReadByteEC(SerialPort serial)
    {
        var data = new byte[3];
        var bytesRead = 0;
        while (bytesRead < 3)
        {
            var b = serial.Read(data, bytesRead, 3 - bytesRead);
            bytesRead += b;

        }
        char d1 = Convert.ToChar(data[0]);
        char d2 = Convert.ToChar(data[1]);
        char d3 = Convert.ToChar(data[2]);
        // error check
        return chooseByteEC(d1, d2, d3);
    }
    













}





