using CliWrap;
using System.IO;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Helpers
{
    public static class Git
    {
        private static Command Cmd(string workingDir)
        {
            return new Command("git")
                .WithWorkingDirectory(workingDir);
        }

        public static Command CanFastForward(string workingDir, string local, string remote)
        {
            return Cmd(workingDir)
                .WithValidation(CommandResultValidation.None)
                .WithArguments(new string[] { "merge-base", "--is-ancestor", local, remote });
        }

        public static Command CheckoutBranch(string workingDir, string branch)
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "checkout", branch });
        }

        public static Command CreateBranch(string workingDir, string branch)
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "branch", branch });
        }

        public static Command CreateBranchAndCheckout(string workingDir, string branch)
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "checkout", "-b", branch });
        }

        public static async Task<bool> SetGitAttributes(string workingDir, string value)
        {
            try
            {
                await File.WriteAllTextAsync(Path.Combine(workingDir, ".gitattributes"), value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Command SetConfigValue(string workingDir, string setting, string value)
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "config", setting, value });
        }

        public static Command Merge(string workingDir, string branch)
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "merge", branch });
        }

        public static Command Commit(string workingDir, string msg = "updated")
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "commit", "-m", msg });
        }

        public static Command Init(string workingDir)
        {
            return Cmd(workingDir)
                .WithArguments("init");
        }

        public static Command AddAll(string workingDir)
        {
            return Cmd(workingDir)
                .WithArguments(new string[] { "add", "-A" });
        }
    }
}
