# Proposal

## Why

Protoc generates message-typed properties (including `google.protobuf.Timestamp`) without any null check, so they are the fields where "may this be null?" is undefined for the consumer; `[NullableReferenceTypes]`, `[DateTime]` and `[DateTimeOffset]` therefore map them to nullable types. Users who know that such a field is always set, or must never be set to null, have no way to express that, and every access needs a `!`, `.Value` or a redundant null check. The standard .NET attributes `[NotNull]` and `[DisallowNull]` express exactly this, and the project already has an established pattern for comment markers on fields and messages.

## What Changes

- New field comment markers `// [NotNull]` and `// [DisallowNull]`: the generated property gets `[global::System.Diagnostics.CodeAnalysis.NotNull]` / `[global::System.Diagnostics.CodeAnalysis.DisallowNull]`.
- The markers apply to fields whose type is a message or `google.protobuf.Timestamp`, whether or not the Timestamp is mapped with `[DateTime]` / `[DateTimeOffset]`.
- The same markers written above a `message` act as a default for that message: every message-typed and Timestamp field of the message gets the attribute.
- New field comment marker `// [MaybeNull]`: exempts a single field from the message-level defaults.
- The markers only add attributes. Property type, getter, setter and serialization stay exactly as they would be without the marker.
- A field-level `[NotNull]` / `[DisallowNull]` on a field of another kind is ignored with a build warning.
- README documents the markers.

Not part of this change: runtime null checks in the setter, project-wide (MSBuild property) variants, inheritance of message-level markers into nested messages, and emitting the `MaybeNull` / `AllowNull` attributes themselves.

## Capabilities

### New Capabilities
- `nullability-attributes`: Emitting standard .NET nullability attributes on generated properties, controlled by comment markers on proto fields and messages.

### Modified Capabilities

None. The project has no specs yet; existing marker behavior is unchanged.

## Impact

- `Source/Porticle.Grpc.TypeMapper/ClassVisitor.cs`: reads the message-level markers and passes them to the property visitor.
- `Source/Porticle.Grpc.TypeMapper/PropertyVisitor.cs`: eligibility check, marker resolution and attribute emission.
- `Source/Porticle.Grpc.UnitTests/Test.proto` and `Tests.cs`: new test messages and tests.
- `README.md`, `CLAUDE.md`: marker documentation.
- Consumers: generated code that uses the markers references `System.Diagnostics.CodeAnalysis.NotNullAttribute` / `DisallowNullAttribute`, which exist in .NET Core 3.0+ and .NET Standard 2.1+. Projects that do not use the markers are unaffected.
- No new package dependencies, no breaking changes.
