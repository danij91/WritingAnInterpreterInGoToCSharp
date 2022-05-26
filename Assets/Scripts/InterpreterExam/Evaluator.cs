using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace InterpreterExam {
    public class Evaluator {
        private static readonly Boolean TRUE = new Boolean {Value = true};
        private static readonly Boolean FALSE = new Boolean {Value = false};

        public static readonly Null NULL = new Null();
        private Dictionary<string, Object> externalClassConstructors = new Dictionary<string, Object>();
        private Builtins btins = new Builtins();

        public void AddLibrary(LibraryClassBase library) {
            foreach (var header in library.Header) {
                externalClassConstructors.Add(header.Key, header.Value);
            }
            
            foreach (var externalFunction in library.Fields) {
                btins.builtins.Add(externalFunction.Key, externalFunction.Value);
            }
        }

        public Object Eval(Node node, Environment env) {
            switch (node) {
                case Program program:
                    return evalProgam(program, env);
                case ExpressionStatement expressionStatement:
                    return Eval(expressionStatement.Expression, env);
                case IntegerLiteral integerLiteral:
                    return new Integer {Value = integerLiteral.Value};
                case CharacterLiteral characterLiteral:
                    return new Character {Value = characterLiteral.Value};
                case BooleanExpression booleanExpression:
                    return nativeBoolToBooleanObject(booleanExpression.Value);
                case RealNumberLiteral realNumberLiteral:
                    return new RealNumber {Value = realNumberLiteral.Value};
                case PrefixExpression prefixExpression:
                    Object prefixRight = Eval(prefixExpression.Right, env);
                    if (isError(prefixRight)) {
                        return prefixRight;
                    }

                    return evalPrefixExpression(prefixExpression, prefixRight, env);
                case InfixExpression infixExpression:
                    return evalInfixExpression(infixExpression, env);
                case PostfixExpression postfixExpression:
                    Object postFixLeft = Eval(postfixExpression.Left, env);
                    if (isError(postFixLeft)) {
                        return postFixLeft;
                    }

                    return evalPostfixExpression(postfixExpression, postFixLeft, env);
                case BlockStatement blockStatement:
                    return evalBlockStatement(blockStatement, env);
                case IfExpression ifExpression:
                    return evalIfExpression(ifExpression, env);
                case IterationExpression forExpression:
                    return evalIterationExpression(forExpression, env);
                case ReturnStatement returnStatement:
                    Object returnVal = Eval(returnStatement.ReturnValue, env);
                    if (isError(returnVal)) {
                        return returnVal;
                    }

                    return new ReturnValue {Value = returnVal};
                case BreakStatement breakStatement:
                    return new Break();
                case AssignStatement assignStatement:
                    return evalAssignStatement(assignStatement, env);
                case InitStatement initStatement:
                    return evalInitStatement(initStatement, env);
                case Identifier identifier:
                    return evalIdentifier(identifier, env);
                case FunctionLiteralExpression functionLiteral:
                    return evalFunctionLiteral(functionLiteral, env);
                case CallExpression callExpression:
                    var function = Eval(callExpression.Function, env);
                    if (isError(function)) {
                        return function;
                    }

                    var args = evalExpressions(callExpression.Arguments, env);
                    if (args.Count == 1 && isError(args[0])) {
                        return args[0];
                    }

                    return applyFunction(function, args);
                case StringLiteral stringLiteral:
                    return new String {Value = stringLiteral.Value};
                case ArrayLiteral arrayLiteral:
                    var elements = evalExpressions(arrayLiteral.Elements, env);
                    if (elements.Count == 1 && isError(elements[0])) {
                        return elements[0];
                    }

                    return new Array {Elements = elements};
                case IndexExpression indexExpression:
                    var left = Eval(indexExpression.Left, env);
                    if (isError(left)) {
                        return left;
                    }

                    var index = Eval(indexExpression.Index, env);

                    if (isError(index)) {
                        return index;
                    }

                    return evalIndexExpression(left, index);
                case HashLiteral hashLiteral:
                    return evalHashLiteral(hashLiteral, env);
                default:
                    return NULL;
            }
        }

        public static Error newError(string format) {
            return new Error {Message = format};
        }

        private Object evalProgam(Program program, Environment env) {
            Object result = null;

            foreach (var statement in program.statements) {
                result = Eval(statement, env);

                switch (result) {
                    case ReturnValue returnValue:
                        return returnValue.Value;
                    case Error error:
                        return error;
                }
            }

            return result;
        }

        private Object evalBlockStatement(BlockStatement block, Environment env) {
            Object result = null;

            foreach (var statement in block.Statements) {
                result = Eval(statement, env);

                if (result is Break) {
                    return result;
                }

                if (result != null) {
                    var rt = result.Type();
                    if (rt == ObjectType.RETURN_VALUE_OBJ || rt == ObjectType.ERROR_OBJ)
                        return result;
                }
            }

            return NULL;
        }

        private Object evalAssignStatement(AssignStatement assignStatement, Environment env) {
            Object assignVal = Eval(assignStatement.Value, env);
            if (isError(assignVal)) {
                return assignVal;
            }

            var getValue = env.Get(assignStatement.Name.Value);

            if (isError(getValue)) {
                return getValue;
            }

            if (assignStatement.assignOperator != "=" && !string.IsNullOrEmpty(assignStatement.assignOperator)) {
                var orgVal = GetNumberObjectValue(getValue);
                var diffVal = GetNumberObjectValue(assignVal);

                switch (assignStatement.assignOperator) {
                    case "+=":
                        assignVal = new RealNumber {Value = orgVal + diffVal};
                        break;
                    case "-=":
                        assignVal = new RealNumber {Value = orgVal - diffVal};
                        break;
                    case "*=":
                        assignVal = new RealNumber {Value = orgVal * diffVal};
                        break;
                    case "/=":
                        assignVal = new RealNumber {Value = orgVal / diffVal};
                        break;
                }
            }

            return env.Set(assignStatement.Name.Value, assignVal);
        }

        private Object evalInitStatement(InitStatement initStatement, Environment env) {
            Object letVal = Eval(initStatement.Value, env);
            if (isError(letVal)) {
                return letVal;
            }

            ObjectType dataType;

            switch (initStatement.Token.Type) {
                case TokenType.INT:
                    dataType = ObjectType.INTEGER_OBJ;
                    break;
                case TokenType.CHAR:
                    dataType = ObjectType.CHARACTER_OBJ;
                    break;
                case TokenType.BOOL:
                    dataType = ObjectType.BOOLEAN_OBJ;
                    break;
                case TokenType.FLOAT:
                    dataType = ObjectType.REAL_NUMBER_OBJ;
                    break;
                case TokenType.VOID:
                    dataType = ObjectType.VOID_OBJ;
                    break;
                case TokenType.CLASS:
                    dataType = ObjectType.CLASS_OBJ;
                    break;
                default:
                    return NULL;
            }

            return env.Declare(initStatement.Name.Value, letVal, dataType);
        }

        private Boolean nativeBoolToBooleanObject(bool input) {
            return input ? TRUE : FALSE;
        }

        private Object evalPrefixExpression(PrefixExpression prefixExpression, Object right, Environment env) {
            switch (prefixExpression.Operator) {
                case "!":
                    return evalBangOperatorExpression(right);
                case "-":
                    return evalMinusPrefixOperatorExpression(right);
                case "++":
                    return evalIncreasePrefixOperatorExpression(prefixExpression.Right, right, env);
                case "--":
                    return evalDecreasePrefixOperatorExpression(prefixExpression.Right, right, env);
                default:
                    return newError($"unknown operator: {prefixExpression.Operator}{right.Type()}");
            }
        }

        private Object evalBangOperatorExpression(Object right) {
            if (right.Equals(TRUE)) {
                return FALSE;
            }

            if (right.Equals(FALSE)) {
                return TRUE;
            }

            if (right.Equals(NULL)) {
                return TRUE;
            }

            return FALSE;
        }

        private Object evalMinusPrefixOperatorExpression(Object right) {
            if (!IsNumber(right)) {
                return newError($"unknown operator: -{right.Type()}");
            }

            var value = GetNumberObjectValue(right);
            return new RealNumber {Value = -value};
        }

        private Object evalIncreasePrefixOperatorExpression(Expression rightExpression, Object right, Environment env) {
            if (!IsNumber(right) || !(rightExpression is Identifier)) {
                return newError($"unknown operator: ++{right.Type()}");
            }

            var ident = (Identifier)rightExpression;

            var value = GetNumberObjectValue(right);
            right = new RealNumber {Value = value + 1};
            env.Set(ident.Value, right);
            return right;
        }

        private Object evalDecreasePrefixOperatorExpression(Expression rightExpression, Object right, Environment env) {
            if (!IsNumber(right) || !(rightExpression is Identifier)) {
                return newError($"unknown operator: ++{right.Type()}");
            }

            var ident = (Identifier)rightExpression;

            var value = GetNumberObjectValue(right);
            right = new RealNumber {Value = value - 1};
            env.Set(ident.Value, right);
            return right;
        }

        private Object evalInfixExpression(InfixExpression infixExpression, Environment env) {
            var Operator = infixExpression.Operator;
            var left = Eval(infixExpression.Left, env);
            if (isError(left)) {
                return left;
            }

            if (left.Type() == ObjectType.CLASS_OBJ) {
                return evalClassInfixExpression(Operator, left, infixExpression.Right);
            }

            Object right = Eval(infixExpression.Right, env);
            if (isError(right)) {
                return right;
            }

            if (IsNumber(left) && IsNumber(right)) {
                return evalNumberInfixExpression(Operator, left, right);
            }

            if (left.Type() == ObjectType.STRING_OBJ && right.Type() == ObjectType.STRING_OBJ) {
                return evalStringInfixExpression(Operator, left, right);
            }


            if (Operator == "==") {
                return nativeBoolToBooleanObject(left.Equals(right));
            }

            if (Operator == "!=") {
                return nativeBoolToBooleanObject(!left.Equals(right));
            }

            if (left.Type() != right.Type()) {
                return newError($"type mismatch: {left.Type()} {Operator} {right.Type()}");
            }

            return newError($"unknown operator: {left.Type()} {Operator} {right.Type()}");
        }

        private Object evalNumberInfixExpression(string Operator, Object left, Object right) {
            float leftVal = GetNumberObjectValue(left);
            float rightVal = GetNumberObjectValue(right);

            switch (Operator) {
                case "+":
                    return new RealNumber {Value = leftVal + rightVal};
                case "-":
                    return new RealNumber {Value = leftVal - rightVal};
                case "*":
                    return new RealNumber {Value = leftVal * rightVal};
                case "/":
                    return new RealNumber {Value = leftVal / rightVal};
                case "<":
                    return nativeBoolToBooleanObject(leftVal < rightVal);
                case ">":
                    return nativeBoolToBooleanObject(leftVal > rightVal);
                case "==":
                    return nativeBoolToBooleanObject(leftVal == rightVal);
                case "!=":
                    return nativeBoolToBooleanObject(leftVal != rightVal);
                default:
                    return newError($"unknown operator: {left.Type()} {Operator} {right.Type()}");
            }
        }

        private Object evalStringInfixExpression(string Operator, Object left, Object right) {
            if (Operator != "+")
                return newError($"unknown operator: {left.Type()} {Operator} {right.Type()}");

            var leftVal = ((String)left).Value;
            var rightVal = ((String)right).Value;

            return new String {Value = leftVal + rightVal};
        }

        private Object evalClassInfixExpression(string Operator, Object left, Expression right) {
            if (Operator != "." || !(right is Identifier))
                return newError($"unknown operator: {left.Type()} {Operator} {right}");

            var classEnv = ((Class)left).env;
            return classEnv.Get(((Identifier)right).Value);
        }

        private Object evalPostfixExpression(PostfixExpression postfixExpression, Object left, Environment env) {
            switch (postfixExpression.Operator) {
                case "++":
                    return evalIncreasePostfixOperatorExpression(postfixExpression.Left, left, env);
                case "--":
                    return evalDecreasePostfixOperatorExpression(postfixExpression.Left, left, env);
                default:
                    return newError($"unknown operator: {postfixExpression.Operator}{left.Type()}");
            }
        }

        private Object evalIncreasePostfixOperatorExpression(Expression rightExpression, Object left, Environment env) {
            if (!IsNumber(left) || !(rightExpression is Identifier)) {
                return newError($"unknown operator: ++{left.Type()}");
            }

            var ident = (Identifier)rightExpression;

            var value = GetNumberObjectValue(left);
            env.Set(ident.Value, new RealNumber {Value = value + 1});
            return left;
        }

        private Object evalDecreasePostfixOperatorExpression(Expression leftExpression, Object left, Environment env) {
            if (!IsNumber(left) || !(leftExpression is Identifier)) {
                return newError($"unknown operator: --{left.Type()}");
            }

            var ident = (Identifier)leftExpression;

            var value = GetNumberObjectValue(left);
            env.Set(ident.Value, new RealNumber {Value = value - 1});
            return left;
        }

        private Object evalIfExpression(IfExpression ie, Environment outer) {
            var env = NewEnclosedEnvironment(outer);

            Object condition = Eval(ie.Condition, env);

            if (isError(condition)) {
                return condition;
            }

            if (isTruthy(condition)) {
                return Eval(ie.Consequence, env);
            }

            if (ie.Alternative != null) {
                return Eval(ie.Alternative, env);
            }

            return NULL;
        }

        private Object evalIterationExpression(IterationExpression fe, Environment outer) {
            var env = NewEnclosedEnvironment(outer);
            Eval(fe.InitStatement, env);
            Object condition = Eval(fe.Condition, env);
            Eval(fe.Change, env);
            int a = 0;
            while (isTruthy(condition) || condition.Equals(NULL)) {
                a++;
                if (a > 100) {
                    return newError("Stack overflow");
                    break;
                }

                Eval(fe.Change, env);
                var currentBlock = Eval(fe.IterationBlockStatement, env);

                if (currentBlock is Break) {
                    break;
                }

                condition = Eval(fe.Condition, env);
            }

            return NULL;
        }

        private List<Object> evalExpressions(List<Expression> exps, Environment env) {
            var results = new List<Object>();

            foreach (var exp in exps) {
                var evaluated = Eval(exp, env);
                if (isError(evaluated)) {
                    var errorResult = new List<Object>();
                    errorResult.Add(evaluated);
                    return errorResult;
                }

                results.Add(evaluated);
            }

            return results;
        }

        private bool isTruthy(Object obj) {
            if (obj.Equals(NULL))
                return false;

            if (obj.Equals(TRUE))
                return true;

            if (obj.Equals(FALSE))
                return false;

            return true;
        }

        public bool isError(Object obj) {
            if (obj != null) {
                return obj.Type() == ObjectType.ERROR_OBJ;
            }

            return false;
        }

        private Object evalIdentifier(Identifier node, Environment env) {
            var getValue = env.Get(node.Value);
            if (!isError(getValue)) {
                return getValue;
            }

            if (btins.builtins.ContainsKey(node.Value)) {
                return btins.builtins[node.Value];
            }

            if (externalClassConstructors.ContainsKey(node.Value)) {
                return externalClassConstructors[node.Value];
            }

            return newError($"identifier not found: {node.Value}");
        }

        private Object evalIndexExpression(Object left, Object index) {
            if (left.Type() == ObjectType.ARRAY_OBJ && index.Type() == ObjectType.INTEGER_OBJ) {
                return evalArrayIndexExpression(left, index);
            }

            if (left.Type() == ObjectType.HASH_OBJ)
                return evalHashIndexExpression(left, index);

            return newError($"index operator not supported: {left.Type()}");
        }

        private Object evalArrayIndexExpression(Object array, Object index) {
            var arrayObject = (Array)array;
            var idx = ((Integer)index).Value;
            var max = arrayObject.Elements.Count - 1;
            if (idx < 0 || idx > max) {
                return NULL;
            }

            return arrayObject.Elements[(int)idx];
        }

        private Object evalHashIndexExpression(Object hash, Object index) {
            var hashObject = (Hash)hash;

            if (!(index is Hashable)) {
                return newError($"unusable as hash key: {index.Type()}");
            }

            var key = (Hashable)index;

            return hashObject.Pairs.ContainsKey(key.HashKey()) ? hashObject.Pairs[key.HashKey()].Value : NULL;
        }

        private Object evalHashLiteral(HashLiteral node, Environment env) {
            var pairs = new Dictionary<HashKey, HashPair>();
            foreach (var pair in node.Pairs) {
                var key = Eval(pair.Key, env);

                if (isError(key)) {
                    return key;
                }

                if (!(key is Hashable)) {
                    return newError($"unusable as hash key: {key.Type()}");
                }

                var hashKey = (Hashable)key;

                var value = Eval(pair.Value, env);

                if (isError(value)) {
                    return value;
                }

                var hashed = hashKey.HashKey();
                pairs.Add(hashed, new HashPair {Key = key, Value = value});
            }

            return new Hash {Pairs = pairs};
        }

        private Object evalFunctionLiteral(FunctionLiteralExpression functionLiteral, Environment env) {
            var Params = functionLiteral.Parameters.ToDictionary(parameter => parameter.Key,
                parameter => GetObjectTypeByTokenType(parameter.Value));
            var body = functionLiteral.Body;

            return new Function {
                Parameters = Params, Env = env, Body = body,
                ReturnType = GetObjectTypeByTokenType(functionLiteral.ReturnType)
            };
        }

        private Object applyFunction(Object fn, List<Object> args) {
            switch (fn) {
                case Function function:
                    var extendedEnv = extendedFunctionEnv(function, args);
                    var evaluated = Eval(function.Body, extendedEnv);
                    return unwrapReturnValue(evaluated);
                case HostFunction hostFunction:
                    return hostFunction.Fn(args);
                default:
                    return newError($"not a function: {fn.Type()}");
            }
        }

        private Environment extendedFunctionEnv(Function fn, List<Object> args) {
            var env = NewEnclosedEnvironment(fn.Env);

            int i = 0;
            foreach (var parameter in fn.Parameters) {
                env.Set(parameter.Key.Value, args[i]);
                i++;
            }

            return env;
        }

        private Environment NewEnclosedEnvironment(Environment outer) {
            var env = new Environment();
            env.outer = outer;
            return env;
        }

        private Object unwrapReturnValue(Object obj) {
            if (obj is ReturnValue returnValue) {
                return returnValue.Value;
            }

            return obj;
        }

        private float GetNumberObjectValue(Object numberObject) {
            switch (numberObject) {
                case Integer integer:
                    return integer.Value;
                case RealNumber realNumber:
                    return realNumber.Value;
                case Boolean boolean:
                    return boolean.Value ? 1f : 0f;
                case Character character:
                    return character.Value;
                default:
                    return 0;
            }
        }

        private bool IsNumber(Object numberObject) {
            switch (numberObject.Type()) {
                case ObjectType.INTEGER_OBJ:
                case ObjectType.REAL_NUMBER_OBJ:
                case ObjectType.BOOLEAN_OBJ:
                case ObjectType.CHARACTER_OBJ:
                    return true;
                default:
                    return false;
            }
        }

        private ObjectType GetObjectTypeByTokenType(TokenType tokenType) {
            switch (tokenType) {
                case TokenType.INT:
                    return ObjectType.INTEGER_OBJ;
                case TokenType.FLOAT:
                    return ObjectType.REAL_NUMBER_OBJ;
                case TokenType.BOOL:
                    return ObjectType.BOOLEAN_OBJ;
                case TokenType.CHAR:
                    return ObjectType.CHARACTER_OBJ;
                case TokenType.VOID:
                    return ObjectType.VOID_OBJ;
                default:
                    return ObjectType.ERROR_OBJ;
            }
        }
    }
}
