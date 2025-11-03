using BenchmarkDotNet.Attributes;

namespace Business;

public class MyBenchmark
{
    [Benchmark]
    public void TestFunction()
    {
        for (int i = 0; i < 1_000_000; i++) Math.Sqrt(i);
    }
}