using BenchmarkDotNet.Running;

internal class Program
{
    public const string CodeSample = """
                           (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
                           (place cross (50 50))
                           (loop 100 ( (shift cross right) (shift cross up) ) )
                           (erase cross)
                           """;

    private static void Main(string[] _)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
            .RunAll();
    }
}
