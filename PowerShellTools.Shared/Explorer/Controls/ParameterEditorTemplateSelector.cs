using System.Windows;
using System.Windows.Controls;
using PowerShellTools.Common;

namespace PowerShellTools.Explorer
{
    internal class ParameterEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnsupportedTemplete { get; set; }
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate SwitchTemplate { get; set; }
        public DataTemplate ByteTemplate { get; set; }
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate LongTemplate { get; set; }
        public DataTemplate ChoiceTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is ParameterModel)
            {
                var parameter = item as ParameterModel;

                switch (parameter.Type)
                {
                    case ParameterType.Boolean:
                        return SwitchTemplate;

                    case ParameterType.Switch:
                        return SwitchTemplate;

                    case ParameterType.Byte:
                        return ByteTemplate;

                    case ParameterType.Int32:
                        return IntTemplate;

                    case ParameterType.Int64:
                        return LongTemplate;

                    case ParameterType.Enum:
                    case ParameterType.Float:
                    case ParameterType.Double:
                    case ParameterType.Decimal:
                    case ParameterType.Char:
                    case ParameterType.String:
                    case ParameterType.Array:
                    case ParameterType.Object:
                    case ParameterType.Unsupported:
                        return StringTemplate;
                    case ParameterType.Choice:
                        return ChoiceTemplate;
                    default:
                        return StringTemplate;;
                }
            }

            return null;
        }
    }
}
