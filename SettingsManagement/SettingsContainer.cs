using CryptoBlock.IOManagement;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace SettingsManagement
    {
        internal class SettingsContainer
        {
            internal class SettingsContainerObjectParseException : Exception
            {
                private readonly object obj;

                internal SettingsContainerObjectParseException(object obj, Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {
                    this.obj = obj;
                }

                internal object Object
                {
                    get { return obj; }
                }
                private static string formatExceptionMessage()
                {
                    return "Could not parse SettingsContainer from specified object.";
                }
            }

            private static readonly OutputReportingProfile DEFAULT_OUTPUT_REPORTING_PROFILE
                = OutputReportingProfile.UserOutputReportingProfile;

            private OutputReportingProfile outputReportingProfile = DEFAULT_OUTPUT_REPORTING_PROFILE;

            internal SettingsContainer(OutputReportingProfile outputReportingProfile = null)
            {
                if (outputReportingProfile != null)
                {
                    this.outputReportingProfile = outputReportingProfile;
                }
            }

            internal static SettingsContainer Parse(dynamic settingsContainerObject)
            {
                try
                {
                    // get index of OutputReportingProfile
                    int outputReportingProfileIndex =
                        settingsContainerObject.OutputReportingProfile.Index;

                    // get OutputReportingProfile corresponding to index
                    OutputReportingProfile outputReportingProfile =
                        OutputReportingProfile.GetOutputReportingProfile(outputReportingProfileIndex);

                    // init a new SettingsContainer
                    return new SettingsContainer(outputReportingProfile);
                }
                catch (RuntimeBinderException runtimeBinderException)
                {
                    throw new SettingsContainerObjectParseException(
                        settingsContainerObject,
                        runtimeBinderException);
                }
            }

            [JsonProperty]
            internal OutputReportingProfile OutputReportingProfile
            {
                get { return outputReportingProfile; }
                set
                {
                    outputReportingProfile = value;
                    ConsoleIOManager.Instance.OutputReportTypes = outputReportingProfile.OutputReportTypes;
                }
            }
        }
    }
}