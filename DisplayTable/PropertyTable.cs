using CryptoBlock.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace TableDisplay
    {
        public class PropertyTable
        {
            public class Property
            {
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

                private void assertHasProperty(Type type, string propertyName)
                {
                    if (!ReflectionUtils.HasProperty(type, propertyName))
                    {
                        throw new PropertyNameNotInTypeException(type, propertyName);
                    }
                }
            }

            public class PropertyColumn : Table.Column
            {
                public class HeadersAndPropertiesCountMismatchException : MismatchException
                {
                    public HeadersAndPropertiesCountMismatchException()
                        : base("propertyNames.Length", "headers.Length")
                    {

                    }
                }

                private Property property;

                public PropertyColumn(string header, int width, Property property)
                    : base(header, width)
                {
                    this.property = property;
                }

                public PropertyColumn(Table.Column column, Property property)
                    : base(column)
                {
                    this.property = property;
                }

                public Property Property
                {
                    get { return property; }
                }

                public static PropertyColumn[] ParseArray(
                    IList<string>headers,
                    IList<int> widths, IList<Property> properties)
                {
                    // assert headers.Count and propertyNames.Count are equal
                    assertValidPropertyArrayLength(headers, properties);

                    // holds result
                    PropertyColumn[] propertyColumns = new PropertyColumn[headers.Count];

                    // parse Column array
                    Table.Column[] columns = Table.Column.ParseArray(headers, widths);

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

                private static void assertValidPropertyArrayLength(
                    IList<string> headers,
                    IList<Property> properties)
                {
                    if(headers.Count != properties.Count)
                    {
                        throw new HeadersAndPropertiesCountMismatchException();
                    }
                }
            }

            public class PropertyRow : Table.Row
            {
                public class PropertyTypeMismatchException : MismatchException
                {
                    public PropertyTypeMismatchException() 
                        : base("obj.GetType()", "property.Type")
                    {

                    }
                }

                public class ObjectsAndPropertiesCountMismatchException : MismatchException
                {
                    public ObjectsAndPropertiesCountMismatchException() 
                        : base("objects.Count()", "properties.Count()")
                    {

                    }
                }

                private readonly Property[] properties;

                public PropertyRow(object obj, IList<Property> properties)
                    : this(CollectionUtils.DuplicateToArray(obj, properties.Count), properties)

                {

                }

                public PropertyRow(
                    IList<object> objects,
                    IList<Property> properties) 
                    : base(getProperyValueArray(objects, properties))
                        
                {
                    this.properties = CollectionUtils.ConvertToArray(properties);
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

                private static string getPropertyValueString(object obj, Property property)
                {   
                    // first assert that object type and property type match
                    assertObjectAndPropertyTypeMatch(obj, property);

                    object propertyValue = ReflectionUtils.GetPropertyValue(obj, property.PropertyName);

                    string propertyValueString = propertyValue.ToString();

                    return propertyValueString;
                }

                private static void assertObjectsAndPropertiesCountMatch(
                    IList<object> objects,
                    IList<Property> properties)
                {
                    if(objects.Count() != properties.Count())
                    {
                        throw new ObjectsAndPropertiesCountMismatchException();
                    }
                }

                private static void assertObjectAndPropertyTypeMatch(object obj, Property property)
                {
                    if(obj.GetType() != property.ClassType)
                    {
                        throw new PropertyTypeMismatchException();
                    }
                }
            }

            public class RowAndColumnPropertyMismatchException : MismatchException
            {
                public RowAndColumnPropertyMismatchException(int index)
                    : base("propertyRow.Properties[i]", "columnProperties[i]")
                {

                }
            }

            private Table table = new Table();

            private List<Property> columnProperties = new List<Property>();
            
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

            public void AddRow(PropertyRow propertyRow)
            {
                table.AddRow(propertyRow);

                // assert row properties match those of the corresponding table columns
                assertRowPropertiesMatchColumns(propertyRow);
            }

            public void AddRowRange(IList<PropertyRow> propertyRows)
            {
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
                    // found a row which properties do not match those of the corresponding table columns
                    // remove added rows from table
                    table.RemoveRowRange(propertyRows);

                    throw rowAndColumnPropertyMismatchException;
                }
            }

            // false if remove was unsuccessful / item not found in row list
            public bool RemoveRow(PropertyRow propertyRow)
            {
                return table.RemoveRow(propertyRow);
            }

            public bool[] RemoveRowRange(IList<PropertyRow> rows)
            {
                return table.RemoveRowRange(rows);
            }

            public void ClearRows()
            {
                table.ClearRows();
            }

            public void AddColumn(PropertyColumn propertyColumn)
            {
                table.AddColumn(propertyColumn);
                columnProperties.Add(propertyColumn.Property);
            }

            public void AddColumnRange(IList<PropertyColumn> propertyColumns)
            {
                table.AddColumnRange(propertyColumns);

                foreach (PropertyColumn propertyColumn in propertyColumns)
                {
                    columnProperties.Add(propertyColumn.Property);
                }
            }

            // false if remove was unsuccessful / item not found in row list
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

            public void ClearColumns()
            {
                table.ClearColumns();
                columnProperties.Clear();
            }

            public string GetColumnHeaderString()
            {
                return table.GetColumnHeaderString();
            }

            public string GetRowString(int rowIndex)
            {
                return table.GetRowString(rowIndex);
            }

            public string GetTableString()
            {
                return table.GetTableString();
            }

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

