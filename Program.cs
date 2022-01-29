using System;
using System.Net.Sockets;
using System.Threading;

namespace Analizator
{
    class Program
    {
        static TcpClient client;
        private const int READ_BUFFER_SIZE = 2048;
        // wielkość odczytującego bufora

        private static byte[] bufferReceiver = new byte[READ_BUFFER_SIZE];
        // inicjalizacja odczytującego bufora

        private static string IP = "156.17.41.18";  // adres analizatora
        private static int Port = 502;              // port dla protokołu modbus

        static void Connect()
        {
            client = new TcpClient(IP, Port);
        }
        static void Disconnect()
        {
            if (client == null) return;
            if (client.Connected)
            {
                client.Close();
                Console.Write("Is Disconnected.");
            }
        }

        public static void Write(byte[] frame)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(frame, 0, 12);
        }

        public static void Read()
        {
            NetworkStream stream = client.GetStream();
            if (stream.CanRead)
            {
                stream.Read(bufferReceiver, 0, READ_BUFFER_SIZE);
            }
        }

        static byte[] readRegister(ushort registerAddress)
        {
            ushort transactionID = 0x00_22;
            ushort protocolID = 0x00_00;
            ushort length = 0x00_06;
            byte unitID = 0x01;
            byte functionCode = 0x03;
            ushort lengthOfRegisters = 0x00_02;

            byte[] frame = new byte[12];
            frame[0] = (byte)(transactionID >> 8);
            frame[1] = (byte)transactionID;

            frame[2] = (byte)(protocolID >> 8);
            frame[3] = (byte)protocolID;

            frame[4] = (byte)(length >> 8);
            frame[5] = (byte)length;

            frame[6] = unitID;

            frame[7] = functionCode;

            frame[8] = (byte)(registerAddress >> 8);
            frame[9] = (byte)registerAddress;

            frame[10] = (byte)(lengthOfRegisters >> 8);
            frame[11] = (byte)lengthOfRegisters;

            Write(frame);
            Thread.Sleep(100);
            Read();
            byte[] received = { bufferReceiver[12], bufferReceiver[11], bufferReceiver[10], bufferReceiver[9] };

            return received;
        }

        static void Main(string[] args)
        {
            Connect();
            if (client.Connected) Console.WriteLine("Is connected.");

            Console.Write("Napiecie: ");
            var napiecie = BitConverter.ToUInt32(readRegister(0x10_02), 0) / 1000.0;
            Console.WriteLine(napiecie + " V");

            Console.Write("Natezenie: ");
            var natezenie = BitConverter.ToUInt32(readRegister(0x10_10), 0) / 1000.0;
            Console.WriteLine(natezenie + " A");

            Console.Write("moc aktywna: ");
            var mocAktywna = BitConverter.ToInt32(readRegister(0x10_30), 0) / 1000.0;
            Console.WriteLine(mocAktywna + " W");

            Console.Write("moc bierna: ");
            var mocBierna = BitConverter.ToInt32(readRegister(0x10_38), 0) / 1000.0;
            Console.WriteLine(mocBierna + " VAR");

            Console.Write("Cosinus: ");
            Console.WriteLine(BitConverter.ToInt32(readRegister(0x10_20), 0) / 1000.0);

            Console.Write("Moc pozorna wyliczona U * I :");
            Console.WriteLine((napiecie * natezenie) + "VA");

            Console.Write("Moc pozorna wyliczona pitagoras:");
            Console.WriteLine(Math.Pow((Math.Pow(mocAktywna, 2.0) + Math.Pow(mocBierna, 2.0)), 0.5) * 1000 + "VA");

            Disconnect();
            Console.ReadKey();
        }
    }
}
