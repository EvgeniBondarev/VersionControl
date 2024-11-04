using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace Core.FileManager
{
    public class FileTransferClient : IFileTransfer
    {
        private readonly string _serverAddress;
        private readonly int _port;

        public FileTransferClient(string serverAddress, int port)
        {
            _serverAddress = serverAddress;
            _port = port;
        }

        public void SendDirectory(string directoryPath, string repositoryName)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), "archive.zip");

            try
            {
                // Архивируем папку
                ZipFile.CreateFromDirectory(directoryPath, zipFilePath);
                Console.WriteLine($"Папка {directoryPath} заархивирована в {zipFilePath}");

                // Отправляем архив на сервер вместе с именем репозитория
                SendFile(zipFilePath, "archive.zip", repositoryName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при передаче файла: " + ex.Message);
            }
            finally
            {
                // Удаляем временный ZIP файл
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        private void SendFile(string filePath, string fileName, string repositoryName)
        {
            using TcpClient client = new TcpClient(_serverAddress, _port);
            using NetworkStream stream = client.GetStream();

            // Отправляем имя репозитория
            byte[] repoNameBytes = Encoding.UTF8.GetBytes(repositoryName);
            stream.Write(BitConverter.GetBytes(repoNameBytes.Length), 0, 4);
            stream.Write(repoNameBytes, 0, repoNameBytes.Length);

            // Отправляем имя файла
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            stream.Write(BitConverter.GetBytes(fileNameBytes.Length), 0, 4);
            stream.Write(fileNameBytes, 0, fileNameBytes.Length);

            // Отправляем содержимое файла
            byte[] fileData = File.ReadAllBytes(filePath);
            stream.Write(BitConverter.GetBytes(fileData.Length), 0, 4);
            stream.Write(fileData, 0, fileData.Length);

            Console.WriteLine("Файл успешно отправлен на сервер.");
        }
    }
}
