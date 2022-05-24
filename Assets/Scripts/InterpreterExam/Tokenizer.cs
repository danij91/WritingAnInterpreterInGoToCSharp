using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public static class Tokenizer {
        private static Dictionary<string, TokenType> kewords = new Dictionary<string, TokenType>() {
            {"int", TokenType.INT},
            {"float", TokenType.FLOAT},
            {"char", TokenType.CHAR},
            {"bool", TokenType.BOOL},
            {"void", TokenType.VOID},
            {"true", TokenType.TRUE},
            {"false", TokenType.FALSE},
            {"if", TokenType.IF},
            {"else", TokenType.ELSE},
            {"return", TokenType.RETURN},
            {"for", TokenType.FOR},
            {"while", TokenType.WHILE},
            {"break", TokenType.BREAK},
        };

        public static TokenType LookupIdent(string ident) {
            if (kewords.ContainsKey(ident)) {
                return kewords[ident];
            }

            return TokenType.IDENT;
        }
    }

    public struct Token {
        public TokenType Type;
        public string Literal;

        public Token(TokenType type, string literal) {
            Type = type;
            Literal = literal;
        }

        public void WriteInfo(int length = 10) {
            var result = Literal;
            int spaceCount = length - Literal.Length;

            if (spaceCount <= 0) {
                spaceCount = 1;
            }

            var type = Type.ToString();

            spaceCount += 9 - type.Length;
            if (Type == TokenType.ILLEGAL) {
                type = "<color=red>" + type + "</color>";
            }

            for (int i = 0; i < spaceCount; i++) {
                result += " ";
            }

            result += type;
        }
    }

    public enum TokenType {
        ILLEGAL,
        EOF,
        
        //식별자 + 리터럴
        INT,
        CHAR,
        FLOAT,
        BOOL,
        VOID,

        //연산자
        ASSIGN,
        PLUS,
        MINUS,
        BANG,
        ASTERISK,
        SLASH,
        LT,
        GT,
        EQ,
        NOT_EQ,
        INCREMENT,
        DECREMENT,

        //구분자
        COMMA,
        SEMICOLON,
        COLON,

        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        LBRACKET,
        RBRACKET,

        //자료형
        IDENT,
        INTEGER,
        STRING,
        CHARACTER,
        REAL_NUMBER,

        //키워드
        TRUE,
        FALSE,
        IF,
        ELSE,
        RETURN,
        BREAK,
        FOR,
        WHILE,
    }
}
