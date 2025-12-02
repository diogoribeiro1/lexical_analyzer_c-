namespace CompiladorAnalisador.Models
{
    public class Token
    {
        public string Lexeme { get; private set; }
        public string Code { get; private set; }
        public int OriginalSize { get; private set; }
        public int AdjustedSize => Lexeme.Length;
        public string SymbolType { get; set; }

        public List<int> Lines { get; private set; }

        public Token(string lexeme, string code, int line, int originalSize)
        {
            Lexeme = lexeme;
            Code = code;
            OriginalSize = originalSize;
            Lines = new List<int> { line };

            SymbolType = InferTypeFromCode(code);
        }

        public void IncrementAppearance(int line)
        {
            if (!Lines.Contains(line))
                Lines.Add(line);
        }

        public string GetLinesString()
        {
            var top5 = Lines.OrderBy(x => x).Take(5);
            return string.Join(", ", top5);
        }
        
        private string InferTypeFromCode(string code)
        {
            switch (code)
            {
                case "idn04": return "intConst";
                case "idn05": return "realConst";
                case "idn06": return "stringConst";
                case "idn07": return "charConst";
                default: return "-";
            }
        }
    }
}