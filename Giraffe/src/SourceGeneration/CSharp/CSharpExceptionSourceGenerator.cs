using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpExceptionSourceGenerator(string exceptionClassName) : CSharpSourceGenerator {
  private const string BaseExceptionClassName = "Exception";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateExceptionClass()]))
      .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateExceptionClass() =>
    ClassDeclaration(exceptionClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(BaseExceptionClassName)))));
}