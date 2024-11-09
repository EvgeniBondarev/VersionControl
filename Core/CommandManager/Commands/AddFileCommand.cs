using Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands
{
    public class AddFileCommand : ICommand
    {
        private FileTransferClient _fileTransferClient;
        public string Name => "add";

        public AddFileCommand(FileTransferClient fileTransferClient)
        {
            _fileTransferClient = fileTransferClient;
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Repository name is required.");
                return;
            }

            string userDirectory = args[0];
            string repoName = args[1];
            try
            {
                _fileTransferClient.SendDirectory(@$"{userDirectory}", repoName);
                Console.WriteLine($"Files from directory `{userDirectory}` added in '{repoName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository created error: {ex.Message}.");
            }

        }
    }
}
