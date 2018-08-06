﻿using CryptoBlock.Utils.IO.SQLite.Queries.Columns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.IO.SQLite.Queries.Conditions
    {
        public class BasicCondition : Condition
        {
            public enum eComparisonType
            {
                Equal, NotEqual, LargerEqual, SmallerEqual, Larger, Smaller
            }

            private static readonly Dictionary<eComparisonType, string> eComparisonTypeToString =
                new Dictionary<eComparisonType, string>()
            {
                    { eComparisonType.Equal, "=" },
                    { eComparisonType.NotEqual, "<>" },
                    { eComparisonType.LargerEqual, ">=" },
                    { eComparisonType.SmallerEqual, "<=" },
                    { eComparisonType.Larger, ">" },
                    { eComparisonType.Smaller, "<" },
            };

            private readonly ValuedTableColumn valuedTableColumn;
            private readonly eComparisonType comparisonType;
            private readonly string queryString;

            public BasicCondition(ValuedTableColumn valuedTableColumn, eComparisonType comparisonType)
            {
                this.valuedTableColumn = valuedTableColumn;
                this.comparisonType = comparisonType;
                this.queryString = buildQueryString();
            }

            public ValuedTableColumn ValuedTableColumn
            {
                get { return valuedTableColumn; }
            }

            public eComparisonType ComparisonType
            {
                get { return comparisonType; }
            }

            public static string ComparisonTypeToString(eComparisonType comparisonType)
            {
                return eComparisonTypeToString[comparisonType];
            }

            string Condition.QueryString
            {
                get { return queryString; }
            }

            private string buildQueryString()
            {
                string fullyQualifiedColumnName = this.valuedTableColumn.FullyQualifiedName;
                string comparisonTypeString = ComparisonTypeToString(this.comparisonType);
                string columnValue = this.valuedTableColumn.Value.ToString();

                return string.Format(
                    "{0} {1} '{2}'",
                    fullyQualifiedColumnName,
                    comparisonTypeString,
                    columnValue);
            }
        }
    }
}