using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Porticle.Grpc.TypeMapper;

public static class SyntaxHelper
{
    /// <summary>
    ///     Parses C# source code and extracts the contained class declaration.
    /// </summary>
    /// <param name="classCode">The complete C# source code containing exactly one class declaration.</param>
    public static ClassDeclarationSyntax ClassFromSource(string classCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(classCode);
        var root = syntaxTree.GetRoot();
        var nestedClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        return nestedClass.WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
    }

    /// <summary>
    ///     Parses C# source code and extracts the contained interface declaration.
    /// </summary>
    /// <param name="classCode">The complete C# source code containing exactly one interface declaration.</param>
    public static InterfaceDeclarationSyntax InterfaceFromSource(string classCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(classCode);
        var root = syntaxTree.GetRoot();
        var nestedClass = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().Single();
        return nestedClass.WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
    }

    /// <summary>
    ///     Parses the source code of a single method and extracts the contained method declaration.
    /// </summary>
    /// <param name="methodCode">The method source code (without enclosing class).</param>
    public static MethodDeclarationSyntax MethodFromSource(string methodCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("class _ { " + methodCode + " }");
        var root = syntaxTree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        return method.WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
    }
}