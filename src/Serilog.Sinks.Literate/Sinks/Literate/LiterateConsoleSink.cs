// Copyright 2015 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Parsing;

namespace Serilog.Sinks.Literate
{    
    class LiterateConsoleSink : ILogEventSink
    {
        const ConsoleColor Text = ConsoleColor.White,
                           Subtext = ConsoleColor.Gray,
                           Punctuation = ConsoleColor.DarkGray,

                           VerboseLevel = ConsoleColor.Gray,
                           DebugLevel = VerboseLevel,
                           InformationLevel = ConsoleColor.White,
                           WarningLevel = ConsoleColor.Yellow,
                           ErrorLevel = ConsoleColor.Red,
                           FatalLevel = ErrorLevel,

                           KeywordSymbol = ConsoleColor.Blue,
                           NumericSymbol = ConsoleColor.Magenta,
                           StringSymbol = ConsoleColor.Cyan,
                           OtherSymbol = ConsoleColor.Green,
                           NameSymbol = Subtext,
                           RawText = ConsoleColor.Yellow;

        const string StackFrameLinePrefix = "   ";

        class LevelFormat
        {
            readonly string _description;
            readonly ConsoleColor _color;

            public LevelFormat(string description, ConsoleColor color)
            {
                _description = description;
                _color = color;
            }

            public string Description { get { return _description; } }
            public ConsoleColor Color { get { return _color; } }
        }

        readonly IDictionary<LogEventLevel, LevelFormat> _levels = new Dictionary<LogEventLevel, LevelFormat>
        {
            { LogEventLevel.Verbose, new LevelFormat("VRB", VerboseLevel) },
            { LogEventLevel.Debug, new LevelFormat("DBG", DebugLevel) },
            { LogEventLevel.Information, new LevelFormat("INF", InformationLevel) },
            { LogEventLevel.Warning, new LevelFormat("WRN", WarningLevel) },
            { LogEventLevel.Error, new LevelFormat("ERR", ErrorLevel) },
            { LogEventLevel.Fatal, new LevelFormat("FTL", FatalLevel) },
        };

        readonly IFormatProvider _formatProvider;
        readonly object _syncRoot = new object();
        readonly MessageTemplate _outputTemplate;

        public LiterateConsoleSink(string outputTemplate, IFormatProvider formatProvider)
        {
            if (outputTemplate == null) throw new ArgumentNullException("outputTemplate");
            _outputTemplate = new MessageTemplateParser().Parse(outputTemplate);
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            var outputProperties = OutputProperties.GetOutputProperties(logEvent);
            
            lock (_syncRoot)
            {
                try
                {
                    foreach (var outputToken in _outputTemplate.Tokens)
                    {
                        var propertyToken = outputToken as PropertyToken;
                        if (propertyToken == null)
                        {
                            RenderOutputTemplateTextToken(outputToken, outputProperties);
                        }
                        else switch (propertyToken.PropertyName)
                        {
                            case OutputProperties.LevelPropertyName:
                                RenderLevelToken(logEvent.Level);
                                break;
                            case OutputProperties.MessagePropertyName:
                                RenderMessageToken(logEvent);
                                break;
                            case OutputProperties.ExceptionPropertyName:
                                RenderExceptionToken(propertyToken, outputProperties);
                                break;
                            default:
                                RenderOutputTemplatePropertyToken(outputToken, outputProperties);
                                break;
                        }
                    }
                }
                finally { Console.ResetColor(); }
            }
        }

        void RenderExceptionToken(
            MessageTemplateToken outputToken,
#if NET40
            IDictionary<string, LogEventPropertyValue> outputProperties)
#else
            IReadOnlyDictionary<string, LogEventPropertyValue> outputProperties)
#endif
        {
            var sw = new StringWriter();
            outputToken.Render(outputProperties, sw, _formatProvider);
            var lines = new StringReader(sw.ToString());
            string nextLine;
            while ((nextLine = lines.ReadLine()) != null)
            {
                Console.ForegroundColor = nextLine.StartsWith(StackFrameLinePrefix) ? Subtext : Text;
                Console.WriteLine(nextLine);
            }
        }

        void RenderOutputTemplatePropertyToken(
            MessageTemplateToken outputToken,
#if NET40
            IDictionary<string, LogEventPropertyValue> outputProperties)
#else
            IReadOnlyDictionary<string, LogEventPropertyValue> outputProperties)
#endif
        {
            Console.ForegroundColor = Subtext;
            outputToken.Render(outputProperties, Console.Out, _formatProvider);
        }

        void RenderLevelToken(LogEventLevel level)
        {
            LevelFormat format;
            if (!_levels.TryGetValue(level, out format))
                format = _levels[LogEventLevel.Warning];

            Console.ForegroundColor = format.Color;

            if (level == LogEventLevel.Error || level == LogEventLevel.Fatal)
            {
                Console.BackgroundColor = format.Color;
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.Write(format.Description);
            Console.ResetColor();
        }

        void RenderOutputTemplateTextToken(
            MessageTemplateToken outputToken,
#if NET40
            IDictionary<string, LogEventPropertyValue> outputProperties)
#else
            IReadOnlyDictionary<string, LogEventPropertyValue> outputProperties)
#endif
        {
            Console.ForegroundColor = Punctuation;
            outputToken.Render(outputProperties, Console.Out, _formatProvider);
        }

        void RenderMessageToken(LogEvent logEvent)
        {
            foreach (var messageToken in logEvent.MessageTemplate.Tokens)
            {
                var messagePropertyToken = messageToken as PropertyToken;
                if (messagePropertyToken != null)
                {
                    LogEventPropertyValue value;
                    if (!logEvent.Properties.TryGetValue(messagePropertyToken.PropertyName, out value))
                    {
                        Console.ForegroundColor = RawText;
                        Console.Write(messagePropertyToken);
                    }
                    else
                    {
                        var scalar = value as ScalarValue;
                        if (scalar != null)
                        {
                            Console.ForegroundColor = GetScalarColor(scalar);

                            if (scalar.Value is string && messagePropertyToken.Format == null && messagePropertyToken.Alignment == null)
                                Console.Write(scalar.Value);
                            else if (scalar.Value is bool && messagePropertyToken.Format == null && messagePropertyToken.Alignment == null)
                                Console.Write(scalar.Value.ToString().ToLowerInvariant());
                            else
                                messagePropertyToken.Render(logEvent.Properties, Console.Out, _formatProvider);
                        }
                        else
                        {
                            PrettyPrint(value, messagePropertyToken.Format, _formatProvider);
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = Text;
                    messageToken.Render(logEvent.Properties, Console.Out, _formatProvider);
                }
            }
        }

        void PrettyPrint(LogEventPropertyValue value, string format, IFormatProvider formatProvider)
        {
            var scalar = value as ScalarValue;
            if (scalar != null)
            {
                Console.ForegroundColor = GetScalarColor(scalar);
                value.Render(Console.Out, format, formatProvider);
                return;
            }

            var seq = value as SequenceValue;
            if (seq != null)
            {
                Console.ForegroundColor = Punctuation;
                Console.Write("[");

                var sep = "";
                foreach (var element in seq.Elements)
                {
                    Console.ForegroundColor = Punctuation;
                    Console.Write(sep);
                    sep = ", ";

                    PrettyPrint(element, null, formatProvider);
                }

                Console.ForegroundColor = Punctuation;
                Console.Write("]");
                return;
            }

            var str = value as StructureValue;
            if (str != null)
            {
                if (str.TypeTag != null)
                {
                    Console.ForegroundColor = Subtext;
                    Console.Write(str.TypeTag);
                    Console.Write(" ");
                }

                Console.ForegroundColor = Punctuation;
                Console.Write("{");

                var sep = "";
                foreach (var prop in str.Properties)
                {
                    Console.ForegroundColor = Punctuation;
                    Console.Write(sep);
                    sep = ", ";

                    Console.ForegroundColor = NameSymbol;
                    Console.Write(prop.Name);

                    Console.ForegroundColor = Punctuation;
                    Console.Write("=");

                    PrettyPrint(prop.Value, null, formatProvider);
                }

                Console.ForegroundColor = Punctuation;
                Console.Write("}");
                return;
            }

            var div = value as DictionaryValue;
            if (div != null)
            {
                Console.ForegroundColor = Punctuation;
                Console.Write("{");

                var sep = "";
                foreach (var element in div.Elements)
                {
                    Console.ForegroundColor = Punctuation;
                    Console.Write(sep);
                    sep = ", ";
                    Console.Write("[");
                    PrettyPrint(element.Key, null, formatProvider);

                    Console.ForegroundColor = Punctuation;
                    Console.Write("]=");

                    PrettyPrint(element.Value, null, formatProvider);
                }

                Console.ForegroundColor = Punctuation;
                Console.Write("}");
                return;
            }

            value.Render(Console.Out, format, formatProvider);
        }

        ConsoleColor GetScalarColor(ScalarValue scalar)
        {
            if (scalar.Value == null || scalar.Value is bool)
                return KeywordSymbol;
            
            if (scalar.Value is string)
                return StringSymbol;
            
            if (scalar.Value.GetType().IsPrimitive || scalar.Value is decimal)
                return NumericSymbol;
            
            return OtherSymbol;
        }
    }
}
