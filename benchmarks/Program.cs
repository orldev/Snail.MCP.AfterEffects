using BenchmarkDotNet.Running;
using Snail.MCP.AfterEffects.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(DiscoveryBenchmarks).Assembly).Run(args);
