using AnimationParser.Core;
using AnimationParser.Core.Commands;
using AnimationParser.Core.Tokens;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<BenchmarkAnimationParser>();

// Benchmark result:
// 1. with code below
//     const string code = """
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         """;
//
//    BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
//    13th Gen Intel Core i5-13500H, 1 CPU, 16 logical and 12 physical cores
//    .NET SDK 9.0.100-rc.1.24452.12
//    [Host]     : .NET 9.0.0 (9.0.24.43107), X64 RyuJIT AVX2
//    DefaultJob : .NET 9.0.0 (9.0.24.43107), X64 RyuJIT AVX2
//
//
//    | Method                     | Mean       | Error    | StdDev   | Gen0   | Allocated |
//    |--------------------------- |-----------:|---------:|---------:|-------:|----------:|
//    | BenchmarkLexing            |   691.7 ns |  4.02 ns |  3.76 ns | 0.0153 |     144 B |
//    | BenchmarkParsing           | 2,276.6 ns | 10.39 ns |  9.21 ns | 0.2289 |    2184 B |
//    | BenchmarkLexingToParsing   | 2,573.0 ns |  6.69 ns |  5.93 ns | 0.2403 |    2264 B |
//    | BenchmarkExecuting         | 2,564.3 ns | 12.91 ns | 11.45 ns | 1.2970 |   12240 B |
//    | BenchmarkLexingToExecuting | 5,671.3 ns | 31.45 ns | 26.27 ns | 1.5411 |   14545 B |
//
//    // * Hints *
//    Outliers
//    BenchmarkAnimationParser.BenchmarkParsing: Default           -> 1 outlier  was  removed (2.94 us)
//    BenchmarkAnimationParser.BenchmarkExecuting: Default         -> 2 outliers were removed (2.86 us, 2.87 us)
//    BenchmarkAnimationParser.BenchmarkLexingToExecuting: Default -> 1 outlier  was  removed (6.79 us)
//
//    // * Legends *
//    Mean      : Arithmetic mean of all measurements
//    Error     : Half of 99.9% confidence interval
//    StdDev    : Standard deviation of all measurements
//    Gen0      : GC Generation 0 collects per 1000 operations
//    Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
//    1 us      : 1 Microsecond (0.000001 sec)
//
//    // * Diagnostic Output - MemoryDiagnoser *
//
//
//    // ***** BenchmarkRunner: End *****
//    Run time: 00:01:28 (88.89 sec), executed benchmarks: 5

// 2. with code below
//     const string code = """
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
//                         (place cross (50 50))
//                         (loop 100 ( (shift cross right) (shift cross up) ) )
//                         (erase cross)
//                         """;

//    BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4460/23H2/2023Update/SunValley3)
//    13th Gen Intel Core i5-13500H, 1 CPU, 16 logical and 12 physical cores
//    .NET SDK 9.0.100-rc.1.24452.12
//    [Host]     : .NET 9.0.0 (9.0.24.43107), X64 RyuJIT AVX2
//    DefaultJob : .NET 9.0.0 (9.0.24.43107), X64 RyuJIT AVX2
//
//
//    | Method                     | Mean      | Error     | StdDev    | Gen0   | Allocated |
//    |--------------------------- |----------:|----------:|----------:|-------:|----------:|
//    | BenchmarkLexing            |  3.961 us | 0.0128 us | 0.0107 us | 0.0153 |     144 B |
//    | BenchmarkParsing           | 13.667 us | 0.0452 us | 0.0423 us | 1.3123 |   12473 B |
//    | BenchmarkLexingToParsing   | 15.703 us | 0.0668 us | 0.0625 us | 1.3123 |   12553 B |
//    | BenchmarkExecuting         | 16.009 us | 0.1072 us | 0.1002 us | 7.6904 |   72480 B |
//    | BenchmarkLexingToExecuting | 33.756 us | 0.2629 us | 0.2330 us | 8.9722 |   84797 B |
//
//    // * Hints *
//    Outliers
//    BenchmarkAnimationParser.BenchmarkLexing: Default          -> 2 outliers were removed (6.84 us, 6.84 us)
//    BenchmarkAnimationParser.BenchmarkLexingToParsing: Default -> 1 outlier  was  removed (21.48 us)
//    BenchmarkAnimationParser.BenchmarkExecuting: Default       -> 1 outlier  was  removed, 2 outliers were detected (16.33 us, 16.43 us)
//
//    // * Legends *
//    Mean      : Arithmetic mean of all measurements
//    Error     : Half of 99.9% confidence interval
//    StdDev    : Standard deviation of all measurements
//    Gen0      : GC Generation 0 collects per 1000 operations
//    Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
//    1 us      : 1 Microsecond (0.000001 sec)
//
//    // * Diagnostic Output - MemoryDiagnoser *
//
//
//    // ***** BenchmarkRunner: End *****
//    Run time: 00:01:21 (81.11 sec), executed benchmarks: 5

[MemoryDiagnoser]
public class BenchmarkAnimationParser
{
    private const string code = """
                                (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
                                (place cross (50 50))
                                (loop 100 ( (shift cross right) (shift cross up) ) )
                                (erase cross)
                                """;

    private List<Token> tokens;
    private List<IAnimationCommand> commands;

    public BenchmarkAnimationParser()
    {
        var lexer = new Lexer(code);
        tokens = lexer.Tokenize().ToList();
        commands = new Parser(tokens).Parse().ToList();
    }

    [Benchmark]
    public void BenchmarkLexing()
    {
        var lexer = new Lexer(code);
        var tokenstream = lexer.Tokenize().GetEnumerator();
        while (tokenstream.MoveNext()) ;
    }

    [Benchmark]
    public void BenchmarkParsing()
    {
        var parser = new Parser(tokens);
        _ = parser.Parse().All(_ => true); // parser is lazy evaluated, so we need to consume it
    }

    [Benchmark]
    public void BenchmarkLexingToParsing()
    {
        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        _ = parser.Parse().All(_ => true); // parser and lexer are lazy evaluated, so we need to consume them
    }

    [Benchmark]
    public void BenchmarkExecuting()
    {
        var context = new AnimationContext();
        foreach (var command in commands)
        {
            command.Execute(context);
        }
    }

    [Benchmark]
    public void BenchmarkLexingToExecuting()
    {
        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());
        var context = new AnimationContext();

        foreach (var command in parser.Parse())
        {
            command.Execute(context);
        }
    }
}
