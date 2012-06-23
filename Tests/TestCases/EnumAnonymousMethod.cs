using System;

public enum MyEnum

{
  One=1,
  Two=2,
  Three=3
}

public static class Program {


  public static bool Check(MyEnum enumValue, int intValue)
  {
    return intValue == (int)enumValue;
  }
  
  public static void  Assert(bool b)
  {
    Console.WriteLine(b ? "PASS" : "FAIL");
  }


  public static void Main(string[] args)
  {

    Assert(Check(MyEnum.Two, 2));

    int localInt = 2;
    Func<MyEnum, int, bool> func = (enumValue, intValue) => intValue == (int)enumValue;

    Assert(func(MyEnum.Two, 2));

    Assert(func(MyEnum.Two, localInt));


    Func<MyEnum, bool> func2 = (enumValue) => localInt == (int)enumValue;
    Assert(func2(MyEnum.Two));

    // Above tests passed in December 2011 version of JSIL, but the one bellow failed.
    // The reason was that it got translated to "this.localInt === this.localMyEnum)" 
    // The later versions correctly translates this to:   "this.localInt === Number(this.localMyEnum)"
    MyEnum localMyEnum = MyEnum.Two;
    Func<bool> func3 = () => localInt == (int)localMyEnum;
    Assert(func3());
    

  }
}