using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Porticle.Grpc.TypeMapper;

public class MethodVisitor : CSharpSyntaxRewriter
{
    private static readonly string[] ReplaceMethods =
    [
        "Equals",
        "GetHashCode",
        "WriteTo",
        "InternalWriteTo",
        "CalculateSize",
        "MergeFrom",
        "InternalMergeFrom"
    ];

    private int classDepth;

    public MethodVisitor(HashSet<PropertyToField> replaceProps)
    {
        ReplaceProps = replaceProps;
    }

    public HashSet<PropertyToField> ReplaceProps { get; }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (classDepth > 0) return node;

        classDepth++;
        var rewrittenClass = base.VisitClassDeclaration(node);
        classDepth--;
        return rewrittenClass;
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (!ReplaceMethods.Contains(node.Identifier.ValueText.Trim())) return base.VisitMethodDeclaration(node);

        var propertyToFieldRewriter = new PropertyToFieldRewriter(ReplaceProps);

        var newBody = (BlockSyntax?)propertyToFieldRewriter.Visit(node.Body);

        if (newBody == null) return base.VisitMethodDeclaration(node);

        return node.WithBody(newBody);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var propertyToFieldRewriter = new PropertyToFieldRewriter(ReplaceProps);
        var newBody = (BlockSyntax?)propertyToFieldRewriter.Visit(node.Body);

        return newBody == null
            ? base.VisitConstructorDeclaration(node)
            : node.WithBody(newBody);
    }
}