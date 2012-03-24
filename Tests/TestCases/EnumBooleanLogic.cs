using System;

public static class Program {
    private enum DelayType {
        NoDelay,
        StartCombat,
        EndRound,
        EndCharacterTurn,
        FleeAttempt,
        FleeSuccessful,
    }

    static bool someBool = true;
    static object highlightedCombatant = new object();
    static DelayType delayType = DelayType.NoDelay;

    public static void Main (string[] args) {
        Console.WriteLine(delayType);

        if ((delayType == DelayType.NoDelay) &&
            (highlightedCombatant != null) && someBool) {
            delayType = DelayType.EndCharacterTurn;
        }

        Console.WriteLine(delayType);
    }
}