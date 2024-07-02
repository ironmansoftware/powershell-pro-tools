using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace HelpEditorOS
{
    public partial class ClipBoardObject
    {
        public Object ClipBoardItem
        {
            set { _clipBoardItem = value; }
            get { return _clipBoardItem; }
        }
        private Object _clipBoardItem;

        public String TypeName
        {
            set { _typeName = value; }
            get { return _typeName; }
        }
        private String _typeName;
    }
    /// <summary>
    /// This is the Parameter Description Obejct used to hold
    /// the temporary parameter help metadata.
    /// </summary>
    public partial class parameterDecription
    {
        /// <summary>
        /// TODO// Need to remove this.
        /// </summary>
        public String CmdletName
        {
            set { _CmdletName = value; }
            get { return _CmdletName; }
        }
        private String _CmdletName;

        public Boolean HelpOnlyParameter
        {
            set { _helpOnlyParameter = value; }
            get { return _helpOnlyParameter; }
        }
        Boolean _helpOnlyParameter = false;

        public Boolean MismatchInfo
        {
            set { _MismatchInfo = value; }
            get { return _MismatchInfo; }
        }
        Boolean _MismatchInfo = false;

        /// <summary>
        /// TODO// Need to remove this.
        /// </summary>
        public String DefaultValue
        {
            set { _DefaultValue = value; }
            get { return _DefaultValue; }
        }
        private String _DefaultValue;


        /// <summary>
        /// TODO// Need to remove this.
        /// </summary>
        public String OldDefaultValue
        {
            set { _OldDefaultValue = value; }
            get { return _OldDefaultValue; }
        }
        private String _OldDefaultValue;



        /// <summary>
        /// This is the new Parameter Description 
        /// entered using this tool.
        /// </summary>
        public String NewDescription
        {
            set { _NewDescription = value; }
            get { return _NewDescription; }
        }
        private String _NewDescription;

        /// <summary>
        /// This ist the Paramter Description entered
        /// from the loaded help xml file.
        /// </summary>
        public String OldDescription
        {
            set { _OldDescription = value; }
            get { return _OldDescription; }
        }
        private String _OldDescription;

        /// <summary>
        /// This is the Name of the parameter
        /// used for identification.
        /// </summary>
        public String Name
        {
            set { _Name = value; }
            get { return _Name; }
        }
        private String _Name;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows wild card expansion
        /// </summary>
        public Boolean Globbing
        {
            set { _Globbing = value; }
            get { return _Globbing; }
        }
        private Boolean _Globbing = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows wild card expansion
        /// </summary>
        public Boolean OldGlobbing
        {
            set { _OldGlobbing = value; }
            get { return _OldGlobbing; }
        }
        private Boolean _OldGlobbing = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean VFP
        {
            set { _VFP = value; }
            get { return _VFP; }
        }
        private Boolean _VFP = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean SpecVFP
        {
            set { _SpecVFP = value; }
            get { return _SpecVFP; }
        }
        private Boolean _SpecVFP = false;


        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean SpecVFPBPN
        {
            set { _SpecVFPBPN = value; }
            get { return _SpecVFPBPN; }
        }
        private Boolean _SpecVFPBPN = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean VFPBPN
        {
            set { _VFPBPN = value; }
            get { return _VFPBPN; }
        }
        private Boolean _VFPBPN = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean VFRA
        {
            set { _VFRA = value; }
            get { return _VFRA; }
        }
        private Boolean _VFRA = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean SpecVFRA
        {
            set { _SpecVFRA = value; }
            get { return _SpecVFRA; }
        }
        private Boolean _SpecVFRA = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean isMandatory
        {
            set { _isMandatory = value; }
            get { return _isMandatory; }
        }
        private Boolean _isMandatory = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean SpecisMandatory
        {
            set { _SpecisMandatory = value; }
            get { return _SpecisMandatory; }
        }
        private Boolean _SpecisMandatory = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean isDynamic
        {
            set { _isDynamic = value; }
            get { return _isDynamic; }
        }
        private Boolean _isDynamic = false;


        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Boolean SpecisDynamic
        {
            set { _SpecisDynamic = value; }
            get { return _SpecisDynamic; }
        }
        private Boolean _SpecisDynamic = false;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public String ParameterType
        {
            set { _ParameterType = value; }
            get { return _ParameterType; }
        }
        private String _ParameterType = "";

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public String SpecParameterType
        {
            set { _SpecParameterType = value; }
            get { return _SpecParameterType; }
        }
        private String _SpecParameterType = "";


        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public String Position
        {
            set { _Position = value; }
            get { return _Position; }
        }
        private String _Position;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public int SpecPosition
        {
            set { _SpecPosition = value; }
            get { return _SpecPosition; }
        }
        private int _SpecPosition;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Collection<parameterAlias> Aliases
        {
            set { _Aliases = value; }
            get { return _Aliases; }
        }
        private Collection<parameterAlias> _Aliases;

        /// <summary>
        /// This value holds a boolean switch 
        /// describing whether the parameter allows VFP
        /// </summary>
        public Collection<parameterAttribute> Attributes
        {
            set { _Attributes = value; }
            get { return _Attributes; }
        }
        private Collection<parameterAttribute> _Attributes;


        /// <summary>
        /// This is the ParameterID.
        /// </summary>
        public Int32 ParameterID
        {
            set { _ParameterID = value; }
            get { return _ParameterID; }
        }
        private Int32 _ParameterID;

    }

    public partial class parameterAlias
    {
        public String Alias
        {
            set { _Alias = value; }
            get { return _Alias; }
        }
        private String _Alias;
    }

    public partial class parameterAttribute
    {
        public String Attribute
        {
            set { _Attribute = value; }
            get { return _Attribute; }
        }
        private String _Attribute;
    }

    /// <summary>
    /// This is the example record.
    /// </summary>
    public partial class example
    {
        public String ExampleDescription
        {
            set { _ExampleDescription = value; }
            get { return _ExampleDescription; }
        }
        private String _ExampleDescription;

        public String OldExampleDescription
        {
            set { _OldExampleDescription = value; }
            get { return _OldExampleDescription; }
        }
        private String _OldExampleDescription;

        public String SpecExample
        {
            set { _SpecExample = value; }
            get { return _SpecExample; }
        }
        private String _SpecExample;


        public String ExampleCmd
        {
            set { _ExampleCmd = value; }
            get { return _ExampleCmd; }
        }
        private String _ExampleCmd;

        public String OldExampleCmd
        {
            set { _OldExampleCmd = value; }
            get { return _OldExampleCmd; }
        }
        private String _OldExampleCmd;


        public String ExampleOutput
        {
            set { _ExampleOutput = value; }
            get { return _ExampleOutput; }
        }
        private String _ExampleOutput;

        public String OldExampleOutput
        {
            set { _OldExampleOutput = value; }
            get { return _OldExampleOutput; }
        }
        private String _OldExampleOutput;


        public String ExampleName
        {
            set { _ExampleName = value; }
            get { return _ExampleName; }
        }
        private String _ExampleName;

        public String OldExampleName
        {
            set { _OldExampleName = value; }
            get { return _OldExampleName; }
        }
        private String _OldExampleName;

        public Int32 ExampleID
        {
            set { _ExampleID = value; }
            get { return _ExampleID; }
        }
        private Int32 _ExampleID;

    }

    /// <summary>
    /// This is the related link item.
    /// </summary>
    public partial class relatedlink
    {
        public String LinkText
        {
            set { _LinkText = value; }
            get { return _LinkText; }
        }
        private String _LinkText;

        public String OldLinkText
        {
            set { _OldLinkText = value; }
            get { return _OldLinkText; }
        }
        private String _OldLinkText;


        public Int32 LinkID
        {
            set { _LinkID = value; }
            get { return _LinkID; }
        }
        private Int32 _LinkID;

    }

    /// <summary>
    /// This is the ParameterSet Object which
    /// contains parameter names.
    /// </summary>
    public partial class parameterSet
    {
        public String Name
        {
            set { _Name = value; }
            get { return _Name; }
        }
        private String _Name;


        public Collection<parametersetParameter> Parameters
        {
            set { _Parameters = value; }
            get { return _Parameters; }
        }
        private Collection<parametersetParameter> _Parameters;
    }

    /// <summary>
    /// This is the Parameteset parameter Name.
    /// </summary>
    public partial class parametersetParameter
    {
        public String Name
        {
            set { _Name = value; }
            get { return _Name; }
        }
        private String _Name;

    }

    /// <summary>
    /// This is the Parameter desciption object
    /// created by this tool
    /// </summary>
    public partial class cmdletDescription
    {


        /// <summary>
        /// This is the Cmdlet name that holds this parameter.
        /// </summary>
        public String CmdletName
        {
            set { _CmdletName = value; }
            get { return _CmdletName; }
        }
        private String _CmdletName;

        public String Verb
        {
            set { _Verb = value; }
            get { return _Verb; }
        }
        private String _Verb;

        public String Noun
        {
            set { _Noun = value; }
            get { return _Noun; }
        }
        private String _Noun;

        public String ShortDescription
        {
            set { _ShortDescription = value; }
            get { return _ShortDescription; }

        } private String _ShortDescription;

        public String OldShortDescription
        {
            set { _OldShortDescription = value; }
            get { return _OldShortDescription; }

        } private String _OldShortDescription;

        public String LongDescription
        {
            set { _LongDescription = value; }
            get { return _LongDescription; }

        } private String _LongDescription;

        public String OldLongDescription
        {
            set { _OldLongDescription = value; }
            get { return _OldLongDescription; }

        } private String _OldLongDescription;

        public String InputType
        {
            set { _InputType = value; }
            get { return _InputType; }

        } private String _InputType;

        public String OldInputType
        {
            set { _OldInputType = value; }
            get { return _OldInputType; }

        } private String _OldInputType;


        public String InputDesc
        {
            set { _InputDesc = value; }
            get { return _InputDesc; }

        } private String _InputDesc;

        public String OldInputDesc
        {
            set { _OldInputDesc = value; }
            get { return _OldInputDesc; }

        } private String _OldInputDesc;

        public String OutputType
        {
            set { _OuputType = value; }
            get { return _OuputType; }

        } private String _OuputType;

        public String OldOutputType
        {
            set { _OldOuputType = value; }
            get { return _OldOuputType; }

        } private String _OldOuputType;

        public String OutputDesc
        {
            set { _OutputDesc = value; }
            get { return _OutputDesc; }

        } private String _OutputDesc;

        public String OldOutputDesc
        {
            set { _OldOutputDesc = value; }
            get { return _OldOutputDesc; }

        } private String _OldOutputDesc;


        public String Note
        {
            set { _Note = value; }
            get { return _Note; }

        } private String _Note;

        public String OldNote
        {
            set { _OldNote = value; }
            get { return _OldNote; }

        } private String _OldNote;


        public Collection<example> Examples
        {
            set { _Examples = value; }
            get { return _Examples; }

        } private Collection<example> _Examples;



        public Collection<relatedlink> RelatedLinks
        {
            set { _RelatedLinks = value; }
            get { return _RelatedLinks; }

        } private Collection<relatedlink> _RelatedLinks;



        public Collection<parameterDecription> ParameterDecription
        {
            set { _ParamDescription = value; }
            get { return _ParamDescription; }

        } private Collection<parameterDecription> _ParamDescription;



        public Collection<parameterSet> ParameterSets
        {
            set { _ParameterSets = value; }
            get { return _ParameterSets; }

        } private Collection<parameterSet> _ParameterSets;

        public partial class ClipBoardObject
        {
            public Object ClipBoardItem
            {
                set { _clipBoardItem = value; }
                get { return _clipBoardItem; }
            }
            private Object _clipBoardItem;

            public String TypeName
            {
                set { _typeName = value; }
                get { return _typeName; }
            }
            private String _typeName;
        }

    }
}
