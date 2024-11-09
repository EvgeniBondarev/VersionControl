using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Core.FileManager
{
    public class FileTransferClient : IFileTransfer
    {
        private readonly string _serverAddress;
        private readonly int _port;
        private static readonly object fileLock = new object();

        public FileTransferClient(string serverAddress, int port)
        {
            _serverAddress = serverAddress;
            _port = port;
        }

        public void SendDirectory(string directoryPath, string repositoryName)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                // Если путь не передан, отправляем только имя репозитория
                try
                {
                    SendEmptyRepository(repositoryName);
                    Console.WriteLine($"Папка {repositoryName} создана на сервере без файлов.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при передаче репозитория: " + ex.Message);
                }
            }
            else
            {
                string zipFilePath = Path.Combine(Path.GetTempPath(), "archive.zip");

                try
                {
                    lock (fileLock)
                    {
                        // Архивируем папку
                        ZipFile.CreateFromDirectory(directoryPath, zipFilePath);
                        Console.WriteLine($"Папка {directoryPath} заархивирована в {zipFilePath}");

                        // Отправляем архив на сервер вместе с именем репозитория
                        SendFile(zipFilePath, "archive.zip", repositoryName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при передаче файла: " + ex.Message);
                }
                finally
                {
                    // Потокобезопасное удаление временного файла
                    lock (fileLock)
                    {
                        if (File.Exists(zipFilePath))
                        {
                            File.Delete(zipFilePath);
                        }
                    }
                }
            }
        }

        private void SendFile(string filePath, string fileName, string repositoryName)
        {
            using TcpClient client = new TcpClient(_serverAddress, _port);
            using NetworkStream stream = client.GetStream();

            byte[] repoNameBytes = Encoding.UTF8.GetBytes(repositoryName);
            stream.Write(BitConverter.GetBytes(repoNameBytes.Length), 0, 4);
            stream.Write(repoNameBytes, 0, repoNameBytes.Length);

            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            stream.Write(BitConverter.GetBytes(fileNameBytes.Length), 0, 4);
            stream.Write(fileNameBytes, 0, fileNameBytes.Length);

            byte[] fileData = File.ReadAllBytes(filePath);
            stream.Write(BitConverter.GetBytes(fileData.Length), 0, 4);
            stream.Write(fileData, 0, fileData.Length);

            Console.WriteLine("Файл успешно отправлен на сервер.");
        }

        private void SendEmptyRepository(string repositoryName)
        {
            using TcpClient client = new TcpClient(_serverAddress, _port);
            using NetworkStream stream = client.GetStream();

            byte[] repoNameBytes = Encoding.UTF8.GetBytes(repositoryName);
            stream.Write(BitConverter.GetBytes(repoNameBytes.Length), 0, 4);
            stream.Write(repoNameBytes, 0, repoNameBytes.Length);

            byte[] fileNameBytes = Encoding.UTF8.GetBytes(string.Empty);  // Отправляем пустое имя файла
            stream.Write(BitConverter.GetBytes(fileNameBytes.Length), 0, 4);
            stream.Write(fileNameBytes, 0, fileNameBytes.Length);

            byte[] fileData = new byte[0];  // Пустой файл
            stream.Write(BitConverter.GetBytes(fileData.Length), 0, 4);
            stream.Write(fileData, 0, fileData.Length);

            Console.WriteLine("Пустой репозиторий успешно отправлен на сервер.");
        }
    }
}
