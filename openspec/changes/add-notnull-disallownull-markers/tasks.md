# Tasks

## 1. Field-level markers on eligible fields

- [x] 1.1 In `Source/Porticle.Grpc.UnitTests/Test.proto`, add a message `TestMessageNullabilityFields` with: a message field marked `// [NotNull]`, one marked `// [DisallowNull]`, one with both markers, one unmarked message field, an unmapped Timestamp marked `// [NotNull]`, a Timestamp marked `// [DateTime] [NotNull]` and one marked `// [DateTimeOffset] [DisallowNull]`; verify `dotnet build Source/Porticle.Grpc.sln -c Test` still succeeds
- [x] 1.2 Extract the message-property test from `ConvertToNullableMessageProperty` into a shared helper in `PropertyVisitor` and use it from that method; verify the existing `[NullableReferenceTypes]` tests still pass with `dotnet test Source/Porticle.Grpc.UnitTests -c Test`
- [x] 1.3 In `PropertyVisitor.VisitPropertyDeclaration`, evaluate eligibility and the field markers on the original node and append `[global::System.Diagnostics.CodeAnalysis.NotNull]` / `[global::System.Diagnostics.CodeAnalysis.DisallowNull]` as new attribute lists after the existing ones on the resulting property (code comments in English); verify by inspecting the generated `obj/Test/net9.0/Test.cs` that the attributes sit between `GeneratedCode` and `public` and the doc comment stays on top
- [x] 1.4 Add unit tests that check via reflection that each property of `TestMessageNullabilityFields` has exactly the expected attributes (one copy each), that the mapped Timestamp properties have the types `DateTime?` / `DateTimeOffset?`, and that the unmarked property has no nullability attribute; verify with `dotnet test Source/Porticle.Grpc.UnitTests -c Test`
- [x] 1.5 Add a unit test that assigns null at runtime to a `[DisallowNull]` property without an exception and that a serialize/parse round trip of the message preserves all values; verify with `dotnet test Source/Porticle.Grpc.UnitTests -c Test`
- [x] 1.6 Add a test method under `#nullable enable` that reads a `[NotNull]` message property into a non-nullable local without `!` and reads `.Value` of the `[NotNull]` `DateTime?` property; verify the Debug build of the solution (warnings as errors) succeeds, which proves no nullable warning is raised

## 2. Message-level defaults and MaybeNull

- [x] 2.1 Extend `Test.proto` with: a message `TestMessageNullabilityDefaults` marked `// [NotNull] [DisallowNull]` containing a message field, a Timestamp field marked `// [DateTimeOffset]`, a `string`, an `int32`, a `repeated` message field, a oneof with a message member, a message field marked `// [MaybeNull]`, a message field marked `// [MaybeNull] [DisallowNull]`, and a nested message with a message field; plus a message marked `// [NullableReferenceTypes] [NotNull]` with a message field; verify `dotnet build Source/Porticle.Grpc.sln -c Test` succeeds
- [x] 2.2 In `ClassVisitor`, read `[NotNull]` / `[DisallowNull]` from the class's leading trivia and pass them to `PropertyVisitor`; in `PropertyVisitor`, apply the resolution rule from design.md (message default unless `[MaybeNull]`, explicit field markers always apply); verify with the tests from 2.3
- [x] 2.3 Add unit tests that check via reflection for `TestMessageNullabilityDefaults`: both attributes on the message and Timestamp properties; none on the string, int32, repeated and oneof-member properties; none on the `[MaybeNull]` property; only `DisallowNull` on the `[MaybeNull] [DisallowNull]` property; none on the nested message's property; and for the `[NullableReferenceTypes] [NotNull]` message that the property is `TypeName?` (via `NullabilityInfoContext`) with `NotNullAttribute`; verify with `dotnet test Source/Porticle.Grpc.UnitTests -c Test`
- [x] 2.4 Build the solution twice in a row without cleaning and verify the second build succeeds and the reflection tests still report exactly one copy of each attribute

## 3. Field-level marker on non-eligible fields

- [x] 3.1 Log a build warning (property name and marker) when `[NotNull]` / `[DisallowNull]` is written on a non-eligible field, and emit no attribute; `[MaybeNull]` never warns; verify by adding a `string` field marked `// [NotNull]` and a `repeated` message field marked `// [DisallowNull]` to the test proto and checking that the build output contains one warning for each and the Debug build still succeeds (if the Debug build fails on these warnings, move both fields to a separate proto file that is post-processed in a dedicated test with a captured logger instead)
- [x] 3.2 Add a unit test that checks via reflection that those two properties have no nullability attribute; verify with `dotnet test Source/Porticle.Grpc.UnitTests -c Test`

## 4. Documentation

- [x] 4.1 Document the markers in `README.md`: add `[NotNull]`, `[DisallowNull]` and `[MaybeNull]` to the marker lists in "TL/DR" and "Usage"; add a section with a proto example (field-level, message-level, `[MaybeNull]` exemption) and the generated C#; state which fields are eligible, that the markers only add attributes and do not guarantee a value at runtime, that a non-eligible field gives a warning, and that the consuming project needs .NET Core 3.0+ / .NET Standard 2.1+; add oneof members to "What is currently not Possible?"; verify the example matches the actual generated output of the test project
- [x] 4.2 Update the marker list in `CLAUDE.md` ("Proto field comments ...") to include the new markers; verify the list matches the markers handled in `ClassVisitor` and `PropertyVisitor`

## 5. Integration check

- [x] 5.1 Run `dotnet build Source/Porticle.Grpc.sln` (Debug, warnings as errors) and `dotnet test Source/Porticle.Grpc.UnitTests -c Test`; verify both succeed with all tests passing
