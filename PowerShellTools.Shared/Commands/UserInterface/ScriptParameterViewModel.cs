using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using PowerShellTools.Common;

namespace PowerShellTools.Commands.UserInterface
{
    /// <summary>
    /// Data structure to provide template definitions and values from the parameter file
    /// </summary>
    internal class ScriptParameterViewModel : ObservableObject, INotifyDataErrorInfo, IComparable<ScriptParameterViewModel>
    {
        private const string ValuePropertyName = "Value";

        private List<object> _allowedValues;
        private object _value;
        private bool _isMandatory;

        public ScriptParameterViewModel(ScriptParameter parameterDefinition)
        {
            this.ParameterDefinition = Arguments.ValidateNotNull(parameterDefinition, "parameterDefinition");

            // Determine parameter type
            if (DoParameterTypeNamesMatch(ParameterDefinition.Type, DataTypeConstants.BoolType))
            {
                this.Type = ParameterType.Boolean;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.SwitchType))
            {
                this.Type = ParameterType.Switch;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.EnumType))
            {
                this.Type = ParameterType.Enum;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.ByteType))
            {
                this.Type = ParameterType.Byte;
            }
            else if (DoParameterTypeNamesMatch(ParameterDefinition.Type, DataTypeConstants.Int32Type))
            {
                this.Type = ParameterType.Int32;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.Int64Type))
            {
                this.Type = ParameterType.Int64;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.SingleType))
            {
                this.Type = ParameterType.Float;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.DoubleType))
            {
                this.Type = ParameterType.Double;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.DecimalType))
            {
                this.Type = ParameterType.Decimal;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.CharType))
            {
                this.Type = ParameterType.Char;
            }
            else if (DoParameterTypeNamesMatch(ParameterDefinition.Type, DataTypeConstants.StringType))
            {
                this.Type = ParameterType.String;
            }
            else if (DoParameterTypeNamesMatch(parameterDefinition.Type, DataTypeConstants.ArrayType))
            {
                this.Type = ParameterType.Array;
            }
            else
            {
                this.Type = ParameterType.Unknown;
            }

            _value = parameterDefinition.DefaultValue;
            _isMandatory = parameterDefinition.IsMandatory;

            Validate();
        }

        /// <summary>
        /// The definition of the parameter in the template file
        /// </summary>
        private ScriptParameter ParameterDefinition
        {
            get;
            set;
        }

        public string Name
        {
            get 
            { 
                return ParameterDefinition.Name; 
            }
        }

        public ParameterType Type
        {
            get;
            private set;
        }

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                Validate();
                NotifyPropertyChanged();
                NotifyPropertyChanged("Watermark");
            }
        }

        /// <summary>
        /// Give a list of allowed values appropriate for UI display purposes 
        /// </summary>
        public IEnumerable<object> AllowedValues
        {
            get
            {
                EnsureAllowedValues();
                return _allowedValues;
            }
        }

        public string ParameterSetName
        {
            get
            {
                return ParameterDefinition.ParameterSetName;
            }
        }

        /// <summary>
        /// Indicates a watermark that should be displayed if the parameter's value
        /// is null or empty string.
        /// </summary>
        public string Watermark
        {
            get
            {
                if (this.Value == null)
                {
                    return Resources.WatermarkNotSet;
                }
                else
                {
                    return null;
                }
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Populate the allowed values as appropriate for the UI, always including null and 
        /// the current value even if they are not included in the CSM model's allowed value
        /// enumeration...
        /// </summary>
        private void EnsureAllowedValues()
        {
            Debug.Assert(ParameterDefinition != null, "You should not be able to create an instance without a backing ParameterDefinition");

            if (_allowedValues == null)
            {
                _allowedValues = new List<object>();
                if (ParameterDefinition.AllowedValues.Count > 0)
                {
                    _allowedValues.AddRange(ParameterDefinition.AllowedValues);
                }
                else if (this.Type == ParameterType.Boolean)
                {
                    _allowedValues.Add(true);
                    _allowedValues.Add(false);
                }
                if (_allowedValues.Count > 0)
                {
                    // If the current value is not in the list of allowed values, we have to
                    //   add it or it won't show up in the dropdowns.
                    if (!_allowedValues.Contains(Value))
                    {
                        _allowedValues.Insert(0, Value);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether two template parameter type names match
        /// </summary>
        /// <param name="typeName1">Type name 1</param>
        /// <param name="typeName2">Type name 2</param>
        /// <returns>True if they match</returns>
        private bool DoParameterTypeNamesMatch(string typeName1, string typeName2)
        {
            return String.Equals(typeName1, typeName2, StringComparison.OrdinalIgnoreCase);
        }

        #region Validation

        /// <summary>
        /// Sets the Error and Warning properties appropriately
        /// </summary>
        private void Validate()
        {
            string warning = null;
            string error = null;

            if (!IsValidType())
            {
                error = Resources.Error_TypeMismatch_2args.FormatCurrentCulture(this.Name, this.ParameterDefinition.Type);
            }
            else if (!IsAllowedValue())
            {
                error = Resources.Error_DisallowedValue_1arg.FormatCurrentCulture(this.Name);
            }
            else if (_isMandatory && (this.Value == null || string.IsNullOrEmpty(this.Value.ToString())))
            {
                warning = Resources.Warning_MandatoryValueNotSet_1arg.FormatCurrentCulture(this.Name);
            }

            // Set the validation error to either an error or warning
            if (error != null)
            {
                this.ValidationResult = new ValidationResult()
                {
                    Message = error
                };
            }
            else if (warning != null)
            {
                this.ValidationResult = new ValidationResult()
                {
                    Message = warning,
                    IsWarning = true
                };
            }
            else
            {
                this.ValidationResult = null;
            }

            // Fire changed events
            if (this.ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(ValuePropertyName));
            }
            NotifyPropertyChanged("HasWarning");
            NotifyPropertyChanged("HasError");
            NotifyPropertyChanged("HelpText");
        }

        /// <summary>
        /// True if the current parameter value falls within the allowed
        ///   values.
        /// </summary>
        /// <returns></returns>
        private bool IsAllowedValue()
        {
            if (!this.ParameterDefinition.AllowedValues.Any())
            {
                return true;
            }

            return this.ParameterDefinition.AllowedValues.Any(allowed =>
                ParameterValueComparer.AreParameterValuesEqual(this.Value, allowed));
        }

        /// <summary>
        /// True if the current parameter value is of the correct type
        /// </summary>
        /// <returns></returns>
        private bool IsValidType()
        {
            if (this.Value == null)
                return true;

            switch (this.Type)
            {
                case ParameterType.Boolean:
                case ParameterType.Switch:
                    return this.Value is bool || this.Value is bool?;

                case ParameterType.Enum:
                    return this.AllowedValues.Contains(this.Value);

                case ParameterType.Byte:
                    return this.Value is byte;

                case ParameterType.Int32:
                    return this.Value is int;

                case ParameterType.Int64:
                    return this.Value is long;

                case ParameterType.Float:
                    float floatResult;
                    return float.TryParse(((string)this.Value).Trim(), out floatResult);

                case ParameterType.Double:
                    double doubleResult;
                    return double.TryParse(((string)this.Value).Trim(), out doubleResult);

                case ParameterType.Decimal:
                    decimal decimalResult;
                    return decimal.TryParse(((string)this.Value).Trim(), out decimalResult);

                case ParameterType.String:
                    return this.Value is string;

                case ParameterType.Char:
                    return (this.Value is string) && (((string)this.Value).Length == 1);

                case ParameterType.Array:
                case ParameterType.Unknown:
                    return true;

                default:
                    Debug.Fail("Unexpected type");
                    return true;
            }
        }

        #region Errors/Warnings

        /// <summary>
        /// The current error, if any, or null
        /// </summary>
        public ValidationResult ValidationResult
        {
            get;
            private set;
        }

        /// <summary>
        /// A string appropriate for help text for screen readers
        /// </summary>
        public string HelpText
        {
            get
            {
                return (this.ValidationResult == null ? null : ValidationResult.Message)
                    ?? this.Watermark;
            }
        }

        public bool HasValidationResult
        {
            get { return this.ValidationResult != null; }
        }

        public bool HasError
        {
            get { return this.ValidationResult != null && !this.ValidationResult.IsWarning; }
        }

        public bool HasWarning
        {
            get { return this.ValidationResult != null && this.ValidationResult.IsWarning; }
        }

        #endregion Error/Warnings

        #region INotifyDataErrorInfo implementation

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            // We only support a single validation error right now
            if (string.Equals(propertyName, ValuePropertyName, StringComparison.OrdinalIgnoreCase))
            {
                if (this.ValidationResult != null)
                {
                    yield return this.ValidationResult;
                }
            }
        }

        /// <summary>
        /// Comparator for sorting script parameter view models
        /// </summary>
        public int CompareTo(ScriptParameterViewModel other)
        {
            return string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        bool INotifyDataErrorInfo.HasErrors
        {
            get { return this.HasValidationResult; }
        }

        #endregion INotifyDataErrorInfo implementation

        #endregion Validation
    }
}
