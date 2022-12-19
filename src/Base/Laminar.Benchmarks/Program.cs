// See https://aka.ms/new-console-template for more information
using Laminar.Benchmarks;

Console.WriteLine("Hello, World!");

var benchmarkOne = new ValuePassingBenchmark();

Console.WriteLine(benchmarkOne.PassValue(4));