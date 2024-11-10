using Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands.Implementation
{
    public class CreateRepoCommand : UserCommand
    {
        private FileTransferClient _fileTransferClient;
        public override string Name => "create";

        public string User;

        public CreateRepoCommand(FileTransferClient fileTransferClient, string user) : base(user)
        {
            _fileTransferClient = fileTransferClient;
        }

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Repository name is required.");
                return;
            }

            string repoName = args[0];
            try
            {
                _fileTransferClient.SendDirectory(string.Empty, $@"{_user}/{repoName}");
                Console.WriteLine($"Repository '{_user}/{repoName}' created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository created error: {ex.Message}.");
            }

        }
    }
}
