using CliWrap;

namespace GitDatabaseMerger.Server.Tests.Helpers
{
    public static class Git
    {
        public static Command Cmd(string workingDir)
        {
            return new Command("git")
                .WithWorkingDirectory(workingDir);
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
