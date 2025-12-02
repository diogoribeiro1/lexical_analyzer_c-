using System.Text;
using CompiladorAnalisador.Models;
using CompiladorAnalisador.Util;

namespace CompiladorAnalisador.Services
{
    public class LexicalAnalyzer
    {
        private readonly FileService _fileService;
        private StreamReader _reader;
        private int _currentState;
        private int _currentLine;
        private readonly List<Token> _tokens;

        public LexicalAnalyzer()
        {
            _fileService = new FileService();
            _tokens = new List<Token>();
            _currentLine = 1;
        }

        public List<Token> AnalyzeFile(string filePath)
        {
            _reader = _fileService.OpenFile(filePath);
            _tokens.Clear();
            _currentLine = 1;

            try
            {
                while (!_reader.EndOfStream)
                {
                    var token = GetNextToken();
                    if (token != null)
                        ProcessToken(token);
                }
            }
            finally
            {
                _reader?.Dispose();
            }

            return _tokens.ToList();
        }

        private Token GetNextToken()
        {
            var currentAtom = new StringBuilder();
            char ch;
            _currentState = 0;

            while (true)
            {
                if (_reader.EndOfStream && _currentState == 0)
                    return null;

                switch (_currentState)
                {
                    case 0:
                        ch = (char)_reader.Peek();
                        if (ch == '\uffff') return null;

                        if (IsWhiteSpace(ch))
                        {
                            ConsumeChar();
                            if (ch == '\n') _currentLine++;
                            break;
                        }

                        if (char.IsLetter(ch) || ch == '_')
                        {
                            _currentState = 1;
                            break;
                        }

                        if (char.IsDigit(ch))
                        {
                            _currentState = 2;
                            break;
                        }

                        switch (ch)
                        {
                            case '(': return CreateSingleCharToken("srs06");
                            case ')': return CreateSingleCharToken("srs07");
                            case '[': return CreateSingleCharToken("srs08");
                            case ']': return CreateSingleCharToken("srs09");
                            case '{': return CreateSingleCharToken("srs10");
                            case '}': return CreateSingleCharToken("srs11");

                            case ';': return CreateSingleCharToken("srs01");
                            case ',': return CreateSingleCharToken("srs02");

                            case ':':
                                ConsumeChar();
                                if (!_reader.EndOfStream && (char)_reader.Peek() == '=')
                                {
                                    ConsumeChar();
                                    return CreateToken(":=", "srs04"); // atribuição
                                }
                                return CreateToken(":", "srs03");

                            case '?': return CreateSingleCharToken("srs05");

                            case '+': return CreateSingleCharToken("srs12");
                            case '-': return CreateSingleCharToken("srs13");
                            case '*': return CreateSingleCharToken("srs14");
                            case '%': return CreateSingleCharToken("srs16");

                            case '/':
                                _currentState = 3;
                                break;

                            case '!':
                                _currentState = 6;
                                break;

                            case '=':
                                _currentState = 7;
                                break;

                            case '<':
                                _currentState = 8;
                                break;

                            case '>':
                                _currentState = 9;
                                break;

                            case '#':
                                return CreateSingleCharToken("srs23");

                            case '"':
                                _currentState = 10;
                                break;

                            case '\'':
                                _currentState = 12;
                                break;

                            default:
                                ConsumeChar();
                                break;
                        }

                        break;

                    case 1:
                        ch = ConsumeChar();
                        currentAtom.Append(ch);

                        char nx = (char)_reader.Peek();

                        if (!char.IsLetterOrDigit(nx) && nx != '_')
                        {
                            string atom = currentAtom.ToString();
                            string lower = atom.ToLower();

                            if (Constants.ReservedWords.Dictionary.ContainsKey(lower))
                                return CreateToken(atom, Constants.ReservedWords.Dictionary[lower]);

                            string code = "idn02";

                            if (_tokens.Count > 0)
                            {
                                var last = _tokens[_tokens.Count - 1];
                                if (last.Lexeme.Equals("program", StringComparison.OrdinalIgnoreCase))
                                {
                                    code = "idn01";
                                }
                                else if (_tokens.Count >= 3)
                                {
                                    var t1 = _tokens[_tokens.Count - 3];
                                    var t2 = _tokens[_tokens.Count - 2];
                                    var t3 = last;

                                    if (t1.Lexeme.Equals("funcType", StringComparison.OrdinalIgnoreCase) &&
                                        IsTypeSpecification(t2.Lexeme) &&
                                        t3.Lexeme == ":")
                                    {
                                        code = "idn03";
                                    }
                                }
                            }

                            return CreateToken(atom, code);
                        }

                        break;

                    case 2:
                        ch = ConsumeChar();
                        currentAtom.Append(ch);

                        char nxNum = (char)_reader.Peek();
                        if (nxNum == '.')
                        {
                            _currentState = 4;
                        }
                        else if (!char.IsDigit(nxNum))
                        {
                            return CreateToken(currentAtom.ToString(), "idn04");
                        }

                        break;

                    case 3:
                        ConsumeChar();
                        char nextSlash = (char)_reader.Peek();

                        if (nextSlash == '/')
                        {
                            ConsumeChar();
                            _currentState = 14;
                        }
                        else if (nextSlash == '*')
                        {
                            ConsumeChar();
                            _currentState = 15;
                        }
                        else
                        {
                            return CreateToken("/", "srs15");
                        }
                        break;

                    case 4:
                        ch = ConsumeChar();
                        currentAtom.Append(ch);

                        if (char.IsDigit((char)_reader.Peek()))
                        {
                            _currentState = 5;
                        }
                        else return null;
                        break;

                    case 5:
                        ch = ConsumeChar();
                        currentAtom.Append(ch);

                        char nxFloat = (char)_reader.Peek();
                        if (!char.IsDigit(nxFloat))
                            return CreateToken(currentAtom.ToString(), "idn05");

                        break;

                    case 6:
                        ConsumeChar();
                        if (!_reader.EndOfStream && (char)_reader.Peek() == '=')
                        {
                            ConsumeChar();
                            return CreateToken("!=", "srs18");
                        }
                        _currentState = 0;
                        break;

                    case 7:
                        ConsumeChar();
                        if (!_reader.EndOfStream && (char)_reader.Peek() == '=')
                        {
                            ConsumeChar();
                            return CreateToken("==", "srs17");
                        }
                        _currentState = 0;
                        break;

                    case 8:
                        ConsumeChar();
                        if (!_reader.EndOfStream && (char)_reader.Peek() == '=')
                        {
                            ConsumeChar();
                            return CreateToken("<=", "srs20");
                        }
                        return CreateToken("<", "srs19");

                    case 9:
                        ConsumeChar();
                        if (!_reader.EndOfStream && (char)_reader.Peek() == '=')
                        {
                            ConsumeChar();
                            return CreateToken(">=", "srs22");
                        }
                        return CreateToken(">", "srs21");

                    case 10:
                        ConsumeChar();
                        _currentState = 11;
                        break;

                    case 11:
                        ch = ConsumeChar();
                        if (ch == '"')
                            return CreateToken($"\"{currentAtom}\"", "idn06");

                        if (ch == '\n' || ch == '\uffff')
                            return null;

                        currentAtom.Append(ch);
                        break;

                    case 12:
                        ConsumeChar();
                        ch = ConsumeChar();
                        currentAtom.Append(ch);

                        if ((char)_reader.Peek() == '\'')
                        {
                            ConsumeChar();
                            return CreateToken($"'{currentAtom}'", "idn07");
                        }

                        return null;

                    case 14:
                        ch = ConsumeChar();
                        if (ch == '\n' || ch == '\uffff')
                        {
                            if (ch == '\n') _currentLine++;
                            _currentState = 0;
                        }
                        break;

                    case 15:
                        ch = ConsumeChar();
                        if (ch == '\n') _currentLine++;

                        if (ch == '*')
                        {
                            if ((char)_reader.Peek() == '/')
                            {
                                ConsumeChar();
                                _currentState = 0;
                            }
                        }
                        else if (ch == '\uffff')
                            return null;

                        break;
                }
            }
        }

        private char ConsumeChar() => (char)_reader.Read();

        private Token CreateSingleCharToken(string code)
        {
            string lex = ((char)_reader.Read()).ToString();
            return CreateToken(lex, code);
        }

        private Token CreateToken(string lexeme, string code)
        {
            var token = new Token(lexeme, code, _currentLine, lexeme.Length);
            return token;
        }

        private void ProcessToken(Token token) => _tokens.Add(token);

        private static bool IsTypeSpecification(string lexeme)
        {
            return lexeme.Equals("integer", StringComparison.OrdinalIgnoreCase) ||
                lexeme.Equals("real", StringComparison.OrdinalIgnoreCase) ||
                lexeme.Equals("string", StringComparison.OrdinalIgnoreCase) ||
                lexeme.Equals("boolean", StringComparison.OrdinalIgnoreCase) ||
                lexeme.Equals("character", StringComparison.OrdinalIgnoreCase) ||
                lexeme.Equals("void", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWhiteSpace(char c) =>
            c == ' ' || c == '\n' || c == '\t' || c == '\r';
    }
}
