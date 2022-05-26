using System.Collections.Generic;

namespace InterpreterExam {
    public class LexetTest {
        public void TestNextToken() {
            var input = @"let five = 5;
let ten = 10;
let add = fn(x, y) {
    x + y;
};

let result  = add(five, ten);
!-/*5;
5<10>5;

if(5 < 10){
    return true;
} else {
    return false;
}

10 == 10;
10 != 9;" 
    + "\"foobar\""
    + "\"foo bar\"";
            Dictionary<TokenType, string> tests = new Dictionary<TokenType, string> {
                {TokenType.DATATYPE_STRING, "foobar"},
                {TokenType.DATATYPE_STRING, "foo bar"},
                {TokenType.EOF, ""},
            };
        }
    }
}
