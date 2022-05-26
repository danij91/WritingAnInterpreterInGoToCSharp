using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InterpreterExam {
    public class Builtins {
        public Builtins() {
            builtins = new Dictionary<string, Object> {
                {"len", new HostFunction {Fn = Len}},
                {"first", new HostFunction {Fn = First}},
                {"last", new HostFunction {Fn = Last}},
                {"rest", new HostFunction {Fn = Rest}},
                {"push", new HostFunction {Fn = Push}},
                {"puts", new HostFunction {Fn = Puts}},
            };
        }

        public Dictionary<string, Object> builtins;

        private Object Len(List<Object> args) {
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

        private Object First(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            if (args[0].Type() != ObjectType.ARRAY_OBJ) {
                return Evaluator.newError($"argument to first must be ARRAY_OBJ, got {args[0].Type()}");
            }

            var arr = (Array)args[0];

            return arr.Elements.Count > 0 ? arr.Elements.First() : Evaluator.NULL;
        }

        private Object Last(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 1");
            }

            if (args[0].Type() != ObjectType.ARRAY_OBJ) {
                return Evaluator.newError($"argument to first must be ARRAY_OBJ, got {args[0].Type()}");
            }

            var arr = (Array)args[0];

            return arr.Elements.Count > 0 ? arr.Elements.Last() : Evaluator.NULL;
        }

        private Object Rest(List<Object> args) {
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

        private Object Push(List<Object> args) {
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

        private Object Puts(List<Object> args) {
            foreach (var arg in args) {
                Debug.Log(arg.Inspect());
            }

            return Evaluator.NULL;
        }
    }
}
