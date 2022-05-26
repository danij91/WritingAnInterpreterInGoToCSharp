using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class Repl {
        private Environment staticEnvironment = new Environment();
        private bool isFirst = true;

        //Start
        public ReplResult RunCodeFresh(string line) {
            if (string.IsNullOrEmpty(line)) {
                return new ReplResult {resultText = "empty code", isError = false};
            }

            var t = new Tokenizer();
            var e = new Evaluator();
            var linker = new Linker(e, t);

            var l = new Lexer(line, t);
            var p = new Parser(l, linker);

            var program = p.ParseProgram();

            if (p.Errors().Count != 0) {
                printParseErrors(p.Errors());
            }

            linker.e = e;

            var env = new Environment();
            var evaluated = e.Eval(program, env);
            if (evaluated != null) {
                return new ReplResult {resultText = evaluated.Inspect(), isError = e.isError(evaluated)};
            }

            return new ReplResult {resultText = "error", isError = false};
        }

        public ReplResult RunCode(string line) {
            if (string.IsNullOrEmpty(line)) {
                return new ReplResult {resultText = "empty code", isError = false};
            }

            var t = new Tokenizer();

            var l = new Lexer(line, t);
            var p = new Parser(l);

            var program = p.ParseProgram();

            if (p.Errors().Count != 0) {
                printParseErrors(p.Errors());
            }

            var e = new Evaluator();

            var evaluated = e.Eval(program, staticEnvironment);
            if (evaluated != null) {
                return new ReplResult {resultText = evaluated.Inspect(), isError = e.isError(evaluated)};
            }

            return new ReplResult {resultText = "error", isError = false};
        }

        private void printParseErrors(List<string> errors) {
            foreach (var error in errors) {
                Debug.LogError(error);
            }
        }
    }

    public struct ReplResult {
        public string resultText;
        public bool isError;
    }
}
