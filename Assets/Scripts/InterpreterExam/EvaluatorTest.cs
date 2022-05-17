using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class EvaluatorTest {
        public void TestBuiltinFunctions() {
            Dictionary<string, object> tests = new Dictionary<string, object> {
                {"len(\"\")", 0},
                {"len(\"four\")", 4},
                {"len(\"hello world\")", 11},
                {"len(1)", "argument to \'len\' not supported, got INTEGER"},
                {"len(\"one\",\"two\")", "wrong number of arguments. got=2, want=1"},
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);

                switch (test.Value) {
                    case int integer:
                        testIntegerObject(evaluated, integer);
                        continue;
                    case string str:
                        if (!(evaluated is Error)) {
                            Debug.LogError($"object is not Error. got{evaluated}");
                            continue;
                        }

                        var errObj = (Error)evaluated;
                        if (errObj.Message != str) {
                            Debug.LogError($"wrong error message. expected={str}, got={errObj.Message}");
                        }

                        continue;
                }
            }
        }

        public void TestStringConcatenation() {
            var input = "\"Hello\" + \" \" + \"World!\"";
            var evaluated = testEval(input);

            if (!(evaluated is String)) {
                Debug.LogError($"object is not String. got {evaluated.Type()}");
                return;
            }

            var str = (String)evaluated;
            if (str.Value != "Hello World!") {
                Debug.LogError($"string has wrong value. got={str.Value}");
            }
        }

        public void TestStringLiteral() {
            var input = "\"Hello World!\"";

            var evaluated = testEval(input);

            if (!(evaluated is String)) {
                Debug.LogError($"object is not String. got {evaluated.Type()}");
                return;
            }

            var str = (String)evaluated;
            if (str.Value != "Hello World!") {
                Debug.LogError($"string has wrong value. got={str.Value}");
            }
        }

        public void TestFunctionApplication() {
            Dictionary<string, long> tests = new Dictionary<string, long> {
                {"let identity = fn(x) { x; }; identity(5);", 5},
                {"let identity = fn(x) { return x; }; identity(5);", 5},
                {"let double = fn(x) { x * 2; }; double(5);", 10},
                {"let add = fn(x, y) { x + y; }; add(5, 5);", 10},
                {"let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", 20},
                {"fn(x { x;}(5)", 5},
            };

            foreach (var test in tests) {
                testIntegerObject(testEval(test.Key), test.Value);
            }
        }

        public void TestFunctionObject() {
            string input = "fn(x) { x + 2; };";

            var evaluated = testEval(input);
            if (!(evaluated is Function)) {
                Debug.LogError($"object is not Function. got{evaluated}");
            }

            Function fn = (Function)evaluated;

            if (fn.Parameters.Count != 1) {
                Debug.LogError($"function has wrong parameters. Parameters count{fn.Parameters.Count}");
            }

            if (fn.Parameters[0].String() != "x") {
                Debug.LogError($"parameter is not \'x\'. got= {fn.Parameters[0].String()}");
            }

            var expectedBody = "(x + 2)";

            if (fn.Body.String() != expectedBody) {
                Debug.LogError($"body is not \'{expectedBody}\'. got= {fn.Body.String()}");
            }
        }

        public void TestLetStatements() {
            Dictionary<string, long> tests = new Dictionary<string, long> {
                {"let a = 5; a;", 5},
                {"let a = 5*5; a;", 25},
                {"let a = 5; let b = a; b;", 5},
                {"let a = 5; let b = a; let c = a + b + 5; c;", 15},
            };

            foreach (var test in tests) {
                testIntegerObject(testEval(test.Key), test.Value);
            }
        }

        public void TestErrorHandling() {
            Dictionary<string, string> tests = new Dictionary<string, string> {
                {"5 + true;", "type mismatch: INTEGER_OBJ + BOOLEAN_OBJ"},
                {"5 + true; 5;", "type mismatch: INTEGER_OBJ + BOOLEAN_OBJ"},
                {"-true", "unknown operator: -BOOLEAN_OBJ"},
                {"true + false;", "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"},
                {"5; true + false; 5;", "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"},
                {"if (10 > 1) { true + false;}", "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"}, {
                    @"if(10>1){
    if(10>1){
        return true + false;
    }

    return 1;
}",
                    "unknown operator: BOOLEAN_OBJ + BOOLEAN_OBJ"
                },
                {"foobar", "identifier not found: foobar"}
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);
                if (!(evaluated is Error)) {
                    Debug.LogError($"no error object returned. got{evaluated}");
                    continue;
                }

                Error errObj = (Error)evaluated;

                if (errObj.Message != test.Value) {
                    Debug.LogError($"wrong error message. expected {test.Value}, get = {errObj.Message}");
                }
            }
        }

        public void TestEvalIntegerExpression() {
            Dictionary<string, long> tests = new Dictionary<string, long> {
                {"5", 5},
                {"10", 10},
                {"-5", -5},
                {"-10", -10},
                {"5 + 5 + 5 + 5 - 10", 10},
                {"2 * 2 * 2 * 2 * 2", 32},
                {"-50 + 100 + -50", 0},
                {"5 * 2 + 10", 20},
                {"5 + 2 * 10", 25},
                {"20 + 2 * -10", 0},
                {"50 / 2 * 2 + 10", 60},
                {"2 *(5 + 10)", 30},
                {"3 * 3 * 3 + 10", 37},
                {"3 * (3 * 3) +10", 37},
                {"(5 + 10 * 2 + 15 / 3) * 2 + -10", 50},
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);
                testIntegerObject(evaluated, test.Value);
            }
        }

        public void TestEvalBooleanExpression() {
            Dictionary<string, bool> tests = new Dictionary<string, bool> {
                {"true == true", true},
                {"false == false", true},
                {"true == false", false},
                {"true != false", true},
                {"false != true", true},
                {"(1 < 2) == true", true},
                {"(1 < 2) == false", false},
                {"(1 > 2) == true", false},
                {"(1 > 2) == false", true},
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);
                testBooleanObject(evaluated, test.Value);
            }
        }

        public void TestEvalBangOperator() {
            Dictionary<string, bool> tests = new Dictionary<string, bool> {
                {"!true", false},
                {"!false", true},
                {"!5", false},
                {"!!true", true},
                {"!!false", false},
                {"!!5", true}
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);
                testBooleanObject(evaluated, test.Value);
            }
        }

        private Object testEval(string input) {
            var l = new Lexer(input);
            var p = new Parser(l);
            var program = p.ParseProgram();
            var e = new Evaluator();
            var env = new Environment();
            return e.Eval(program, env);
        }

        public void TestIfElseExpressions() {
            Dictionary<string, object> tests = new Dictionary<string, object> {
                {"if (true) { 10 }", 10},
                {"if (false) { 10 }", null},
                {"if (1) { 10 }", 10},
                {"if (1 < 2) { 10 }", 10},
                {"if (1 > 2) { 10 }", null},
                {"if (1 > 2) { 10 } else { 20 }", 20},
                {"if (1 < 2) { 10 } else { 20 }", 10},
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);
                if (test.Value is int) {
                    testIntegerObject(evaluated, Convert.ToInt64(test.Value));
                }
                else {
                    testNullObject(evaluated);
                }
            }
        }

        public void TestReturnStatement() {
            Dictionary<string, long> tests = new Dictionary<string, long> {
                {"return 10;", 10},
                {"return 10; 9;", 10},
                {"return 2*5; 9;", 10},
                {"9; return 10; 9;", 10}, {
                    @"if(10>1){
    if(10>1){
        return 10;
    }
    return 1;
}",
                    10
                }
            };

            foreach (var test in tests) {
                var evaluated = testEval(test.Key);
                testIntegerObject(evaluated, test.Value);
            }
        }

        private bool testNullObject(Object obj) {
            if (!obj.Equals(Evaluator.NULL)) {
                Debug.LogError($"object is not NULL. got {obj}");
                return false;
            }

            return true;
        }

        private bool testIntegerObject(Object obj, long expected) {
            Integer result;
            if (obj is Integer integer) {
                result = integer;
            }
            else {
                Debug.LogError($"object is not Integer. got{obj}");
                return false;
            }

            if (result.Value != expected) {
                Debug.LogError($"object has wrong value. got {result.Value}, want {expected}");
                return false;
            }

            return true;
        }

        private bool testBooleanObject(Object obj, bool expected) {
            Boolean result;
            if (obj is Boolean boolean) {
                result = boolean;
            }
            else {
                Debug.LogError($"object is not Boolean. got{obj.Type()}");
                return false;
            }

            if (result.Value != expected) {
                Debug.LogError($"object has wrong value. got {result.Value}, want {expected}");
                return false;
            }

            return true;
        }
    }
}
