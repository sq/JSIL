module public Program
    let formatChar isChar c =
        match c with
        | '\'' when isChar -> "\\\'"
        | '\"' when not isChar -> "\\\""
        //| '\n' -> "\\n"
        //| '\r' -> "\\r"
        //| '\t' -> "\\t"
        | '\\' -> "\\\\"
        | '\b' -> "\\b"
        | _ when System.Char.IsControl(c) ->
                let d1 = (int c / 100) % 10
                let d2 = (int c / 10) % 10
                let d3 = int c % 10
                "\\" + d1.ToString() + d2.ToString() + d3.ToString()
        | _ -> c.ToString()

    let public Main (args : System.String[]) : System.Int32 =
        let formatted = formatChar true 'a'
        System.Console.WriteLine formatted
        0