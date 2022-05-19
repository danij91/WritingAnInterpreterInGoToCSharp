using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace InterpreterExam {
    public interface Node {
        public string TokenLiteral();
        public string String();
    }

    public interface Statement : Node {
        public void statementNode();
    }

    public interface Expression : Node {
        public void expressionNode();
    }

    public struct Program : Node {
        public List<Statement> statements;

        public string TokenLiteral() {
            if (statements.Count > 0) {
                return statements[0].TokenLiteral();
            }

            return "";
        }

        public string String() {
            var buffer = "";

            foreach (var statement in statements) {
                buffer += statement.String();
            }

            return buffer;
        }
    }

    public struct DataStatement : Statement {
        public Token Token;
        public Identifier Name;
        public Expression Value;
        public void statementNode() { }

        public string String() {
            var buffer = "";
            buffer += TokenLiteral() + " ";
            buffer += Name.String();
            buffer += " = ";

            if (Value != null) {
                buffer += Value.String();
            }

            buffer += ";";

            return buffer;
        }

        public string TokenLiteral() {
            return Token.Literal;
        }
    }

    public struct AssignStatement : Statement {
        public Token Token;
        public Identifier Name;
        public Expression Value;
        public void statementNode() { }

        public string String() {
            var buffer = "";
            buffer += TokenLiteral() + " ";
            buffer += Name.String();
            buffer += " = ";

            if (Value != null) {
                buffer += Value.String();
            }

            buffer += ";";

            return buffer;
        }

        public string TokenLiteral() {
            return Token.Literal;
        }
    }

    public struct ReturnStatement : Statement {
        public Token Token;
        public Expression ReturnValue;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public void statementNode() { }

        public string String() {
            var buffer = "";
            buffer += TokenLiteral() + " ";
            if (ReturnValue != null) {
                buffer += ReturnValue.String();
            }

            buffer += ";";

            return buffer;
        }
    }

    public struct ExpressionStatement : Statement {
        public Token Token;
        public Expression Expression;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public void statementNode() { }

        public string String() {
            return Expression != null ? Expression.String() : "";
        }
    }

    public struct BlockStatement : Statement {
        public Token Token;
        public List<Statement> Statements;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";
            buffer = Statements.Aggregate(buffer, (current, s) => current + s.String());
            return buffer;
        }

        public void statementNode() { }
    }

    public struct Identifier : Expression {
        public Token Token;
        public string Value;

        public Identifier(Token t, string value) {
            Token = t;
            Value = value;
        }

        public void expressionNode() { }

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            return Value;
        }
    }

    public struct IntegerLiteral : Expression {
        public Token Token;
        public long Value;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            return Token.Literal;
        }

        public void expressionNode() { }
    }

    public struct StringLiteral : Expression {
        public Token Token;
        public string Value;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            return Token.Literal;
        }

        public void expressionNode() { }
    }

    public struct CharacterLiteral : Expression {
        public Token Token;
        public char Value;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            return Token.Literal;
        }

        public void expressionNode() { }
    }

    public struct RealNumberLiteral : Expression {
        public Token Token;
        public float Value;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            return Token.Literal;
        }

        public void expressionNode() { }
    }

    public struct PrefixExpression : Expression {
        public Token Token;
        public string Operator;
        public Expression Right;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";

            buffer += "(";
            buffer += Operator;
            buffer += Right.String();
            buffer += ")";

            return buffer;
        }

        public void expressionNode() { }
    }

    public struct InfixExpression : Expression {
        public Token Token;
        public Expression Left;
        public string Operator;
        public Expression Right;

        public string TokenLiteral() {
            throw new NotImplementedException();
        }

        public string String() {
            var buffer = "";
            buffer += "(";
            buffer += Left.String();
            buffer += " " + Operator + " ";
            buffer += Right.String();
            buffer += ")";

            return buffer;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }

    public struct BooleanExpression : Expression {
        public Token Token;
        public bool Value;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            return Token.Literal;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }

    public struct IfExpression : Expression {
        public Token Token;
        public Expression Condition;
        public BlockStatement Consequence;
        public BlockStatement? Alternative;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            string buffer = "";
            buffer += "if";
            buffer += Condition.String();
            buffer += " ";
            buffer += Consequence.String();

            if (Alternative == null)
                return buffer;

            buffer += " else ";
            buffer += Alternative?.String();

            return buffer;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }

    public struct FunctionLiteralExpression : Expression {
        public Token Token;
        public List<Identifier> Parameters;
        public BlockStatement Body;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";
            List<string> Params = new List<string>();
            foreach (var parameter in Parameters) {
                Params.Add(parameter.String());
            }

            buffer += TokenLiteral();
            buffer += "(";
            buffer += string.Join(", ", Params);
            buffer += ") ";
            buffer += Body.String();

            return buffer;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }

    public struct CallExpression : Expression {
        public Token Token;
        public Expression Function;
        public List<Expression> Arguments;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";
            var args = Arguments.Select(argument => argument.String()).ToList();
            buffer += Function.String();
            buffer += "(";
            buffer += string.Join(", ", args);
            buffer += ")";
            return buffer;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }

    public struct ArrayLiteral : Expression {
        public Token Token;
        public List<Expression> Elements;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";
            var elements = Elements.Select(element => element.String()).ToList();
            buffer += "[";
            buffer += string.Join(", ", elements);
            buffer += "]";

            return buffer;
        }

        public void expressionNode() { }
    }

    public struct IndexExpression : Expression {
        public Token Token;
        public Expression Left;
        public Expression Index;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";
            buffer += "(";
            buffer += Left.String();
            buffer += "[";
            buffer += Index.String();
            buffer += "]";
            buffer += ")";

            return buffer;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }

    public struct HashLiteral : Expression {
        public Token Token;
        public Dictionary<Expression, Expression> Pairs;

        public string TokenLiteral() {
            return Token.Literal;
        }

        public string String() {
            var buffer = "";
            var pairs = Pairs.Select(pair => pair.Key.String() + ":" + pair.Value.String()).ToList();
            buffer += "{";
            buffer += string.Join(", ", pairs);
            buffer += "}";

            return buffer;
        }

        public void expressionNode() {
            throw new NotImplementedException();
        }
    }
}
