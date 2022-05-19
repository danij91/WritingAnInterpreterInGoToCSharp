using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class Environment {
        private Dictionary<string, Object> store = new Dictionary<string, Object>();
        private Dictionary<string, ObjectType> types = new Dictionary<string, ObjectType>();
        public Environment outer { get; set; }

        public Tuple<Object, bool> Get(string name) {
            var isExist = store.ContainsKey(name);

            if (!isExist && outer != null) {
                return outer.Get(name);
            }

            return new Tuple<Object, bool>(isExist ? store[name] : null, isExist);
        }
        
        public Object Set(string name, Object val, ObjectType type) {
            switch (type) {
                case ObjectType.INTEGER_OBJ:
                    val = ParseInt(name, val);
                    break;
                case ObjectType.REAL_NUMBER_OBJ:
                    val = PalseFloat(name, val);
                    break;
                case ObjectType.BOOLEAN_OBJ:
                    val = ParseBool(name, val);
                    break;
                case ObjectType.CHARACTER_OBJ:
                    val = ParseChar(name, val);
                    break;
                default:
                    return Evaluator.newError($"invalid data type : '{val.Type()}'");
            }

            if (val.Type() == ObjectType.ERROR_OBJ) {
                return val;
            }

            if (store.ContainsKey(name) && types.ContainsKey(name)) {
                store[name] = val;
            }
            else {
                types.Add(name, val.Type());
                store.Add(name, val);
            }

            return val;
        }

        private Object ParseInt(string name, Object val) {
            switch (val.Type()) {
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

        private Object PalseFloat(string name, Object val) {
            switch (val.Type()) {
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

        private Object ParseChar(string name, Object val) {
            switch (val.Type()) {
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

        private Object ParseBool(string name, Object val) {
            switch (val.Type()) {
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
    }
}
