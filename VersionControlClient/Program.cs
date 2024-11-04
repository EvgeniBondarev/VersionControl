using System.Net.Sockets;
using System.Net;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter command (e.g., ADD <username> <repoName> <folderPath>):");
        string command = Console.ReadLine();
        string[] commandParts = command.Split(' ');

        if (commandParts[0] == "ADD" && commandParts.Length == 4)
        {
            string username = commandParts[1];
            string repoName = commandParts[2];
            string folderPath = commandParts[3];

            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

            foreach (var filePath in Directory.GetFiles(folderPath))
            {
                string fileName = Path.GetFileName(filePath);
                byte[] fileData = File.ReadAllBytes(filePath);
                files.Add(fileName, fileData);
            }

            await SendAddCommand(username, repoName, files);
        }
        else
        {
            Console.WriteLine("Invalid command.");
        }
    }

    private static async Task SendAddCommand(string username, string repoName, Dictionary<string, byte[]> files)
    {
        using (TcpClient client = new TcpClient("localhost", 5000))
        using (NetworkStream stream = client.GetStream())
        {
            string command = $"ADD {username} {repoName}";
            byte[] commandData = Encoding.UTF8.GetBytes(command);
            await stream.WriteAsync(commandData, 0, commandData.Length);

            foreach (var file in files)
            {
                byte[] fileNameData = Encoding.UTF8.GetBytes(file.Key);
                byte[] fileNameLength = BitConverter.GetBytes(fileNameData.Length);
                byte[] fileDataLength = BitConverter.GetBytes(file.Value.Length);

                await stream.WriteAsync(fileNameLength, 0, fileNameLength.Length);
                await stream.WriteAsync(fileNameData, 0, fileNameData.Length);
                await stream.WriteAsync(fileDataLength, 0, fileDataLength.Length);
                await stream.WriteAsync(file.Value, 0, file.Value.Length);
            }
        }
        Console.WriteLine("Files sent successfully.");
    }
}