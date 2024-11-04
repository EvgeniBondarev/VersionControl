using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core.FileManager
{
    public class FileTransferServer : IFileTransfer
    {
        private readonly int _port;
        private readonly string _destinationDirectory;

        public FileTransferServer(int port, string destinationDirectory)
        {
            _port = port;
            _destinationDirectory = destinationDirectory;
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            Console.WriteLine($"Сервер запущен и слушает порт {_port}...");

            while (true)
            {
                using TcpClient client = listener.AcceptTcpClient();
                using NetworkStream stream = client.GetStream();

                try
                {
                    // Получаем тип запроса от клиента
                    byte[] requestTypeBytes = new byte[4];
                    stream.Read(requestTypeBytes, 0, 4);
                    int requestType = BitConverter.ToInt32(requestTypeBytes, 0);

                    if (requestType == 1) // Запрос на отправку директории
                    {
                        SendDirectory(stream);
                    }
                    else // Запрос на получение файла
                    {
                        ReceiveFile(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обработке запроса: " + ex.Message);
                }
            }
        }

        private void ReceiveFile(NetworkStream stream)
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

            // Сохраняем и разархивируем файл
            string receivedFilePath = Path.Combine(_destinationDirectory, fileName);
            File.WriteAllBytes(receivedFilePath, fileData);
            Console.WriteLine($"Файл {fileName} получен и сохранен в {_destinationDirectory}");

            ExtractFile(receivedFilePath);
        }

        private void SendDirectory(NetworkStream stream)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), "requested_directory.zip");

            try
            {
                // Архивируем папку
                ZipFile.CreateFromDirectory(_destinationDirectory, zipFilePath);
                Console.WriteLine($"Директория {_destinationDirectory} заархивирована для отправки.");

                // Отправляем архивированный файл клиенту
                SendFile(stream, zipFilePath, "requested_directory.zip");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке директории: " + ex.Message);
            }
            finally
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
            }
        }

        private void SendFile(NetworkStream stream, string filePath, string fileName)
        {
            // Отправляем имя файла
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
            stream.Write(BitConverter.GetBytes(fileNameBytes.Length), 0, 4);
            stream.Write(fileNameBytes, 0, fileNameBytes.Length);

            // Отправляем содержимое файла
            byte[] fileData = File.ReadAllBytes(filePath);
            stream.Write(BitConverter.GetBytes(fileData.Length), 0, 4);
            stream.Write(fileData, 0, fileData.Length);

            Console.WriteLine($"Файл {fileName} успешно отправлен клиенту.");
        }

        private void ExtractFile(string zipFilePath)
        {
            // Извлечение файла аналогично вашему коду
            string tempDirectory = Path.Combine(_destinationDirectory, "TempExtract");
            Directory.CreateDirectory(tempDirectory);
            ZipFile.ExtractToDirectory(zipFilePath, tempDirectory);
            Console.WriteLine($"Файл {zipFilePath} успешно разархивирован в {tempDirectory}");

            foreach (string filePath in Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(tempDirectory, filePath);
                string destinationPath = Path.Combine(_destinationDirectory, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                File.Copy(filePath, destinationPath, overwrite: true);
            }

            Directory.Delete(tempDirectory, recursive: true);
            File.Delete(zipFilePath);
            Console.WriteLine($"Файл {zipFilePath} успешно разархивирован и заменён в {_destinationDirectory}");
        }
    }
}
