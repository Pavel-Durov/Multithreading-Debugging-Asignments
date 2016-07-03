﻿using WinHandlesQuerier.Core.Infra;
using CommandLine;
using CommandLine.Text;


namespace Consumer.CmdParams
{
    public class Options
    {
        [Option('d', "dump", HelpText = "Input dump file absolute path.")]
        public string DumpFile { get; set; }

        [Option('p', "live", HelpText = "Enter Live process pid.")]
        public int LivePid { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }
    }
}
