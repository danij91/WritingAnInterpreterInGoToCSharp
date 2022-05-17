using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public static class ParserTest {
        public static bool IsExpectedStatementCount(this Program program, int expectedCount) {
            var isExpectedCount = expectedCount != program.statements.Count;
            if (isExpectedCount)
                Debug.LogError(
                    $"program.statement does not contain {expectedCount} statements. got {program.statements.Count}");
            return isExpectedCount;
        }

        public static bool IsExpectedArgumentCount(this CallExpression callExpression, int expectedCount) {
            var isExpectedCount = expectedCount != callExpression.Arguments.Count;
            if (isExpectedCount)
                Debug.LogError(
                    $"wrong length of arguments. got {callExpression.Arguments.Count}");
            return isExpectedCount;
        }

        public static bool IsExpectedStatementType<T>(this Statement statement) {
            var isExpressionStatement = statement is T;
            if (!isExpressionStatement) {
                Debug.LogError($"statement is not {typeof(T)} got {statement.GetType()}");
            }

            return isExpressionStatement;
        }

        public static bool IsExpectedExpressionType<T>(this Expression expression) {
            var isExpressionStatement = expression is T;
            if (!isExpressionStatement) {
                Debug.LogError($"expression is not {typeof(T)} got {expression.GetType()}");
            }

            return isExpressionStatement;
        }

        public static bool testIdentifier(this Expression expression, string expectedValue) {
            if (!expression.IsExpectedExpressionType<Identifier>()) {
                return false;
            }

            var ident = (Identifier)expression;

            if (ident.Value != expectedValue) {
                Debug.LogError($"ident.Value is not {expectedValue} got {ident.Value}");
                return false;
            }

            if (ident.TokenLiteral() != expectedValue) {
                Debug.LogError($"ident.TokenLiteral is not {expectedValue} got {ident.TokenLiteral()}");
                return false;
            }

            return true;
        }

        public static bool testInfixExpression<T, T2>(this Expression exp, T left, string Operator, T2 right) {
            if (!IsExpectedExpressionType<InfixExpression>(exp)) {
                return false;
            }

            InfixExpression opExp = (InfixExpression)exp;

            if (!testLiteralExpression(opExp.Left, left)) {
                return false;
            }

            if (opExp.Operator != Operator) {
                Debug.LogError($"exp.Operator is not{Operator}. got {opExp.Operator}");
                return false;
            }

            if (!testLiteralExpression(opExp.Right, right)) {
                return false;
            }

            return true;
        }

        public static bool testLiteralExpression<T>(this Expression exp, T expected) {
            switch (expected) {
                case int i:
                    return testIntegerLiteral(exp, (long)i);
                case long l:
                    return testIntegerLiteral(exp, l);
                case string s:
                    return testIdentifier(exp, s);
                case bool b:
                    return testBooleanLiteral(exp, b);
            }

            Debug.LogError($"type of exp not handled. got{exp}");
            return false;
        }

        public static bool testIntegerLiteral(this Expression il, long value) {
            IntegerLiteral integ;
            if (!IsExpectedExpressionType<IntegerLiteral>(il)) {
                return false;
            }

            integ = (IntegerLiteral)il;

            if (integ.Value != value) {
                Debug.LogError($"integ.Value is not {value} got {integ.Value}");
                return false;
            }

            if (integ.TokenLiteral() != value.ToString()) {
                Debug.LogError($"ident.TokenLiteral is not {value} got {integ.TokenLiteral()}");
                return false;
            }

            return true;
        }

        public static bool testBooleanLiteral(this Expression exp, bool value) {
            BooleanExpression bo;
            if (!IsExpectedExpressionType<BooleanExpression>(exp)) {
                return false;
            }

            bo = (BooleanExpression)exp;

            if (bo.Value != value) {
                Debug.LogError($"bo.Value is not {value} got {bo.Value}");
                return false;
            }

            if (bo.TokenLiteral() != value.ToString()) {
                Debug.LogError($"ident.TokenLiteral is not {value} got {bo.TokenLiteral()}");
                return false;
            }

            return true;
        }
    }
}
