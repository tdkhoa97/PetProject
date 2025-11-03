// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using Business;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<MyBenchmark>();