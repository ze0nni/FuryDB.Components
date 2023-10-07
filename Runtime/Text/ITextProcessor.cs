using Fury.Strings;

namespace FDB.Components.Text
{
    public delegate bool VariableProcessorDelegate(StringRef variable, ref FormatBuffer buffer);

    public interface ITextProcessor
    {
        string Execute(string format, string[] args = null, VariableProcessorDelegate variableProcessor = null);
    }
}