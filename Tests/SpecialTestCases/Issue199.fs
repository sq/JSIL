module public Program
    let rec f () = f() 

    let public Main (args : System.String[]) : System.Int32 =
        0