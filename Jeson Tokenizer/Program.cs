using System;
using System.Collections;
using System.Collections.Generic;
namespace First
{
    public delegate bool InputCondition(Input input);
    public class Input
    {
        private readonly string input;
        private readonly int length;
        private int position;
        private int lineNumber;
        //Properties
        public int Length
        {
            get
            {
                return this.length;
            }
        }
        public int Position
        {
            get
            {
                return this.position;
            }
        }
        public int NextPosition
        {
            get
            {
                return this.position + 1;
            }
        }
        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }
        public char Character
        {
            get
            {
                if (this.position > -1) return this.input[this.position];
                else return '\0';
            }
        }
        public Input(string input)
        {
            this.input = input;
            this.length = input.Length;
            this.position = -1;
            this.lineNumber = 1;
        }
        public bool hasMore(int numOfSteps = 1)
        {
            if (numOfSteps <= 0) throw new Exception("Invalid number of steps");
            return (this.position + numOfSteps) < this.length;
        }
        public bool hasLess(int numOfSteps = 1)
        {
            if (numOfSteps <= 0) throw new Exception("Invalid number of steps");
            return (this.position - numOfSteps) > -1;
        }
        //callback -> delegate
        public Input step(int numOfSteps = 1)
        {
            if (this.hasMore(numOfSteps))
                this.position += numOfSteps;
            else
            {
                throw new Exception("There is no more step");
            }
            return this;
        }
        public Input back(int numOfSteps = 1)
        {
            if (this.hasLess(numOfSteps))
                this.position -= numOfSteps;
            else
            {
                throw new Exception("There is no more step");
            }
            return this;
        }
        public Input reset() { return this; }
        public char peek(int numOfSteps = 1)
        {
            if (this.hasMore()) return this.input[this.NextPosition];
            return '\0';
        }
        public string loop(InputCondition condition)
        {
            string buffer = "";
            while (this.hasMore() && condition(this))
                buffer += this.step().Character;
            return buffer;
        }
    }
    public class Token
    {
        public int Position { set; get; }
        public int LineNumber { set; get; }
        public string Type { set; get; }
        public string Value { set; get; }
        public Token(int position, int lineNumber, string type, string value)
        {
            this.Position = position;
            this.LineNumber = lineNumber;
            this.Type = type;
            this.Value = value;
        }
    }
    public abstract class Tokenizable
    {
        public abstract bool tokenizable(Tokenizer tokenizer);
        public abstract Token tokenize(Tokenizer tokenizer);
    }
    public class Tokenizer
    {
        public List<Token> tokens;
        public bool enableHistory;
        public Input input;
        public Tokenizable[] handlers;
        public Tokenizer(string source, Tokenizable[] handlers)
        {
            this.input = new Input(source);
            this.handlers = handlers;
        }
        public Tokenizer(Input source, Tokenizable[] handlers)
        {
            this.input = source;
            this.handlers = handlers;
        }
        public Token tokenize()
        {
            foreach (var handler in this.handlers)
                if (handler.tokenizable(this)) return handler.tokenize(this);
            return null;
        }
        public List<Token> all() { return null; }
    }
    public class StringKey : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            //Console.WriteLine(currentCharacter);
            return currentCharacter == '"' && t.input.hasMore();
        }
        static bool stringKey(Input input)
        {
            char currentCharacter = input.peek();
            return Char.IsLetterOrDigit(currentCharacter) || currentCharacter == '"' || currentCharacter == ' ';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "StringKey", t.input.loop(stringKey));
        }
    }
    public class Boolean : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            //Console.WriteLine(currentCharacter);
            return Char.IsLetter(currentCharacter) && t.input.hasMore();
        }
        static bool boolean(Input input)
        {
            char currentCharacter = input.peek();
            return Char.IsLetter(currentCharacter) ;
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "boolean", t.input.loop(boolean));
        }
    }
    public class NumberTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return Char.IsLetterOrDigit(t.input.peek()) || t.input.peek() == '.' || t.input.peek() == '-' || t.input.peek() == 'e';
        }
        static bool isDigit(Input input)
        {
            return Char.IsLetterOrDigit(input.peek()) || input.peek() == '.' || input.peek() == '-' || input.peek() == 'e';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "number", t.input.loop(isDigit));
        }
    }

    public class WhiteSpaceTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return t.input.peek() == ' ';
        }
        static bool isWhiteSpace(Input input)
        {
            return input.peek() == ' ';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "whitespace", t.input.loop(isWhiteSpace));
        }
    }

    public class ObjectStart : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            return currentCharacter == '{';
        }
        static bool objectStart(Input input)
        {
            char currentCharacter = input.peek();
            return currentCharacter == '{';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "ObjectStart", t.input.loop(objectStart));
        }
    }
    public class ObjectEnd : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            return currentCharacter == '}';
        }
        static bool objectEnd(Input input)
        {
            char currentCharacter = input.peek();
            return currentCharacter == '}';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "ObjectEnd", t.input.loop(objectEnd));
        }
    }
    public class Colon : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            return currentCharacter == ':';
        }
        static bool colon(Input input)
        {
            char currentCharacter = input.peek();
            return currentCharacter == ':';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "Colon", t.input.loop(colon));
        }
    }

    public class Comma : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            return currentCharacter == ',';
        }
        static bool comma(Input input)
        {
            char currentCharacter = input.peek();
            return currentCharacter == ',';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "Comma", t.input.loop(comma));
        }
    }

    public class ArrayStart : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            return currentCharacter == '[';
        }
        static bool arrayStart(Input input)
        {
            char currentCharacter = input.peek();
            return currentCharacter == '[';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "ArrayStart", t.input.loop(arrayStart));
        }
    }
    public class ArrayEnd : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            char currentCharacter = t.input.peek();
            return currentCharacter == ']';
        }
        static bool arrayEnd(Input input)
        {
            char currentCharacter = input.peek();
            return currentCharacter == ']';
        }
        public override Token tokenize(Tokenizer t)
        {
            return new Token(t.input.Position, t.input.LineNumber,
                "ArrayEnd", t.input.loop(arrayEnd));
        }
    }

    public abstract class JsonValue
    {

    }
    public class JkeyValue<TValue> : JsonValue
    {
        public string key { set; get; }
        public TValue value { set; get; }
        public string type { set; get; }


        public override string ToString()
        {
            return "KEY: " + key.ToString() + "\nVALUE: " + value + "\nTYPE: " + type;
        }
    }

    public class Jparser
    {
        public string readKey(string s)
        {
            string jKey = null;
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] == '{')
                {
                    i++;
                    if (s[i] == '"')
                    {
                        i++;
                        while (s[i] != '"' && i < s.Length)
                        {
                            jKey += s[i];
                            i++;
                        }

                    }
                }
                return jKey;
            }
            return jKey;
        }

        public JsonValue jkeyValue(string s)
        {
            //read Json key
            string key;
            key = readKey(s);
            s = s.Substring(key.Length + 4, s.Length - (key.Length + 4));

            //read Json value
            int i = 0;
            if (s[i] == '"')
            {
                //type is Jstring
                JkeyValue<string> j = new JkeyValue<string>();
                i++;
                string val = null;
                while (s[i] != '"' && i < s.Length)
                {
                    val += s[i];
                    i++;
                }
                j.key = key;
                j.value = val;
                j.type = "Jstring";

                return j;

            }
            else if (char.IsNumber(s[i]))
            {
                //type is Jnumber
                JkeyValue<string> j = new JkeyValue<string>();
                j.key = key;
                j.type = "Jnumber";

                string val = null;
                while (i < s.Length && char.IsNumber(s[i]) || s[i] == '-' || s[i] == '+' || s[i] == 'e' || s[i] == 'E' || s[i] == '-' || s[i] == '.')
                {
                    val += s[i];
                    i++;
                }

                Input input = new Input(val);
                Program p = new Program();
                j.value = p.CheckNumber(input);
                return j;


            }
            else if (char.IsLetter(s[i]) && s[i] != '{')
            {

                //type is Jnumber
                if (s[i] == 'f' || s[i] == 't')
                {
                    JkeyValue<string> j = new JkeyValue<string>();
                    j.key = key;
                    j.type = "Jbool";

                    string val = null;
                    while (i < s.Length && char.IsLetter(s[i]) && (s[i] != '}' || s[i] != ']' || s[i] != ','))
                    {
                        val += s[i];
                        i++;
                    }
                    j.value = val;
                    return j;
                }
                else if (s[i] == 'n')
                {
                    JkeyValue<string> j = new JkeyValue<string>();
                    j.key = key;
                    j.type = "Jnull";

                    string val = null;
                    while (i < s.Length && char.IsLetter(s[i]) && (s[i] != '}' || s[i] != ']' || s[i] != ','))
                    {
                        val += s[i];
                        i++;
                    }
                    j.value = val;
                    return j;
                }


            }

            else if (s[i] == '{')
            {

                JkeyValue<List<JsonValue>> j = new JkeyValue<List<JsonValue>>();

                j.key = key;
                j.type = "Jobject";

                string val = null;
                while (i < s.Length)
                {
                    val += s[i];
                    i++;
                }
                j.value = new List<JsonValue>();
                j.value.Add(jkeyValue(val));


                return j;
            }
            else if (s[i] == '[')
            {
                JkeyValue<List<string>> j = new JkeyValue<List<string>>();
                j.value = new List<string>();
                j.key = key;
                j.type = "Jarray";

                string val = null;
                while (i < s.Length && s[i] != ']')
                {
                    while (i < s.Length && s[i] != ',')
                    {
                        val += s[i];
                        i++;
                    }
                    j.value.Add(val);
                    i++;
                }
                return j;
            }

            return null;
        }

    }

    class Program
    {
        public static string CheckString(Input input)
        {
            char currentCharacter = input.peek();
            string tokenString = "";
            bool continueString = true;
            bool oneEscapeChar = false;
            char[] escapeSequence = { '\"', '\\', '/', 'b', 'f', 'n', 'r', 't', 'u' };
            int uCounter = 1;
            if (currentCharacter == '\"')
            {
                //tokenString += input.peek();
                currentCharacter = input.step().Character;
                while (input.hasMore() && continueString )
                {
                    if (input.peek() == '\\' )
                    {
                        tokenString += input.peek();
                        input.step();
                        foreach (var chr in escapeSequence)
                        {
                            if (input.peek() == escapeSequence[8])
                            {
                                tokenString += input.peek();
                                input.step();
                                //Console.WriteLine(tokenString + input.peek());
                                //oneEscapeChar = true;  
                                while (uCounter < 4)
                                {
                                    int hexValue = Convert.ToInt32(Char.ToUpperInvariant(input.peek()));
                                    //byte hexValue = Encoding.ASCII.GetByte(hexChar);
                                    if ((hexValue > 64 && hexValue < 71) || ((hexValue >= 0) && (hexValue <= 9)))
                                    {
                                        //Console.WriteLine(hexValue + " inside if");
                                        tokenString += input.peek();
                                        uCounter++;
                                        input.step();
                                    }
                                    else
                                    {
                                        continueString = false;
                                        break;
                                    }
                                }
                                //Console.WriteLine("out of while");
                            }
                            else if (input.peek() == chr)
                            {
                                tokenString += input.peek();
                                input.step();
                                oneEscapeChar = true;
                            }
                            else
                            {
                                continueString = false;
                            }
                            if (oneEscapeChar)
                                break;
                        }
                    }
                    tokenString += input.peek();
                    input.step();
                    if (input.peek() == '\"')
                    {
                        return tokenString;
                    }
                }
                if (input.peek() == '"')
                {
                    tokenString += input.peek();
                    return tokenString;
                }
                return "failed";
            }
            else
            {
                return "null";
            }
        }
        public string CheckNumber(Input input)
        {
            int fraction = 0;
            bool startLoop = false;
            int exponent = 0;
            string number = "";
            char currentCharacter = input.peek();
            {
                if (currentCharacter == '-')
                {
                    number += currentCharacter;
                    input.step();
                    currentCharacter = input.peek();
                    if (Char.IsDigit(currentCharacter))
                    {
                        number += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                        startLoop = true;
                    }
                }
                else if (Char.IsDigit(currentCharacter))
                {
                    number += currentCharacter;
                    input.step();
                    currentCharacter = input.peek();
                    startLoop = true;
                }
                while (input.hasMore() && startLoop && (fraction < 2) && (exponent < 2))
                {
                    if (currentCharacter == '-')
                    {
                        throw new Exception("More than one sign");
                    }
                    else if (Char.IsDigit(currentCharacter))
                    {
                        number += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                    }
                    else if (currentCharacter == '.' && (fraction < 2))
                    {
                        number += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                        fraction++;
                    }
                    else if (currentCharacter == 'e' || currentCharacter == 'E')
                    {
                        number += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                        exponent++;
                        if ((currentCharacter == '-') || (currentCharacter == '+'))
                        {
                            number += currentCharacter;
                            input.step();
                            currentCharacter = input.peek();
                        }
                    }
                    else
                    {
                        throw new Exception("failed");
                    }
                }
                if (!startLoop || (fraction >= 2 || exponent >= 2))
                {
                    throw new Exception("failed");
                }
                return number;
            }
        }
        public static string CheckBool(Input input)
        {
            string[] arr = { "true", "false", "null" };
            string boolString = "";
            char currentCharacter = input.peek();
            if (currentCharacter == 't')
            {
                char[] trueArray = arr[0].ToCharArray();
                foreach (char chr in trueArray)
                {
                    if (currentCharacter == chr)
                    {
                        boolString += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                    }
                    else
                    {
                        throw new Exception("Unexpected true value.");
                    }
                }
                return boolString;
            }
            else if (currentCharacter == 'f')
            {
                char[] falseArray = arr[1].ToCharArray();
                foreach (char chr in falseArray)
                {
                    if (currentCharacter == chr)
                    {
                        boolString += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                    }
                    else
                    {
                        throw new Exception("Unexpected false value.");
                    }
                }
                return boolString;
            }
            else if (currentCharacter == 'n')
            {
                char[] nullArray = arr[2].ToCharArray();
                foreach (char chr in nullArray)
                {
                    if (currentCharacter == chr)
                    {
                        boolString += currentCharacter;
                        input.step();
                        currentCharacter = input.peek();
                    }
                    else
                    {
                        throw new Exception("Unexpected null value.");
                    }
                }
                return boolString;
            }
            else
            {
                throw new Exception("not true, false, null.");
            }
        }
        public static string CheckObject(Input input)
        {
            char currentCharacter = input.peek();
            string arrayString = "";
            if (currentCharacter == '{')
            {
                arrayString += currentCharacter;
                input.step();
                currentCharacter = input.peek();
                while (Char.IsWhiteSpace(currentCharacter) && input.hasMore())
                {
                    currentCharacter = input.peek();
                    arrayString += currentCharacter;
                    input.step();
                }
                while (input.hasMore() && currentCharacter != '}')
                {
                    if (Char.IsLetterOrDigit(currentCharacter))
                    {
                        while (input.hasMore() && (Char.IsLetterOrDigit(currentCharacter) || Char.IsWhiteSpace(currentCharacter)))
                        {
                            arrayString += currentCharacter;
                            input.step();
                            currentCharacter = input.peek();
                        }
                    }
                    else if (currentCharacter == ',')
                    {
                        currentCharacter = input.peek();
                        arrayString += currentCharacter;
                        input.step();
                    }
                    else if (currentCharacter == '{')
                    {
                        while (input.hasMore() && currentCharacter != '}')
                        {
                            currentCharacter = input.peek();
                            arrayString += currentCharacter;
                            input.step();
                        }
                        if (currentCharacter == '}')
                        {
                            currentCharacter = input.peek();
                            arrayString += currentCharacter;
                            input.step();
                        }
                    }
                    else if (Char.IsWhiteSpace(currentCharacter))
                    {
                        while (Char.IsWhiteSpace(currentCharacter) && input.hasMore())
                        {
                            currentCharacter = input.peek();
                            arrayString += currentCharacter;
                            input.step();
                        }
                    }
                }
                if (currentCharacter == ']')
                {
                    arrayString += currentCharacter;
                    return arrayString;
                }
                else
                {
                    return "Error in array.";
                }
            }
            else
            {
                return "Error in array.";
            }
        }
        public static string CheckArray(Input input)
        {
            char currentCharacter = input.peek();
            string arrayString = "";
            if (currentCharacter == '[')
            {
                arrayString += currentCharacter;
                input.step();
                currentCharacter = input.peek();
                while (Char.IsWhiteSpace(currentCharacter) && input.hasMore())
                {
                    currentCharacter = input.peek();
                    arrayString += currentCharacter;
                    input.step();
                }
                while (input.hasMore() && currentCharacter != ']')
                {
                    if (Char.IsLetterOrDigit(currentCharacter))
                    {
                        while (input.hasMore() && (Char.IsLetterOrDigit(currentCharacter) || Char.IsWhiteSpace(currentCharacter)))
                        {
                            arrayString += currentCharacter;
                            input.step();
                            currentCharacter = input.peek();
                        }
                    }
                    else if (currentCharacter == ',')
                    {
                        currentCharacter = input.peek();
                        arrayString += currentCharacter;
                        input.step();
                    }
                    else if (currentCharacter == '{')
                    {
                        while (input.hasMore() && currentCharacter != '}')
                        {
                            currentCharacter = input.peek();
                            arrayString += currentCharacter;
                            input.step();
                        }
                        if (currentCharacter == '}')
                        {
                            currentCharacter = input.peek();
                            arrayString += currentCharacter;
                            input.step();
                        }
                    }
                    else if (Char.IsWhiteSpace(currentCharacter))
                    {
                        while (Char.IsWhiteSpace(currentCharacter) && input.hasMore())
                        {
                            currentCharacter = input.peek();
                            arrayString += currentCharacter;
                            input.step();
                        }
                    }
                }
                if (currentCharacter == ']')
                {
                    arrayString += currentCharacter;
                    return arrayString;
                }
                else
                {
                    return "Error in array.";
                }
            }
            else
            {
                return "Error in array.";
            }
        }
        static void Main(string[] args)
        {
            Stack stack = new Stack();
            Stack stackRev = new Stack();
            Program p = new Program();
            Tokenizer t = new Tokenizer(new Input(" {\"Name\":\"Ali\"}"), new Tokenizable[] {
                new WhiteSpaceTokenizer(),
                new Boolean(),
                new StringKey(),
                new NumberTokenizer(),
                new ObjectStart(),
                new ObjectEnd(),
                new Colon(),
                new Comma(),
                new ArrayStart(),
                new ArrayEnd()

            });


            Token token = t.tokenize();

            string checkString = "";


            string peek = null;
            while (token != null)
            {
                if (token.Value.Contains("\""))
                {
                    Input i = new Input(token.Value);
                    checkString = CheckString(i);
                    //  Console.WriteLine(checkString);
                }
                if (token.Type.Equals("number"))
                {
                    Input i = new Input(token.Value);
                    p.CheckNumber(i);
                    //  Console.WriteLine(checkNumber);
                }
                if (token.Type.Equals("boolean"))
                {
                    Input i = new Input(token.Value);
                    CheckBool(i);
                    //  Console.WriteLine(checkNumber);
                }
                if (token.Type.Equals("ObjectStart"))
                {
                    Input i = new Input(token.Value);
                    CheckObject(i);
                    //  Console.WriteLine(checkNumber);
                }
                if (token.Type.Equals("ArrayStart"))
                {
                    Input i = new Input(token.Value);
                    CheckArray(i);
                    //  Console.WriteLine(checkNumber);
                }
                if (checkString.Equals("failed")) { stack.Push("invalid"); }


                if (stack.Count != 0) { peek = (string)stack.Peek(); }
                if (stack.Count == 0 && (token.Value.Equals("{"))) { stack.Push(token.Value); }
                else if (token.Type.Equals("whitespace")) { }
                else if (stack.Count != 0 && token.Value != peek && token.Type.Equals("boolean")) { stack.Push(token.Value); }
/*                else if (stack.Count != 0 && token.Value != peek && token.Type.Equals("ObjectStart")) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && token.Type.Equals("ArrayStart")) { stack.Push(token.Value); }*/
                else if (stack.Count != 0 && token.Value != peek && token.Type.Equals("StringKey")) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && token.Value.Equals(":")) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && (token.Value.Equals(",") || token.Value.Equals('}'))) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && token.Type.Equals("number")) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && token.Value.Equals("}")) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && token.Value.Equals("[")) { stack.Push(token.Value); }
                else if (stack.Count != 0 && token.Value != peek && token.Value.Equals("]")) { stack.Push(token.Value); }
                token = t.tokenize();



            }

            string jesonFormat = "";
            foreach (Object obj in stack)
            {
                //Console.Write(obj);

                stackRev.Push(obj);

            }
            //Console.WriteLine();

            if (stackRev.Contains("invalid"))
            {
                Console.WriteLine("invalid json format");
            }
            else
            {
                foreach (Object obj in stackRev)
                {
                    // Console.Write(obj);

                    jesonFormat += obj;


                }
            }


            JsonValue j;
            Jparser jparser = new Jparser();
            j = jparser.jkeyValue(jesonFormat);
            Console.WriteLine(j);
            

        }
    }
}