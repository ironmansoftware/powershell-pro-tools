using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class ApplicationData
    {
        public string Title { get; set; }
        public OutputModeOption OutputMode { get; set; }
        public bool PassThru { get; set; }
        public DataTable DataTable { get; set; }
    }

    public class DataTable
    {
        public List<DataTableRow> Data { get; set; }
        public List<DataTableColumn> DataColumns { get; set; }
        public DataTable(List<DataTableColumn> columns, List<DataTableRow> data)
        {
            DataColumns = columns;

            Data = data;
        }
    }

    public class DataTableColumn
    {
        [JsonIgnore]
        public Type Type => Type.GetType(StringType);
        public string Label { get; set; }
        //Serializable Version of Type
        public string StringType { get; set; }
        public string PropertyScriptAccessor { get; set; }
        public DataTableColumn(string label, string propertyScriptAccessor)
        {
            Label = label;
            PropertyScriptAccessor = propertyScriptAccessor;
        }

        //Distinct column defined by Label, Prop Accessor
        public override bool Equals(object obj)
        {
            DataTableColumn b = obj as DataTableColumn;
            return b.Label == Label && b.PropertyScriptAccessor == PropertyScriptAccessor;
        }
        public override int GetHashCode()
        {
            return Label.GetHashCode() + PropertyScriptAccessor.GetHashCode();
        }
        public override string ToString()
        {
            //Needs to be encoded to embed safely in xaml
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Label + PropertyScriptAccessor));
        }
    }

    public interface IValue : IComparable
    {
        string DisplayValue { get; set; }
    }
    public class DecimalValue : IValue
    {
        public string DisplayValue { get; set; }
        public decimal SortValue { get; set; }

        public int CompareTo(object obj)
        {
            DecimalValue otherDecimalValue = obj as DecimalValue;
            if (otherDecimalValue == null) return 1;
            return Decimal.Compare(SortValue, otherDecimalValue.SortValue);
        }
    }
    public class StringValue : IValue
    {
        public string DisplayValue { get; set; }
        public int CompareTo(object obj)
        {
            StringValue otherStringValue = obj as StringValue;
            if (otherStringValue == null) return 1;
            return DisplayValue.CompareTo(otherStringValue.DisplayValue);
        }
    }
    public class DataTableRow
    {
        //key is datacolumn hash code
        //have to do it this way because JSON can't serialize objects as keys
        public Dictionary<string, IValue> Values { get; set; }
        public int OriginalObjectIndex { get; set; }
        public DataTableRow(Dictionary<string, IValue> data, int originalObjectIndex)
        {
            Values = data;
            OriginalObjectIndex = originalObjectIndex;
        }
    }

    public enum OutputModeOption
    {
        /// <summary>
        /// None is the default and it means OK and Cancel will not be present
        /// and no objects will be written to the pipeline.
        /// The selectionMode of the actual list will still be multiple.
        /// </summary>
        None,
        /// <summary>
        /// Allow selection of one single item to be written to the pipeline.
        /// </summary>
        Single,
        /// <summary>
        ///Allow select of multiple items to be written to the pipeline.
        /// </summary>
        Multiple
    }

    public class Serializers
    {
        private readonly static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public static string ObjectToJson<T>(T obj)
        {
            var jsonString = JsonConvert.SerializeObject(obj, jsonSerializerSettings);

            return ToBase64String(jsonString);
        }

        public static T ObjectFromJson<T>(string base64Json)
        {
            var jsonString = FromBase64String(base64Json);

            return JsonConvert.DeserializeObject<T>(jsonString, jsonSerializerSettings);
        }


        private static string FromBase64String(string base64string)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64string));
        }

        private static string ToBase64String(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }

    public class TypeGetter
    {
        private static readonly Type _psPropertyExpressionType;
        private static readonly Type _psPropertyExpressionResultType;
        private static readonly MethodInfo _getValueMethod;
        private static readonly PropertyInfo _getResultProperty;
        private static readonly ConstructorInfo _psPropertyExpressionConstructor;
        static TypeGetter()
        {
            var dotNetCore = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription != null;
            if (dotNetCore)
            {
                _psPropertyExpressionType = typeof(PowerShell).Assembly.GetType("Microsoft.PowerShell.Commands.PSPropertyExpression");
                _psPropertyExpressionConstructor = _psPropertyExpressionType.GetConstructor(new Type[] {typeof(ScriptBlock)});
                _psPropertyExpressionResultType = typeof(PowerShell).Assembly.GetType("Microsoft.PowerShell.Commands.PSPropertyExpressionResult");
                _getValueMethod = _psPropertyExpressionType.GetMethod("GetValues", BindingFlags.Instance | BindingFlags.Public, null, new Type[] {typeof(PSObject)}, null);
                _getResultProperty = _psPropertyExpressionResultType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            }
            else 
            {
                _psPropertyExpressionType = typeof(PowerShell).Assembly.GetType("Microsoft.PowerShell.Commands.Internal.Format.MshExpression");
                _psPropertyExpressionConstructor = _psPropertyExpressionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {typeof(ScriptBlock)}, null);
                if (_psPropertyExpressionConstructor == null) throw new Exception("Constructor was null");
                _psPropertyExpressionResultType = typeof(PowerShell).Assembly.GetType("Microsoft.PowerShell.Commands.Internal.Format.MshExpressionResult");
                _getValueMethod = _psPropertyExpressionType.GetMethod("GetValues", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] {typeof(PSObject)}, null);
                if (_getValueMethod == null) throw new Exception("GetValues was null");
                _getResultProperty = _psPropertyExpressionResultType.GetProperty("Result", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_getResultProperty == null) throw new Exception("Result was null");
            }
        }

        private PSCmdlet _cmdlet;

        public TypeGetter(PSCmdlet cmdlet)
        {
            _cmdlet = cmdlet;
        }
        public FormatViewDefinition GetFormatViewDefinitionForObject(PSObject obj)
        {
            var typeName = obj.BaseObject.GetType().FullName;

            var types = _cmdlet.InvokeCommand.InvokeScript(@"Microsoft.PowerShell.Utility\Get-FormatData " + typeName).ToList();

            //No custom type definitions found - try the PowerShell specific format data
            if (types == null || types.Count == 0) 
            {
                types = _cmdlet.InvokeCommand
                    .InvokeScript(@"Microsoft.PowerShell.Utility\Get-FormatData -PowerShellVersion $PSVersionTable.PSVersion " + typeName).ToList();

                if (types == null || types.Count == 0)
                {
                    return null;
                }
            }

            var extendedTypeDefinition = types[0].BaseObject as ExtendedTypeDefinition;

            return extendedTypeDefinition.FormatViewDefinition[0];
        }

        public DataTableRow CastObjectToDataTableRow(PSObject ps, List<DataTableColumn> dataColumns, int objectIndex)
        {
            Dictionary<string, IValue> valuePairs = new Dictionary<string, IValue>();
            foreach (var dataColumn in dataColumns)
            {
                var expression = _psPropertyExpressionConstructor.Invoke(new object[] {ScriptBlock.Create(dataColumn.PropertyScriptAccessor)});
                var enumerable = _getValueMethod.Invoke(expression, new object[]{ps}) as IEnumerable;
                if (enumerable == null) continue;

                var enumerator = enumerable.GetEnumerator();

                if (!enumerator.MoveNext()) continue;
                var psResult = enumerator.Current;

                var stringValue = String.Empty;
                if (psResult != null)
                {
                    stringValue = _getResultProperty.GetValue(psResult)?.ToString();
                }

                var isDecimal = decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var decimalValue);

                if (isDecimal)
                {
                    valuePairs[dataColumn.ToString()] = new DecimalValue { DisplayValue = stringValue, SortValue = decimalValue };
                }
                else
                {
                    valuePairs[dataColumn.ToString()] = new StringValue { DisplayValue = stringValue };
                }
            }

            return new DataTableRow(valuePairs, objectIndex);
        }

        private void SetTypesOnDataColumns(List<DataTableRow> dataTableRows, List<DataTableColumn> dataTableColumns)
        {
            var dataRows = dataTableRows.Select(x => x.Values);

            foreach (var dataColumn in dataTableColumns)
            {
                dataColumn.StringType = typeof(decimal).FullName;
            }

            //If every value in a column could be a decimal, assume that it is supposed to be a decimal
            foreach (var dataRow in dataRows)
            {
                foreach (var dataColumn in dataTableColumns)
                {
                    if (!(dataRow[dataColumn.ToString()] is DecimalValue))
                    {
                        dataColumn.StringType = typeof(string).FullName;
                    }
                }
            }
        }
        private List<DataTableColumn> GetDataColumnsForObject(List<PSObject> psObjects)
        {
            var dataColumns = new List<DataTableColumn>();
            foreach (PSObject obj in psObjects)
            {
                var labels = new List<string>();

                FormatViewDefinition fvd = GetFormatViewDefinitionForObject(obj);

                var propertyAccessors = new List<string>();

                if (fvd == null)
                {
                    if (PSObjectIsPrimitive(obj))
                    {
                        labels = new List<string> { obj.BaseObject.GetType().Name };
                        propertyAccessors = new List<string> { "$_" };
                    }
                    else
                    {
                        labels = obj.Properties.Select(x => x.Name).ToList();
                        propertyAccessors = obj.Properties.Select(x => $"$_.\"{x.Name}\"").ToList();
                    }
                }
                else
                {
                    var tableControl = fvd.Control as TableControl;

                    var definedColumnLabels = tableControl.Headers.Select(x => x.Label);

                    var displayEntries = tableControl.Rows[0].Columns.Select(x => x.DisplayEntry);

                    var propertyLabels = displayEntries.Select(x => x.Value);

                    //Use the TypeDefinition Label if availble otherwise just use the property name as a label
                    labels = definedColumnLabels.Zip(propertyLabels, (definedColumnLabel, propertyLabel) =>
                    {
                        if (String.IsNullOrEmpty(definedColumnLabel))
                        {
                            return propertyLabel;
                        }
                        return definedColumnLabel;
                    }).ToList();


                    propertyAccessors = displayEntries.Select(x =>
                       {
                           //If it's a propety access directly
                           if (x.ValueType == DisplayEntryValueType.Property)
                           {
                               return $"$_.\"{x.Value}\"";
                           }
                           //Otherwise return access script
                           return x.Value;
                       }).ToList();
                }

                for (var i = 0; i < labels.Count; i++)
                {
                    dataColumns.Add(new DataTableColumn(labels[i], propertyAccessors[i]));
                }
            }
            return dataColumns.Distinct().ToList();
        }

        public DataTable CastObjectsToTableView(List<PSObject> psObjects)
        {
            List<FormatViewDefinition> objectFormats = psObjects.Select(GetFormatViewDefinitionForObject).ToList();

            var dataTableColumns = GetDataColumnsForObject(psObjects);

            foreach (var dataColumn in dataTableColumns)
            {
                _cmdlet.WriteVerbose(dataColumn.ToString());
            }

            List<DataTableRow> dataTableRows = new List<DataTableRow>();
            for (var i = 0; i < objectFormats.Count; i++)
            {
                var dataTableRow = CastObjectToDataTableRow(psObjects[i], dataTableColumns, i);
                dataTableRows.Add(dataTableRow);
            }

            SetTypesOnDataColumns(dataTableRows, dataTableColumns);

            return new DataTable(dataTableColumns, dataTableRows);
        }


        //Types that are condisidered primitives to PowerShell but not C#
        private readonly static List<string> additionalPrimitiveTypes = new List<string> { "System.String",
            "System.Decimal",
            "System.IntPtr",
            "System.Security.SecureString",
            "System.Numerics.BigInteger"
        };
        private bool PSObjectIsPrimitive(PSObject ps)
        {
            var psBaseType = ps.BaseObject.GetType();

            return psBaseType.IsPrimitive || psBaseType.IsEnum || additionalPrimitiveTypes.Contains(psBaseType.FullName);
        }
    }
}