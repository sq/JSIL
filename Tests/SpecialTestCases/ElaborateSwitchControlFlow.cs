using System;
using System.Collections.Generic;

public class MalformedRenderstringException : Exception {
    public MalformedRenderstringException (string message)
        : base(message) {
    }
}

public static class Program {
    public static object[] MyMethod (object[] layers, string rstring, char delim) {
        int cur_pos, next_pos, len, layer_number;
        String str = rstring.Trim().ToUpper();            
        String cur_token;
        object cur_layer;
        List<object> layer_queue = new List<object>();
                
        cur_pos = 0;
        len = str.Length;
        while (cur_pos < len) {
            next_pos = str.IndexOf(delim, cur_pos);
            if (next_pos == -1) next_pos = len;
            cur_token = str.Substring(cur_pos, next_pos - cur_pos).Trim();
            switch (cur_token) {
                case "R":
                    cur_layer = new object();
                    layer_queue.Add(cur_layer);
                    break;
                case "E":
                    cur_layer = new object();
                    layer_queue.Add(cur_layer);
                    break;
                default: // tile layer
                    try {
                        layer_number = Int32.Parse(cur_token);
                        if (layer_number <= 0) throw new Exception();                            
                    }
                    catch (Exception) { throw new MalformedRenderstringException(rstring); } // not a positive integer                        
                    cur_layer = layers[layer_number - 1];
                    layer_queue.Add(cur_layer);
                    break;                        
            }
            cur_pos = next_pos + 1;
        }

        var list = layer_queue.ToArray();
        return list;
    }

    public static void Main (string[] args) {
        var layers = new object[6];
        var result = MyMethod(layers, "1,2,R,E,3,4,5", ',');
        Console.WriteLine(result.Length);
    }
}