// ContentService publishes ambient statics on rebuild, so test classes may not run against each other.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]
