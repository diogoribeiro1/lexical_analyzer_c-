using System.Text;
using CompiladorAnalisador.Models;

namespace CompiladorAnalisador.Services
{
    public class ReportGenerator
    {
        private readonly FileService _fileService;
        private readonly ReportHeader _reportHeader;

        public ReportGenerator()
        {
            _fileService = new FileService();
            _reportHeader = new ReportHeader();
        }

        public void GenerateLexicalReport(List<Token> tokens, string inputPath)
        {
            var outputPath = _fileService.GetOutputPath(inputPath, ".LEX");
            string fileName = Path.GetFileName(inputPath);

            var categoryCTokens = tokens.Where(t => t.Code.StartsWith("idn")).ToList();
            var uniqueSymbols = GroupUniqueSymbols(categoryCTokens);

            var symbolIndexMap = uniqueSymbols
                .Select((sym, idx) => new { sym.Lexeme, Index = idx + 1 })
                .ToDictionary(x => x.Lexeme, x => x.Index);

            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                _reportHeader.WriteLexicalHeader(writer, fileName);

                foreach (var token in tokens)
                {
                    string indexStr = "";

                    if (symbolIndexMap.ContainsKey(token.Lexeme))
                    {
                        indexStr = symbolIndexMap[token.Lexeme].ToString();
                    }

                    writer.WriteLine(
                        $"Lexeme: {token.Lexeme}, Código: {token.Code}, ÍndiceTabSimb: {indexStr}, Linha: {token.Lines.First()}.");
                }
            }

            Console.WriteLine($"Relatório lexical gerado: {outputPath}");
        }

        public void GenerateSymbolTableReport(List<Token> tokens, string inputPath)
        {
            string fileName = Path.GetFileName(inputPath);

            var outputPath = _fileService.GetOutputPath(inputPath, ".TAB");

            var categoryCTokens = tokens
                .Where(t => t.Code.StartsWith("idn"))
                .ToList();

            var uniqueSymbols = GroupUniqueSymbols(categoryCTokens);

            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                _reportHeader.WriteSymbolTableHeader(writer, fileName);
                WriteSymbolTable(writer, uniqueSymbols);
            }

            Console.WriteLine($"Tabela de símbolos gerada: {outputPath}");
        }

        private void WriteSymbolTable(StreamWriter writer, List<SymbolTableEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                string linesStr = string.Join(", ", entry.Lines);

                writer.WriteLine(
                    $"Entrada: {i + 1}, Codigo: {entry.Code}, Lexeme: {entry.Lexeme}, QtdCharAntesTrunc: {entry.OriginalSize}, QtdCharDepoisTrunc: {entry.TruncatedSize}, TipoSimb: {entry.SymbolType}, Linhas: {{ {linesStr} }}.");
                writer.WriteLine(new string('-', 71));
            }
        }

        private List<SymbolTableEntry> GroupUniqueSymbols(List<Token> tokens)
        {
            var grouped = tokens
                .GroupBy(t => t.Lexeme)
                .Select(g =>
                {
                    var firstOccurrence = g.First();

                    var allLines = g.SelectMany(t => t.Lines).Distinct().OrderBy(x => x).ToList();

                    return new SymbolTableEntry
                    {
                        Lexeme = firstOccurrence.Lexeme,
                        Code = firstOccurrence.Code,
                        SymbolType = firstOccurrence.SymbolType,
                        OriginalSize = firstOccurrence.OriginalSize,
                        TruncatedSize = firstOccurrence.AdjustedSize,
                        Lines = allLines
                    };
                })
                .OrderBy(x => x.Code)
                .ThenBy(x => x.Lexeme)
                .ToList();

            return grouped;
        }

        private class SymbolTableEntry
        {
            public string Lexeme { get; set; }
            public string Code { get; set; }
            public string SymbolType { get; set; }
            public int OriginalSize { get; set; }
            public int TruncatedSize { get; set; }
            public List<int> Lines { get; set; }
        }
    }

    public class ReportHeader
    {
        private const string TEAM_CODE_SYMBOL = "EQ03";

        private const string MEMBER1 = "Kauã Vilas Boas | kaua.boas@ucsal.edu.br | 71 99609-9664";
        private const string MEMBER2 = "Gabriel Cunha | gabrielsouza.cunha@ucsal.edu.br | 71 98212-3984";
        private const string MEMBER3 = "Diogo Ramos | diogo.ramos@ucsal.edu.br | 71 98686-7971";
        private const string MEMBER4 = "Atila Macedo | atila.macedo@ucsal.edu.br | 71 99251-9722";

        private void WriteMembers(StreamWriter writer)
        {
            writer.WriteLine("Código da Equipe: {0}", TEAM_CODE_SYMBOL);
            writer.WriteLine("Componentes:");
            writer.WriteLine("  {0}", MEMBER1);
            writer.WriteLine("  {0}", MEMBER2);
            writer.WriteLine("  {0}", MEMBER3);
            writer.WriteLine("  {0}", MEMBER4);
            writer.WriteLine("");
        }

        public void WriteLexicalHeader(StreamWriter writer, string fileName)
        {
            WriteMembers(writer);
            writer.WriteLine($"RELATÓRIO DE ANÁLISE LÉXICA. Texto fonte analisado: {fileName}");
            writer.WriteLine("");
        }

        public void WriteSymbolTableHeader(StreamWriter writer, string fileName)
        {
            WriteMembers(writer);
            writer.WriteLine($"RELATÓRIO DE ANÁLISE LÉXICA. Texto fonte analisado: {fileName}");
            writer.WriteLine("");
        }
    }
}