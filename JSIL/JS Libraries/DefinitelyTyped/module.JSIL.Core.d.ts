import {$private as $jsilCore} from "./internals/JSIL.Core";

export declare namespace System {
    let Object: $jsilCore.System.Object.Factory;
    let String: $jsilCore.System.String.Factory;
    let Byte: $jsilCore.System.Byte.Factory;
    let SByte: $jsilCore.System.SByte.Factory;
    let Int16: $jsilCore.System.Int16.Factory;
    let UInt16: $jsilCore.System.UInt16.Factory;
    let Int32: $jsilCore.System.Int32.Factory;
    let UInt32: $jsilCore.System.UInt32.Factory;
    let Single: $jsilCore.System.Single.Factory;
    let Double: $jsilCore.System.Double.Factory;
    let Boolean: $jsilCore.System.Boolean.Factory;
    let Char: $jsilCore.System.Char.Factory;
    let Array: $jsilCore.System.Array.Factory;
}

export declare namespace JSIL {
    let BoxedVariable: $jsilCore.JSIL.BoxedVariable.Factory;
    let MemberReference: $jsilCore.JSIL.MemberReference.Factory;
    let ArrayElementReference: $jsilCore.JSIL.ArrayElementReference.Factory;
}