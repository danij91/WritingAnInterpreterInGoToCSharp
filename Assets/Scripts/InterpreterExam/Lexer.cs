using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace InterpreterExam {
    public class Lexer {
        private string input;
        private int position;
        private int readPosition;
        private byte ch;

        public bool isLast { get; set; }

        public Lexer(string input) {
            this.input = input;
            readChar();
        }

        private void readChar() {
            if (readPosition >= input.Length) {
                isLast = true;
                ch = 0;
            }
            else {
                ch = (byte)input[readPosition];
            }

            position = readPosition;
            readPosition += 1;
        }

        public Token NextToken() {
            Token tok;
            skipWhiteSpace();
            switch (ch) {
                case (byte)'=':
                    if (peekChar() == '=') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.EQ, literal);
                    }
                    else {
                        tok = new Token(TokenType.ASSIGN, (char)ch + "");
                    }

                    break;
                case (byte)'+':
                    if (peekChar() == '+') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.INCREMENT, literal);
                    }
                    else if(peekChar() == '=') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.ASSIGN, literal);
                    }
                    else {
                        tok = new Token(TokenType.PLUS, (char)ch + "");
                    }
                    
                    break;
                case (byte)'-':
                    if (peekChar() == '-') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.DECREMENT, literal);
                    }
                    else if(peekChar() == '=') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.ASSIGN, literal);
                    }
                    else {
                        tok = new Token(TokenType.MINUS, (char)ch + "");
                    }
                    
                    break;
                case (byte)'!':
                    if (peekChar() == '=') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.NOT_EQ, literal);
                    }
                    else {
                        tok = new Token(TokenType.BANG, (char)ch + "");
                    }

                    break;
                case (byte)'/':
                    if (peekChar() == '=') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.ASSIGN, literal);
                    }
                    else {
                        tok = new Token(TokenType.SLASH, (char)ch + "");
                    }
                    break;
                case (byte)'*':
                    if (peekChar() == '=') {
                        var tempCh = ch;
                        readChar();
                        var literal = "" + (char)tempCh + (char)ch;
                        tok = new Token(TokenType.ASSIGN, literal);
                    }
                    else {
                        tok = new Token(TokenType.ASTERISK, (char)ch + "");
                    }
                    break;
                case (byte)'<':
                    tok = new Token(TokenType.LT, (char)ch + "");
                    break;
                case (byte)'>':
                    tok = new Token(TokenType.GT, (char)ch + "");
                    break;
                case (byte)';':
                    tok = new Token(TokenType.SEMICOLON, (char)ch + "");
                    break;
                case (byte)'(':
                    tok = new Token(TokenType.LPAREN, (char)ch + "");
                    break;
                case (byte)')':
                    tok = new Token(TokenType.RPAREN, (char)ch + "");
                    break;
                case (byte)',':
                    tok = new Token(TokenType.COMMA, (char)ch + "");
                    break;
                case (byte)'{':
                    tok = new Token(TokenType.LBRACE, (char)ch + "");
                    break;
                case (byte)'}':
                    tok = new Token(TokenType.RBRACE, (char)ch + "");
                    break;
                case (byte)'[':
                    tok = new Token(TokenType.LBRACKET, (char)ch + "");
                    break;
                case (byte)']':
                    tok = new Token(TokenType.RBRACKET, (char)ch + "");
                    break;
                case (byte)':':
                    tok = new Token(TokenType.COLON, (char)ch + "");
                    break;
                case (byte)'"':
                    tok = new Token(TokenType.STRING, readString());
                    break;
                case (byte)'\'':
                    tok = new Token(TokenType.CHARACTER, readCharacter());
                    break;
                case 0:
                    tok = new Token(TokenType.EOF, "");
                    break;
                default:
                    if (isLetter(ch)) {
                        var literal = readIdentifier();
                        var type = Tokenizer.LookupIdent(literal);
                        return new Token(type, literal);
                    }
                    else if (isDigit(ch)) {
                        var tuple = readNumber();
                        return new Token(tuple.Item1, tuple.Item2);
                    }
                    else {
                        tok = new Token(TokenType.ILLEGAL, (char)ch + "");
                    }

                    break;
            }

            readChar();

            return tok;
        }

        private string readString() {
            var tempPosition = position + 1;
            while (true) {
                readChar();
                if (ch == '"' || ch == 0) {
                    break;
                }
            }

            return input.Substring(tempPosition, position - tempPosition);
        }

        private string readCharacter() {
            var tempPosition = position + 1;
            while (true) {
                readChar();
                if (ch == '\'' || ch == 0) {
                    break;
                }
            }

            return input.Substring(tempPosition, position - tempPosition);
        }

        private string readIdentifier() {
            var tempPosition = position;
            while (isLetter(ch)) {
                readChar();
            }

            return input.Substring(tempPosition, position - tempPosition);
        }

        private bool isLetter(byte ch) {
            return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';
        }

        private void skipWhiteSpace() {
            while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r') {
                readChar();
            }
        }

        private Tuple<TokenType, string> readNumber() {
            var tempPosition = position;
            var type = TokenType.INTEGER;

            while (isDigit(ch) || ch == '.') {
                if (ch == '.') {
                    type = TokenType.REAL_NUMBER;
                }

                readChar();
            }

            return new Tuple<TokenType, string>(type, input.Substring(tempPosition, position - tempPosition));
        }

        private bool isDigit(byte ch) {
            return '0' <= ch && ch <= '9';
        }

        private byte peekChar() {
            if (readPosition >= input.Length) {
                return 0;
            }

            return (byte)input[readPosition];
        }
    }
}
