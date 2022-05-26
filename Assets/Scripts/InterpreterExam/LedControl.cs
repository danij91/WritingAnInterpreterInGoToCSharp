using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class LedControl : LibraryClassBase {
        public override void Initialize() {
            Header.Add("LedControl", new HostFunction {Fn = LedControlClass});
        }

        private Object LedControlClass(List<Object> args) {
            if (args.Count < 4) {
                return Evaluator.newError("argument is not 4");
            }

            var ledCtrlClass = new Class{env = new Environment()};
            ledCtrlClass.env.Declare("setRow", new HostFunction {Fn = SetRow}, ObjectType.INTEGER_OBJ);
            return ledCtrlClass;
        }

        private Object SetRow(List<Object> args) {
            foreach (var arg in args) {
                Debug.Log(arg);
            }

            return new Integer {Value = 10};
        }
    }
}
