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
    namespace ConfigurationManagement
    {
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

                this.index = index;
                outputReportingProfileIndexToOutputReportingProfile[index] = this;

                this.title = title;
                this.description = description;
            }

            public static OutputReportingProfile DebugOutputReportingProfile
            {
                get { return debugOutputReportingProfile; }
            }

            public static OutputReportingProfile UserExtendedOutputReportingProfile
            {
                get { return userExtendedOutputReportingProfile; }
            }

            public static OutputReportingProfile UserOutputReportingProfile
            {
                get { return userOutputReportingProfile; }
            }

            [JsonIgnore]
            public eOutputReportType[] OutputReportTypes
            {
                get { return outputReportTypes; }
            }

            [JsonProperty]
            public int Index
            {
                get { return index; }
            }

            [JsonIgnore]
            public string Title
            {
                get { return title; }
            }

            [JsonIgnore]
            public string Description
            {
                get { return description; }
            }

            [JsonIgnore]
            public string MenuOptionLine
            {
                get { return Title + " - " + Description; }
            }

            public static OutputReportingProfile GetOutputReportingProfile(
                int outputReportingProfileIndex)
            {
                return outputReportingProfileIndexToOutputReportingProfile[outputReportingProfileIndex];
            }
        }
    }
}