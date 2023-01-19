using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Order2VPos.Core.Common;
using Order2VPos.Core.VPosClient.MasterData;

namespace Order2VPos.Core.VPosClient
{
    public static class VPosCom
    {
        public const string SendDelimiter = "\0";

        const string Secret = "*6H@6TF7bDrCbU-V1.0";
        const int port = 1050;

        static void SendBase64String(ref Socket socket, string text)
        {
            socket.Send(Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)) + SendDelimiter));
        }

        static Socket GetVPosSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(AppSettings.Default.VPosIPAddress), port));
            byte[] bytes = new byte[6];
            socket.Receive(bytes);
            string salt = Encoding.UTF8.GetString(bytes);
            var md5 = MD5.Create();
            socket.Send(md5.ComputeHash(Encoding.UTF8.GetBytes(Secret + salt)));
            return socket;
        }

        static byte[] GetResponse(Socket socket)
        {
            var buffer = new byte[256];
            int bytesRead;
            List<byte> responseBytes = new List<byte>();
            while ((bytesRead = socket.Receive(buffer)) > 0)
            {
                responseBytes.AddRange(buffer.Take(bytesRead));
            }

            return responseBytes.ToArray();
        }

        public static MasterDataResponse GetMasterData()
        {
            Socket socket = GetVPosSocket();
            SendBase64String(ref socket, "{\"GetMasterData\":1}");

            string jsonText = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(GetResponse(socket))));

            socket.Close();

            return JsonConvert.DeserializeObject<MasterDataResponse>(jsonText);
        }

        public static void SendMessage(string message)
        {
            Socket socket = GetVPosSocket();
            SendBase64String(ref socket, $"{{\"Message\":\"{message}\"}}");
            socket.Close();
        }

        public static async Task<VPosResponse> SendReceipt(Receipt receipt)
        {
            return await Task.Run(() =>
            {
                Socket socket = GetVPosSocket();
                SendBase64String(ref socket, receipt.ToJson());

                string jsonText = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(GetResponse(socket))));

                socket.Close();

                return JsonConvert.DeserializeObject<VPosResponse>(jsonText);
            }
            );
        }
    }
}
