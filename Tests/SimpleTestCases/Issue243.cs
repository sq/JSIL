using System;
using System.Collections.Generic;
using System.Linq;

public static class Program { 
    public static void Main (string[] args) {
        int outermostValue = 5;
        var sourceItems = new int[] { 1, 2, 4 };

        var result =
            sourceItems.SelectMany(
                item => {
                    var outerValue = item + outermostValue;
                    var resultItems = new int[] { outerValue, item * 2 };

                    return resultItems.Select(
                        (i) => i + outerValue
                    );
                }
            );

        var resultArray = result.ToArray();
        Console.WriteLine(resultArray.Length);

        foreach (var item in resultArray)
            Console.WriteLine(item);
    }
}