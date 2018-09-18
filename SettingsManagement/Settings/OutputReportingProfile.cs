using CryptoBlock.IOManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoBlock.IOManagement.ConsoleIOManager;

namespace CryptoBlock
{
    namespace ConfigurationManagement.Settings
    {
        /// <summary>
        /// thrown if a <see cref="OutputReportingProfile"/> with specified index
        /// does not exist.
        /// </summary>
        public class OutputReportingProfileNotFoundException : Exception
        {
            private int outputReportingProfileIndex;

            public OutputReportingProfileNotFoundException(int outputReportingProfileIndex)
                : base(formatExceptionMessage(outputReportingProfileIndex))
            {
                this.outputReportingProfileIndex = outputReportingProfileIndex;
            }

            public int OutputReportingProfileIndex
            {
                get { return outputReportingProfileIndex; }
            }

            private static string formatExceptionMessage(int outputReportingProfileIndex)
            {
                return string.Format(
                    "OutputReportingProfile associated with specified index '{0}' not fond.",
                    outputReportingProfileIndex);
            }
        }

        /// <summary>
        /// reresents a profile determining what kind of notices should be displayed
        /// to console.
        /// </summary>
        /// <remarks>
        /// no new instances of this type may be created. use pre-defined profiles, supplied as 
        /// static properties.
        /// </remarks>
        public class OutputReportingProfile
        {
            private static readonly Dictionary<int, OutputReportingProfile> 
                outputReportingProfileIndexToOutputReportingProfile =
                    new Dictionary<int, OutputReportingProfile>();

            private static readonly OutputReportingProfile debugOutputReportingProfile =
                new OutputReportingProfile(
                    new eOutputReportType[]
                    {
                            eOutputReportType.ExceptionLog,
                            eOutputReportType.SystemCritical,
                            eOutputReportType.System,
                            eOutputReportType.CommandExecution
                    },
                    0,
                    "Debugging",
                    "report all types of notices & display error logs"
                    );

            private static readonly OutputReportingProfile userExtendedOutputReportingProfile =
                new OutputReportingProfile(
                    new eOutputReportType[]
                    {
                                eOutputReportType.SystemCritical,
                                eOutputReportType.System,
                                eOutputReportType.CommandExecution
                    },
                    1,
                    "User-extended",
                    "report all types of notices");

            private static readonly OutputReportingProfile userOutputReportingProfile =
                new OutputReportingProfile(
                    new eOutputReportType[]
                    {
                                eOutputReportType.SystemCritical,
                                eOutputReportType.CommandExecution
                    },
                    2,
                    "User",
                    "report only critical system notices");

            private readonly eOutputReportType[] outputReportTypes;
            private readonly int index;
            private readonly string title;
            private readonly string description;

            private OutputReportingProfile(
                eOutputReportType[] outputReportTypes,
                int index,
                string title,
                string description)
            {
                this.outputReportTypes = outputReportTypes;

                // set index and bind index to this instance in the dictionary
                this.index = index;
                outputReportingProfileIndexToOutputReportingProfile[index] = this;

                this.title = title;
                this.description = description;
            }

            /// <summary>
            /// output profile for debugging: all kinds of notices are displayed
            /// and exceptions are logged to console (in addition to being logged to error file).
            /// </summary>
            public static OutputReportingProfile DebugOutputReportingProfile
            {
                get { return debugOutputReportingProfile; }
            }

            /// <summary>
            /// output profile for advanced users: most kinds of notices are displayed.
            /// </summary>
            public static OutputReportingProfile UserExtendedOutputReportingProfile
            {
                get { return userExtendedOutputReportingProfile; }
            }

            /// <summary>
            /// output profile for regular users: only most essential kinds of notices are displayed.
            /// </summary>
            public static OutputReportingProfile UserOutputReportingProfile
            {
                get { return userOutputReportingProfile; }
            }

            /// <summary>
            /// array of <see cref="eOutputReportType"/>s associated with this profile.
            /// </summary>
            [JsonIgnore]
            public eOutputReportType[] OutputReportTypes
            {
                get { return outputReportTypes; }
            }

            /// <summary>
            /// index of this profile.
            /// </summary>
            [JsonProperty]
            public int Index
            {
                get { return index; }
            }

            /// <summary>
            /// title of this profile.
            /// </summary>
            [JsonIgnore]
            public string Title
            {
                get { return title; }
            }

            /// <summary>
            /// description of this profile.
            /// </summary>
            [JsonIgnore]
            public string Description
            {
                get { return description; }
            }

            /// <summary>
            /// a string containing this profile title and description. used in a menu context.
            /// </summary>
            [JsonIgnore]
            public string MenuOptionLine
            {
                get { return Title + " - " + Description; }
            }

            /// <summary>
            /// returns the <see cref="OutputReportingProfile"/> associated with
            /// <paramref name="outputReportingProfileIndex"/>.
            /// </summary>
            /// <param name="outputReportingProfileIndex"></param>
            /// <returns>
            /// <see cref="OutputReportingProfile"/> associated with
            /// <paramref name="outputReportingProfileIndex"/>
            /// </returns>
            /// <exception cref="OutputReportingProfileNotFoundException">
            /// <seealso cref="assertOutputReportingProfileExists(int)"/>
            /// </exception>
            public static OutputReportingProfile GetOutputReportingProfile(
                int outputReportingProfileIndex)
            {
                assertOutputReportingProfileExists(outputReportingProfileIndex);
                return outputReportingProfileIndexToOutputReportingProfile[outputReportingProfileIndex];
            }

            /// <summary>
            /// asserts that an <see cref="OutputReportingProfile"/> with specified 
            /// <paramref name="outputReportingProfileIndex"/> exists.
            /// </summary>
            /// <param name="outputReportingProfileIndex"></param>
            /// <exception cref="OutputReportingProfileNotFoundException">
            /// thrown if an <see cref="OutputReportingProfile"/> with specified 
            /// <paramref name="outputReportingProfileIndex"/> does not exist
            /// </exception>
            private static void assertOutputReportingProfileExists(int outputReportingProfileIndex)
            {
                if(!outputReportingProfileIndexToOutputReportingProfile.ContainsKey(
                    outputReportingProfileIndex))
                {
                    throw new OutputReportingProfileNotFoundException(outputReportingProfileIndex);
                }
            }
        }
    }
}