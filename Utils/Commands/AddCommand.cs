using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionControlServer;

namespace Utils.Commands
{
    public class AddCommand : BaseFileCommand
    {
        private readonly FileManager _fileManager;

        public AddCommand(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public override string Name => "ADD";

        public override string Execute(string[] args, Dictionary<string, byte[]> files)
        {
            if (args.Length < 2)
            {
                return "Error: Username and repository name required.";
            }

            string username = args[0];
            string repoName = args[1];

            return _fileManager.AddFilesToRepository(username, repoName, files);
        }

        public override string Execute(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
