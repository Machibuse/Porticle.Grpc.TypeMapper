using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Porticle.Grpc.TypeMapper;

/// <summary>
///     Ein spezialisierter Rewriter, der alle Vorkommen eines bestimmten
///     Identifier-Namens durch einen anderen ersetzt.
/// </summary>
public class PropertyToFieldRewriter : CSharpSyntaxRewriter
{
    public PropertyToFieldRewriter(HashSet<PropertyToField> replaceNames)
    {
        ReplaceNames = replaceNames;
    }

    public HashSet<PropertyToField> ReplaceNames { get; }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (node.Parent is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax oneofCase } memberAccess
            && memberAccess.Name == node
            && oneofCase.Identifier.ValueText.EndsWith("OneofCase", StringComparison.Ordinal))
        {
            return base.VisitIdentifierName(node);
        }

        // The last part of a qualified type name (e.g. global::Google.Protobuf.WellKnownTypes.Timestamp) is not a property
        // reference, even if a property with the same name exists (proto field "timestamp")
        if (node.Parent is QualifiedNameSyntax qualifiedName && qualifiedName.Right == node) return base.VisitIdentifierName(node);

        // A member access is only a property reference if it is accessed on a simple instance (other.Timestamp, this.Timestamp).
        // Everything else is a namespace qualified type (global::Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(...))
        if (node.Parent is MemberAccessExpressionSyntax parentAccess
            && parentAccess.Name == node
            && parentAccess.Expression is not (IdentifierNameSyntax or ThisExpressionSyntax))
        {
            return base.VisitIdentifierName(node);
        }

        var mapping = ReplaceNames.SingleOrDefault(field => field.PropertyName == node.Identifier.Text);

        if (mapping != null) return SyntaxFactory.IdentifierName(mapping.FieldName).WithTriviaFrom(node);

        return base.VisitIdentifierName(node);
    }
}