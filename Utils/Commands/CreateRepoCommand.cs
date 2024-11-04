using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionControlServer;

namespace Utils.Commands
{
    public class CreateRepoCommand : BaseCommand
    {
        private readonly FileManager _fileManager;

        public CreateRepoCommand(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public override string Name => "CREATE_REPO";

        public override string Execute(string[] args)
        {
            if (args.Length < 2)
            {
                return "Error: Username and repository name required.";
            }

            string username = args[0];
            string repoName = args[1];

            // Используем FileManager для создания директории репозитория
            return _fileManager.CreateRepositoryDirectory(username, repoName);
        }
    }
}
