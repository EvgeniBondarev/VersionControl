using Core.CommentManager;

namespace Core.CommandManager.Commands.Implementation
{
    public class CommentCommand : UserCommand
    {
        private CommentClient _commentClient;
        public override string Name => "comment";

        public CommentCommand(CommentClient commentClient, string user) : base(user)
        {
            _commentClient = commentClient;
        }

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Repository name is required.");
                return;
            }

            try
            {
                string repoName = args[0];

                if (args.Length > 1)
                {
                    string comment = string.Join(" ", args.Skip(1));
                    _commentClient.SendComment(repoName, comment, _user);
                }
                else
                {
                    List<string> comments = _commentClient.GetComments(repoName);
                    foreach (string comment in comments)
                    {
                        Console.WriteLine(comment);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository created error: {ex.Message}.");
            }
        }


    }
}

