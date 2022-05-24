using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InterpreterExam {
    public static class Builtins {
        public static Dictionary<string, Object> builtins = new Dictionary<string, Object> {
            {"len", new Builtin {Fn = Len}},
            {"first", new Builtin {Fn = First}},
            {"last", new Builtin {Fn = Last}},
            {"rest", new Builtin {Fn = Rest}},
            {"push", new Builtin {Fn = Push}},
            {"puts", new Builtin {Fn = Puts}},
        };

        private static Object Len(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            switch (args[0]) {
                case Array array:
                    return new Integer {Value = array.Elements.Count};
                case String str:
                    return new Integer {Value = str.Value.Length};
                default:
                    return Evaluator.newError($"argument to len not supported, got {args[0].Type()}");
            }
        }

        private static Object First(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            if (args[0].Type() != ObjectType.ARRAY_OBJ) {
                return Evaluator.newError($"argument to first must be ARRAY_OBJ, got {args[0].Type()}");
            }

            var arr = (Array)args[0];

            return arr.Elements.Count > 0 ? arr.Elements.First() : Evaluator.NULL;
        }

        private static Object Last(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            if (args[0].Type() != ObjectType.ARRAY_OBJ) {
                return Evaluator.newError($"argument to first must be ARRAY_OBJ, got {args[0].Type()}");
            }

            var arr = (Array)args[0];

            return arr.Elements.Count > 0 ? arr.Elements.Last() : Evaluator.NULL;
        }

        private static Object Rest(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            if (args[0].Type() != ObjectType.ARRAY_OBJ) {
                return Evaluator.newError($"argument to first must be ARRAY_OBJ, got {args[0].Type()}");
            }

            var arr = (Array)args[0];
            var length = arr.Elements.Count;

            if (length > 0) {
                var newElements = arr.Elements.ToList();
                newElements.RemoveAt(0);
                return new Array {Elements = newElements};
            }

            return Evaluator.NULL;
        }

        private static Object Push(List<Object> args) {
            if (args.Count != 2) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            if (args[0].Type() != ObjectType.ARRAY_OBJ) {
                return Evaluator.newError($"argument to first must be ARRAY_OBJ, got {args[0].Type()}");
            }

            var arr = (Array)args[0];

            var newElements = arr.Elements.ToList();
            newElements.Add(args[1]);

            return new Array {Elements = newElements};
        }

        private static Object Puts(List<Object> args) {
            foreach (var arg in args) {
                Debug.Log(arg.Inspect());
            }

            return Evaluator.NULL;
        }
    }
}
