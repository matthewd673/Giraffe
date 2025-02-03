namespace Giraffe;
public class ReferenceParser
{
  ReferenceScanner scanner;
  public ReferenceParser(ReferenceScanner scanner)
  {
    this.scanner = scanner;
  }

  private bool See(params int[] terminals) => terminals.Contains(scanner.Peek().Type);
  private ReferenceScanner.Token Eat(int terminal) => See(terminal) ? scanner.Eat() : throw new Exception();

  // TODO: Hand-written
  public void Parse() {
    if (See(0, 1, 2)) {
      ParseS();
      Eat(5); // Eof
      Console.WriteLine("Good");
      return;
    }

    throw new Exception();
  }

  private void ParseS()
  {
    if (See(0, 1, 2))
    {
      ParseA();
      ParseB();
      ParseC();
      ParseD();
      ParseE();
      return;
    }

    throw new Exception();
  }

  private void ParseA()
  {
    if (See(0))
    {
      Eat(0);
      return;
    }

    if (See(1))
    {
      return;
    }

    throw new Exception();
  }

  private void ParseB()
  {
    if (See(1))
    {
      Eat(1);
      return;
    }

    if (See(2))
    {
      return;
    }

    throw new Exception();
  }

  private void ParseC()
  {
    if (See(2))
    {
      Eat(2);
      return;
    }

    throw new Exception();
  }

  private void ParseD()
  {
    if (See(3))
    {
      Eat(3);
      return;
    }

    if (See(4))
    {
      return;
    }

    throw new Exception();
  }

  private void ParseE()
  {
    if (See(4))
    {
      Eat(4);
      return;
    }

    if (See(5))
    {
      return;
    }

    throw new Exception();
  }
}