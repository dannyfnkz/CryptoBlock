using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace IOManagement
    {
        /// <summary>
        /// represents a menu dialog containing a prompt message and a list of options,
        /// selectable by index.
        /// </summary>
        public class MenuDialog
        {
            /// <summary>
            /// thrown if specified <see cref="MenuDialog"/> option index is invalid.
            /// </summary>
            public class InvalidOptionIndexException : Exception
            {
                private readonly int optionIndex;

                public InvalidOptionIndexException(int optionIndex)
                    : base(formatExceptionMessage(optionIndex))
                {
                    this.optionIndex = optionIndex;
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

            /// <summary>
            /// menu dialog prompt message.
            /// </summary>
            public string PromptMessage
            {
                get { return promptMessage; }
            }

            /// <summary>
            /// array of menu dialog options.
            /// </summary>
            public string[] Options
            {
                get { return options; }
            }

            /// <summary>
            /// string representation of the menu dialog.
            /// </summary>
            public string DisplayString
            {
                get { return displayString; }
            }

            /// <summary>
            /// value of the minimum menu option index. Note: indexing starts from 1.
            /// </summary>
            public int MinValidOptionIndex
            {
                get { return 1; }
            }

            /// <summary>
            /// value of the maximum menu option index.
            /// </summary>
            public int MaxValidOptionIndex
            {
                get { return Options.Length; }
            }

            /// <summary>
            /// returns whether specified <paramref name="optionIndex"/> is a valid menu option index.
            /// </summary>
            /// <param name="optionIndex"></param>
            /// <seealso cref="MinValidOptionIndex"/>
            /// <seealso cref="MaxValidOptionIndex"/>
            /// <returns>
            /// true if specified <paramref name="optionIndex"/> is a valid menu option index.
            /// else false
            /// </returns>
            public bool IsValidOptionIndex(int optionIndex)
            {
                return optionIndex >= MinValidOptionIndex && optionIndex <= MaxValidOptionIndex;
            }

            /// <summary>
            /// returns menu option corresponding to specified <paramref name="optionIndex"/>.
            /// </summary>
            /// <param name="optionIndex"></param>
            /// <returns>
            /// menu option corresponding to specified <paramref name="optionIndex"/>
            /// </returns>
            /// <exception cref="InvalidOptionIndexException">
            /// <seealso cref="assertValidOptionIndex(int)"/>
            /// </exception>
            public string GetOption(int optionIndex)
            {
                assertValidOptionIndex(optionIndex);

                int optionArrayIndex = optionIndex - 1;
                return Options[optionArrayIndex];
            }

            /// <summary>
            /// returns the string representation of the menu dialog.
            /// </summary>
            /// <returns>
            /// string representation of the menu dialog
            /// </returns>
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

                // append menu options
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

            /// <summary>
            /// asserts that <paramref name="optionIndex"/> is valid.
            /// </summary>
            /// <seealso cref="IsValidOptionIndex(int)"/>
            /// <param name="optionIndex"></param>
            /// <exception cref="InvalidOptionIndexException">
            /// thrown if <paramref name="optionIndex"/> is invalid
            /// </exception>
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

