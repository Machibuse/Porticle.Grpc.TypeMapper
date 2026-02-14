# CLAUDE.md

## Project Overview

Porticle.Grpc.TypeMapper is a Roslyn-based post-processor shipped as a NuGet development dependency. It hooks into the build pipeline after `Protobuf_Compile` and rewrites protoc-generated C# classes to add support for Guids, nullable strings, and nullable enums — features not natively supported by Protocol Buffers.

Proto field comments (`[GrpcGuid]`, `[NullableString]`, `[NullableEnum]`) or global project properties control which transformations are applied.

## Solution Structure

```
Source/
  Porticle.Grpc.sln
  Porticle.Grpc.TypeMapper/     # Main library (NuGet package, net8.0 + net9.0)
  Porticle.Grpc.UnitTests/      # MSTest unit tests (net9.0)
  Porticle.Grpc.Test/            # Integration test with proto files
  Porticle.Grpc.TestWithoutProto/ # Package consumption test without proto compilation
```

## Key Source Files

| File | Role |
|------|------|
| `ProtoPostProcessor.cs` | MSBuild Task entry point — finds generated files, runs visitors, writes output |
| `ClassVisitor.cs` | CSharpSyntaxRewriter — orchestrates property and method transformations per class |
| `PropertyVisitor.cs` | Rewrites property types/getters/setters (Guid, nullable string, nullable enum) |
| `MethodVisitor.cs` | Updates Equals/GetHashCode/WriteTo/MergeFrom etc. to use backing fields |
| `PropertyToFieldRewriter.cs` | Replaces property identifiers with field names inside method bodies |
| `ListWrappers.cs` | Source code strings for `RepeatedFieldGuidWrapper` and `IListWithRangeAdd<T>` |
| `SyntaxHelper.cs` | Static helpers to parse C# source strings into Roslyn syntax nodes |
| `StringUtils.cs` | Snake_case to CamelCase conversion matching protoc/gRPC naming conventions |
| `Extensions.cs` | Small helper extensions (CheckNotNull, GetGetter, GetSetter) |
| `Porticle.Grpc.TypeMapper.targets` | MSBuild targets file shipped in the NuGet package |

## Build & Test

```bash
dotnet build Source/Porticle.Grpc.sln
dotnet test Source/Porticle.Grpc.UnitTests
```

## Coding Conventions

- All code comments (including XML doc comments) must be written in **English**.
- Target frameworks: net8.0 and net9.0.
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`).
- Implicit usings are enabled.
- Debug builds treat warnings as errors.
