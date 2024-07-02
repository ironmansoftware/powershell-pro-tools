using Microsoft.VisualStudio.Project.Samples.CustomProject;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudioTools.Project
{
    [CLSCompliant(false), ComVisible(true)]
    public class SingleFileGeneratorNodeProperties : FileNodeProperties
    {
        #region fields
        private EventHandler<HierarchyNodeEventArgs> onCustomToolChanged;
        private EventHandler<HierarchyNodeEventArgs> onCustomToolNameSpaceChanged;
        #endregion

        #region custom tool events
        internal event EventHandler<HierarchyNodeEventArgs> OnCustomToolChanged
        {
            add { onCustomToolChanged += value; }
            remove { onCustomToolChanged -= value; }
        }

        internal event EventHandler<HierarchyNodeEventArgs> OnCustomToolNameSpaceChanged
        {
            add { onCustomToolNameSpaceChanged += value; }
            remove { onCustomToolNameSpaceChanged -= value; }
        }

        #endregion

        #region properties
        [SRCategoryAttribute(SR.Advanced)]
        [LocDisplayName(SR.CustomTool)]
        [SRDescriptionAttribute(SR.CustomToolDescription)]
        public virtual string CustomTool
        {
            get
            {
                return this.HierarchyNode.ItemNode.GetMetadata(ProjectFileConstants.Generator);
            }
            set
            {
                if (CustomTool != value)
                {
                    this.HierarchyNode.ItemNode.SetMetadata(ProjectFileConstants.Generator, value != string.Empty ? value : null);
                    HierarchyNodeEventArgs args = new HierarchyNodeEventArgs(this.HierarchyNode);
                    if (onCustomToolChanged != null)
                    {
                        onCustomToolChanged(this.Node, args);
                    }
                }
            }
        }

        [SRCategoryAttribute(SR.Advanced)]
        [LocDisplayName(SR.CustomToolNamespace)]
        [SRDescriptionAttribute(SR.CustomToolNamespaceDescription)]
        public virtual string CustomToolNamespace
        {
            get
            {
                return this.HierarchyNode.ItemNode.GetMetadata(ProjectFileConstants.CustomToolNamespace);
            }
            set
            {
                if (CustomToolNamespace != value)
                {
                    this.HierarchyNode.ItemNode.SetMetadata(ProjectFileConstants.CustomToolNamespace, value != String.Empty ? value : null);
                    HierarchyNodeEventArgs args = new HierarchyNodeEventArgs(this.HierarchyNode);
                    if (onCustomToolNameSpaceChanged != null)
                    {
                        onCustomToolNameSpaceChanged(this.Node, args);
                    }
                }
            }
        }
        #endregion

        #region ctors
        internal SingleFileGeneratorNodeProperties(HierarchyNode node)
            : base(node)
        {
        }
        #endregion
    }

}
