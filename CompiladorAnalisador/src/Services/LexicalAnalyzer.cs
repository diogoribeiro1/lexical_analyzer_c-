using System;
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

            InferSymbolTypesFromDeclarations();
            return _tokens.ToList();
        }

        private Token GetNextToken()
        {
            var currentAtom = new StringBuilder();
            int atomLength = 0; // Tamanho total do átomo (antes da truncagem)
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
                                    return CreateToken(":=", "srs04");
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
                        atomLength++;
                        if (currentAtom.Length < 35)
                            currentAtom.Append(ch);

                        char nx = (char)_reader.Peek();

                        if (!char.IsLetterOrDigit(nx) && nx != '_')
                        {
                            string atom = currentAtom.ToString();
                            string lower = atom.ToLower();

                            if (Constants.ReservedWords.Dictionary.ContainsKey(lower))
                                return CreateToken(atom, Constants.ReservedWords.Dictionary[lower], atomLength);

                            string code = "idn02";

                            if (_tokens.Count > 0)
                            {
                                var last = _tokens[_tokens.Count - 1];

                                if (last.Lexeme.Equals("PROGRAM", StringComparison.OrdinalIgnoreCase))
                                {
                                    code = "idn01";
                                }
                                else if (_tokens.Count >= 3)
                                {
                                    var t1 = _tokens[_tokens.Count - 3];
                                    var t2 = _tokens[_tokens.Count - 2];
                                    var t3 = last;

                                    if (t1.Lexeme.Equals("FUNCTYPE", StringComparison.OrdinalIgnoreCase) &&
                                        IsTypeSpecification(t2.Lexeme) &&
                                        t3.Lexeme == ":")
                                    {
                                        code = "idn03";
                                    }
                                }
                            }

                            return CreateToken(atom, code, atomLength);
                        }

                        break;

                    case 2:
                        ch = ConsumeChar();
                        atomLength++;
                        if (currentAtom.Length < 35)
                            currentAtom.Append(ch);

                        char nxNum = (char)_reader.Peek();
                        if (nxNum == '.')
                        {
                            _currentState = 4;
                        }
                        else if (!char.IsDigit(nxNum))
                        {
                            return CreateToken(currentAtom.ToString(), "idn04", atomLength);
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
                        atomLength++;
                        if (currentAtom.Length < 35)
                            currentAtom.Append(ch);

                        if (char.IsDigit((char)_reader.Peek()))
                        {
                            _currentState = 5;
                        }
                        else return null;
                        break;

                    case 5:
                        ch = ConsumeChar();
                        atomLength++;
                        if (currentAtom.Length < 35)
                            currentAtom.Append(ch);

                        char nxFloat = (char)_reader.Peek();
                        if (!char.IsDigit(nxFloat))
                            return CreateToken(currentAtom.ToString(), "idn05", atomLength);

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
                        atomLength = 1;
                        _currentState = 11;
                        break;

                    case 11:
                        ch = ConsumeChar();
                        atomLength++;

                        if (ch == '"')
                            return CreateToken($"\"{currentAtom}\"", "idn06", atomLength);

                        if (ch == '\n' || ch == '\uffff')
                            return null;

                        if (currentAtom.Length < 35)
                            currentAtom.Append(ch);
                        break;

                    case 12:
                        ConsumeChar();
                        atomLength = 1;

                        ch = ConsumeChar();
                        atomLength++;
                        if (currentAtom.Length < 35)
                            currentAtom.Append(ch);

                        if ((char)_reader.Peek() == '\'')
                        {
                            ConsumeChar();
                            atomLength++;
                            return CreateToken($"'{currentAtom}'", "idn07", atomLength);
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

        private Token CreateToken(string lexeme, string code, int originalLength = -1)
        {
            if (originalLength <= 0)
                originalLength = lexeme.Length;

            var token = new Token(lexeme.ToUpper(), code, _currentLine, originalLength);
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

        private void InferSymbolTypesFromDeclarations()
        {
            var symbolTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _tokens.Count; i++)
            {
                var tk = _tokens[i];

                if (tk.Lexeme == "VARTYPE" && i + 3 < _tokens.Count)
                {
                    var typeToken = _tokens[i + 1];
                    var colon = _tokens[i + 2];

                    if (colon.Lexeme != ":")
                        continue;

                    string tipoSimb = MapTypeSpecification(typeToken.Lexeme);
                    if (tipoSimb == "-") continue;

                    int j = i + 3;
                    while (j < _tokens.Count)
                    {
                        var t = _tokens[j];

                        if (t.Lexeme == ";" || t.Lexeme == "ENDDECLARATIONS")
                            break;

                        if (t.Code == "idn02")
                        {
                            symbolTypeMap[t.Lexeme] = tipoSimb;
                        }

                        j++;
                    }
                }

                if (tk.Lexeme == "FUNCTYPE" && i + 3 < _tokens.Count)
                {
                    var typeToken = _tokens[i + 1];
                    var colon = _tokens[i + 2];
                    var funcName = _tokens[i + 3];

                    if (colon.Lexeme != ":" || funcName.Code != "idn03")
                        continue;

                    string tipoSimb = MapTypeSpecification(typeToken.Lexeme);
                    if (tipoSimb == "-") continue;

                    symbolTypeMap[funcName.Lexeme] = tipoSimb;
                }
                if (tk.Lexeme == "PARAMTYPE" && i + 3 < _tokens.Count)
                {
                    var typeToken = _tokens[i + 1];
                    var colon = _tokens[i + 2];

                    if (colon.Lexeme != ":")
                        continue;

                    string tipoSimb = MapTypeSpecification(typeToken.Lexeme);
                    if (tipoSimb == "-") continue;

                    int j = i + 3;
                    while (j < _tokens.Count)
                    {
                        var t = _tokens[j];

                        // fim da lista de parâmetros: fecha parêntese ou vírgula/; dependendo da gramática
                        if (t.Lexeme == ")" || t.Lexeme == ";" || t.Lexeme == "ENDDECLARATIONS")
                            break;

                        if (t.Code == "idn02") // parâmetros são identificadores como variáveis
                        {
                            symbolTypeMap[t.Lexeme] = tipoSimb; // N -> IN
                        }

                        j++;
                    }
                }
            }

            foreach (var token in _tokens)
            {
                if (token.Code == "idn01" || token.Code == "idn02" || token.Code == "idn03")
                {
                    if (symbolTypeMap.TryGetValue(token.Lexeme, out var symType))
                    {
                        token.SymbolType = symType;
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
        }

        private static string MapTypeSpecification(string lexeme)
        {
            switch (lexeme.ToUpper())
            {
                case "INTEGER": return "IN";
                case "REAL": return "FL";
                case "STRING": return "ST";
                case "CHARACTER": return "CH";
                case "BOOLEAN": return "BL";
                case "VOID": return "VD";
                default: return "-";
            }
        }

    }
}
