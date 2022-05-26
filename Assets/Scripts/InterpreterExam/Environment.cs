using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class Environment {
        private Dictionary<string, Object> store = new Dictionary<string, Object>();
        private Dictionary<string, ObjectType> types = new Dictionary<string, ObjectType>();

        public Environment outer { get; set; }

        public Object Get(string name) {
            if (!store.ContainsKey(name)) {
                if (outer != null) {
                    return outer.Get(name);
                }

                return Evaluator.newError($"'{name}' was not declared in this scope");
            }

            return store[name];
        }

        public Object Declare(string name, Object val, ObjectType type) {
            if (store.ContainsKey(name)) {
                return Evaluator.newError($"'{name}' was already declared in this scope");
            }

            switch (type) {
                case ObjectType.INTEGER_OBJ:
                    val = ParseInt(val);
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    val = PalseFloat(val);
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    val = ParseBool(val);
                    break;
                case ObjectType.CHARACTER_OBJ:
                    val = ParseChar(val);
                    break;
                case ObjectType.CLASS_OBJ:
                    val = ParseClass(val);
                    break;
                case ObjectType.VOID_OBJ:
                    break;
                default:
                    return Evaluator.newError($"invalid data type : '{type}'");
            }

            if (val.Type() == ObjectType.ERROR_OBJ) {
                return val;
            }

            types.Add(name, val.Type());
            store.Add(name, val);

            return val;
        }

        public Object Set(string name, Object val) {
            var isExist = store.ContainsKey(name);

            if (!isExist) {
                if (outer != null) {
                    return outer.Set(name, val);
                }

                return Evaluator.newError($"'{name}' was not declared in this scope");
            }

            var orgType = types[name];

            switch (orgType) {
                case ObjectType.INTEGER_OBJ:
                    val = ParseInt(val);
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    val = PalseFloat(val);
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    val = ParseBool(val);
                    break;
                case ObjectType.CHARACTER_OBJ:
                    val = ParseChar(val);
                    break;
                case ObjectType.CLASS_OBJ:
                    val = ParseClass(val);
                    break;
                case ObjectType.VOID_OBJ:
                    break;
            }

            if (val.Type() == ObjectType.ERROR_OBJ) {
                return val;
            }

            store[name] = val;

            return val;
        }

        private Object ParseInt(Object val) {
            switch (val.Type()) {
                case ObjectType.FUNCTION_OBJ:
                    var functionObj = (Function)val;
                    if (functionObj.ReturnType != ObjectType.INTEGER_OBJ) {
                        return Evaluator.newError($"invalid data type : '{val.Type()}'");
                    }
                    break;
                case ObjectType.HOST_FUNCTION_OBJ:
                    break;
                case ObjectType.INTEGER_OBJ:
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    var floatObj = (RealNumber)val;
                    val = new Integer {Value = (int)floatObj.Value};
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    var booleanObj = (Boolean)val;
                    val = new Integer {Value = booleanObj.Value ? 1 : 0};
                    break;
                case ObjectType.CHARACTER_OBJ:
                    var characterObj = (Character)val;
                    val = new RealNumber {Value = characterObj.Value};
                    break;
                default:
                    return Evaluator.newError($"invalid conversion from '{val.Type()}' to '{ObjectType.INTEGER_OBJ}'");
            }

            return val;
        }

        private Object PalseFloat(Object val) {
            switch (val.Type()) {
                case ObjectType.FUNCTION_OBJ:
                    var functionObj = (Function)val;
                    if (functionObj.ReturnType != ObjectType.REAL_NUMBER_OBJ) {
                        return Evaluator.newError($"invalid data type : '{val.Type()}'");
                    }

                    break;
                case ObjectType.HOST_FUNCTION_OBJ:
                    break;
                case ObjectType.INTEGER_OBJ:
                    var integerObj = (Integer)val;
                    val = new RealNumber {Value = integerObj.Value};
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    var booleanObj = (Boolean)val;
                    val = new RealNumber {Value = booleanObj.Value ? 1 : 0};
                    break;
                case ObjectType.CHARACTER_OBJ:
                    var characterObj = (Character)val;
                    val = new RealNumber {Value = characterObj.Value};
                    break;
                default:
                    return Evaluator.newError(
                        $"invalid conversion from '{val.Type()}' to '{ObjectType.REAL_NUMBER_OBJ}'");
            }

            return val;
        }

        private Object ParseChar(Object val) {
            switch (val.Type()) {
                case ObjectType.FUNCTION_OBJ:
                    var functionObj = (Function)val;
                    if (functionObj.ReturnType != ObjectType.CHARACTER_OBJ) {
                        return Evaluator.newError($"invalid data type : '{val.Type()}'");
                    }

                    break;
                case ObjectType.HOST_FUNCTION_OBJ:
                    break;
                case ObjectType.INTEGER_OBJ:
                    var integerObj = (Integer)val;
                    val = new Character {Value = (char)integerObj.Value};
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    var realNumberObj = (RealNumber)val;
                    val = new Character {Value = (char)realNumberObj.Value};
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    var booleanObj = (Boolean)val;
                    val = new Character {Value = booleanObj.Value ? (char)1 : (char)0};
                    break;
                case ObjectType.CHARACTER_OBJ:
                    break;
                default:
                    return Evaluator.newError(
                        $"invalid conversion from '{val.Type()}' to '{ObjectType.CHARACTER_OBJ}'");
            }

            return val;
        }

        private Object ParseBool(Object val) {
            switch (val.Type()) {
                case ObjectType.FUNCTION_OBJ:
                    var functionObj = (Function)val;
                    if (functionObj.ReturnType != ObjectType.BOOLEAN_OBJ) {
                        return Evaluator.newError($"invalid data type : '{val.Type()}'");
                    }

                    break;
                case ObjectType.HOST_FUNCTION_OBJ:
                    break;
                case ObjectType.INTEGER_OBJ:
                    var integerObj = (Integer)val;
                    val = new Boolean {Value = integerObj.Value != 0};
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    var realNumberObj = (RealNumber)val;
                    val = new Boolean {Value = realNumberObj.Value != 0};
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    break;
                case ObjectType.CHARACTER_OBJ:
                    var characterObj = (Character)val;
                    val = new Boolean {Value = characterObj.Value != 0};
                    break;
                default:
                    return Evaluator.newError(
                        $"invalid conversion from '{val.Type()}' to '{ObjectType.BOOLEAN_OBJ}'");
            }

            return val;
        }
        
        private Object ParseClass(Object val) {
            if (val.Type() != ObjectType.CLASS_OBJ) {
                return Evaluator.newError(
                    $"invalid conversion from '{val.Type()}' to '{ObjectType.HOST_FUNCTION_OBJ}'");
            }

            return val;
        }

        private Object ParseHostFunction(Object val) {
            if (val.Type() != ObjectType.HOST_FUNCTION_OBJ) {
                return Evaluator.newError(
                    $"invalid conversion from '{val.Type()}' to '{ObjectType.HOST_FUNCTION_OBJ}'");
            }

            return val;
        }
    }
}
