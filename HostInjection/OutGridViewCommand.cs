using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;
using Newtonsoft.Json;

namespace PowerShellToolsPro.Cmdlets.VSCode 
{
    /// Enum for SelectionMode parameter.
    /// </summary>
    [Cmdlet(VerbsData.Out, "VSCodeGridView", DefaultParameterSetName = "PassThru")]
    public class OutGridViewCmdletCommand : VSCodeCmdlet
    {
        #region Properties

        private const string DataNotQualifiedForGridView = "DataNotQualifiedForGridView";

        private List<PSObject> PSObjects = new List<PSObject>();

        #endregion Properties

        #region Input Parameters

        [Parameter(ValueFromPipeline = true)]
        public PSObject InputObject { get; set; } = AutomationNull.Value;
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; }
        [Parameter(ParameterSetName = "OutputMode")]
        public OutputModeOption OutputMode { set; get; }
        [Parameter(ParameterSetName = "PassThru")]
        public SwitchParameter PassThru
        {
            set { this.OutputMode = value.IsPresent ? OutputModeOption.Multiple : OutputModeOption.None; }

            get { return OutputMode == OutputModeOption.Multiple ? new SwitchParameter(true) : new SwitchParameter(false); }
        }

        #endregion Input Parameters

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            base.ResponseTimeout = int.MaxValue;
            if (InputObject == null || InputObject == AutomationNull.Value)
            {
                return;
            }

            IDictionary dictionary = InputObject.BaseObject as IDictionary;
            if (dictionary != null)
            {
                // Dictionaries should be enumerated through because the pipeline does not enumerate through them.
                foreach (DictionaryEntry entry in dictionary)
                {
                    ProcessObject(PSObject.AsPSObject(entry));
                }
            }
            else
            {
                ProcessObject(InputObject);
            }
        }
        private void ProcessObject(PSObject input)
        {

            object baseObject = input.BaseObject;

            // Throw a terminating error for types that are not supported.
            if (baseObject is ScriptBlock ||
                baseObject is SwitchParameter ||
                baseObject is PSReference ||
                baseObject is PSObject)
            {
                ErrorRecord error = new ErrorRecord(
                    new FormatException("Invalid data type for Out-GridView"),
                    DataNotQualifiedForGridView,
                    ErrorCategory.InvalidType,
                    null);

                this.ThrowTerminatingError(error);
            }

            PSObjects.Add(input);
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            base.EndProcessing();

            //Return if no objects
            if (PSObjects.Count == 0)
            {
                return;
            }

            var TG = new TypeGetter(this);

            var dataTable = TG.CastObjectsToTableView(PSObjects);
            var applicationData = new ApplicationData
            {
                Title = Title,
                OutputMode = OutputMode,
                PassThru = PassThru,
                DataTable = dataTable
            };

            var selectedIndexesString = SendCommand("Out-GridView", applicationData);

            if (selectedIndexesString == null)
                return;

            var selectedIndexes = JsonConvert.DeserializeObject<int[]>(selectedIndexesString);

            foreach (int idx in selectedIndexes)
            {
                var selectedObject = PSObjects[idx];
                if (selectedObject == null)
                {
                    continue;
                }
                this.WriteObject(selectedObject, false);
            }
        }
    }
}