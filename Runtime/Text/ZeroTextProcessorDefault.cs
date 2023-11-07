using Fury.Strings;
using System;

namespace FDB.Components.Text
{
    public class ZeroTextProcessorDefault : ITextProcessor
    {
        public static readonly Lazy<ZeroTextProcessorDefault> _instance = new Lazy<ZeroTextProcessorDefault>();
        public static ZeroTextProcessorDefault Instance => _instance.Value;

        readonly ZeroFormat _format = new ZeroFormat(1024);
        readonly ZeroFormat.VariableProcessorDelegate _internalVariableProcessor;
        VariableProcessorDelegate _currentVariableProcessor;

        public ZeroTextProcessorDefault()
        {
            _internalVariableProcessor = VariableProcessor;
        }

        public string Execute(
            string format,
            string currentString = null,
            Args args = null, 
            VariableProcessorDelegate variableProcessor = null)
        {
            _currentVariableProcessor = variableProcessor;

            _format.Setup(
                format,
                args,
                variableProcessor == null ? null : _internalVariableProcessor);
            var result = _format.ToString(currentString);

            _currentVariableProcessor = null;
            return result;
        }

        private void VariableProcessor(StringRef variable, FormatBuffer buffer)
        {
            _currentVariableProcessor(variable, buffer);
        }
    }
}
