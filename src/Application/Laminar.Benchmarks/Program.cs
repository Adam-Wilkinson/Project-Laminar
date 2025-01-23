// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using Laminar.Benchmarks;

BenchmarkRunner.Run<ValuePassingBenchmark>();