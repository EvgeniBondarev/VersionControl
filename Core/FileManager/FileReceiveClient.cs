using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace Core.FileManager
{
    public class FileReceiveClient : IFileTransfer
    {
        private readonly string _serverAddress;
        private readonly int _port;
        private static readonly object fileLock = new object();

        public FileReceiveClient(string serverAddress, int port)
        {
            _serverAddress = serverAddress;
            _port = port;
        }

        public void ReceiveDirectory(string destinationDirectory, string repositoryName, int? versionNumber = null)
        {
            using TcpClient client = new TcpClient(_serverAddress, _port);
            using NetworkStream stream = client.GetStream();

            try
            {
                // Отправляем имя репозитория
                byte[] repositoryNameBytes = Encoding.UTF8.GetBytes(repositoryName);
                stream.Write(BitConverter.GetBytes(repositoryNameBytes.Length), 0, 4);
                stream.Write(repositoryNameBytes, 0, repositoryNameBytes.Length);

                // Отправляем номер версии (если он указан)
                byte[] versionBytes = BitConverter.GetBytes(versionNumber.HasValue ? versionNumber.Value : -1);
                stream.Write(versionBytes, 0, 4);

                // Получаем архив и извлекаем его
                ReceiveFile(stream, destinationDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении файла: " + ex.Message);
            }
        }


        private void ReceiveFile(NetworkStream stream, string destinationDirectory)
        {
            byte[] nameSizeBytes = new byte[4];
            stream.Read(nameSizeBytes, 0, 4);
            int nameSize = BitConverter.ToInt32(nameSizeBytes, 0);

            byte[] nameBytes = new byte[nameSize];
            stream.Read(nameBytes, 0, nameSize);
            string fileName = Encoding.UTF8.GetString(nameBytes);

            byte[] fileSizeBytes = new byte[4];
            stream.Read(fileSizeBytes, 0, 4);
            int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

            byte[] fileData = new byte[fileSize];
            int totalRead = 0;
            while (totalRead < fileSize)
            {
                totalRead += stream.Read(fileData, totalRead, fileSize - totalRead);
            }

            lock (fileLock)
            {
                string receivedFilePath = Path.Combine(destinationDirectory, fileName);
                using (FileStream fs = new FileStream(receivedFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(fileData, 0, fileData.Length);
                }

                Console.WriteLine($"Файл {fileName} получен и сохранен в {destinationDirectory}");
                ExtractFile(receivedFilePath, destinationDirectory);
            }
        }

        private void ExtractFile(string zipFilePath, string destinationDirectory)
        {
            string tempDirectory = Path.Combine(destinationDirectory, "TempExtract");
            Directory.CreateDirectory(tempDirectory);

            lock (fileLock)
            {
                // Извлекаем содержимое архива во временную директорию
                ZipFile.ExtractToDirectory(zipFilePath, tempDirectory);
            }

            Console.WriteLine($"Файл {zipFilePath} успешно разархивирован во временную директорию {tempDirectory}");

            lock (fileLock)
            {
                foreach (string filePath in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(tempDirectory, filePath);
                    string destinationPath = Path.Combine(destinationDirectory, relativePath);

                    // Создаем директории и копируем файлы
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                    File.Copy(filePath, destinationPath, true);
                }

                Directory.Delete(tempDirectory, true);
                File.Delete(zipFilePath);
                Console.WriteLine($"Файл {zipFilePath} успешно разархивирован и сохранен в {destinationDirectory}");
            }
        }

    }
}
