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

        public void RequestDirectory(string savePath)
        {
            using TcpClient client = new TcpClient(_serverAddress, _port);
            using NetworkStream stream = client.GetStream();

            // Отправляем запрос на получение директории (код 1)
            stream.Write(BitConverter.GetBytes(1), 0, 4);

            // Получаем ZIP файл от сервера
            ReceiveFile(stream, savePath);
        }

        public void SendDirectory(string directoryPath)
        {
            // Код отправки директории остается без изменений
            string zipFilePath = Path.Combine(Path.GetTempPath(), "archive.zip");
            try
            {
                ZipFile.CreateFromDirectory(directoryPath, zipFilePath);
                Console.WriteLine($"Папка {directoryPath} заархивирована в {zipFilePath}");
                SendFile(zipFilePath, "archive.zip");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при передаче файла: " + ex.Message);
            }
            finally
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        private void SendFile(string filePath, string fileName)
        {
            using TcpClient client = new TcpClient(_serverAddress, _port);
            using NetworkStream stream = client.GetStream();

            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            stream.Write(BitConverter.GetBytes(fileNameBytes.Length), 0, 4);
            stream.Write(fileNameBytes, 0, fileNameBytes.Length);

            byte[] fileData = File.ReadAllBytes(filePath);
            stream.Write(BitConverter.GetBytes(fileData.Length), 0, 4);
            stream.Write(fileData, 0, fileData.Length);

            Console.WriteLine("Файл успешно отправлен на сервер.");
        }

        private void ReceiveFile(NetworkStream stream, string savePath)
        {
            // Получаем имя файла
            byte[] nameSizeBytes = new byte[4];
            stream.Read(nameSizeBytes, 0, 4);
            int nameSize = BitConverter.ToInt32(nameSizeBytes, 0);

            byte[] nameBytes = new byte[nameSize];
            stream.Read(nameBytes, 0, nameSize);
            string fileName = Encoding.UTF8.GetString(nameBytes);

            // Получаем содержимое файла
            byte[] fileSizeBytes = new byte[4];
            stream.Read(fileSizeBytes, 0, 4);
            int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

            byte[] fileData = new byte[fileSize];
            int totalRead = 0;
            while (totalRead < fileSize)
            {
                totalRead += stream.Read(fileData, totalRead, fileSize - totalRead);
            }

            // Сохраняем ZIP файл
            string zipFilePath = Path.Combine(savePath, fileName);
            File.WriteAllBytes(zipFilePath, fileData);
            Console.WriteLine($"Файл {fileName} получен и сохранен в {zipFilePath}");

            // Разархивируем файл с заменой
            ExtractFile(zipFilePath, savePath);
        }

        private void ExtractFile(string zipFilePath, string destinationDirectory)
        {
            // Создаем временную директорию для извлечения
            string tempDirectory = Path.Combine(Path.GetTempPath(), "TempExtract");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                // Извлекаем содержимое ZIP файла во временную директорию
                ZipFile.ExtractToDirectory(zipFilePath, tempDirectory);
                Console.WriteLine($"Файл {zipFilePath} успешно разархивирован во временную директорию {tempDirectory}");

                // Копируем файлы из временной директории в целевую с заменой
                foreach (string filePath in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(tempDirectory, filePath);
                    string destinationPath = Path.Combine(destinationDirectory, relativePath);

                    // Создаем директорию, если она не существует
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                    // Копируем файл с заменой, если он существует
                    File.Copy(filePath, destinationPath, overwrite: true);
                    Console.WriteLine($"Файл {filePath} успешно скопирован в {destinationPath}");
                }
            }
            finally
            {
                // Удаляем временную директорию и ZIP файл
                Directory.Delete(tempDirectory, recursive: true);
                File.Delete(zipFilePath);
                Console.WriteLine($"Временные файлы удалены: {zipFilePath} и {tempDirectory}");
            }
        }
    }
}
