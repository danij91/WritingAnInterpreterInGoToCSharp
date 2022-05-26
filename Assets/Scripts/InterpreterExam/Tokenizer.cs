using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InterpreterExam {
    public class Tokenizer {
        private Dictionary<string, TokenType> kewords = new Dictionary<string, TokenType> {
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
            {"include", TokenType.INCLUDE},
        };

        public void AddHeader(LibraryClassBase library) {
            foreach (var header in library.Header) {
                kewords.Add(header.Key, TokenType.CLASS);
            }
        }

        public TokenType LookupIdent(string ident) {
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
        CLASS,

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
        DOT,

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
        SHARP,

        //자료형
        IDENT,
        DATATYPE_INT,
        DATATYPE_STRING,
        DATATYPE_CHAR,
        DATATYPE_FLOAT,
        DATATYPE_CLASS,

        //키워드
        TRUE,
        FALSE,
        IF,
        ELSE,
        RETURN,
        BREAK,
        FOR,
        WHILE,
        INCLUDE,
    }
}
