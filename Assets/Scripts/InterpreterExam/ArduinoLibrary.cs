using System;
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace InterpreterExam {
    public class ArduinoLibrary : LibraryClassBase {
        private enum PIN_MODE {
            INPUT,
            INPUT_PULLUP,
            OUTPUT
        }

        private enum PIN_STATE {
            LOW,
            HIGH,
        }

        public override void Initialize() {
            Fields.Add("pinMode", new HostFunction {Fn = pinMode});
            Fields.Add("digitalWrite", new HostFunction {Fn = digitalWrite});
            Fields.Add("delay", new HostFunction {Fn = delay});
            Fields.Add("INPUT", new Integer {Value = (long)PIN_MODE.INPUT});
            Fields.Add("INPUT_PULLUP", new Integer {Value = (long)PIN_MODE.INPUT_PULLUP});
            Fields.Add("OUTPUT", new Integer {Value = (long)PIN_MODE.OUTPUT});
            Fields.Add("LOW", new Integer {Value = (long)PIN_STATE.LOW});
            Fields.Add("HIGH", new Integer {Value = (long)PIN_STATE.HIGH});
        }

        public Object pinMode(List<Object> args) {
            if (args.Count != 2) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 2");
            }

            var pinIndex = (Integer)args[0];
            var Mode = (Integer)args[1];
            Debug.Log($"{pinIndex.Value} is setted to {(PIN_MODE)Mode.Value}");
            return null;
        }

        public Object digitalWrite(List<Object> args) {
            if (args.Count != 2) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 2");
            }

            var pinIndex = (Integer)args[0];
            var state = (Integer)args[1];
            Debug.Log($"{pinIndex.Value} is writed {(PIN_STATE)state.Value}");

            return null;
        }

        public Object delay(List<Object> args) {
            if (args.Count != 1) {
                return Evaluator.newError($"wrong number of arguments. got {args.Count}, want = 2");
            }

            if (!(args[0] is Integer)) {
                return Evaluator.newError(
                    $"wrong type of arguments. got {args[0].Type()}, want = {ObjectType.INTEGER_OBJ}");
            }

            var milliseconds = (Integer)args[0];

            Thread.Sleep((int)milliseconds.Value);
            Debug.Log($"delayed {milliseconds.Value} milliseconds");

            return null;
        }

        private IEnumerator WaitAndPrint(float waitTime) {
            while (true) {
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}
