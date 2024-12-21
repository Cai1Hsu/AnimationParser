using BenchmarkDotNet.Running;

internal class Program
{
    public static string SampleCode = """
                                      (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
                                      (place cross (50 50))
                                      (loop 100 ( (shift cross right) (shift cross up) ) )
                                      (erase cross)
                                      """;

    private static void Main(string[] _)
    {
        SampleCode = string.Join('\n', Enumerable.Repeat(SampleCode, 1000));

        Console.WriteLine($"SampleCode length: {SampleCode.Length}");

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
            .RunAll();
    }
}
