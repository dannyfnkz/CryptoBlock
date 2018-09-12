using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace IOManagement
    {
        public class MenuDialog
        {
            public class InvalidOptionIndexException : Exception
            {
                private readonly int optionIndex;

                public InvalidOptionIndexException(int optionIndex)
                    : base(formatExceptionMessage(optionIndex))
                {

                }

                public int OptionIndex
                {
                    get { return optionIndex; }
                }

                private static string formatExceptionMessage(int optionIndex)
                {
                    return string.Format(
                        "Specified option index '{0}' was invalid: must be a non-negative integer "
                        + "smaller than option count.",
                        optionIndex);
                }
            }

            private readonly string promptMessage;
            private readonly string[] options;
            private readonly string displayString;

            public MenuDialog(string promptMessage, string[] options)
            {
                this.promptMessage = promptMessage;
                this.options = options;

                this.displayString = constructDisplayString();
            }

            public string PromptMessage
            {
                get { return promptMessage; }
            }

            public string[] Options
            {
                get { return options; }
            }

            public string DisplayString
            {
                get { return displayString; }
            }

            public int MinValidOptionIndex
            {
                get { return 1; }
            }

            public int MaxValidOptionIndex
            {
                get { return Options.Length; }
            }

            public bool IsValidOptionIndex(int optionIndex)
            {
                return optionIndex >= MinValidOptionIndex && optionIndex <= MaxValidOptionIndex;
            }

            public string GetOption(int optionIndex)
            {
                assertValidOptionIndex(optionIndex);

                int optionArrayIndex = optionIndex - 1;
                return Options[optionArrayIndex];
            }

            private string constructDisplayString()
            {
                StringBuilder displayStringBuilder = new StringBuilder();

                // append header
                displayStringBuilder.Append(promptMessage);
                displayStringBuilder.AppendFormat(
                    " ({0} - {1}): ",
                    MinValidOptionIndex
                    , MaxValidOptionIndex);
                displayStringBuilder.Append(Environment.NewLine);

                // append option menu
                for(int i = 0; i < this.Options.Length; i++)
                {
                    int optionNumber = i + 1;
                    string optionString = this.Options[i];
                    displayStringBuilder.AppendFormat("{0}. {1}", optionNumber, optionString);

                    if(i < this.Options.Length - 1)
                    {
                        displayStringBuilder.Append(Environment.NewLine);
                    }
                }

                return displayStringBuilder.ToString();
            }

            private void assertValidOptionIndex(int optionIndex)
            {
                if(!IsValidOptionIndex(optionIndex))
                {
                    throw new InvalidOptionIndexException(optionIndex);
                }
            }
        }
    }
}

