using System;
using System.Reflection;
using System.Text;
using System.Linq;
using static CryptoBlock.Utils.ExceptionUtils;
using System.Collections.Generic;

namespace CryptoBlock
{
    namespace Utils.Strings
    {
        /// <summary>
        /// contains <see cref="string"/> extension methods.
        /// </summary>
        public static class StringExtensionMethods
        {
            public static string[] SplitByBlocks(
                this string str,
                string blockDelimiter,
                string blockStartMarker,
                string blockEndMarker)
            {
                List<string> splitParts = new List<string>();

                bool blockStarted = false;
                int remainingSubstringStartIndex = 0;
                int curStringIndex = 0;

                while(curStringIndex < str.Length)
                {
                    if(blockStarted) // in the middle of a block
                    {
                        if(str.StartsWith(curStringIndex, blockEndMarker)) // blockEndMarker detected
                        {
                            blockStarted = false;

                            // skip to last character of blockEndMarker
                            curStringIndex += blockEndMarker.Length - 1; 
                        }
                    }
                    else // not in the middle of a block
                    {
                        if(str.StartsWith(curStringIndex, blockStartMarker)) // blockStartMarker detected
                        {
                            blockStarted = true;

                            // skip to last character of blockStartMarker
                            curStringIndex += blockStartMarker.Length - 1; 
                        }  
                        else if (str.StartsWith(curStringIndex, blockDelimiter)) // blockSeparator detected
                        {
                            // get a substring of str, starting at character immediately after
                            // last split, and ending at character immediately before
                            // blockSeparator, omitting blockStartMarker and blockEndMarker
                            string splitPart = str.SubstringByIndexWithout(
                                remainingSubstringStartIndex, 
                                curStringIndex,
                                blockStartMarker,
                                blockEndMarker);
                            splitParts.Add(splitPart);

                            // skip to last character of blockStartMarker
                            curStringIndex += blockDelimiter.Length - 1; 

                            // update index of character immediately after split
                            remainingSubstringStartIndex = curStringIndex + 1; 
                        }
                    }

                    // go to next character in str
                    curStringIndex++;
                }

                // some characters at end of str were not added as a split part
                if (remainingSubstringStartIndex < str.Length) 
                {
                    // get a substring of str, starting at character immediately after
                    // last split, and ending at last character of str,
                    // omitting blockStartMarker and blockEndMarker
                    string splitPart = str.SubstringByIndexWithout(
                        remainingSubstringStartIndex, 
                        str.Length,
                        blockStartMarker,
                        blockEndMarker);
                    splitParts.Add(splitPart);
                }

                return splitParts.ToArray();
            }

            /// <summary>
            /// splits <paramref name="str"/> into multiple strings, according to parameter list 
            /// <paramref name="parameters"/>.
            /// </summary>
            /// <remarks>
            /// this method uses and extends<see cref="String.Split(string[], StringSplitOptions)"/> 
            /// to allow it to receive <c>params string[], StringSplitOptions</c> as arguments.
            /// </remarks>
            /// <param name="str"></param>
            /// <param name="splitOptions"></param>
            /// <param name="parameters">an array of strings used to split <paramref name="str"/>.</param>
            /// <returns>
            /// an array containing the strings obtained from splitting <paramref name="str"/>
            /// </returns>
            public static string[] Split(
                this string str,
                StringSplitOptions splitOptions,
                params string[] parameters)
            {
                return str.Split(parameters, splitOptions);
            }

            
            /// <summary>
            /// splits <paramref name="str"/> into multiple strings, according to parameter list 
            /// <paramref name="parameters"/>.
            /// </summary>
            /// <remarks>
            /// this method calls <see cref="Split(string, StringSplitOptions, string[])"/> with argument
            /// <c>StringSplitOptions.None</c>.
            /// </remarks>
            /// <param name="str"></param>
            /// <param name="parameters">an array of strings used to split <paramref name="str"/>.</param>
            /// <returns>
            /// an array containing the strings obtained from splitting <paramref name="str"/>
            /// </returns>
            public static string[] Split(this string str, params string[] parameters)
            {
                return Split(str, StringSplitOptions.None, parameters);
            }

            /// <summary>
            /// returns a substring of <paramref name="str"/> in range
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>),
            /// omitting all occurrences of any expression contained in specified
            /// <paramref name="expressionsToOmit"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <param name="expressionsToOmit"></param>
            /// <returns>
            /// substring of <paramref name="str"/> in range
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>),
            /// omitting all occurrences of any expression contained in specified
            /// <paramref name="expressionsToOmit"/>
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="StringUtils.AssertValidRangeIndicesInString(int, int, string)"/>
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="StringUtils.AssertValidStringLengths(int[])"/>
            /// </exception>
            public static string SubstringByIndexWithout(
                this string str,
                int startIndex, 
                int endIndex,
                params string[] expressionsToOmit)
            {
                StringUtils.AssertValidRangeIndicesInString(startIndex, endIndex, str);
                StringUtils.AssertValidStringLengths(endIndex - startIndex);

                StringBuilder substringBuilder = new StringBuilder();

                // current index in str, starting at startIndex
                int curStringIndex = startIndex;

                // index of the first occurrence of any of the specified expressionsToOmit,
                // starting at curStringIndex
                int indexOfFirstOccurrenceOfChosenExpressionToOmit;

                // expression whose occurrence is the first to appear, starting at curStringIndex
                string chosenExpressionToOmit;

                // get index of first occurrence of any of the specified expressionsToOmit,
                // starting at beginning of str
                indexOfFirstOccurrenceOfChosenExpressionToOmit = str.IndexOfAny(
                        curStringIndex,
                        expressionsToOmit,
                        out chosenExpressionToOmit);

                // occurrence of an expression to omit, starting at index in range 
                // [curStringIndex, endIndex), was found
                while (
                    indexOfFirstOccurrenceOfChosenExpressionToOmit != -1
                    && indexOfFirstOccurrenceOfChosenExpressionToOmit < endIndex)
                {
                    // append all characters from str, upto first occurrence of expressionToOmit,
                    // to substringBuilder
                    while (curStringIndex < indexOfFirstOccurrenceOfChosenExpressionToOmit)
                    {
                        substringBuilder.Append(str[curStringIndex]);
                        curStringIndex++;
                    }

                    // skip over expressionToOmit
                    curStringIndex += chosenExpressionToOmit.Length;

                    if(curStringIndex < endIndex) // endIndex not yet reached
                    {
                        // get index of first occurrence of any of the specified expressionsToOmit,
                        // starting at curStringIndex
                        indexOfFirstOccurrenceOfChosenExpressionToOmit = str.IndexOfAny(
                                curStringIndex,
                                expressionsToOmit,
                                out chosenExpressionToOmit);
                    }
                    else // endIndex reached - no next occurrence available
                    {
                        indexOfFirstOccurrenceOfChosenExpressionToOmit = -1;
                    }
                }

                // append the remainder of str (after last occurrence of an expression to omit)
                // to substringBuilder
                while (curStringIndex < endIndex)
                {
                    substringBuilder.Append(str[curStringIndex]);
                    curStringIndex++;
                }

                return substringBuilder.ToString();
            }

            /// <summary>
            /// returns the index of the first occurrence of the first expression in
            /// specified <paramref name="expressions"/> array which appears in
            /// in <paramref name="str"/>. if no expression appears in <paramref name="str"/> returns -1.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="startIndex"></param>
            /// <param name="expressions"></param>
            /// <param name="chosenExpression">
            /// the expression which appears first in specified <paramref name="str"/>
            /// </param>
            /// <returns>
            /// index of the first occurrence of the first expression in
            /// specified <paramref name="expressions"/> array which appears in
            /// in <paramref name="str"/>,
            /// or -1 if no expression appears in <paramref name="str"/>
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="StringUtils.AssertValidIndexInString(int, string, string)"/>
            /// </exception>
            public static int IndexOfAny(
                this string str,
                int startIndex,
                string[] expressions,
                out string chosenExpression)
            {
                StringUtils.AssertValidIndexInString(startIndex, str, "startIndex");

                int indexOfFirstOccurrenceOfChosenExpression = int.MaxValue;
                chosenExpression = null;

                // get the index of the first occurrence of any of the specified expressions
                foreach (string expression in expressions)
                {
                    // get index of first occurrence of expression, starting at startIndex
                    int indexOfNextOccurrenceOfExpressionToOmit = str.IndexOf(
                        expression,
                        startIndex);

                    // first occurrence of expression, starting at startIndex, found in str
                    if (indexOfNextOccurrenceOfExpressionToOmit != -1)
                    {
                        // get the min index of the first expression occurrence between
                        // all specified expressions
                        indexOfFirstOccurrenceOfChosenExpression = Math.Min(
                            indexOfFirstOccurrenceOfChosenExpression,
                            indexOfNextOccurrenceOfExpressionToOmit);
                        chosenExpression = expression;
                    }
                }

                // no occurrence of any of the specified expressions was found in str
                if (indexOfFirstOccurrenceOfChosenExpression == int.MaxValue)
                {
                    indexOfFirstOccurrenceOfChosenExpression = -1;
                }

                return indexOfFirstOccurrenceOfChosenExpression;
            }

            /// <summary>
            /// returns a substring of specified <paramref name="str"/> in the range
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>).
            /// </summary>
            /// <param name="str"></param>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <returns>
            /// substring of specified <paramref name="str"/> in the range
            /// [<paramref name="startIndex"/>, <paramref name="endIndex"/>)
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="StringUtils.AssertValidRangeIndicesInString(int, int, string)"/>
            /// </exception>
            public static string SubstringByIndex(this string str, int startIndex, int endIndex)
            {
                StringUtils.AssertValidRangeIndicesInString(startIndex, endIndex, str);

                int substringLength = endIndex - startIndex;
                StringUtils.AssertValidStringLengths(substringLength);

                return str.Substring(startIndex, substringLength);
            }

            /// <summary>
            /// returns a substring of <paramref name="str"/>,
            /// starting immediately after <paramref name="prefix"/>.
            /// if <paramref name="str"/> does not start with <paramref name="prefix"/>,
            /// returns <paramref name="str"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefix"></param>
            /// <returns>
            /// substring of <paramref name="str"/>, starting immediately after <paramref name="prefix"/>, 
            /// if <paramref name="str"/> starts with <paramref name="prefix"/>,
            /// else <paramref name="str"/>
            /// </returns>
            public static string GetSubstringAfterPrefix(this string str, string prefix)
            {
                string substringAfterPrefix = str.StartsWith(prefix)
                    ? str.Substring(prefix.Length)
                    : str;

                return substringAfterPrefix;
            }

            /// <summary>
            /// returns whether <paramref name="str"/> starts with one of the
            /// specified <paramref name="prefixes"/>,
            /// beginning at specified <paramref name="startIndex"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="startIndex"></param>
            /// <param name="prefixes"></param>
            /// <returns>
            /// true if <paramref name="str"/> starts with one of the
            /// specified <paramref name="prefixes"/>,
            /// beginning at specified <paramref name="startIndex"/>,
            /// else false
            /// </returns>
            public static bool StartsWith(this string str, int startIndex, params string[] prefixes)
            {
                bool startsWithOneOfPrefixes = false;

                foreach(string prefix in prefixes)
                {
                    startsWithOneOfPrefixes = str.StartsWith(startIndex, prefix);

                    if(startsWithOneOfPrefixes)
                    {
                        break;
                    }
                }

                return startsWithOneOfPrefixes;
            }

            /// <summary>
            /// returns whether <paramref name="str"/> starts with specified <paramref name="prefix"/>,
            /// beginning at specified <paramref name="startIndex"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="startIndex"></param>
            /// <param name="prefix"></param>
            /// <returns>
            /// true if <paramref name="str"/> starts with specified <paramref name="prefix"/>,
            /// beginning at specified <paramref name="startIndex"/>,
            /// else false
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="StringUtils.AssertValidIndexInString(int, string, string)"/>
            /// </exception>
            public static bool StartsWith(this string str, int startIndex, string prefix)
            {
                StringUtils.AssertValidIndexInString(startIndex, str, "startIndex");

                bool startsWithPrefix;

                int curStringIndex;
                int curPrefixIndex;

                // run through str, starting at startIndex, and compare each of its characters
                // with the corresponding character in prefix
                for (curStringIndex = startIndex, curPrefixIndex = 0;
                    curPrefixIndex < prefix.Length && curStringIndex < str.Length;
                    curStringIndex++, curPrefixIndex++)
                {
                    // mismatch between character in str and its corresponding character in prefix
                    if (str[curStringIndex] != prefix[curPrefixIndex])
                    {
                        break;
                    }
                }

                // string starts with prefix (at startIndex)
                // iff end of prefix was reached, i.e all characters of prefix and str match
                startsWithPrefix = curPrefixIndex == prefix.Length;

                return startsWithPrefix;
            }

            /// <summary>
            /// returns whether <paramref name="str"/> starts with one of the 
            /// specified <paramref name="prefixes"/>.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefixes"></param>
            /// <returns>
            /// true if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// else false
            /// </returns>
            public static bool StartsWith(this string str, params string[] prefixes)
            {
                foreach (string prefix in prefixes)
                {
                    if (str.StartsWith(prefix))
                    {
                        return true;
                    }
                }

                return false;
            }

            // if str starts with one of the prefixes in in params array, returns that prefix
            // else, returns null
            /// <summary>
            /// if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// returns that prefix; else returns null.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="prefixes"></param>
            /// <returns>
            /// if <paramref name="str"/> starts with one of the prefixes in <paramref name="prefixes"/>,
            /// returns that prefix
            /// else, returns null. 
            /// </returns>
            public static string GetPrefixIfStartsWith(this string str, params string[] prefixes)
            {
                foreach (string prefix in prefixes)
                {
                    if (str.StartsWith(prefix))
                    {
                        return prefix;
                    }
                }

                return null;
            }

            /// <summary>
            /// if <paramref name="inputString"/> is longer than <paramref name="maxStringLength"/>,
            /// shorten string to its first <paramref name="maxStringLength"/> characters,
            /// where the last <paramref name="cutSuffix"/>.Length characters are replaced by 
            /// <paramref name="cutSuffix"/>.
            /// </summary>
            /// <param name="inputString"></param>
            /// <param name="maxStringLength">maximum allowed string length</param>
            /// <param name="cutSuffix">appended to end of <paramref name="inputString"/> if it is shortened</param>
            /// <returns>
            /// inputString shortened to its first <paramref name="maxStringLength"/> -
            /// <paramref name="cutSuffix"/>.Length characters, with <paramref name="cutSuffix"/>
            /// appended to ending
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// <seealso cref="ExceptionUtils.AssertMethodParametersNotNull(MethodParameter[])"/>
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <seealso cref="NumberUtils.AssertAtLeast(int, int)"/>
            /// </exception>
            public static string ShortenIfLongerThan(
                this string inputString,
                int maxStringLength,
                string cutSuffix = "..")
            {
                ExceptionUtils.AssertMethodParametersNotNull(
                    new MethodParameter(inputString, "inputString"));

                // maxStringLength must be at least cutSuffix.Length
                NumberUtils.AssertAtLeast(maxStringLength, cutSuffix.Length);

                string resultString;

                if(inputString.Length > maxStringLength) // inputString longer than maxStringLength
                {
                    // cut inputString to maxStringLength, replacing its ending with cutSuffix
                    int resultStringLength = maxStringLength - cutSuffix.Length;
                    resultString = inputString.Substring(0, resultStringLength) + cutSuffix;
                }
                else // inputString not longer than maxStringLength - inputString remains unchanged
                {
                    resultString = inputString;
                }

                return resultString;
            }

            /// <summary>
            /// returns a copy of <paramref name="str"/> where the characters
            /// at the specified <paramref name="indices"/> are set to uppercase.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="indices"></param>
            /// <returns>
            /// a copy of <paramref name="str"/> where the characters
            /// at the specified <paramref name="indices"/> are set to uppercase.
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="StringUtils.AssertValidIndexInString(int, string, string)"/>
            /// </exception>
            public static string CharactersAtIndicesToUpper(this string str, params int[] indices)
            {
                // assert that all indices are valid
                for (int i = 0; i < indices.Length; i++)
                {
                    StringUtils.AssertValidIndexInString(
                        indices[i],
                        str,
                        string.Format("indices[{0}]", i)
                    );
                }

                char[] stringCharArray = str.ToCharArray();

                foreach (int index in indices)
                {
                    stringCharArray[index] = char.ToUpper(stringCharArray[index]);
                }

                return new string(stringCharArray);
            }

            /// <summary>
            /// returns a copy of <paramref name="str"/> where the characters
            /// at the specified <paramref name="indices"/> are set to uppercase,
            /// and the rest are set to lowercase.
            /// </summary>
            /// <param name="str"></param>
            /// <param name="indices"></param>
            /// <returns>
            /// a copy of <paramref name="str"/> where the characters
            /// at the specified <paramref name="indices"/> are set to uppercase,
            /// and the rest are set to lowercase
            /// </returns>
            public static string MakeOnlyCharactersAtIndicesUpper(
                this string str, 
                params int[] indices)
            {               
                // assert that all indices are valid
                for(int i = 0; i < indices.Length; i++)
                {
                    StringUtils.AssertValidIndexInString(
                        indices[i],
                        str,
                        string.Format("indices[{0}]", i)
                    );
                }

                string upperCaseString;

                char[] stringCharArray = str.ToCharArray();
                
                // turn all characters to lowercase
                for(int i = 0; i < stringCharArray.Length; i++)
                {
                    stringCharArray[i] = char.ToLower(stringCharArray[i]);
                }

                // make characters at specified indices uppercase
                foreach(int index in indices)
                {
                    stringCharArray[index] = char.ToUpper(stringCharArray[index]);
                }

                upperCaseString = new string(stringCharArray);

                return upperCaseString;
            }
        }
    }
}

