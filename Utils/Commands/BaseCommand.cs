using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public abstract string Name { get; }
        public abstract string Execute(string[] args);
    }

}
