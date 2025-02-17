namespace Giraffe.SourceGeneration;

public abstract record SourceFile<TContents>(string Filename, TContents Contents) where TContents : notnull;