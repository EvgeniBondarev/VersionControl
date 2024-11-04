using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Commands
{
    public class CommitCommand : BaseCommand
    {
        public override string Name => "COMMIT";

        public override string Execute(string[] args)
        {
            if (args.Length < 1)
            {
                return "Error: Commit message required.";
            }

            string commitMessage = args[0];
            // Логика выполнения коммита
            return $"Changes committed with message: '{commitMessage}'";
        }
    }
}
