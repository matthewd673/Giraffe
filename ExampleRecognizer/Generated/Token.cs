namespace ExampleRecognizer.Generated;
public readonly struct Token(int type, string image)
{
    public int Type { get; } = type;
    public string Image { get; } = image;
}