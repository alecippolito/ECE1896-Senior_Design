
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;

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

    // SendFile()
    //
    // First, it sends:
    //      [MESSAGE_START] (1 byte)
    // For each chunk (of arbitrary size), it sends:
    //      [CHUNK_START] (1 byte)
    //      <CHUNK_LENGTH> (4 bytes)
    //      <Data>
    //      [CHUNK_END] (1 byte)
    // After all chunks complete, it sends:
    //      [MESSAGE_END] (1 byte)
    public static void SendFile(string fileName, SerialPort serial, int maxBytesPerChunk)
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
                    Console.WriteLine(bytes.GetType());
                    // SERIAL: send [CHUNK_END] byte
                    serial.Write(new byte[] { (byte)SERIAL_CHARS.CHUNK_START }, 0, 1);
                }
            }
        }

        // SERIAL: send [MESSAGE_END] byte
        serial.Write(new byte[] { (byte)SERIAL_CHARS.MESSAGE_END }, 0, 1);
    }

    public static bool ReceiveFile(string fileName, SerialPort serial)
    {
        // SERIAL: receive [MESSAGE_START] byte
        if (!ReceiveCode(serial, SERIAL_CHARS.MESSAGE_START))
        {
            return false;
        }

        // FILE: create stream for writing
        using (var stream = File.Create(fileName))
        {
            // FILE: create binary writer to write to stream
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
            {
                while (true)
                {
                    // SERIAL: receive either [CHUNK_START] or [MESSAGE_END] byte
                    // Add  expeted codes into list to pass in
                    List<SERIAL_CHARS> expectedCodes = new List<SERIAL_CHARS>();
                    expectedCodes.Add(SERIAL_CHARS.CHUNK_START);
                    expectedCodes.Add(SERIAL_CHARS.MESSAGE_END);
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
                    var bytesRead = serial.Read(lenBytes, 0, 4);
                    if (bytesRead != 4) // if error reading
                    {
                        Console.WriteLine("Error: Expecting 4 bytes for the chunk length, but got " + bytesRead.ToString() + " instead.");
                        return false;
                    }
                    var chunkLength = BitConverter.ToInt32(lenBytes, 0);

                    // SERIAL: read chunk
                    var chunkData = new byte[chunkLength];
                    bytesRead = serial.Read(chunkData, 0, chunkLength);
                    if (bytesRead != chunkLength) // if error reading
                    {
                        Console.WriteLine("Error: Expecting " + chunkLength.ToString() + " bytes of data, but got " + bytesRead.ToString() + " instead.");
                        return false;
                    }

                    // FILE: Write chunk
                    writer.Write(chunkData, 0, chunkLength);

                    // SERIAL: read [CHUNK_END] byte
                    if (!ReceiveCode(serial, SERIAL_CHARS.CHUNK_END))
                    {
                        return true;
                    }

                }
            }
        }

        return true;
    }

    // recieve message start
    private static bool ReceiveCode(SerialPort serial, SERIAL_CHARS expectedCode)
    {
        var b = serial.ReadByte();

        Console.WriteLine(b);
        if (b == -1) // stream was closed
        {
            Console.WriteLine("Error: Expecting code " + expectedCode.ToString() + ", but stream was closed.");
            return false;
        }
        if (!Enum.IsDefined(typeof(SERIAL_CHARS), (SERIAL_CHARS)b)) // if b isn't a valid code
        {
            Console.WriteLine("Error: Expecting code " + expectedCode.ToString() + ", but received a non-code value of " + b.ToString() + " instead.");
            return false;
        }
        var receivedcode = (SERIAL_CHARS)b;
        if (expectedCode != receivedcode) // if received code is not the expected code
        {
            Console.WriteLine("Error: Expecting code " + expectedCode.ToString() + ", but got " + receivedcode.ToString() + " instead.");
            return false;
        }
        return true;
    }

    // reecieve message end
    private static bool ReceiveCode(SerialPort serial, List<SERIAL_CHARS> expectedCodes, out SERIAL_CHARS receivedCode)
    {
        var b = serial.ReadByte();
        Console.WriteLine(b);
        if (b == -1) // stream was closed
        {
            Console.WriteLine("Error: Expecting code, but stream was closed.");
            receivedCode = SERIAL_CHARS.UNKNOWN;
            return false;
        }
        if (!Enum.IsDefined(typeof(SERIAL_CHARS), (SERIAL_CHARS)b)) // if b isn't a valid code
        {
            Console.WriteLine("Error: Expecting code, but received a non-code value of " + b.ToString() + " instead.");
            receivedCode = SERIAL_CHARS.UNKNOWN;
            return false;
        }
        receivedCode = (SERIAL_CHARS)b;
        if (!expectedCodes.Contains(receivedCode))  // if received code is not one of the expected codes
        {
            Console.WriteLine("Error: Received unexpected code of " + receivedCode.ToString() + ".");
            return false;
        }
        return true;
    }
}

// hamming code


