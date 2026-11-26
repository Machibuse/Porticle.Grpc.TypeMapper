# Spec Delta

## Purpose

Lets proto authors put standard .NET nullability attributes on generated C# properties of message-typed and Timestamp fields by writing comment markers above a proto field or message, so that consumers get precise nullable analysis without changing the generated runtime behavior.

## ADDED Requirements

### Requirement: Eligible fields
The nullability markers SHALL apply only to eligible fields. A field is eligible when its proto type is a message, including `google.protobuf.Timestamp` and regardless of whether the Timestamp is mapped with `[DateTime]` or `[DateTimeOffset]`. Scalar, string, bytes, enum, wrapper-typed (`google.protobuf.StringValue` and similar), repeated, map and oneof member fields are not eligible.

#### Scenario: Message-typed field is eligible
- **WHEN** a field of another message type has the comment `// [NotNull]`
- **THEN** the generated property has the `System.Diagnostics.CodeAnalysis.NotNullAttribute`

#### Scenario: Unmapped Timestamp field is eligible
- **WHEN** a `google.protobuf.Timestamp` field without `[DateTime]` / `[DateTimeOffset]` has the comment `// [NotNull]`
- **THEN** the generated `Timestamp` property has the `NotNullAttribute`

#### Scenario: Mapped Timestamp field is eligible
- **WHEN** a `google.protobuf.Timestamp` field has the comment `// [DateTime] [NotNull]`
- **THEN** the generated property has the type `DateTime?` and the `NotNullAttribute`

### Requirement: Field-level NotNull marker
When the comment of an eligible proto field contains the marker `[NotNull]`, the generated C# property SHALL carry the attribute `global::System.Diagnostics.CodeAnalysis.NotNull`.

#### Scenario: Consumer reads a NotNull message property
- **WHEN** consumer code with nullable reference types enabled assigns the value of a message-typed property marked `[NotNull]` to a non-nullable variable
- **THEN** the compiler reports no nullable warning, even if the property type is `TypeName?`

#### Scenario: Consumer reads a NotNull DateTime property
- **WHEN** consumer code with nullable reference types enabled reads `.Value` of a `DateTime?` property marked `[NotNull]`
- **THEN** the compiler reports no nullable warning

#### Scenario: Field without marker
- **WHEN** an eligible field has no `[NotNull]` marker and its message has no message-level `[NotNull]` marker
- **THEN** the generated property does not have the `NotNullAttribute`

### Requirement: Field-level DisallowNull marker
When the comment of an eligible proto field contains the marker `[DisallowNull]`, the generated C# property SHALL carry the attribute `global::System.Diagnostics.CodeAnalysis.DisallowNull`.

#### Scenario: Consumer assigns null to a DisallowNull property
- **WHEN** consumer code with nullable reference types enabled assigns `null` to a property marked `[DisallowNull]`
- **THEN** the compiler reports a nullable warning

#### Scenario: Field without marker
- **WHEN** an eligible field has no `[DisallowNull]` marker and its message has no message-level `[DisallowNull]` marker
- **THEN** the generated property does not have the `DisallowNullAttribute`

### Requirement: Message-level markers act as defaults
When the comment above a proto message contains `[NotNull]` and/or `[DisallowNull]`, every eligible field of that message SHALL get the corresponding attribute(s) as if the marker were written on the field. Fields that are not eligible MUST be left unchanged without any warning. The default applies only to the fields of that message itself, not to fields of messages nested inside it.

#### Scenario: Message-level NotNull
- **WHEN** a message with the comment `// [NotNull]` contains a message-typed field, a Timestamp field, a string field and an int32 field, none with their own markers
- **THEN** the message-typed and Timestamp properties have the `NotNullAttribute`, the string and int32 properties do not, and the build produces no warning

#### Scenario: Message-level NotNull and DisallowNull together
- **WHEN** a message has the comment `// [NotNull] [DisallowNull]`
- **THEN** every eligible property of that message has both attributes

#### Scenario: Nested message is not affected
- **WHEN** a message with the comment `// [NotNull]` contains a nested message definition without its own marker
- **THEN** the properties of the nested message class do not have the `NotNullAttribute`

### Requirement: MaybeNull marker exempts a field from message-level defaults
When the comment of a proto field contains the marker `[MaybeNull]`, the message-level `[NotNull]` and `[DisallowNull]` defaults SHALL NOT be applied to that field. The marker itself MUST NOT add any attribute. `[NotNull]` or `[DisallowNull]` written on the same field still apply.

#### Scenario: Exempted field
- **WHEN** a message has the comment `// [NotNull] [DisallowNull]` and one of its message-typed fields has the comment `// [MaybeNull]`
- **THEN** that property has neither attribute and all other eligible properties of the message have both

#### Scenario: Exemption with an explicit field marker
- **WHEN** a message has the comment `// [NotNull] [DisallowNull]` and one of its message-typed fields has the comment `// [MaybeNull] [DisallowNull]`
- **THEN** that property has the `DisallowNullAttribute` and does not have the `NotNullAttribute`

#### Scenario: MaybeNull without message-level markers
- **WHEN** a field has the comment `// [MaybeNull]` in a message without message-level nullability markers
- **THEN** the generated property is identical to the output without the marker and the build produces no warning

### Requirement: Field-level marker on a field that is not eligible
When `[NotNull]` or `[DisallowNull]` is written on a field that is not eligible, the system SHALL NOT add the attribute and SHALL report a build warning that names the property and the marker. The build MUST NOT fail because of it.

#### Scenario: Marker on a string field
- **WHEN** a `string` field has the comment `// [NotNull]`
- **THEN** the build succeeds with a warning naming the property and the generated property does not have the `NotNullAttribute`

#### Scenario: Marker on a repeated field
- **WHEN** a `repeated` message field has the comment `// [DisallowNull]`
- **THEN** the build succeeds with a warning naming the property and the generated property does not have the `DisallowNullAttribute`

### Requirement: Markers do not change generated behavior
The nullability markers SHALL only add attributes. Property type, getter, setter, serialization and all other generated members MUST be identical to the output for the same field without the marker.

#### Scenario: Assigning null at runtime
- **WHEN** null is assigned at runtime to a message-typed property marked `[DisallowNull]`
- **THEN** no exception is thrown and the property returns null afterwards

#### Scenario: Round trip is unaffected
- **WHEN** a message with fields marked `[NotNull]` and `[DisallowNull]` is serialized and parsed again
- **THEN** all field values equal the original values

### Requirement: Markers combine with existing transformations
The nullability markers SHALL be usable together with `[DateTime]` / `[DateTimeOffset]` on the field and with `[NullableReferenceTypes]` on the message. The existing transformation MUST be applied unchanged and the attribute added on top.

#### Scenario: Inside a NullableReferenceTypes message
- **WHEN** a message has the comment `// [NullableReferenceTypes] [NotNull]` and contains a message-typed field
- **THEN** the generated property has the type `TypeName?` and the `NotNullAttribute`

#### Scenario: Message-level default on a mapped Timestamp
- **WHEN** a message has the comment `// [DisallowNull]` and contains a Timestamp field with the comment `// [DateTimeOffset]`
- **THEN** the generated property has the type `DateTimeOffset?` and the `DisallowNullAttribute`

### Requirement: Processing is idempotent
Running the post-processor again on an already processed file SHALL NOT add a second copy of a nullability attribute.

#### Scenario: Incremental rebuild
- **WHEN** a project with marked fields and messages is built twice without cleaning
- **THEN** each affected property carries each attribute exactly once and the build succeeds
