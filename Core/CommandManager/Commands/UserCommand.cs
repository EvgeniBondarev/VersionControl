using Core.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommandManager.Commands
{
    public abstract class UserCommand : ICommand
    {
        protected readonly string _user;
        public abstract string Name { get; }

        protected UserCommand(string user)
        {
            _user = user;
        }
        public abstract void Execute(string[] args);
    }
}
