using Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands
{
    public class CreateRepoCommand : ICommand
    {
        private FileTransferClient _fileTransferClient;
        public string Name => "create";

        public CreateRepoCommand(FileTransferClient fileTransferClient)
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

            string repoName = args[0];
            try
            {
                _fileTransferClient.SendDirectory(string.Empty, repoName);
                Console.WriteLine($"Repository '{repoName}' created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository created error: {ex.Message}.");
            }
            
        }
    }
}
