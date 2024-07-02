using System;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.CodeDom.Compiler;
using System.IO;
using IMS.FormDesigner;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Collections.Generic;
using PowerShellToolsPro.FormsDesigner;

namespace IM.WinForms
{
    public class frmMain : UserControl
	{
		public PropertyGrid propertyGrid;
		private Splitter splitter1;
		private Panel pnlViewHost;
        public ToolboxService lstToolbox;
        private DesignerHost host;
        public string designerFile;
        public string codeFile;
        private IComponent[] selectedControls;
        private bool unsaved;
        private Control designer;

        private string formNewName;
        private string formOldName;

        private readonly ILanguage _language;

        public event EventHandler DesignerDirty;

        public frmMain(ILanguage language, string designerFile, string codeFile, EventGenerationType eventGenerationType)
		{
            _language = language;
            this.designerFile = designerFile;
            this.codeFile = codeFile;

            InitializeComponent();

            try
            {
                Initialize(eventGenerationType);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load designer. " + ex.Message, "Failed to load designer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
		{
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.lstToolbox = new IM.WinForms.ToolboxService();
            this.pnlViewHost = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            //
            // propertyGrid
            //
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.propertyGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.propertyGrid.Location = new System.Drawing.Point(0, 119);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(224, 312);
            this.propertyGrid.TabIndex = 0;

            //
            // splitter1
            //
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(985, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 455);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            //
            // lstToolbox
            //
            this.lstToolbox.BackColor = System.Drawing.SystemColors.Control;
            this.lstToolbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstToolbox.ItemHeight = 16;
            this.lstToolbox.LineColor = System.Drawing.Color.Empty;
            this.lstToolbox.Location = new System.Drawing.Point(0, 0);
            this.lstToolbox.Name = "lstToolbox";
            this.lstToolbox.SelectedCategory = null;
            this.lstToolbox.Size = new System.Drawing.Size(224, 115);
            this.lstToolbox.Sorted = true;
            this.lstToolbox.TabIndex = 2;
            //
            // pnlViewHost
            //
            this.pnlViewHost.BackColor = System.Drawing.SystemColors.Window;
            this.pnlViewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlViewHost.Location = new System.Drawing.Point(0, 0);
            this.pnlViewHost.Name = "pnlViewHost";
            this.pnlViewHost.Size = new System.Drawing.Size(985, 455);
            this.pnlViewHost.TabIndex = 3;
            //
            // frmMain
            //
            this.Controls.Add(this.pnlViewHost);
            this.Controls.Add(this.splitter1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "frmMain";
            this.Size = new System.Drawing.Size(989, 455);
            this.ResumeLayout(false);

		}
		#endregion

		private ServiceContainer serviceContainer = null;
		private MenuCommandService menuService = null;

		private void Initialize(EventGenerationType eventGenerationType)
		{
            lstToolbox.designPanel = pnlViewHost;
            PopulateToolbox(lstToolbox);
            InitializeHost(eventGenerationType);
            propertyGrid.Site = new DesignSite(this.host, "propertyGrid");
            propertyGrid.PropertyTabs.AddTabType(typeof(EventTab));
        }

        private void InitializeHost(EventGenerationType eventGenerationType)
        {
            serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(INameCreationService), new NameCreationService());
            serviceContainer.AddService(typeof(IUIService), new UIService(this));
            serviceContainer.AddService(typeof(IEventBindingService), _language.GetEventBindingService(serviceContainer, codeFile));

            var resourceFile = codeFile.Replace(".ps1", ".resources.ps1");

            serviceContainer.AddService(typeof(IResourceService), new ResourceService(resourceFile));
            serviceContainer.AddService(typeof(IToolboxService), lstToolbox);

            menuService = new MenuCommandService();
            serviceContainer.AddService(typeof(IMenuCommandService), menuService);

            Form form;
            IRootDesigner rootDesigner;
            var codeDomProvider = _language.GetCodeDomProvider(codeFile, designerFile, eventGenerationType);

            host = new DesignerHost(serviceContainer, codeDomProvider, designerFile);

            if (!File.Exists(designerFile) || host.RootDesigner == null)
            {
                form = (Form)host.CreateComponent(typeof(Form));
                form.TopLevel = false;
                form.Text = "Form1";
            }

            rootDesigner = host.RootDesigner;
            designer = (Control)rootDesigner.GetView(ViewTechnology.Default);
            designer.Dock = DockStyle.Fill;
            designer.KeyUp += View_KeyUp;
            designer.PreviewKeyDown += Designer_KeyDown;
            designer.Cursor = Cursors.Hand;
            pnlViewHost.Controls.Add(designer);

            ISelectionService s = (ISelectionService)serviceContainer.GetService(typeof(ISelectionService));
            s.SelectionChanged += new EventHandler(OnSelectionChanged);
            host.Activate();

            host.ComponentRename += (sender, args) =>
            {
                if (args.Component is Form)
                {
                    formOldName = args.OldName;
                    formNewName = args.NewName;
                }
            };
            host.ComponentChanged += (sender, e) => SetDirty();
            host.TransactionClosed += (sender, e) => SetDirty();
            host.Saved += Host_Saved;
        }

        private void Designer_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (selectedControls != null)
            {
                foreach (Control control in selectedControls.Where(m => m is Control))
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        control.Top--;
                    }
                    if (e.KeyCode == Keys.Down)
                    {
                        control.Top++;
                    }
                    if (e.KeyCode == Keys.Left)
                    {
                        control.Left--;
                    }
                    if (e.KeyCode == Keys.Right)
                    {
                        control.Left++;
                    }
                }
            }
        }

        public void Form_FormClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (unsaved)
            {
                var dr = MessageBox.Show("Are you sure you want to quit? There are unsaved changes", "Exiting", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }

        }

        private void View_KeyUp(object sender, KeyEventArgs e)
        {
            if (selectedControls != null && e.KeyCode == Keys.Delete)
            {
                foreach(var control in selectedControls)
                {
                    host.DestroyComponent(control);
                }
            }
        }

        private void Host_Saved(object sender, EventArgs e)
        {
            unsaved = false;
        }

        private void SetDirty()
        {
            unsaved = true;
            if (DesignerDirty != null)
            {
                DesignerDirty(this, new EventArgs());
            }
        }

        private void PopulateToolbox(IToolboxService toolbox)
		{
            toolbox.AddToolboxItem(new ToolboxItem(typeof(BackgroundWorker)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(BindingNavigator)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(BindingSource)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(Button)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(CheckBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(CheckedListBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ColorDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ComboBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ContextMenuStrip)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(DataGridView)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(DateTimePicker)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(DomainUpDown)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ErrorProvider)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(FileSystemWatcher)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(FlowLayoutPanel)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(FolderBrowserDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(FontDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(GroupBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(HelpProvider)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(HScrollBar)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ImageList)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(Label)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(LinkLabel)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ListBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ListView)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(MaskedTextBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(MenuStrip)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(MonthCalendar)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(NotifyIcon)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(NumericUpDown)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(OpenFileDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(PageSetupDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(Panel)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(PictureBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(PrintDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(PrintPreviewControl)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(PrintPreviewDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ProgressBar)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(PropertyGrid)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(RadioButton)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(RichTextBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(SaveFileDialog)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(SplitContainer)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(Splitter)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(StatusStrip)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(TabControl)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(TableLayoutPanel)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(TextBox)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(System.Windows.Forms.Timer)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ToolStrip)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ToolStripContainer)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(ToolTip)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(TrackBar)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(TreeView)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(VScrollBar)));
            toolbox.AddToolboxItem(new ToolboxItem(typeof(WebBrowser)));
        }

		private void OnSelectionChanged(object sender, System.EventArgs e)
		{
			ISelectionService s = (ISelectionService)serviceContainer.GetService(typeof(ISelectionService));

			object[] selection;
			if (s.SelectionCount == 0)
				propertyGrid.SelectedObject = null;
			else
			{
				selection = new object[s.SelectionCount];
				s.GetSelectedComponents().CopyTo(selection, 0);
				propertyGrid.SelectedObjects = selection;
                selectedControls = selection.Cast<IComponent>().ToArray();
            }

			//if (s.PrimarySelection == null)
			//	//TODO: lblSelectedComponent.Text = "";
			//else
			//{
			//	IComponent component = (IComponent)s.PrimarySelection;

   //             //TODO: lblSelectedComponent.Text = component.Site.Name + " (" + component.GetType().Name + ")";
			//}
		}

		private void mnuDelete_Click(object sender, System.EventArgs e)
		{
			menuService.GlobalInvoke(StandardCommands.Delete);
		}

        public void Save(bool saveAs = false, EventGenerationType eventGenerationType = EventGenerationType.Variable)
        {
            try
            {
                var rs = serviceContainer.GetService(typeof(IResourceService)) as ResourceService;

                if (string.IsNullOrEmpty(this.codeFile) || saveAs)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = _language.FileFilter;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        rs._resourceFile = saveFileDialog.FileName.ToLower().Replace(".ps1", ".resources.ps1");
                        this.codeFile = saveFileDialog.FileName;
                        this.designerFile = saveFileDialog.FileName.ToLower().Replace(".ps1", ".designer.ps1");
                        host.FileName = this.designerFile;

                        CodeDomProvider codeDomProvider = null;
                        try
                        {
                            codeDomProvider = _language.GetCodeDomProvider(this.codeFile, this.designerFile, eventGenerationType);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                        }
                        this.host.CodeDomProvider = codeDomProvider;
                    }
                    else
                    {
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(formNewName))
                {
                    var contents = File.ReadAllText(codeFile);
                    var scriptBlock = ScriptBlock.Create(contents);

                    var variableExpressions = scriptBlock.Ast.FindAll(m => m is VariableExpressionAst variable && variable.VariablePath.UserPath.Equals(formOldName, StringComparison.OrdinalIgnoreCase), true);
                    var replacements = new Dictionary<Ast, string>();

                    foreach(var varexpression in variableExpressions)
                    {
                        replacements.Add(varexpression, "$" + formNewName);
                    }

                    var offset = 0;
                    var astContents = scriptBlock.Ast.ToString();
                    foreach (var replacement in replacements.OrderBy(m => m.Key.Extent.StartOffset))
                    {
                        var startOffset = replacement.Key.Extent.StartOffset + offset;
                        var endOffset = replacement.Key.Extent.EndOffset + offset;

                        astContents = astContents.Remove(startOffset, endOffset - startOffset);
                        astContents = astContents.Insert(startOffset, replacement.Value);
                        offset += replacement.Value.Length - (endOffset - startOffset);
                    }

                    File.WriteAllText(codeFile, astContents);
                    formNewName = null;
                    formOldName = null;
                }

                host.Save();
                rs.Generate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save form! " + ex.Message, "Failed to save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CopySelectedControl()
        {
            if (selectedControls == null || !selectedControls.Any()) return;
            ControlFactory.CopyCtrl2ClipBoard(selectedControls.First() as Control);
        }

        public void CutSelectedControl()
        {
            if (selectedControls == null || !selectedControls.Any()) return;
            ControlFactory.CopyCtrl2ClipBoard(selectedControls.First() as Control);
            host.Remove(selectedControls.First());
        }

        public void PasteControl()
        {
            var control = ControlFactory.GetCtrlFromClipBoard();
            if (control == null) return;
            var rootControl = host.RootComponent as Control;

            control.Top += 10;
            control.Left += 10;

            rootControl.Controls.Add(control);
            host.Add(control);
        }
    }
}
