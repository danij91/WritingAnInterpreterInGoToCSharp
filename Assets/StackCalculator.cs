using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class StackCalculator {
    private Stack<string> stack = new Stack<string>();
    private List<string> postFixList = new List<string>();
    private Queue<string> tokenQueue;
    private bool isRightBracesDequeued;
    private bool isLeftBracesDequeued;
    private string currentToken;

    public string InfixToPostFix(string infix) {
        var list = Regex.Split(infix, @"\s*([-+/*()])\s*")
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        tokenQueue = new Queue<string>(list);
        NextToken();

        while (currentToken != "end") {
            if (IsOperator(currentToken)) {
                var lastOpPriority = GetLastPriority();
                var currentOpPriority = GetPriority(currentToken);

                if (lastOpPriority < currentOpPriority) {
                    var poppedOperator = PopStack();

                    if (poppedOperator != "(" && poppedOperator != ")") {
                        postFixList.Add(poppedOperator);
                    }

                    continue;
                }

                PushStack(currentToken);
            }
            else {
                postFixList.Add(currentToken);
            }

            NextToken();
        }

        while (stack.Count != 0) {
            var poppedOperator = PopStack();

            if (poppedOperator != "(" && poppedOperator != ")") {
                postFixList.Add(poppedOperator);
            }
        }

        var expression = postFixList.Aggregate("", (current, postFix) => current + postFix + " ");
        var result = CalculateStack();

        stack.Clear();
        postFixList.Clear();

        return infix + "\n" + expression + "\n" + result;
    }

    private int GetPriority(string op) {
        switch (op) {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            case "(":
                return isLeftBracesDequeued ? 0 : 4;
            case ")":
                return isRightBracesDequeued ? 5 : 0;
            default:
                return 3;
        }
    }

    private int GetLastPriority() {
        if (stack.Count == 0) {
            return 5;
        }

        var op = stack.Peek();
        return GetPriority(op);
    }

    private void NextToken() {
        if (tokenQueue.Count <= 0) {
            currentToken = "end";
            return;
        }

        currentToken = tokenQueue.Dequeue();

        if (currentToken == "(")
            isLeftBracesDequeued = true;


        if (currentToken == ")")
            isRightBracesDequeued = true;
    }


    private void PushStack(string op) {
        if (op == "(") {
            isLeftBracesDequeued = false;
        }

        if (op == ")") {
            isRightBracesDequeued = false;
        }

        stack.Push(op);
    }

    private string PopStack() {
        var op = stack.Pop();
        return op;
    }

    private string CalculateStack() {
        foreach (var postFix in postFixList) {
            if (!IsOperator(postFix)) {
                stack.Push(postFix);
                continue;
            }

            var b = stack.Pop();
            var a = stack.Pop();
            var result = Calculate(postFix, a, b);
            stack.Push(result);
        }

        return stack.Pop();
    }

    private string Calculate(string op, string a, string b) {
        var aF = float.Parse(a);
        var bF = float.Parse(b);

        return op switch {
            "+" => (aF + bF).ToString(),
            "-" => (aF - bF).ToString(),
            "*" => (aF * bF).ToString(),
            "/" => (aF / bF).ToString(),
            _ => throw new Exception()
        };
    }

    private bool IsOperator(string data) {
        return Regex.IsMatch(data, @"\s*([-+/*()])\s*");
    }
}
