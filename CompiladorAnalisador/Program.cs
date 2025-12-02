using System;
using System.IO;
using CompiladorAnalisador.Services;

namespace CompiladorAnalisador
{
    class Program
    {
        private static readonly LexicalAnalyzer _lexicalAnalyzer = new LexicalAnalyzer();
        private static readonly ReportGenerator _reportGenerator = new ReportGenerator();
        private static readonly FileService _fileService = new FileService();

        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    string input = args[0]?.Trim().Replace("\"", "");
                    RunCompilation(input);
                }
                else
                {
                    ShowBanner();

                    while (true)
                    {
                        Console.WriteLine();
                        Console.Write("Digite o caminho do arquivo fonte (sem precisar informar a extensão .252) ou 'sair' para encerrar: ");
                        string input = Console.ReadLine()?.Trim()
                            .Replace("\"", "");

                        if (string.IsNullOrEmpty(input)) continue;

                        if (input.Equals("sair", StringComparison.OrdinalIgnoreCase) ||
                            input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Encerrando o compilador...");
                            break;
                        }

                        RunCompilation(input);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nERRO CRÍTICO NO PROGRAMA: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
            }
        }

        private static void RunCompilation(string inputPath)
        {
            // Permitir que o usuário não informe a extensão:
            // se não tiver extensão, assume .252
            if (string.IsNullOrWhiteSpace(inputPath))
                throw new ArgumentException("Caminho do arquivo não informado.");

            string filePath = inputPath;

            string extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
            {
                // usuário passou algo como: C:\pasta\teste
                filePath = filePath + ".252";
                extension = ".252";
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"Iniciando compilação: {Path.GetFileName(filePath)}");

            try
            {
                _fileService.ValidateFile(filePath);

                if (!extension.Equals(".252", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception(
                        $"Extensão inválida: '{extension}'. O compilador aceita apenas arquivos '.252'.");
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(">> Executando Análise Léxica...");
                Console.ResetColor();

                var tokens = _lexicalAnalyzer.AnalyzeFile(filePath);

                if (tokens == null || tokens.Count == 0)
                {
                    Console.WriteLine("AVISO: Nenhum token foi encontrado ou o arquivo está vazio.");
                }
                else
                {
                    Console.WriteLine($"   Tokens identificados: {tokens.Count}");
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(">> Gerando Relatórios...");
                Console.ResetColor();

                // IMPORTANTE:
                // Assumindo que o ReportGenerator usa o filePath para montar o caminho,
                // ele agora deve gerar .LEX e .TAB no MESMO DIRETÓRIO do arquivo .252
                _reportGenerator.GenerateLexicalReport(tokens, filePath);
                _reportGenerator.GenerateSymbolTableReport(tokens, filePath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[SUCESSO] Compilação finalizada! Verifique os relatórios no mesmo diretório do arquivo fonte.");
                Console.ResetColor();
            }
            catch (FileNotFoundException fnfEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERRO] Arquivo não encontrado: {fnfEx.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERRO] Falha na compilação: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine(new string('-', 60));
            }
        }

        private static void ShowBanner()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("         COMPILADOR CAATINGUAGE 2025-2  (v1.0)              ");
            Console.WriteLine("============================================================");
            Console.WriteLine(" Instruções:");
            Console.WriteLine("  1. Digite o caminho completo do arquivo fonte,");
            Console.WriteLine("     sem precisar informar a extensão .252.");
            Console.WriteLine("  2. O compilador irá procurar automaticamente o arquivo .252.");
            Console.WriteLine("  3. Os relatórios .LEX e .TAB serão gerados no mesmo diretório");
            Console.WriteLine("     onde o arquivo fonte se encontra.");
            Console.WriteLine("============================================================");
            Console.ResetColor();
        }
    }
}
