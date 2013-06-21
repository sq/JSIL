module public Program
    let testFunction1 xs =
        xs |> List.map (fun (x,y,z:string list) -> 

            let inpArgTys = 
                match y with 
                | [_] -> [x]
                | _ -> []

            let niceNames = 
                match z with 
                | nms when nms.Length = 1 -> nms
                | [nm] -> inpArgTys 
                | nms -> nms

            (niceNames, niceNames))

    let public Main (args : System.String[]) : System.Int32 =
        0