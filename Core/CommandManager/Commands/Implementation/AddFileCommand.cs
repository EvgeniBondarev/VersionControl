using Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands.Implementation
{
    public class AddFileCommand : UserCommand
    {
        private FileTransferClient _fileTransferClient;
        public override string Name => "add";

        public AddFileCommand(FileTransferClient fileTransferClient, string user) : base(user)
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

            string userDirectory = args[0];
            string repoName = args[1];
            try
            {
                _fileTransferClient.SendDirectory(@$"{userDirectory}", $@"{_user}/{repoName}");
                Console.WriteLine($"Files from directory `{userDirectory}` added in '{_user}/{repoName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository created error: {ex.Message}.");
            }

        }
    }
}
