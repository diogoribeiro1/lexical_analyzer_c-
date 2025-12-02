using System;
using System.IO;

namespace CompiladorAnalisador.Services
{
    public class FileService
    {
        public StreamReader OpenFile(string path)
        {
            var fullPath = GetFullPath(path);
            ValidateFile(fullPath);
            return new StreamReader(fullPath);
        }

        public void ValidateFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Caminho do arquivo é obrigatório");

            var fullPath = GetFullPath(path);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Arquivo não encontrado: {fullPath}");
        }

        public string GetOutputPath(string inputPath, string extension)
        {
            var fullInputPath = GetFullPath(inputPath);

            var directory = Path.GetDirectoryName(fullInputPath)
                           ?? Directory.GetCurrentDirectory();

            var baseFileName = Path.GetFileNameWithoutExtension(fullInputPath);

            return Path.Combine(directory, baseFileName + extension);
        }

        private string GetFullPath(string path)
        {
            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(Directory.GetCurrentDirectory(), path);
        }
    }
}
