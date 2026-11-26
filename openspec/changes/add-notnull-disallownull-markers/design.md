# Design

## Context

See proposal.md for motivation. Observed current state:

- Field markers are detected by a substring check on the property's leading trivia (`property.GetLeadingTrivia().ToFullString().Contains("[Marker]")`) inside `PropertyVisitor.CheckProperty`. Protoc copies the proto field comment into a `/// <summary>` doc comment, which is part of that trivia.
- The message-level marker `[NullableReferenceTypes]` is read the same way from the class's leading trivia in `ClassVisitor.VisitClassDeclaration` and passed to `PropertyVisitor` as a constructor argument. `ClassVisitor` creates one `PropertyVisitor` per class; nested classes are visited separately and `PropertyVisitor` does not descend into them (`classDepth > 0`).
- `CheckProperty` is a chain of early returns: the first matching transformation returns a rewritten property, otherwise `null` (property unchanged). `VisitPropertyDeclaration` returns `newProperty ?? node`.
- `ConvertToNullableMessageProperty` already contains a structural test for "this property is a message field": the type starts with `global::`, the setter has a body without `_hasBits`, the getter returns a plain identifier, and the backing field has no initializer (which excludes required enums). An unmapped `Timestamp` property passes this test because Timestamp is a message.
- `[DateTime]` / `[DateTimeOffset]` rewrite a Timestamp property to `global::System.DateTime?` / `global::System.DateTimeOffset?` with a different getter and setter, so the structural test no longer matches the rewritten property.
- Protoc emits each property with two attribute lists (`DebuggerNonUserCodeAttribute`, `GeneratedCode`), one per line, between the doc comment and the `public` keyword.
- Re-processing is prevented per class by the `[Porticle.Grpc.TypeMapper]` marker comment in `ClassVisitor`.

Scratch compiles confirmed the compiler behavior the specs rely on:

- `[DisallowNull]` produces CS8625 on `x.Prop = null` and `[NotNull]` suppresses the warning on reads, both when the property is declared inside `#nullable disable` (oblivious type) and inside `#nullable enable` with a `T?` type.
- On a `DateTime?` property, `[DisallowNull]` produces CS8607 on `x.Prop = null` and `[NotNull]` suppresses CS8629 on `x.Prop.Value`.

## Goals / Non-Goals

**Goals:**
- One place that resolves the effective markers and adds the attributes, independent of which transformation ran before.
- No change to any existing transformation.

**Non-Goals:**
- Covering oneof members, repeated, map, wrapper-typed, string or scalar fields.
- Runtime null checks, project-wide switches, inheritance into nested messages.
- Emitting `MaybeNullAttribute` or `AllowNullAttribute`.

## Decisions

### Message-level markers are read in `ClassVisitor` and passed to `PropertyVisitor`

`ClassVisitor` reads `[NotNull]` and `[DisallowNull]` from the class's leading trivia next to the existing `[NullableReferenceTypes]` check and passes them to the `PropertyVisitor` constructor as two additional flags.

- Why: identical to the existing message-level mechanism, and the one-visitor-per-class structure gives "this message only, not nested messages" without extra code.
- Alternative considered: inheriting into nested messages. Rejected because `[NullableReferenceTypes]` does not inherit either and the user asked for "in this message".

### Eligibility is decided on the original protoc property

Extract the structural test from `ConvertToNullableMessageProperty` into a shared helper (`IsMessageTypedProperty` or similar) and evaluate it on the **original** node in `VisitPropertyDeclaration`, before any transformation. `ConvertToNullableMessageProperty` uses the same helper.

- Why the original node: a Timestamp mapped with `[DateTime]` no longer looks like a message property afterwards; the original always does. This yields "message or Timestamp, mapped or not" from a single rule.
- Why reuse the existing test: it already separates messages from required enums and optional scalars and is covered by the existing `[NullableReferenceTypes]` tests.
- Consequence: oneof members are not eligible, because their getter is a conditional expression instead of a plain identifier. This is intended: a oneof member is null whenever another case is set, so a message-level `[NotNull]` must not reach it. A field-level marker on a oneof member produces the "not eligible" warning.

### Marker resolution

For each property, computed from the original node's leading trivia:

```
fieldNotNull      = trivia contains "[NotNull]"
fieldDisallowNull = trivia contains "[DisallowNull]"
maybeNull         = trivia contains "[MaybeNull]"

if eligible:
    notNull      = fieldNotNull      || (messageNotNull      && !maybeNull)
    disallowNull = fieldDisallowNull || (messageDisallowNull && !maybeNull)
else:
    no attributes; warn once per marker if fieldNotNull / fieldDisallowNull
```

- `[MaybeNull]` cancels only inherited defaults; explicit field markers always win. This allows "disallow null but may return null" on a single field inside a strict message (`// [MaybeNull] [DisallowNull]`).
- `[MaybeNull]` emits no attribute and never warns, also not on fields that are not eligible or in messages without defaults, so it can be written defensively.
- Alternative considered: `[MaybeNull]` emits `System.Diagnostics.CodeAnalysis.MaybeNull`. Rejected: the properties in question are already nullable or oblivious, so the attribute would add nothing, and it does not express the opt-out from `[DisallowNull]` (that would be `AllowNull`).

### Apply attributes as a final step in `VisitPropertyDeclaration`

After `CheckProperty` has produced its result (`newProperty ?? node`), the resolved attributes are added to the resulting property.

- Why not inside `CheckProperty`: it has roughly a dozen return points; adding the attribute in each branch is error-prone and would be forgotten for future transformations.

### Append the attribute as a new attribute list after the existing ones

Use `AddAttributeLists` so the new list comes after protoc's `GeneratedCode` list and directly before the modifiers. The new list gets the indentation of the existing lists and a trailing line break.

- Why not prepend: the property's leading trivia (doc comment, `#nullable enable`) hangs on the first token of the first attribute list. Prepending would require moving that trivia and risks placing an attribute above the doc comment or above a `#nullable` directive.
- Fallback: if a property has no attribute lists (not the case for protoc output today), the new list takes over the property's leading trivia so comment and directives stay on top.

### Fully qualified attribute names

Emit `[global::System.Diagnostics.CodeAnalysis.NotNull]` and `[global::System.Diagnostics.CodeAnalysis.DisallowNull]`, matching the style of the attributes protoc already emits. Generated files have no `using` for that namespace, and a message or nested type named `NotNull` in user protos must not capture the name.

### Field-level marker on a non-eligible field is a warning, not an error

Reported with `log.LogWarning`, naming the property and the marker.

- Why not an error like `[NullableString]` on a non-nullable string: the markers are plain English words in brackets that match attribute names and are more likely to appear in prose comments; a hard build failure for that is disproportionate.
- Why not silent: a marker that does nothing on a `string` field would otherwise look like a bug.
- Debug builds of this repository set C# `TreatWarningsAsErrors`; that does not promote MSBuild task warnings, so the test project can contain such a field. If it turns out to break the Debug build, the non-eligible cases move to a separate proto that is checked by a dedicated test instead (see tasks).

### Marker matching stays a plain substring check

Same mechanism and same limitations as the existing markers. `[NotNull]`, `[DisallowNull]` and `[MaybeNull]` are not substrings of each other or of any existing marker.

### Idempotency relies on the existing class marker

No extra duplicate check is added. A processed class is skipped entirely on re-processing, which already covers the attributes. A rebuild check guards this assumption.

## Risks / Trade-offs

- [Consumers on .NET Framework or .NET Standard 2.0 do not have these attribute types and get a compile error when they use the markers] → Document the requirement (.NET Core 3.0+ / .NET Standard 2.1+) in the README. Only projects that opt in by writing a marker are affected.
- [`[NotNull]` is a promise the generator cannot verify; a field that was never set still returns null at runtime, and with a message-level `[NotNull]` this applies to every message field at once] → State in the README that the markers only influence compiler analysis and that a parsed message can still contain unset fields.
- [A message comment that mentions `[NotNull]` in prose switches the default on for the whole message] → Same behavior as `[NullableReferenceTypes]`; document that the marker text is reserved.
- [The eligibility helper is now shared with `[NullableReferenceTypes]`; a change to it affects both features] → Covered by the existing nullable-reference-type tests plus the new ones.
- [oneof members cannot be annotated] → Accepted limitation, listed under "What is currently not possible" in the README.
