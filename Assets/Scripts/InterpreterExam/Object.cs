using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InterpreterExam {
    public interface Object {
        public ObjectType Type();
        public string Inspect();
    }

    public interface Hashable {
        public HashKey HashKey();
    }

    public struct Integer : Object, Hashable {
        public long Value;

        public ObjectType Type() {
            return ObjectType.INTEGER_OBJ;
        }

        public string Inspect() {
            return Value.ToString();
        }

        public HashKey HashKey() {
            return new HashKey {Type = Type(), Value = Value};
        }
    }

    public struct String : Object, Hashable {
        public string Value;

        public ObjectType Type() {
            return ObjectType.STRING_OBJ;
        }

        public string Inspect() {
            return Value;
        }

        public HashKey HashKey() {
            return new HashKey {Type = Type(), Value = Value.GetHashCode()};
        }
    }

    public struct Boolean : Object, Hashable {
        public bool Value;

        public ObjectType Type() {
            return ObjectType.BOOLEAN_OBJ;
        }

        public string Inspect() {
            return Value.ToString();
        }

        public HashKey HashKey() {
            int value;
            if (Value) {
                value = 1;
            }
            else {
                value = 0;
            }

            return new HashKey {Type = Type(), Value = value};
        }
    }

    public struct Null : Object {
        public ObjectType Type() {
            return ObjectType.NULL_OBJ;
        }

        public string Inspect() {
            return "null";
        }
    }

    public struct ReturnValue : Object {
        public Object Value;

        public ObjectType Type() {
            return ObjectType.RETURN_VALUE_OBJ;
        }

        public string Inspect() {
            return Value.Inspect();
        }
    }

    public struct Error : Object {
        public string Message;

        public ObjectType Type() {
            return ObjectType.ERROR_OBJ;
        }

        public string Inspect() {
            return "ERROR: " + Message;
        }
    }

    public struct Function : Object {
        public List<Identifier> Parameters;
        public BlockStatement Body;
        public Environment Env;

        public ObjectType Type() {
            return ObjectType.FUNCTION_OBJ;
        }

        public string Inspect() {
            var buffer = "";

            var Params = Parameters.Select(parameter => parameter.String()).ToList();

            buffer += "fn";
            buffer += "(";
            buffer += string.Join(", ", Params);
            buffer += ") {\n";
            buffer += Body.String();
            buffer += "\n";

            return buffer;
        }
    }

    public struct Builtin : Object {
        public Func<List<Object>, Object> Fn;

        public ObjectType Type() {
            return ObjectType.BUILTIN_OBJ;
        }

        public string Inspect() {
            return "builtin function";
        }
    }

    public struct Array : Object {
        public List<Object> Elements;

        public ObjectType Type() {
            return ObjectType.ARRAY_OBJ;
        }

        public string Inspect() {
            var buffer = "";
            var elements = Elements.Select(el => el.Inspect()).ToList();

            buffer += "[";
            buffer += string.Join(", ", elements);
            buffer += "]";

            return buffer;
        }
    }

    public struct Hash : Object {
        public Dictionary<HashKey, HashPair> Pairs;

        public ObjectType Type() {
            return ObjectType.HASH_OBJ;
        }

        public string Inspect() {
            var buffer = "";

            var pairs = Pairs.Select(pair => pair.Value.Key.Inspect() + ":" + pair.Value.Value.Inspect()).ToList();
            buffer += "{";
            buffer += string.Join(", ", pairs);
            buffer += "}";

            return buffer;
        }
    }

    public struct HashPair {
        public Object Key;
        public Object Value;
    }

    public struct HashKey {
        public ObjectType Type;
        public long Value;
    }


    public enum ObjectType {
        INTEGER_OBJ,
        BOOLEAN_OBJ,
        NULL_OBJ,
        RETURN_VALUE_OBJ,
        ERROR_OBJ,
        FUNCTION_OBJ,
        STRING_OBJ,
        ARRAY_OBJ,
        BUILTIN_OBJ,
        HASH_OBJ
    }
}