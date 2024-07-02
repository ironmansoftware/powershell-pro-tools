using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelpEditorOS
{
    public partial class SnapinView
    {
        public String Name
        {
            set { _Name = value; }
            get { return _Name; }
        }
        private String _Name;

        public String PsVersion
        {
            set { _PsVersion = value; }
            get { return _PsVersion; }
        }
        private String _PsVersion;

        public String PsDescription
        {
            set { _PsDescription = value; }
            get { return _PsDescription; }
        }
        private String _PsDescription;

        public String PsModule
        {
            set { _PsModule = value; }
            get { return _PsModule; }
        }
        private String _PsModule;
    }

}
