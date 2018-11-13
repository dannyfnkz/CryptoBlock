using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Strings
    {
        public static class StringBuilderExtensionMethods
        {
            public static void AppendFormatLine(
                this StringBuilder stringBuilder,
                String format,
                params object[] args)
            {
                stringBuilder.AppendFormat(format, args);
                stringBuilder.Append(Environment.NewLine);
            }

            public static void AppendFormatTabbedLine(
                this StringBuilder stringBuilder,
                String format,
                params object[] args)
            {
                stringBuilder.Append(StringUtils.TabChar);
                stringBuilder.AppendFormatLine(format, args);
            }
        }
    }
}