using System;
using System.IO;

namespace VersionControlServer
{
    public class FileManager
    {
        private readonly string _baseDirectory;

        public FileManager(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public string CreateRepositoryDirectory(string username, string repoName)
        {
            string userDirectory = Path.Combine(_baseDirectory, username);
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            string repoDirectory = Path.Combine(userDirectory, repoName);
            if (!Directory.Exists(repoDirectory))
            {
                Directory.CreateDirectory(repoDirectory);
                return $"Repository directory '{repoDirectory}' created successfully.";
            }

            return $"Repository '{repoName}' already exists for user '{username}'.";
        }

        public string AddFilesToRepository(string username, string repoName, Dictionary<string, byte[]> files)
        {
            string repoDirectory = Path.Combine(_baseDirectory, username, repoName);
            if (!Directory.Exists(repoDirectory))
            {
                return $"Error: Repository '{repoName}' does not exist for user '{username}'.";
            }

            foreach (var file in files)
            {
                string filePath = Path.Combine(repoDirectory, file.Key);
                File.WriteAllBytes(filePath, file.Value);
            }

            return $"Files successfully added to repository '{repoName}'.";
        }
    }
}
