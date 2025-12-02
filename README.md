# Compilador Caatinguage 2025-2

Este projeto implementa um **Analisador Léxico** para a linguagem *Caatinguage 2025-2*. Desenvolvido em C# (.NET 6.0), a ferramenta lê arquivos fonte com a extensão `.252`, processa os tokens e gera relatórios detalhados de análise léxica e tabela de símbolos.

## 📋 Funcionalidades

* **Análise Léxica Completa**: Identificação de tokens, incluindo palavras reservadas, identificadores, constantes (inteiro, real, string, char) e símbolos especiais.
* **Relatórios Automáticos**:
    * **Arquivo .LEX**: Relatório contendo a sequência de tokens identificados.
    * **Arquivo .TAB**: Tabela de símbolos contendo identificadores e constantes agrupados.
* **Validação**: Verificação automática da extensão `.252` e existência do arquivo.
* **Modos de Uso**: Suporte para execução via linha de comando ou menu interativo.

## 🚀 Manual de Instalação

### Pré-requisitos

* [**.NET SDK 6.0**](https://dotnet.microsoft.com/download/dotnet/6.0) instalado.

### Passos para Instalação

1. **Clone o repositório** (ou extraia os arquivos do projeto).
2. **Acesse a pasta da solução**:
```bash
   cd CompiladorAnalisador
```
3. **Compile o projeto**:
```bash
   dotnet build
```

## 📖 Manual de Uso

Você pode executar o compilador de duas formas:

### 1. Modo Interativo

Execute o programa sem argumentos. Ele solicitará o caminho do arquivo.
```bash
dotnet run
```

* Digite o caminho do arquivo (ex: `teste.252`) quando solicitado.
* Para encerrar, digite `sair` ou `exit`.

### 2. Modo Linha de Comando

Passe o caminho do arquivo diretamente como argumento.
```bash
dotnet run -- "caminho/do/arquivo.252"
```

## 📂 Entradas e Saídas

* **Entrada**: O arquivo deve ter a extensão `.252`.
   * Exemplo de código pode ser visto no arquivo `teste.252` incluído no projeto.
* **Saída**: Os relatórios são gerados na pasta `Output/Report` localizada na raiz da solução.
   * Exemplo: Se a entrada for `teste.252`, serão gerados `teste.LEX` e `teste.TAB`.

## 🛠 Estrutura do Projeto

* `Program.cs`: Ponto de entrada e interface com o usuário.
* `Services/`:
   * `LexicalAnalyzer.cs`: Lógica de identificação de tokens (Autômato Finito).
   * `ReportGenerator.cs`: Geração dos arquivos de relatório.
   * `FileService.cs`: Manipulação de arquivos e diretórios.
* `Util/Constants.cs`: Definição de palavras reservadas e códigos de tokens.

## 👨‍💻 Autores (Equipe EQ03)

Informações extraídas do cabeçalho dos relatórios:

* Kauã Vilas Boas
* Gabriel Cunha
* Diogo Ramos
* Atila Macedo
