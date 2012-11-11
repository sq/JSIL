using System;

public class Battle {
    public static Battle Current;
    public int Value = 0;

    public abstract class ITargetHandler {
        public abstract void Setup ();
        public Battle ThisBattle { get { return Battle.Current; } }
    }

    public class TargetOneEnemy : ITargetHandler {
        public override void Setup () {
            ThisBattle.Value += 1;
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        var battleA = new Battle();
        var battleB = new Battle();
        Battle.Current = battleB;

        Console.WriteLine("{0}, {1}", battleA.Value, battleB.Value);

        (new Battle.TargetOneEnemy()).Setup();

        Console.WriteLine("{0}, {1}", battleA.Value, battleB.Value);

        Battle.Current = battleA;
        (new Battle.TargetOneEnemy()).Setup();

        Console.WriteLine("{0}, {1}", battleA.Value, battleB.Value);
    }
}