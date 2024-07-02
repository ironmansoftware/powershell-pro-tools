using System;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace IMS.FormDesigner
{

	public class ControlFactory
	{
		public static Control CreateControl(string ctrlName, string partialName)
		{
			try
			{
                var controlAsm = Assembly.LoadWithPartialName(partialName);
				var controlType = controlAsm.GetType(partialName + "." + ctrlName);
				var ctrl = (Control)Activator.CreateInstance(controlType);
				return ctrl;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Failed to create control" + ex.Message);
				return new Control();
			}
		}

		public static void SetControlProperties(Control control, Hashtable propertyList)
		{
			var properties = TypeDescriptor.GetProperties(control);

			foreach (PropertyDescriptor property in properties)
            {
                if (!propertyList.Contains(property.Name)) continue;

                var obj = propertyList[property.Name];
                try
                {
                    property.SetValue(control, obj);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

            }

		}

		public static Control CloneCtrl(Control control)
		{
			var cbCtrl = new CBFormCtrl(control);
			var newControl = CreateControl(cbCtrl.CtrlName, cbCtrl.PartialName);

			SetControlProperties(newControl, cbCtrl.PropertyList);

			return newControl;
		}

		public static void CopyCtrl2ClipBoard(Control control)
		{
			var cbCtrl = new CBFormCtrl(control);
			var ido = new DataObject();

			ido.SetData(CBFormCtrl.Format.Name, true, cbCtrl);
			Clipboard.SetDataObject(ido, false);

		}

		public static Control GetCtrlFromClipBoard()
		{
			var ctrl = new Control();

			IDataObject ido = Clipboard.GetDataObject();
			if (ido.GetDataPresent(CBFormCtrl.Format.Name))
			{
				CBFormCtrl cbCtrl = ido.GetData(CBFormCtrl.Format.Name) as CBFormCtrl;

				ctrl = ControlFactory.CreateControl(cbCtrl.CtrlName, cbCtrl.PartialName);
				ControlFactory.SetControlProperties(ctrl, cbCtrl.PropertyList);

			}
			return ctrl;
		}


	}

	[Serializable()]
	public class CBFormCtrl
	{
        private string ctrlName;
		private string partialName;
		private Hashtable propertyList = new Hashtable();

		static CBFormCtrl()
		{
			Format = DataFormats.GetFormat(typeof(CBFormCtrl).FullName);
		}

		public static DataFormats.Format Format { get; }

        public string CtrlName
		{
			get => ctrlName;
            set => ctrlName = value;
        }

		public string PartialName
		{
			get => partialName;
            set => partialName = value;
        }

		public Hashtable PropertyList => propertyList;


        public CBFormCtrl()
		{

		}

		public CBFormCtrl(Control ctrl)
		{
			CtrlName = ctrl.GetType().Name;
			PartialName = ctrl.GetType().Namespace;

			var properties = TypeDescriptor.GetProperties(ctrl);

			foreach (PropertyDescriptor myProperty in properties)
			{
				try
				{
					if (myProperty.PropertyType.IsSerializable)
						propertyList.Add(myProperty.Name, myProperty.GetValue(ctrl));
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex.Message);
				}

			}

		}
	}
}
