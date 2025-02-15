using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpExceptionSourceGenerator(string exceptionClassName) : CSharpSourceGenerator {
  private const string BaseExceptionClassName = "Exception";
  private const string MessageParameterName = "message";

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateExceptionClass()]))
      .NormalizeWhitespace();

  private ClassDeclarationSyntax GenerateExceptionClass() =>
    ClassDeclaration(exceptionClassName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(BaseExceptionClassName)))))
      .WithMembers(List<MemberDeclarationSyntax>([GenerateConstructorMessageOverload()]));

  private MemberDeclarationSyntax GenerateConstructorMessageOverload() =>
    ConstructorDeclaration(Identifier(exceptionClassName))
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier(MessageParameterName))
                                                                .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))))))
      .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                                              ArgumentList(SingletonSeparatedList(Argument(IdentifierName(MessageParameterName))))))
      .WithBody(Block());
}