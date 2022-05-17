using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Febucci.UI;
using InterpreterExam;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleUI : MonoBehaviour {
    [SerializeField] private InputField input;
    [SerializeField] private TextAnimatorPlayer outputTextAnimPlayer;
    [SerializeField] private TMP_Text output;
    [SerializeField] private Button button;

    private int count = 0;
    private Lexer lexer;
    public Repl repl;

    private void Awake() {
        input.onEndEdit.AddListener(PlayRepl);
        repl = new Repl();
        button.onClick.AddListener(PlayReplWithInputText);

        var et = new EvaluatorTest();
    }

    private void PlayReplWithInputText() {
        PlayRepl(input.text);
    }

    private void PlayRepl(string inputText) {
        ReplResult result = repl.RunCode(inputText);
        outputTextAnimPlayer.ShowText(result.resultText);
        output.color = result.isError ? Color.red : Color.white;
        input.text = "";
        EventSystem.current.SetSelectedGameObject(input.gameObject, null);
        input.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void TestNextToken() {
        //         var input
        //             = @"let five  = 5;
        //         let ten = 10;
        //         
        //         let add = fn(x, y) {
        //             x + y;
        //         };
        //         
        //         let result = add(five, ten);
        //         !-/*5;
        //         5 < 10 > 5;
        //
        //         if (5 < 10) {
        //             return true;
        //         } else {
        //             return false;
        //         }
        //
        //         10 == 10;
        //         10 != 9;
        //         ";
        //             = @"let five  = 5;
        //         let ten = 10;
        //         
        //         let add = fn(x, y) {
        //             x + y;
        //         };
        //         
        //         let result = add(five, ten);
        //         !-/*5;
        //         5 < 10 > 5;
        //
        //         if (5 < 10) {
        //             return true;
        //         } else {
        //             return false;
        //         } 
        //         ";
        // var input = @"let five  = 5;
        // let ten = 10;
        //
        // let add = fn(x, y) {
        //     x + y;
        // };
        //
        // let result = add(five, ten);
        // ";
        // var input = @"=+(){},;";
        string input = @"
let x = 5;
let y = 10;
let foobar = 838383;
";
        var l = new Lexer(input);
        while (!l.isLast) {
            var tok = l.NextToken();
            tok.WriteInfo();
            Debug.Log(tok.Literal + " // " + tok.Type);
        }
    }

    public void TestLetStatements() {
        string input = @"
        let x = 5;
        let y = 10;
        let foobar = 838383;
        ";

        //         string input = @"
        // let x   5;
        // let = 10;
        // let 838383;
        // ";
        var l = new Lexer(input);
        var p = new Parser(l);

        Program program = p.ParseProgram();
        checkParserErrors(p);

        if (program.statements.Count != 3) {
            Debug.LogError($"program.Statemonts does not contain 3 statements. get={program.statements.Count}.");
            return;
        }
    }

    public void TestReturnStatements() {
        string input = @"
return 5;
return 10;
return 993322;
";
        var l = new Lexer(input);
        var p = new Parser(l);

        Program program = p.ParseProgram();
        checkParserErrors(p);
    }

    public void TestString() {
        string input = @"
let myVar = anotherVar;
";
        var l = new Lexer(input);
        var p = new Parser(l);

        Program program = p.ParseProgram();
        Debug.Log(program.String());
        foreach (var statement in program.statements) {
            Debug.Log(statement.String() + " // " + statement.TokenLiteral());
        }

        checkParserErrors(p);
    }

    public void TestIdentifierExpression() {
        string input = @"
foobar;
";
        var l = new Lexer(input);
        var p = new Parser(l);

        Program program = p.ParseProgram();
        Debug.Log(program.String());

        checkParserErrors(p);
    }

    public void TestIntegerLiteralExpression() {
        string input = @"
5;
";
        var l = new Lexer(input);
        var p = new Parser(l);

        Program program = p.ParseProgram();
        Debug.Log(program.String());

        checkParserErrors(p);
    }

    public void TestPrefixExpression() {
        string input = this.input.text;
        var l = new Lexer(input);
        var p = new Parser(l);

        Program program = p.ParseProgram();
        Debug.Log(program.String());

        checkParserErrors(p);
    }

    public void TestParsingInfixExpression() {
        testExample[] tests = {
            new testExample("-a * b", "((-a) * b)"),
            new testExample("!-a", "(!(-a))"),
            new testExample("a + b + c", "((a + b) + c)"),
            new testExample("a + b - c", "((a + b) - c)"),
            new testExample("a * b * c", "((a * b) * c)"),
            new testExample("a * b / c", "((a * b) / c)"),
            new testExample("a + b / c", "(a + (b / c))"),
            new testExample("a + b * c + d / e - f", "(((a + (b * c)) + (d / e)) - f)"),
            new testExample("3 + 4; -5 * 5", "(3 + 4)((-5) * 5)"),
            new testExample("5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))"),
            new testExample("5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))"),
            new testExample("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"),
            new testExample("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"),
        };

        foreach (var tt in tests) {
            var l = new Lexer(tt.input);
            var p = new Parser(l);

            Program program = p.ParseProgram();

            if (program.String() != tt.expected) {
                Debug.LogError($"exp.Operator is not {tt.expected}. got {program.String()}");
            }
            else {
                Debug.Log("pass");
            }
        }
    }

    public void TestParsingBoolExpression() {
        testExample[] tests = {
            new testExample("1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)"),
            new testExample("(5 + 5) * 2", "((5 + 5) * 2)"),
            new testExample("2 / (5 + 5)", "(2 / (5 + 5))"),
            new testExample("-(5 + 5)", "(-(5 + 5))"),
            new testExample("!(true == true)", "(!(true == true))")
        };
        foreach (var tt in tests) {
            var l = new Lexer(tt.input);
            var p = new Parser(l);

            Program program = p.ParseProgram();

            if (program.String() != tt.expected) {
                Debug.LogError($"exp.Operator is not {tt.expected}. got {program.String()}");
            }
            else {
                Debug.Log("pass");
            }
        }
    }

    public void TestParsingIfExpression() {
        testExample[] tests = {
            new testExample(@"if (x < y) { x } else { y }", ""),
        };
        foreach (var tt in tests) {
            var l = new Lexer(tt.input);
            var p = new Parser(l);

            Program program = p.ParseProgram();
            Debug.Log(program.String());
        }
    }

    public void TestFunctionLiteralParsing() {
        testExample[] tests = {
            new testExample(@"let myFunction = fn(x, y) { return x + y;}", ""),
        };

        foreach (var tt in tests) {
            var l = new Lexer(tt.input);
            var p = new Parser(l);

            Program program = p.ParseProgram();
            Debug.Log(program.String());
        }
    }

    public void TestCallExpressionParsing() {
        testExample[] tests = {
            new testExample(@"add(1, 2 * 3, 4 + 5);", ""),
        };

        foreach (var tt in tests) {
            var l = new Lexer(tt.input);
            var p = new Parser(l);

            Program program = p.ParseProgram();
            Debug.Log(program.String());

            program.IsExpectedStatementCount(1);

            ExpressionStatement stmt;
            if (program.statements[0].IsExpectedStatementType<ExpressionStatement>()) {
                stmt = (ExpressionStatement)program.statements[0];
            }
            else {
                return;
            }

            CallExpression exp;

            if (stmt.Expression.IsExpectedExpressionType<CallExpression>()) {
                exp = (CallExpression)stmt.Expression;
            }
            else {
                return;
            }

            if (!ParserTest.testIdentifier(exp, "add")) {
                return;
            }

            if (!exp.IsExpectedArgumentCount(3)) {
                return;
            }

            exp.Arguments[0].testLiteralExpression(1);
            exp.Arguments[1].testInfixExpression(2, "*", 3);
            exp.Arguments[2].testInfixExpression(4, "+", 5);
        }
    }


    private struct testExample {
        public string input;
        public string expected;

        public testExample(string input, string expected) {
            this.input = input;
            this.expected = expected;
        }
    }

    private void checkParserErrors(Parser p) {
        List<string> errors = p.Errors();

        if (errors.Count <= 0) {
            return;
        }

        Debug.LogError($"parser has {errors.Count} errors");

        foreach (var error in errors) {
            Debug.LogError(error);
        }
    }
}
