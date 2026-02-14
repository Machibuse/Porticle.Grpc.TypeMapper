using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Porticle.Grpc.TypeMapper.SyntaxHelper;

namespace Porticle.Grpc.TypeMapper;

public class ClassVisitor(TaskLoggingHelper log, bool wrapAllNonNullableStrings, bool wrapAllNullableStringValues) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var marker = "[Porticle.Grpc.TypeMapper]";

        if (node.GetLeadingTrivia().ToFullString().Contains(marker))
            // Skip if marker exists - class alreqady patched
            return node;

        // Add marker
        var trivia = node.GetLeadingTrivia().Add(SyntaxFactory.Comment("/// <remark>" + marker + "</remark>")).Add(SyntaxFactory.CarriageReturnLineFeed);
        node = node.WithLeadingTrivia(trivia);

        var propertyVisitor = new PropertyVisitor(log, wrapAllNonNullableStrings, wrapAllNullableStringValues);
        node = (ClassDeclarationSyntax)propertyVisitor.Visit(node);

        if (propertyVisitor.NeedGuidConverter)
        {
            node = node.AddMembers(ClassFromSource(ListWrappers.RepeatedFieldGuidWrapper));
            node = node.AddMembers(InterfaceFromSource(ListWrappers.IListWithRangeAdd));
        }

        var methodVisitor = new MethodVisitor(propertyVisitor.ReplaceProps);
        node = (ClassDeclarationSyntax)methodVisitor.Visit(node);

        // Add ICustomDiagnosticMessage implementation for message classes
        // Note: The interface is added to the base list via text replacement in ProtoPostProcessor
        // because Roslyn's AddBaseListTypes breaks #if/#endif preprocessor directives in the base list.
        var isMessageClass = node.BaseList?.Types.Any(t => t.ToString().Contains("IMessage")) ?? false;
        if (isMessageClass)
        {
            node = node.AddMembers(MethodFromSource(
                "public string ToDiagnosticString() { try { return System.Text.Json.JsonSerializer.Serialize(this); } catch(Exception ex) { return \"Message Invalid - \" + ex.Message; } }"));
        }

        return node;
    }
}