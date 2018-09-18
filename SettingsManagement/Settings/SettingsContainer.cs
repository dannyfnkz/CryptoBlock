using CryptoBlock.IOManagement;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace ConfigurationManagement.Settings
    {
        /// <summary>
        /// contains the configurable settings of the program.
        /// </summary>
        internal class SettingsContainer
        {
            /// <summary>
            /// thrown if <see cref="SettingsContainer"/> parse from a <see cref="JObject"/>
            /// failed.
            /// </summary>
            internal class SettingsContainerJsonObjectParseException : Exception
            {
                private readonly JObject jsonObject;

                internal SettingsContainerJsonObjectParseException(
                    JObject jsonObject, 
                    Exception innerException)
                    : base(formatExceptionMessage(), innerException)
                {
                    this.jsonObject = jsonObject;
                }

                internal JObject JsonObject
                {
                    get { return jsonObject; }
                }
                private static string formatExceptionMessage()
                {
                    return "Could not parse SettingsContainer from specified JObject.";
                }
            }

            private static readonly OutputReportingProfile DEFAULT_OUTPUT_REPORTING_PROFILE
                = OutputReportingProfile.UserOutputReportingProfile;

            private OutputReportingProfile outputReportingProfile = DEFAULT_OUTPUT_REPORTING_PROFILE;

            /// <summary>
            /// initialize setting container with specified <paramref name="outputReportingProfile"/>.
            /// if <paramref name="outputReportingProfile"/> is null, a default 
            /// <see cref="OutputReportingProfile"/> is used.
            /// </summary>
            /// <param name="outputReportingProfile"></param>
            internal SettingsContainer(OutputReportingProfile outputReportingProfile = null)
            {
                if (outputReportingProfile != null)
                {
                    this.outputReportingProfile = outputReportingProfile;
                }
            }

            /// <summary>
            /// parses <see cref="SettingsContainer"/> from <paramref name="settingsContainerJsonObject"/>,
            /// which is expected to be a <see cref="JObject"/>.
            /// </summary>
            /// <param name="settingsContainerJsonObject"></param>
            /// <returns>
            /// <see cref="SettingsContainer"/> parsed from <paramref name="settingsContainerJsonObject"/>
            /// </returns>
            /// <exception cref="SettingsContainerJsonObjectParseException">
            /// thrown if <see cref="SettingsContainer"/> parse failed
            /// </exception>
            internal static SettingsContainer Parse(dynamic settingsContainerJsonObject)
            {
                try
                {
                    // get index of OutputReportingProfile
                    int outputReportingProfileIndex =
                        settingsContainerJsonObject.OutputReportingProfile.Index;

                    // get OutputReportingProfile corresponding to index
                    OutputReportingProfile outputReportingProfile =
                        OutputReportingProfile.GetOutputReportingProfile(outputReportingProfileIndex);

                    // init a new SettingsContainer
                    return new SettingsContainer(outputReportingProfile);
                }
                catch (RuntimeBinderException runtimeBinderException)
                {
                    throw new SettingsContainerJsonObjectParseException(
                        settingsContainerJsonObject,
                        runtimeBinderException);
                }
            }

            /// <summary>
            /// current setting for the program <see cref="Settings.OutputReportingProfile"/>.
            /// </summary>
            [JsonProperty]
            internal OutputReportingProfile OutputReportingProfile
            {
                get { return outputReportingProfile; }
                set
                {
                    outputReportingProfile = value;

                    // set the OutputReportTypes associated with the selected OutputReportingProfile
                    // in ConsoleIOManager
                    ConsoleIOManager.Instance.OutputReportTypes = outputReportingProfile.OutputReportTypes;
                }
            }
        }
    }
}