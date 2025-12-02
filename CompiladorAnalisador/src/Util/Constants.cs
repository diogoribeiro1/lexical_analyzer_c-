namespace CompiladorAnalisador.Util
{
    public class Constants
    {
        public static class ReservedWords
        {
            public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>
            {
                { "integer", "prs01" },
                { "real", "prs02" },
                { "character", "prs03" },
                { "string", "prs04" },
                { "boolean", "prs05" },
                { "void", "prs06" },

                { "true", "prs07" },
                { "false", "prs08" },

                { "vartype", "prs09" },       
                { "functype", "prs10" },      
                { "paramtype", "prs11" },     

                { "declarations", "prs12" },
                { "enddeclarations", "prs13" },

                { "program", "prs14" },
                { "endprogram", "prs15" },

                { "functions", "prs16" },
                { "endfunctions", "prs17" },
                { "endfunction", "prs18" },

                { "return", "prs19" },
                { "if", "prs20" },
                { "else", "prs21" },
                { "endif", "prs22" },

                { "while", "prs23" },
                { "endwhile", "prs24" },

                { "break", "prs25" },

                { "print", "prs26" },

                // Símbolos Reservados (SRS)
                { ";", "srs01" },
                { ",", "srs02" },
                { ":", "srs03" },
                { ":=", "srs04" },
                { "?", "srs05" },
                { "(", "srs06" },
                { ")", "srs07" },
                { "[", "srs08" },
                { "]", "srs09" },
                { "{", "srs10" },
                { "}", "srs11" },

                { "+", "srs12" },
                { "-", "srs13" },
                { "*", "srs14" },
                { "/", "srs15" },
                { "%", "srs16" },

                { "==", "srs17" },
                { "!=", "srs18" },
                { "<", "srs19" },
                { "<=", "srs20" },
                { ">", "srs21" },
                { ">=", "srs22" }
            };
        }

        public static class TokenCategory
        {
            public const string ProgramName = "idn01";
            public const string Variable = "idn02";
            public const string FunctionName = "idn03";
            public const string IntConst = "idn04";
            public const string RealConst = "idn05";
            public const string StringConst = "idn06";
            public const string CharConst = "idn07";
        }
    }
}