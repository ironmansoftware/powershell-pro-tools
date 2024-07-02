using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace IMS.FormDesigner
{
    public class EventTab : PropertyTab
    {
        public override string TabName => "Events";

        public override Bitmap Bitmap
        {
            get
            {
                using (Stream resourceStream = typeof(EventTab).Assembly.GetManifestResourceStream("IMS.FormDesigner.events.bmp"))
                {
                    if (resourceStream == null)
                        return null;

                    return new Bitmap(resourceStream);
                }
            }
        }

        public override PropertyDescriptorCollection GetProperties(object component)
        {
            return GetProperties(null, component, null);
        }

        /// <summary>Gets the default property from the specified object.</summary>
        /// <param name="obj">The object to retrieve the default property of. </param>
        /// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptor" /> indicating the default property.</returns>
        public override PropertyDescriptor GetDefaultProperty(object obj)
        {
            IEventBindingService eventPropertyService = this.GetEventPropertyService(obj, (ITypeDescriptorContext)null);
            if (eventPropertyService == null)
                return (PropertyDescriptor)null;
            EventDescriptor defaultEvent = TypeDescriptor.GetDefaultEvent(obj);
            if (defaultEvent != null)
                return eventPropertyService.GetEventProperty(defaultEvent);
            return (PropertyDescriptor)null;
        }

        private IEventBindingService GetEventPropertyService(
          object obj,
          ITypeDescriptorContext context)
        {
            IEventBindingService eventBindingService = (IEventBindingService)null;
            if (eventBindingService == null && obj is IComponent)
            {
                ISite site = ((IComponent)obj).Site;
                if (site != null)
                    eventBindingService = (IEventBindingService)site.GetService(typeof(IEventBindingService));
            }
            if (eventBindingService == null && context != null)
                eventBindingService = (IEventBindingService)context.GetService(typeof(IEventBindingService));
            return eventBindingService;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            IEventBindingService eventPropertyService = this.GetEventPropertyService(component, context);
            if (eventPropertyService == null)
                return new PropertyDescriptorCollection((PropertyDescriptor[])null);
            EventDescriptorCollection events = TypeDescriptor.GetEvents(component, attributes);
            PropertyDescriptorCollection descriptorCollection = eventPropertyService.GetEventProperties(events);
            Attribute[] attributes1 = new Attribute[attributes.Length + 1];
            Array.Copy((Array)attributes, 0, (Array)attributes1, 0, attributes.Length);
            attributes1[attributes.Length] = (Attribute)DesignerSerializationVisibilityAttribute.Content;
            PropertyDescriptorCollection properties1 = TypeDescriptor.GetProperties(component, attributes1);
            if (properties1.Count > 0)
            {
                ArrayList arrayList = (ArrayList)null;
                for (int index = 0; index < properties1.Count; ++index)
                {
                    PropertyDescriptor oldPropertyDescriptor = properties1[index];
                    if (oldPropertyDescriptor.Converter.GetPropertiesSupported() && TypeDescriptor.GetEvents(oldPropertyDescriptor.GetValue(component), attributes).Count > 0)
                    {
                        if (arrayList == null)
                            arrayList = new ArrayList();
                        PropertyDescriptor property = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, (Attribute)MergablePropertyAttribute.No);
                        arrayList.Add((object)property);
                    }
                }
                if (arrayList != null)
                {
                    PropertyDescriptor[] propertyDescriptorArray = new PropertyDescriptor[arrayList.Count];
                    arrayList.CopyTo((Array)propertyDescriptorArray, 0);
                    PropertyDescriptor[] properties2 = new PropertyDescriptor[descriptorCollection.Count + propertyDescriptorArray.Length];
                    descriptorCollection.CopyTo((Array)properties2, 0);
                    Array.Copy((Array)propertyDescriptorArray, 0, (Array)properties2, descriptorCollection.Count, propertyDescriptorArray.Length);
                    descriptorCollection = new PropertyDescriptorCollection(properties2);
                }
            }
            return descriptorCollection;
        }

        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            return GetProperties(null, component, attributes);
        }
    }
}
