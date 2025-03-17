using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Giraffe.SourceGeneration.CSharp;

public class CSharpEnumSourceGenerator : CSharpSourceGenerator {
  public required string EnumName { get; init; }
  public required List<string> EnumMembers { get; init; }

  public override CompilationUnitSyntax Generate() =>
    CompilationUnit()
      .WithMembers(List<MemberDeclarationSyntax>([GenerateNamespaceDeclaration(FileNamespace),
                                                  GenerateEnumDeclaration()]))
      .NormalizeWhitespace();

  private EnumDeclarationSyntax GenerateEnumDeclaration() =>
    EnumDeclaration(EnumName)
      .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
      .WithMembers(GenerateEnumMembers());

  private SeparatedSyntaxList<EnumMemberDeclarationSyntax> GenerateEnumMembers() =>
    SeparatedList<EnumMemberDeclarationSyntax>(GenerateCommaSeparatedList(EnumMembers, GenerateEnumMember));

  private static EnumMemberDeclarationSyntax GenerateEnumMember(string member) =>
    EnumMemberDeclaration(Identifier(StringToSafeUpperCamelCase(member)));
}