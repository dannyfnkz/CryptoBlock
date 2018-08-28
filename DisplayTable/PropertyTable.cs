using CryptoBlock.Utils;
using CryptoBlock.Utils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static CryptoBlock.TableDisplay.Table;

namespace CryptoBlock
{
    namespace TableDisplay
    {
        /// <summary>
        /// represents a table where each column's value type is a <see cref="Property"/>.
        /// </summary>
        /// <seealso cref="Property"/>
        /// <seealso cref="PropertyColumn"/>
        /// <seealso cref="PropertyRow"/>
        public class PropertyTable
        {
            /// <summary>
            /// represents a coupling of a <see cref="System.Type"/> and and a property name,
            /// where the specified <see cref="System.Type"/> has a public property with that
            /// aforementioned name.
            /// </summary>
            public class Property
            {
                /// <summary>
                /// thrown if specified property name does not exist in specified <see cref="System.Type"/>
                /// </summary>
                public class PropertyNameNotInTypeException : Exception
                {
                    private Type classType;
                    private string propertyName;

                    public PropertyNameNotInTypeException(Type classType, string propertyName)
                        : base(formatExceptionMessage(classType, propertyName))
                    {
                        this.classType = classType;
                        this.propertyName = propertyName;
                    }

                    public Type ClassType
                    {
                        get { return classType; }
                    }

                    public string PropertyName
                    {
                        get
                        { return propertyName; }
                    }

                    private static string formatExceptionMessage(Type type, string PropertyName)
                    {
                        return string.Format(
                            "Property {0} does not exist in type {1}.",
                            PropertyName,
                            type.FullName);
                    }
                }

                private Type classType;
                private string propertyName;

                public Property(Type classType, string propertyName)
                {
                    assertHasProperty(classType, propertyName);

                    this.classType = classType;
                    this.propertyName = propertyName;
                }

                public Type ClassType
                {
                    get { return classType; }
                }

                public string PropertyName
                {
                    get { return propertyName; }
                }

                public override bool Equals(object obj)
                {
                    if(obj == null)
                    {
                        return false;
                    }

                    Property other = obj as Property;

                    return other != null
                        && (other.propertyName == this.propertyName && other.classType == this.classType);
                }

                public static bool operator ==(Property property1, Property property2)
                {
                    return property1.Equals(property2);
                }

                public static bool operator !=(Property property1, Property property2)
                {
                    return !(property1 == property2);
                }

                public override int GetHashCode()
                {
                    return CollectionUtils.GetHashCode(this);
                }

                /// <summary>
                /// asserts that <paramref name="type"/> has a public property
                /// whose name is <paramref name="propertyName"/>. 
                /// </summary>
                /// <param name="type"></param>
                /// <param name="propertyName"></param>
                /// <exception cref="PropertyNameNotInTypeException">
                /// thrown if <paramref name="type"/> does not have a public property
                /// whose name is <paramref name="propertyName"/>
                /// </exception>
                /// <seealso cref="ReflectionUtils.HasPublicProperty(Type, string)"/>
                private void assertHasProperty(Type type, string propertyName)
                {
                    if (!ReflectionUtils.HasPublicProperty(type, propertyName))
                    {
                        throw new PropertyNameNotInTypeException(type, propertyName);
                    }
                }
            }

            /// <summary>
            /// represents a column whose value type is a <see cref="Property"/>.
            /// </summary>
            public class PropertyColumn : Column
            {
                /// <summary>
                /// thrown if specified propertyNames and headers <see cref="IList{T}"/> do not have the same
                /// number of elements.
                /// </summary>
                public class HeadersAndPropertiesCountMismatchException : MismatchException
                {
                    public HeadersAndPropertiesCountMismatchException()
                        : base("propertyNames.Count", "headers.Count")
                    {

                    }
                }

                private Property property;

                public PropertyColumn(
                    string header,
                    int width,
                    Property property,
                    string cutSuffix = DEFAULT_CUT_SUFFIX)
                    : base(header, width, cutSuffix)
                {
                    this.property = property;
                }

                public PropertyColumn(Column column, Property property)
                    : base(column)
                {
                    this.property = property;
                }

                public Property Property
                {
                    get { return property; }
                }

                /// <summary>
                /// parses <paramref name="headers"/>, <paramref name="widths"/>, and <paramref name="properties"/>
                /// as a <see cref="PropertyColumn"/> array of length <paramref name="headers"/>.Count,
                /// where the i'th item has 
                /// (<paramref name="headers"/>[i], <paramref name="widths"/>[i], <paramref name="properties"/>[i])
                /// as its values.
                /// </summary>
                /// <param name="headers"></param>
                /// <param name="widths"></param>
                /// <param name="properties"></param>
                /// <returns>
                /// <see cref="PropertyColumn"/> array of length <paramref name="headers"/>.Count,
                /// where the i'th item has 
                /// (<paramref name="headers"/>[i], <paramref name="widths"/>[i], <paramref name="properties"/>[i])
                /// as its values
                /// </returns>
                /// <exception cref="Column.WidhtsAndHeadersCountMismatchException">
                /// <seealso cref="Column.ParseArray(IList{string}, IList{int})"/>
                /// </exception>
                /// <exception cref="HeadersAndPropertiesCountMismatchException">
                /// <seealso cref="assertHeadersAndPropertiesCountsMatch(IList{string}, IList{Property})"/>
                /// </exception>
                public static PropertyColumn[] ParseArray(
                    IList<string>headers,
                    IList<int> widths, IList<Property> properties)
                {
                    // assert headers.Count and propertyNames.Count are equal
                    assertHeadersAndPropertiesCountsMatch(headers, properties);

                    // holds result
                    PropertyColumn[] propertyColumns = new PropertyColumn[headers.Count];

                    // parse Column array
                    Table.Column[] columns = Column.ParseArray(headers, widths);

                    // parse PropertyColumn array using columns array and propertyNames array
                    for(int i = 0; i < propertyColumns.Length; i++)
                    {
                        propertyColumns[i] = new PropertyColumn(columns[i], properties[i]);
                    }

                    return propertyColumns;
                }

                public override bool Equals(object obj)
                {
                    if(obj == null)
                    {
                        return false;
                    }

                    PropertyColumn other = obj as PropertyColumn;

                    return other != null
                        && base.Equals(other)
                        && this.property == other.property;
                }

                public static bool operator ==(PropertyColumn propertyColumn1, PropertyColumn propertyColumn2)
                {
                    return propertyColumn1.Equals(propertyColumn2);
                }

                public static bool operator !=(PropertyColumn propertyColumn1, PropertyColumn propertyColumn2)
                {
                    return !(propertyColumn1 == propertyColumn2);
                }

                public override int GetHashCode()
                {
                    return CollectionUtils.GetHashCode(this);
                }

                /// <summary>
                /// asserts that <paramref name="headers"/> and <paramref name="properties"/>
                /// have the same number of elements.
                /// </summary>
                /// <param name="headers"></param>
                /// <param name="properties"></param>
                /// <exception cref="HeadersAndPropertiesCountMismatchException">
                /// thrown if <paramref name="headers"/> and <paramref name="properties"/> do not have the same
                /// number of elements.
                /// </exception>
                private static void assertHeadersAndPropertiesCountsMatch(
                    IList<string> headers,
                    IList<Property> properties)
                {
                    if(headers.Count != properties.Count)
                    {
                        throw new HeadersAndPropertiesCountMismatchException();
                    }
                }
            }

            /// <summary>
            /// represents a <see cref="Row"/> whose column values are represented by
            /// couplings of <see cref="Property"/>s and <see cref="System.Object"/>s,
            /// so that for each column, the actual column value is
            /// <see cref="System.Object"/>.<see cref="Property"/>.
            /// </summary>
            public class PropertyRow : Row
            {
                /// <summary>
                /// thrown if specified <see cref="System.Object"/> and <see cref="Property"/> do not have the same
                /// <see cref="System.Type"/>.
                /// </summary>
                /// <seealso cref="MismatchException"/>
                public class PropertyTypeMismatchException : MismatchException
                {
                    public PropertyTypeMismatchException() 
                        : base("obj.GetType()", "property.Type")
                    {

                    }
                }

                /// <summary>
                /// thrown if specified <see cref="System.Object"/> and <see cref="Property"/>
                /// <see cref="System.Collections.Generic.IList{T}"/>s do not have the same number of elements. 
                /// </summary>
                /// /// <seealso cref="MismatchException"/>
                public class ObjectsAndPropertiesCountMismatchException : MismatchException
                {
                    public ObjectsAndPropertiesCountMismatchException() 
                        : base("objects.Count()", "properties.Count()")
                    {

                    }
                }

                // string representation of column value where either the object or property
                // are null
                private const string NULL_PROPERTY_VALUE_STRING = "N/A";

                private readonly Property[] properties;

                /// <summary>
                /// initializes a <see cref="PropertyRow"/> where all columns share the same <paramref name="obj"/>.
                /// </summary>
                /// <param name="obj"></param>
                /// <param name="properties"></param>
                /// <seealso cref="PropertyRow(IList{object},IList{Property})"/>
                public PropertyRow(object obj, IList<Property> properties)
                    : this(CollectionUtils.DuplicateToArray(obj, properties.Count), properties)

                {

                }

                /// <summary>
                /// initializes a <see cref="PropertyRow"/> where the i'th item is a coupling of
                /// <paramref name="objects"/>[i] and <paramref name="properties"/>[i].
                /// </summary>
                /// <param name="objects"></param>
                /// <param name="properties"></param>
                /// <seealso cref="getPropertyValueString(object, Property)"/>
                /// <exception cref="ArgumentNullException">
                /// <seealso cref="CollectionUtils.ConvertToArray{T}(IEnumerable{T})"/>
                /// </exception>
                public PropertyRow(
                    IList<object> objects,
                    IList<Property> properties) 
                    : base(getProperyValueArray(objects, properties))
                        
                {
                    this.properties = CollectionUtils.ConvertToArray(properties);
                }

                public static string NullPropetyValueString
                {
                    get { return NULL_PROPERTY_VALUE_STRING; }
                }

                public Property[] Properties
                {
                    get { return properties; }
                }

                public override bool Equals(object obj)
                {
                    if(obj == null)
                    {
                        return false;
                    }

                    PropertyRow other = obj as PropertyRow;

                    return other != null
                        && base.Equals(other)
                        && Enumerable.SequenceEqual(this.properties, other.properties);
                }

                public static bool operator ==(PropertyRow propertyRow1, PropertyRow propertyRow2)
                {
                    return propertyRow1.Equals(propertyRow2);
                }

                public static bool operator !=(PropertyRow propertyRow1, PropertyRow propertyRow2)
                {
                    return !(propertyRow1 == propertyRow2);
                }

                public override int GetHashCode()
                {
                    return CollectionUtils.GetHashCode(this);
                }

                /// <summary>
                /// returns an array of length <paramref name="objects"/>.Count,
                /// where the value of the i'th element is
                /// <paramref name="objects"/>[i].<paramref name="properties"/>[i].
                /// </summary>
                /// <param name="objects"></param>
                /// <param name="properties"></param>
                /// <returns>
                /// array of length <paramref name="objects"/>.Count,
                /// where the value of the i'th element is
                /// <paramref name="objects"/>[i].<paramref name="properties"/>[i].
                /// </returns>
                /// <exception cref="ObjectsAndPropertiesCountMismatchException">
                /// <seealso cref="assertObjectsAndPropertiesCountMatch(IList{object}, IList{Property})"/>
                /// </exception>
                private static string[] getProperyValueArray(
                    IList<object> objects,
                    IList<Property> properties)
                {
                    // assert objects.Count() and properties.Count() match 
                    assertObjectsAndPropertiesCountMatch(objects, properties);

                    string[] propertyValues = new string[objects.Count()];

                    // fill property value array using the Object and Property corresponding
                    // to each index
                    for (int i = 0; i < propertyValues.Length; i++)
                    {
                        object currentObject = objects[i];
                        Property currentProperty = properties[i];

                        propertyValues[i] = getPropertyValueString(currentObject, currentProperty);
                    }

                    return propertyValues;
                }

                /// <summary>
                /// returns the string representation of the coupling
                /// (<paramref name="obj"/>,<paramref name="property"/>),
                /// which is <paramref name="obj"/>.<paramref name="property"/> if both arguments are not null,
                /// and <see cref="NullPropetyValueString"/> otherwise.
                /// </summary>
                /// <param name="obj"></param>
                /// <param name="property"></param>
                /// <returns>
                /// string representation of the coupling (<paramref name="obj"/>,<paramref name="property"/>).
                /// </returns>
                /// <seealso cref="ReflectionUtils.GetPropertyValue(object, string)"/>
                /// <exception cref="PropertyTypeMismatchException">
                /// <seealso cref="assertObjectAndPropertyTypeMatch(object, Property)"/>
                /// </exception>
                private static string getPropertyValueString(object obj, Property property)
                {
                    string propertyValueString;

                    if (obj == null)
                    {
                        propertyValueString = NULL_PROPERTY_VALUE_STRING;
                    }
                    else
                    {
                        // first assert that object type and property type match
                        assertObjectAndPropertyTypeMatch(obj, property);

                        object propertyValue = ReflectionUtils.GetPropertyValue(obj, property.PropertyName);

                        if (propertyValue == null)
                        {
                            propertyValueString = NULL_PROPERTY_VALUE_STRING;
                        }
                        else
                        {
                            propertyValueString = propertyValue.ToString();
                        }
                    }

                    return propertyValueString;
                }

                /// <summary>
                /// asserts that <paramref name="objects"/>.Count and <paramref name="properties"/>.Count are equal.
                /// </summary>
                /// <param name="objects"></param>
                /// <param name="properties"></param>
                /// <exception cref="ObjectsAndPropertiesCountMismatchException">
                /// thrown if <paramref name="objects"/>.Count and <paramref name="properties"/>.Count are not equal.
                /// </exception>
                private static void assertObjectsAndPropertiesCountMatch(
                    IList<object> objects,
                    IList<Property> properties)
                {
                    if(objects.Count() != properties.Count())
                    {
                        throw new ObjectsAndPropertiesCountMismatchException();
                    }
                }

                /// <summary>
                /// asserts that <paramref name="obj"/>.GetType() == <paramref name="property"/>.ClassType.
                /// </summary>
                /// <param name="obj"></param>
                /// <param name="property"></param>
                /// <exception cref="PropertyTypeMismatchException">
                /// thrown if <paramref name="obj"/>.GetType() != <paramref name="property"/>.ClassType
                /// </exception>
                private static void assertObjectAndPropertyTypeMatch(object obj, Property property)
                {
                    if(obj.GetType() != property.ClassType)
                    {
                        throw new PropertyTypeMismatchException();
                    }
                }
            }

            /// <summary>
            /// thrown if a <see cref="PropertyRow"/> was attempted to be added,
            /// which has a column whose <see cref="Property"/> is different
            /// from the corresponding <see cref="PropertyColumn"/> in column list of <see cref="PropertyTable"/>.
            /// </summary>
            public class RowAndColumnPropertyMismatchException : MismatchException
            {
                public RowAndColumnPropertyMismatchException(int index)
                    : base("propertyRow.Properties[i]", "columnProperties[i]")
                {

                }
            }

            // underlying table
            private Table table = new Table();

            // properties corresponding to table columns
            private List<Property> columnProperties = new List<Property>();
            
            /// <summary>
            /// initializes a <see cref="PropertyTable"/>
            /// whose column list contains all <see cref="PropertyColumn"/>s from <paramref name="propertyColumns"/>
            /// and row list contains all <see cref="PropertyRow"/>s from <paramref name="propertyRows"/>.
            /// </summary>
            /// <param name="propertyColumns"></param>
            /// <param name="propertyRows"></param>
            public PropertyTable(
                IList<PropertyColumn> propertyColumns = null,
                IList<PropertyRow> propertyRows = null)
            {
                if(propertyColumns != null)
                {
                    AddColumnRange(propertyColumns);
                }             
                if(propertyRows != null)
                {
                    AddRowRange(propertyRows);
                }
            }

            public int ColumnCount
            {
                get { return table.ColumnCount; }
            }

            public int RowCount
            {
                get { return table.RowCount; }
            }

            public bool EmptyOfRows
            {
                get { return table.EmptyOfRows; }
            }

            /// <summary>
            /// adds <paramref name="propertyRow"/> to <see cref="PropertyTable"/>'s row list.
            /// </summary>
            /// <param name="propertyRow"></param>
            /// <exception cref="Table.RowColumnCountMismatchException">
            /// <seealso cref="Table.AddRow(Row)"/>
            /// </exception>
            /// <exception cref="RowAndColumnPropertyMismatchException">
            /// <seealso cref="assertRowPropertiesMatchColumns(PropertyRow)"/>
            /// </exception>
            public void AddRow(PropertyRow propertyRow)
            {
                table.AddRow(propertyRow);

                // assert row properties match those of the corresponding table columns
                assertRowPropertiesMatchColumns(propertyRow);
            }

            /// <summary>
            /// adds all <see cref="PropertyRow"/>s in <paramref name="propertyRows"/> to <see cref="PropertyTable"/>'s
            /// row list.
            /// </summary>
            /// <seealso cref="Table.AddRowRange{T}(IList{T})"/>
            /// <param name="propertyRows"></param>
            /// <exception cref="Table.RowColumnCountMismatchException">
            /// <seealso cref=" Table.AddRowRange{T}(IList{T})"/>
            /// </exception>
            /// <exception cref="System.ArgumentNullException">
            /// <seealso cref="Table.AddRowRange{T}(IList{T})"/>
            /// </exception>
            /// <exception cref="RowAndColumnPropertyMismatchException">
            /// <seealso cref="assertRowPropertiesMatchColumns(PropertyRow)"/>
            /// </exception>
            public void AddRowRange(IList<PropertyRow> propertyRows)
            {
                // rows added to table before asserting that row properties match those of table's column list
                // in order to make sure table's column count and each added row's column count are equal
                // (in this case an exception is thrown and propertyRows are not added to table)
                table.AddRowRange(propertyRows);

                try
                {
                    // assert that for each row,
                    // row properties match those of the corresponding table columns
                    foreach (PropertyRow propertyRow in propertyRows)
                    {
                        assertRowPropertiesMatchColumns(propertyRow);
                    }
                }
                catch(RowAndColumnPropertyMismatchException rowAndColumnPropertyMismatchException)
                {
                    // found a row whose properties do not match those of the corresponding table columns
                    // remove all added rows from table
                    table.RemoveRowRange(propertyRows);

                    throw rowAndColumnPropertyMismatchException;
                }
            }

            // false if remove was unsuccessful / item not found in row list
            /// <summary>
            /// <para>
            /// removes <paramref name="propertyRow"/> from <see cref="PropertyTable"/>'s row list if it exists there.
            /// </para>
            /// <para>
            /// returns whether <paramref name="propertyRow"/> existed in table
            /// prior to being removed.
            /// </para>
            /// </summary>
            /// <seealso cref="Table.RemoveRow(Row)"/>
            /// <param name="propertyRow"></param>
            /// <returns>
            /// true if<paramref name="propertyRow"/> existed in <see cref="PropertyTable"/>'s row list
            /// before being removed,
            /// else false
            /// </returns>
            public bool RemoveRow(PropertyRow propertyRow)
            {
                return table.RemoveRow(propertyRow);
            }

            /// <summary>
            /// removes each <see cref="PropertyRow"/> in <paramref name="rows"/> from <see cref="PropertyTable"/>'s
            /// row list, if it exists there.
            /// <para/>
            /// returns a bool array of length <paramref name="rows"/>.Count where the i'th item is
            /// true iff <paramref name="rows"/>[i] existed in <see cref="PropertyTable"/> row li
            /// </summary>
            /// <param name="rows"></param>
            /// <returns>
            /// bool array of length <paramref name="rows"/>.Count where the i'th item is
            /// true iff <paramref name="rows"/>[i] existed in <see cref="PropertyTable"/> row list 
            /// </returns>
            public bool[] RemoveRowRange(IList<PropertyRow> rows)
            {
                return table.RemoveRowRange(rows);
            }

            /// <summary>
            /// removes all rows from table row list.
            /// </summary>
            /// <seealso cref="Table.ClearRows"/>
            public void ClearRows()
            {
                table.ClearRows();
            }

            /// <summary>
            /// adds <paramref name="propertyColumn"/> to table's column list.
            /// </summary>
            /// <seealso cref="Table.AddColumn(Column)"/>
            /// <param name="propertyColumn"></param>
            /// <exception cref="Table.OperationRequiresEmptyTableException">
            /// <seealso cref="Table.AddColumn(Column)"/>
            /// </exception>
            public void AddColumn(PropertyColumn propertyColumn)
            {
                table.AddColumn(propertyColumn);
                columnProperties.Add(propertyColumn.Property);
            }

            /// <summary>
            /// adds <paramref name="propertyColumns"/> to table column list.
            /// </summary>
            /// <param name="propertyColumns"></param>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="Table.AddColumnRange{T}(IList{T})"/>
            /// </exception>
            public void AddColumnRange(IList<PropertyColumn> propertyColumns)
            {
                table.AddColumnRange(propertyColumns);

                foreach (PropertyColumn propertyColumn in propertyColumns)
                {
                    columnProperties.Add(propertyColumn.Property);
                }
            }

            // false if remove was unsuccessful / item not found in row list
            /// <summary>
            /// removes <paramref name="propertyColumn"/> from <see cref="PropertyTable"/>'s column list,
            /// if it exists in column list.
            /// returns whether <paramref name="propertyColumn"/> existed in <see cref="PropertyTable"/>'s column list
            /// before being removed.
            /// </summary>
            /// <seealso cref="Table.RemoveColumn(Column)"/>
            /// <param name="propertyColumn"></param>
            /// <returns>
            /// true if <paramref name="propertyColumn"/> existed in <see cref="PropertyTable"/>'s column list
            /// before being removed,
            /// else false
            /// </returns>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="Table.RemoveColumn(Column)"/>
            /// </exception>
            public bool RemoveColumn(PropertyColumn propertyColumn)
            {
                // remove from column list
                bool removalResult = table.RemoveColumn(propertyColumn);

                if(removalResult)
                {
                    // remove property corresponding to removed column
                    columnProperties.Remove(propertyColumn.Property);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// removes each <see cref="PropertyColumn"/> in <paramref name="propertyColumns"/> from
            /// <see cref="PropertyTable"/>'s column list, if it exists in column list.
            /// returns a bool array of length <paramref name="propertyColumns"/>.Count where
            /// the i'th element is true iff it existed in <see cref="PropertyTable"/>'s column list before
            /// being removed.
            /// </summary>
            /// <param name="propertyColumns"></param>
            /// <seealso cref="Table.RemoveColumnRange{T}(IList{T})"/>
            /// <returns>
            /// bool array of length <paramref name="propertyColumns"/>.Count where
            /// the i'th element is true iff it existed in <see cref="PropertyTable"/>'s column list before
            /// being removed.
            /// </returns>
            /// <exception cref="OperationRequiresEmptyTableException">
            /// <seealso cref="Table.RemoveColumnRange{T}(IList{T})"/>
            /// </exception>
            public bool[] RemoveColumnRange(IList<PropertyColumn> propertyColumns)
            {
                // remove from column list
                bool[] removalResult = table.RemoveColumnRange(propertyColumns);

                // for each removed column, remove property corresponding to column
                for (int i = 0; i < removalResult.Length; i++)
                {
                    PropertyColumn currentPropertyColumn = propertyColumns[i];
                    bool currentColumnRemoved = removalResult[i];
                 
                    if (currentColumnRemoved)
                    {
                        // remove property corresponding to removed column
                        columnProperties.Remove(currentPropertyColumn.Property);
                    }
                }

                return removalResult;
            }

            /// <summary>
            /// removes all <see cref="PropertyColumn"/>s from table column list.
            /// </summary>
            /// <seealso cref="Table.ClearColumns"/>
            /// <exception cref="Table.OperationRequiresEmptyTableException">
            /// <seealso cref="Table.ClearColumns"/>
            /// </exception>
            public void ClearColumns()
            {
                table.ClearColumns();
                columnProperties.Clear();
            }

            /// <summary>
            /// returns a string representation of the table header.
            /// </summary>
            /// <seealso cref="Table.GetTableHeaderString"/>
            /// <returns>
            /// string representation of the table header.
            /// </returns>
            public string GetColumnHeaderString()
            {
                return table.GetTableHeaderString();
            }

            /// <summary>
            /// returns a string representation of the <paramref name="rowIndex"/>'s row.
            /// </summary>
            /// <param name="rowIndex"></param>
            /// <seealso cref="Table.GetRowString(int)"/>
            /// <returns>
            /// string representation of the <paramref name="rowIndex"/>'s row.
            /// </returns>
            /// <exception cref="IndexOutOfRangeException">
            /// <seealso cref="Table.GetRowString(int)"/>
            /// </exception>
            public string GetRowString(int rowIndex)
            {
                return table.GetRowString(rowIndex);
            }

            /// <summary>
            /// returns a string representation of the table.
            /// </summary>
            /// <seealso cref="Table.GetTableDisplayString"/>
            /// <returns>
            /// string representation of the table
            /// </returns>
            public string GetTableDisplayString()
            {
                return table.GetTableDisplayString();
            }

            /// <summary>
            /// asserts that <paramref name="propertyRow"/>'s column properties match 
            /// <see cref="PropertyTable"/>'s column properties.
            /// </summary>
            /// <param name="propertyRow"></param>
            /// <exception cref="RowAndColumnPropertyMismatchException">
            /// thrown if <paramref name="propertyRow"/>'s column properties did not match 
            /// <see cref="PropertyTable"/>'s column properties.
            /// </exception>
            private void assertRowPropertiesMatchColumns(PropertyRow propertyRow)
            {
                for(int i = 0; i < propertyRow.Properties.Length; i++)
                {
                    Property currentColumnProperty = columnProperties[i];
                    Property currentRowProperty = propertyRow.Properties[i];

                    if (currentColumnProperty != currentRowProperty)
                    {
                        throw new RowAndColumnPropertyMismatchException(i);
                    }
                }
            }
        }
    }
}

