namespace Giraffe.RDT;

public record TopLevel(EntryRoutine EntryRoutine, List<Routine> Routines, SemanticAction MemberDeclarations) : Node;