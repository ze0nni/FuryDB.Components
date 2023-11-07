using Fury.Strings;

namespace FDB.Components.Text
{
    public delegate bool VariableProcessorDelegate(StringRef variable, FormatBuffer buffer);

    public interface ITextProcessor
    {
        string Execute(
            string format,
            string currentString = null,
            Args args = null,
            VariableProcessorDelegate variableProcessor = null);
    }
}