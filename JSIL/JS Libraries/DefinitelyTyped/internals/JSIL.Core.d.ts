export class StaticType<T, TIn, TOut> {
    private _t: T;
    private _in: TIn;
    private _out: TOut;
}

export class Type<T, TIn, TOut> extends StaticType<T, TIn, TOut> {
    $Is(input: any): boolean;
    $As(input: any): TOut;
    $Cast(input: any): TOut;
    /*readonly */$UndefinedIn: TIn;
    /*readonly */$UndefinedInternal: T;
    /*readonly */$Undefined: TOut;
}

export class NullArg {
    private _brand: any;
}

declare let __jsObject: Object;

export declare namespace $private{
    namespace System {
        namespace Object {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance;
            interface Static extends Type<Instance, TIn, TOut> {
                new (): TOut
            }
            type Factory = Static;
        }
        namespace String {
            type Instance = string;
            type TIn = Instance | Object.TIn;
            type TOut = Instance & Object.TOut;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: string): TOut;
            }
            type Factory = Static;
        }
        namespace Byte {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & number;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace SByte {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & number;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace Int16 {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & number;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace UInt16 {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & number;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace Int32 {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & number;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace UInt32 {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & number;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace Single {
            type Instance = number
            type TIn = Instance;
            type TOut = Instance;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace Double {
            type Instance = number
            type TIn = Instance;
            type TOut = Instance;
            interface Static extends Type<Instance, TIn, TOut> {
                new(arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace Boolean {
            type Instance = boolean
            type TIn = Instance;
            type TOut = Instance;
            interface Static extends Type<Instance, TIn, TOut> {
                new(arg: number): TOut;
            }
            type Factory = Static;
        }
        namespace Char {
            class Instance {
                private _brand: any;
            }
            type TIn = Instance;
            type TOut = Instance & string;
            interface Static extends Type<Instance, TIn, TOut> {
                new (arg: number): TOut;
            }
            type Factory = Static;
        }

        namespace Array {
            class Instance<TType, InTType, OutTType, TSize> {
                private _brand: any;
                private _TType_brand: OutTType;
                private _TSize_brand: TSize;
            }
            type TIn<TType, InTType, OutTType, TSize> = Instance<TType, InTType, OutTType, TSize>;
            type TOut<TType, InTType, OutTType, TSize> = Instance<TType, InTType, OutTType, TSize> & OutTType[];
            interface Static<TType, InTType, OutTType, TSize> extends Type<Instance<TType, InTType, OutTType, TSize>, TIn<TType, InTType, OutTType, TSize>, TOut<TType, InTType, OutTType, TSize>> {
            }
            interface Factory {
                Of<TType, InTType, OutTType>(__T: Type<TType, InTType, OutTType>): Vector.Static<TType, InTType, OutTType>
                //Of<InTType, OutTType, TSize>(TType: TypePair<InTType, OutTType>, TSize: TSize): Static<InTType, OutTType, TSize>
            }
        }

        namespace Vector {
            class Instance<TType, InTType, OutTType> {
                private _brand: any;
                private _TType_brand: OutTType;
            }
            type TIn<TType, InTType, OutTType> = Instance<TType, InTType, OutTType> | Array.TIn<TType, InTType, OutTType, "1">;
            type TOut<TType, InTType, OutTType> = Instance<TType, InTType, OutTType> & Array.TOut<TType, InTType, OutTType, "1">;
            interface Static<TType, InTType, OutTType> extends Type<Instance<TType, InTType, OutTType>, TIn<TType, InTType, OutTType>, TOut<TType, InTType, OutTType>> {
            }
        }
    }

    namespace JSIL {
        namespace Reference {
            interface Instance<TType, InTType, OutTType> {
                get(): OutTType;
                set(value: TType): void;
            }
            type TIn<TType, InTType, OutTType> = Instance<TType, InTType, OutTType>;
            type TOut<TType, InTType, OutTType> = Instance<TType, InTType, OutTType>;
        }

        namespace BoxedVariable {
            class Instance<TType> {
                private _brand: any;
                private _TType_brand: TType;
            }
            type TIn<TType> = Instance<TType> | Reference.TIn<TType, TType, TType>;
            type TOut<TType> = Instance<TType> & Reference.TIn<TType, TType, TType>;
            interface Factory {
                new <TType>(arg: TType): TOut<TType>
            }
        }

        namespace MemberReference {
            class Instance<TType> {
                private _brand: any;
                private _TType_brand: TType;
            }
            type TIn<TType> = Instance<TType> | Reference.TIn<TType, TType, TType>;
            type TOut<TType> = Instance<TType> & Reference.TIn<TType, TType, TType>;
            interface Factory {
                //TODO: think how to make it static typed
                new <TType>(object: Object, memberName: string): TOut<TType>
            }
        }

        namespace ArrayElementReference {
            class Instance<TType, InTType, OutTType> {
                private _brand: any;
                private _TType_brand: InTType;
            }
            type TIn<TType, InTType, OutTType> = Instance<TType, InTType, OutTType> | Reference.TIn<TType, InTType, OutTType>;
            type TOut<TType, InTType, OutTType> = Instance<TType, InTType, OutTType> & Reference.TOut<TType, InTType, OutTType>;
            interface Factory {
                new <TType, InTType, OutTType>(array: System.Vector.Instance<TType, InTType, OutTType>, index: System.Int32.Instance): TOut<TType, InTType, OutTType>
            }
        }
    }
}