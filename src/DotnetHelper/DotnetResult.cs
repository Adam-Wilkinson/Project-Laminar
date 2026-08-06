namespace DotnetHelper;

public record DotnetResult(int ExitCode, string StdOut, string StdErr, string Command);

public static class DotnetResultHelpers
{
    extension(Task<DotnetResult> resultTask)
    {
        public async Task<DotnetResult> ThrowOnError()
        {
            var result = await resultTask;

            if (result.ExitCode != 0)
            {
                Console.WriteLine($"dotnet {result.Command} failed. Exit code: {result.ExitCode}");
                Console.WriteLine("StdOut:");
                Console.WriteLine(result.StdOut);
                Console.WriteLine("StdErr:");
                await Console.Error.WriteLineAsync(result.StdErr);
                throw new Exception($"dotnet {result.Command} failed. Exit code: {result.ExitCode}");
            }

            return result;
        }
    }
}