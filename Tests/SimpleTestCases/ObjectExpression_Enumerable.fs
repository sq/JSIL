module public Program
open System
open System.Collections
open System.Collections.Generic
let test() =   
    let enumerator () = 
      { new IEnumerator<'U> with
            member x.Current = failwith "invalid"
        interface IEnumerator with
            member x.Current = failwith "invalid"
            member x.MoveNext() = false
            member self.Reset() = failwith "reset not supported"
        interface System.IDisposable with
            member x.Dispose() = () } 
    { new IEnumerable<'U> with
        member x.GetEnumerator() = enumerator ()
      interface IEnumerable with
        member x.GetEnumerator() = (enumerator () :> IEnumerator) }
let public Main (args:System.String array) =        
    for i in test() do
        Console.WriteLine("Item: {0}", i)
    0
