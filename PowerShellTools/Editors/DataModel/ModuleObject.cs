using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HelpEditorOS
{
    public class ModuleObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private String _Name;
        private String _Version;
        private String _Descrition;
        private String _ModuleType;

        public String Name
        {
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
            get { return _Name; }
        }

        public String Version
        {
            set
            {
                if (_Version != value)
                {
                    _Version = value;
                    this.OnPropertyChanged("Version");
                }
            }
            get { return _Version; }
        }

        public String Descrition
        {
            set
            {
                if (_Descrition != value)
                {
                    _Descrition = value;
                    this.OnPropertyChanged("Descrition");
                }
            }
            get { return _Descrition; }
        }

        public String ModuleType
        {
            set
            {
                if (_ModuleType != value)
                {
                    _ModuleType = value;
                    this.OnPropertyChanged("ModuleType");
                }
            }
            get { return _ModuleType; }
        }


        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

  
}
