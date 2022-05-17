using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace InterpreterExam {
    public class Parser {
        private Lexer l;
        private Token curToken;
        private Token peekToken;
        private bool isCurTokenAdded;
        private bool isPeekTokenAdded;
        private List<string> errors = new List<string>();
        private Dictionary<TokenType, Func<Expression>> prefixParseFns = new Dictionary<TokenType, Func<Expression>>();
        private Dictionary<TokenType, Func<Expression, Expression>> infixParseFns =
            new Dictionary<TokenType, Func<Expression, Expression>>();
        private Dictionary<TokenType, PRECEDENCES> precedences = new Dictionary<TokenType, PRECEDENCES>() {
            {TokenType.EQ, PRECEDENCES.EQUALS},
            {TokenType.NOT_EQ, PRECEDENCES.EQUALS},
            {TokenType.LT, PRECEDENCES.LESSGREATER},
            {TokenType.GT, PRECEDENCES.LESSGREATER},
            {TokenType.PLUS, PRECEDENCES.SUM},
            {TokenType.MINUS, PRECEDENCES.SUM},
            {TokenType.SLASH, PRECEDENCES.PRODUCT},
            {TokenType.ASTERISK, PRECEDENCES.PRODUCT},
            {TokenType.LPAREN, PRECEDENCES.CALL},
            {TokenType.LBRACKET, PRECEDENCES.INDEX}
        };

        public Parser(Lexer l) {
            this.l = l;

            registerPrefix(TokenType.IDENT, parseIdentifier);
            registerPrefix(TokenType.INT, parseIntegerLiteral);
            registerPrefix(TokenType.BANG, parsePrefixExpression);
            registerPrefix(TokenType.MINUS, parsePrefixExpression);
            registerPrefix(TokenType.TRUE, parseBooleanExpression);
            registerPrefix(TokenType.FALSE, parseBooleanExpression);
            registerPrefix(TokenType.LPAREN, parseGroupedExpression);
            registerPrefix(TokenType.IF, parseIfExpression);
            registerPrefix(TokenType.FUNCTION, parseFunctionLiteralExpression);
            registerPrefix(TokenType.STRING, parseStringLiteral);
            registerPrefix(TokenType.LBRACKET, parseArrayLiteral);
            registerPrefix(TokenType.LBRACE, parseHashLiteral);

            registerInfix(TokenType.PLUS, parseInfixExpression);
            registerInfix(TokenType.MINUS, parseInfixExpression);
            registerInfix(TokenType.SLASH, parseInfixExpression);
            registerInfix(TokenType.ASTERISK, parseInfixExpression);
            registerInfix(TokenType.EQ, parseInfixExpression);
            registerInfix(TokenType.NOT_EQ, parseInfixExpression);
            registerInfix(TokenType.LT, parseInfixExpression);
            registerInfix(TokenType.GT, parseInfixExpression);
            registerInfix(TokenType.LPAREN, parseCallExpression);
            registerInfix(TokenType.LBRACKET, parseIndexExpression);

            NextToken();
            NextToken();
        }

        private void NextToken() {
            curToken = peekToken;
            peekToken = l.NextToken();
        }

        public Program ParseProgram() {
            var program = new Program {
                statements = new List<Statement>()
            };

            while (curToken.Type != TokenType.EOF) {
                var stmt = parseStatement();

                if (stmt != null) {
                    program.statements.Add(stmt);
                }

                NextToken();
            }

            return program;
        }

        [CanBeNull]
        private Statement parseStatement() {
            switch (curToken.Type) {
                case TokenType.LET:
                    return parseLetStateMent();
                case TokenType.RETURN:
                    return parseReturnStatement();
                default:
                    return parseExpressionStatement();
            }
        }

        private LetStatement? parseLetStateMent() {
            var stmt = new LetStatement {
                Token = curToken
            };

            if (!expectPeek(TokenType.IDENT)) {
                return null;
            }

            stmt.Name = new Identifier(curToken, curToken.Literal);

            if (!expectPeek(TokenType.ASSIGN)) {
                return null;
            }

            NextToken();

            stmt.Value = parseExpression(PRECEDENCES.LOWEST);

            if (peekTokenIs(TokenType.SEMICOLON)) {
                NextToken();
            }

            return stmt;
        }

        private ReturnStatement? parseReturnStatement() {
            var stmt = new ReturnStatement {
                Token = curToken
            };

            NextToken();

            stmt.ReturnValue = parseExpression(PRECEDENCES.LOWEST);

            if (peekTokenIs(TokenType.SEMICOLON)) {
                NextToken();
            }

            return stmt;
        }

        private ExpressionStatement parseExpressionStatement() {
            var stmt = new ExpressionStatement {
                Expression = parseExpression(PRECEDENCES.LOWEST)
            };

            if (peekTokenIs(TokenType.SEMICOLON)) {
                NextToken();
            }

            return stmt;
        }

        private BlockStatement parseBlockStatement() {
            var block = new BlockStatement();
            block.Statements = new List<Statement>();

            NextToken();

            while (!curTokenIs(TokenType.RBRACE) && !curTokenIs(TokenType.EOF)) {
                var stmt = parseStatement();
                if (stmt != null) {
                    block.Statements.Add(stmt);
                }

                NextToken();
            }

            return block;
        }

        private Expression parseExpression(PRECEDENCES precedence) {
            if (!prefixParseFns.ContainsKey(curToken.Type)) {
                noPrefixParseFnError(curToken.Type);
                return null;
            }

            var prefix = prefixParseFns[curToken.Type];
            var leftExp = prefix();

            while (!peekTokenIs(TokenType.SEMICOLON) && precedence < peekPrecedence()) {
                if (!infixParseFns.ContainsKey(peekToken.Type)) {
                    Debug.Log("error");
                    return leftExp;
                }

                var infix = infixParseFns[peekToken.Type];

                NextToken();
                leftExp = infix(leftExp);
            }

            return leftExp;
        }

        private void noPrefixParseFnError(TokenType t) {
            errors.Add($"no prefix parse function for {t} found");
        }

        private Expression parseIdentifier() {
            return new Identifier(curToken, curToken.Literal);
        }

        private Expression parseIntegerLiteral() {
            var lit = new IntegerLiteral {
                Token = curToken
            };

            long value;
            try {
                value = long.Parse(curToken.Literal);
            }
            catch (Exception e) {
                errors.Add($"could not parse {curToken.Literal} as integer");
                return null;
            }

            lit.Value = value;

            return lit;
        }

        private Expression parseStringLiteral() {
            return new StringLiteral {Token = curToken, Value = curToken.Literal};
        }

        private Expression parseArrayLiteral() {
            var array = new ArrayLiteral {Token = curToken};
            array.Elements = parseExpressionList(TokenType.RBRACKET);

            return array;
        }

        private Expression parsePrefixExpression() {
            var expression = new PrefixExpression {
                Token = curToken,
                Operator = curToken.Literal
            };

            NextToken();

            expression.Right = parseExpression(PRECEDENCES.PREFIX);

            return expression;
        }

        private Expression parseInfixExpression(Expression left) {
            var expression = new InfixExpression() {
                Token = curToken,
                Operator = curToken.Literal,
                Left = left
            };

            PRECEDENCES precedence = curPrecedence();
            NextToken();
            expression.Right = parseExpression(precedence);

            return expression;
        }

        private Expression parseBooleanExpression() {
            return new BooleanExpression {Token = curToken, Value = curTokenIs(TokenType.TRUE)};
        }

        private Expression parseGroupedExpression() {
            NextToken();
            var exp = parseExpression(PRECEDENCES.LOWEST);

            return !expectPeek(TokenType.RPAREN) ? null : exp;
        }

        private Expression parseIfExpression() {
            var expression = new IfExpression {Token = curToken};

            if (!expectPeek(TokenType.LPAREN))
                return null;


            NextToken();
            expression.Condition = parseExpression(PRECEDENCES.LOWEST);

            if (!expectPeek(TokenType.RPAREN))
                return null;


            if (!expectPeek(TokenType.LBRACE))
                return null;

            expression.Consequence = parseBlockStatement();

            if (peekTokenIs(TokenType.ELSE)) {
                NextToken();

                if (!expectPeek(TokenType.LBRACE))
                    return null;

                expression.Alternative = parseBlockStatement();
            }

            return expression;
        }

        private Expression parseFunctionLiteralExpression() {
            var lit = new FunctionLiteralExpression {Token = curToken};

            if (!expectPeek(TokenType.LPAREN))
                return null;

            lit.Parameters = parseFunctionParameters();

            if (!expectPeek(TokenType.LBRACE))
                return null;

            lit.Body = parseBlockStatement();

            return lit;
        }

        private List<Identifier> parseFunctionParameters() {
            var identifiers = new List<Identifier>();

            if (peekTokenIs(TokenType.RPAREN)) {
                NextToken();
                return identifiers;
            }

            NextToken();

            var ident = new Identifier {Token = curToken, Value = curToken.Literal};
            identifiers.Add(ident);
            while (peekTokenIs(TokenType.COMMA)) {
                NextToken();
                NextToken();
                ident = new Identifier {Token = curToken, Value = curToken.Literal};
                identifiers.Add(ident);
            }

            if (!expectPeek(TokenType.RPAREN))
                return null;

            return identifiers;
        }

        private Expression parseCallExpression(Expression function) {
            var exp = new CallExpression() {
                Token = curToken,
                Function = function
            };

            exp.Arguments = parseExpressionList(TokenType.RPAREN);

            return exp;
        }

        private List<Expression> parseExpressionList(TokenType end) {
            var list = new List<Expression>();

            if (peekTokenIs(end)) {
                NextToken();
                return list;
            }

            NextToken();
            list.Add(parseExpression(PRECEDENCES.LOWEST));

            while (peekTokenIs(TokenType.COMMA)) {
                NextToken();
                NextToken();
                list.Add(parseExpression(PRECEDENCES.LOWEST));
            }

            if (!expectPeek(end)) {
                return null;
            }

            return list;
        }

        private Expression parseIndexExpression(Expression left) {
            var exp = new IndexExpression {Token = curToken, Left = left};
            NextToken();
            exp.Index = parseExpression(PRECEDENCES.LOWEST);

            if (!expectPeek(TokenType.RBRACKET)) {
                return null;
            }

            return exp;
        }

        private Expression parseHashLiteral() {
            var hash = new HashLiteral {Token = curToken};
            hash.Pairs = new Dictionary<Expression, Expression>();

            while (!peekTokenIs(TokenType.RBRACE)) {
                NextToken();
                var key = parseExpression(PRECEDENCES.LOWEST);

                if (!expectPeek(TokenType.COLON)) {
                    return null;
                }

                NextToken();
                var value = parseExpression(PRECEDENCES.LOWEST);

                hash.Pairs.Add(key, value);

                if (!peekTokenIs(TokenType.RBRACE) && !expectPeek(TokenType.COMMA)) {
                    return null;
                }
            }

            if (!expectPeek(TokenType.RBRACE)) {
                return null;
            }

            return hash;
        }

        private bool curTokenIs(TokenType t) {
            return curToken.Type == t;
        }

        private bool peekTokenIs(TokenType t) {
            return peekToken.Type == t;
        }

        private bool expectPeek(TokenType t) {
            if (peekTokenIs(t)) {
                NextToken();
                return true;
            }

            peekError(t);
            return false;
        }

        public List<string> Errors() {
            return errors;
        }

        private void peekError(TokenType t) {
            var msg = $"expected next token to be {t}, got {peekToken.Type}+({peekToken.Literal}) insted";
            errors.Add(msg);
        }

        private void registerPrefix(TokenType tokenType, Func<Expression> prefixParseFn) {
            prefixParseFns.Add(tokenType, prefixParseFn);
        }

        private void registerInfix(TokenType tokenType, Func<Expression, Expression> infixParseFn) {
            infixParseFns.Add(tokenType, infixParseFn);
        }

        private PRECEDENCES peekPrecedence() {
            return precedences.ContainsKey(peekToken.Type) ? precedences[peekToken.Type] : PRECEDENCES.LOWEST;
        }

        private PRECEDENCES curPrecedence() {
            return precedences.ContainsKey(curToken.Type) ? precedences[curToken.Type] : PRECEDENCES.LOWEST;
        }
    }

    public enum PRECEDENCES {
        LOWEST,
        EQUALS,
        LESSGREATER,
        SUM,
        PRODUCT,
        PREFIX,
        CALL,
        INDEX,
    }
}
