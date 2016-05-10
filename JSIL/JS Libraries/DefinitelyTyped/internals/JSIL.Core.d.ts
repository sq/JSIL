export class StaticTypePair<TIn, TOut> {
    private _in: TIn;
    private _out: TOut;
}

export class TypePair<TIn, TOut> extends StaticTypePair<TIn, TOut> {
    $Is(input: any): boolean;
    $As(input: any): TOut;
    $Cast(input: any): TOut;
    /*readonly */$TypedUndefinedInternal: TIn;
    /*readonly */$TypedUndefined: TOut;
}

declare let __jsObject: Object;

export declare namespace $private{
    namespace System {
        namespace Object {
            class Instance {
                private _brand: any;
            }
            type Type = Instance;
            interface Static extends TypePair<Instance, Type> {
                new (): Type
            }
            type Factory = Static;
        }
        namespace String {
            type Instance = string;
            type Type = Instance & Object.Type;
            interface Static extends TypePair<Instance, Type> {
                new (arg: string): Type;
            }
            type Factory = Static;
        }
        namespace Byte {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & number;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace SByte {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & number;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace Int16 {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & number;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace UInt16 {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & number;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace Int32 {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & number;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace UInt32 {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & number;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace Single {
            type Instance = number
            type Type = Instance;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace Double {
            type Instance = number
            type Type = Instance;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace Boolean {
            type Instance = boolean
            type Type = Instance;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }
        namespace Char {
            class Instance {
                private _brand: any;
            }
            type Type = Instance & string;
            interface Static extends TypePair<Instance, Type> {
                new (arg: number): Type;
            }
            type Factory = Static;
        }

        namespace Array {
            class Instance<InTType, OutTType, TSize> {
                private _brand: any;
                private _TType_brand: OutTType;
                private _TSize_brand: TSize;
            }
            type Type<InTType, OutTType, TSize> = Instance<InTType, OutTType, TSize> & OutTType[];
            interface Static<InTType, OutTType, TSize> extends TypePair<Instance<InTType, OutTType, TSize>, Type<InTType, OutTType, TSize>> {
            }
            interface Factory {
                Of<InTType, OutTType>(__T: TypePair<InTType, OutTType>): Vector.Static<InTType, OutTType>
                //Of<InTType, OutTType, TSize>(TType: TypePair<InTType, OutTType>, TSize: TSize): Static<InTType, OutTType, TSize>
            }
        }

        namespace Vector {
            class Instance<InTType, OutTType> {
                private _brand: any;
                private _TType_brand: OutTType;
            }
            type Type<InTType, OutTType> = Instance<InTType, OutTType> & Array.Type<InTType, OutTType, "1">;
            interface Static<InTType, OutTType> extends TypePair<Instance<InTType, OutTType>, Type<InTType, OutTType>> {
            }
        }
    }

    namespace JSIL {
        namespace Reference {
            interface Instance<InTType, OutTType> {
                get(): OutTType;
                set(value: InTType): void;
            }
            type Type<InTType, OutTType> = Instance<InTType, OutTType>;
        }

        namespace BoxedVariable {
            class Instance<TType> {
                private _brand: any;
                private _TType_brand: TType;
            }
            type Type<TType> = Instance<TType> & Reference.Type<TType, TType>;
            interface Factory {
                new <TType>(arg: TType): Type<TType>
            }
        }

        namespace MemberReference {
            class Instance<TType> {
                private _brand: any;
                private _TType_brand: TType;
            }
            type Type<TType> = Instance<TType> & Reference.Type<TType, TType>;
            interface Factory {
                //TODO: think how to make it static typed
                new <TType>(object: Object, memberName: string): Type<TType>
            }
        }

        namespace ArrayElementReference {
            class Instance<InTType, OutTType> {
                private _brand: any;
                private _TType_brand: InTType;
            }
            type Type<InTType, OutTType> = Instance<InTType, OutTType> & Reference.Type<InTType, OutTType>;
            interface Factory {
                new <InTType, OutTType>(array: System.Vector.Instance<InTType, OutTType>, index: System.Int32.Instance): Type<InTType, OutTType>
            }
        }
    }
}