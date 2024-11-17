using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommentManager
{
    public class CommentClient
    {
        private readonly string _serverAddress;
        private readonly int _port;

        public CommentClient(string serverAddress, int port)
        {
            _serverAddress = serverAddress;
            _port = port;
        }

        // Отправить комментарий
        public void SendComment(string repositoryName, string comment, string username = "defaultUser")
        {
            try
            {
                using TcpClient client = new TcpClient(_serverAddress, _port);
                using NetworkStream stream = client.GetStream();

                // Отправка команды для отправки комментария
                stream.WriteByte(1);

                // Отправка данных
                SendString(stream, repositoryName);
                SendString(stream, comment);
                SendString(stream, username);

                Console.WriteLine($"Комментарий для репозитория '{repositoryName}' отправлен пользователем '{username}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке комментария: " + ex.Message);
            }
        }

        // Получить комментарии для указанного репозитория
        public List<string> GetComments(string repositoryName)
        {
            List<string> comments = new List<string>();
            try
            {
                
                using TcpClient client = new TcpClient(_serverAddress, _port);
                using NetworkStream stream = client.GetStream();

                stream.WriteByte(2);

                SendString(stream, repositoryName);

                int commentsCount = ReceiveInt(stream);

                for (int i = 0; i < commentsCount; i++)
                {
                    string date = ReceiveString(stream);
                    string username = ReceiveString(stream);
                    string comment = ReceiveString(stream);
                    comments.Add($"[{date}] {username}: {comment}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении комментариев: " + ex.Message);
            }
            return comments;
        }

        private void SendString(NetworkStream stream, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            stream.Write(BitConverter.GetBytes(dataBytes.Length), 0, 4);
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        private string ReceiveString(NetworkStream stream)
        {
            byte[] sizeBytes = new byte[4];
            stream.Read(sizeBytes, 0, 4);
            int size = BitConverter.ToInt32(sizeBytes, 0);

            byte[] dataBytes = new byte[size];
            stream.Read(dataBytes, 0, size);
            return Encoding.UTF8.GetString(dataBytes);
        }

        private int ReceiveInt(NetworkStream stream)
        {
            byte[] intBytes = new byte[4];
            stream.Read(intBytes, 0, 4);
            return BitConverter.ToInt32(intBytes, 0);
        }
    }

}
