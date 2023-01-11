// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Laminar.Benchmarks;

var test = new ValuePassingBenchmark();
test.Setup();
Console.WriteLine("Setup Complete");
Console.ReadLine();
Console.WriteLine(test.PassValueFields(4));

// Console.WriteLine(ValuePassingBenchmark.TestRun());
// BenchmarkRunner.Run<ValuePassingBenchmark>();