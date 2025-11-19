global using Clocktower.ServerTests.TestHelpers;
global using FluentAssertions;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Moq;
global using System.Net;


[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]