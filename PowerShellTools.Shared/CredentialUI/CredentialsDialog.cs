using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerShellTools.CredentialUI
{
    /// <summary>Encapsulates dialog functionality from the Credential Management API.</summary>
    public sealed class CredentialsDialog
    {
        private bool _alwaysDisplay = false;
        private bool _excludeCertificates = true;
        private bool _persist = true;
        private bool _keepName = false;
        private string _name = String.Empty;
        private SecureString _password = new SecureString();
        private bool _saveChecked = false;
        private string _target = String.Empty;
        private string _caption = String.Empty;
        private string _message = String.Empty;
        private bool _validName = false;

        /// <summary>Initializes a new instance of the <see cref="T:SecureCredentialsLibrary.CredentialsDialog"/> class
        /// with the specified target.</summary>
        /// <param name="target">The name of the target for the credentials, typically a server name.</param>
        public CredentialsDialog(string target)
            : this(target, null)
        { }
        /// <summary>Initializes a new instance of the <see cref="T:SecureCredentialsLibrary.CredentialsDialog"/> class
        /// with the specified target and caption.</summary>
        /// <param name="target">The name of the target for the credentials, typically a server name.</param>
        /// <param name="caption">The caption of the dialog (null will cause a system default title to be used).</param>
        public CredentialsDialog(string target, string caption)
            : this(target, caption, null)
        { }

        /// <summary>Initializes a new instance of the <see cref="T:SecureCredentialsLibrary.CredentialsDialog"/> class
        /// with the specified target, caption, message and banner.</summary>
        /// <param name="target">The name of the target for the credentials, typically a server name.</param>
        /// <param name="caption">The caption of the dialog (null will cause a system default title to be used).</param>
        /// <param name="message">The message of the dialog (null will cause a system default message to be used).</param>
        /// <param name="banner">The image to display on the dialog (null will cause a system default image to be used).</param>
        public CredentialsDialog(string target, string caption, string message)
        {
            this.Target = target;
            this.Caption = caption;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets if the dialog will be shown even if the credentials
        /// can be returned from an existing credential in the credential manager.
        /// </summary>
        public bool AlwaysDisplay
        {
            get
            {
                return _alwaysDisplay;
            }
            set
            {
                _alwaysDisplay = value;
            }
        }

        /// <summary>
        /// Gets or sets if the dialog is populated with name/password only.
        /// </summary>
        public bool ExcludeCertificates
        {
            get
            {
                return _excludeCertificates;
            }
            set
            {
                _excludeCertificates = value;
            }
        }

        /// <summary>
        /// Gets or sets if the credentials are to be persisted in the credential manager.
        /// </summary>
        public bool Persist
        {
            get
            {
                return _persist;
            }
            set
            {
                _persist = value;
            }
        }

        /// <summary>
        /// Gets or sets if the name is read-only.
        /// </summary>
        public bool KeepName
        {
            get
            {
                return _keepName;
            }
            set
            {
                _keepName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name for the credentials.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > NativeCredentialsUI.MAX_USERNAME_LENGTH)
                    {
                        string message = String.Format(
                            Thread.CurrentThread.CurrentUICulture,
                            "The name has a maximum length of {0} characters.",
                            NativeCredentialsUI.MAX_USERNAME_LENGTH);
                        throw new ArgumentException(message, "Name");
                    }
                }
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the password for the credentials.
        /// </summary>
        public SecureString Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > NativeCredentialsUI.MAX_PASSWORD_LENGTH)
                    {
                        string message = String.Format(
                            Thread.CurrentThread.CurrentUICulture,
                            "The password has a maximum length of {0} characters.",
                            NativeCredentialsUI.MAX_PASSWORD_LENGTH);
                        throw new ArgumentException(message, "Password");
                    }
                }
                _password = value;
            }
        }

        /// <summary>
        /// Gets or sets if the save checkbox status.
        /// </summary>
        public bool SaveChecked
        {
            get
            {
                return _saveChecked;
            }
            set
            {
                _saveChecked = value;
            }
        }

        /// <summary>
        /// Gets or sets if validate the user name.
        /// </summary>
        public bool ValidName
        {
            get
            {
                return _validName;
            }
            set
            {
                _validName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the target for the credentials, typically a server name.
        /// </summary>
        public string Target
        {
            get
            {
                return _target == String.Empty ? "target" : _target;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("The target cannot be a null value.", "Target");
                }
                else if (value.Length > NativeCredentialsUI.MAX_GENERIC_TARGET_LENGTH)
                {
                    string message = String.Format(
                        Thread.CurrentThread.CurrentUICulture,
                        "The target has a maximum length of {0} characters.",
                        NativeCredentialsUI.MAX_GENERIC_TARGET_LENGTH);
                    throw new ArgumentException(message, "Target");
                }
                _target = value;
            }
        }

        
        /// <summary>
        /// Gets or sets the caption of the dialog.
        /// </summary>
        /// <remarks>
        /// A null value will cause a system default caption to be used.
        /// </remarks>
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > NativeCredentialsUI.MAX_CAPTION_LENGTH)
                    {
                        string message = String.Format(
                            Thread.CurrentThread.CurrentUICulture,
                            "The caption has a maximum length of {0} characters.",
                            NativeCredentialsUI.MAX_CAPTION_LENGTH);
                        throw new ArgumentException(message, "Caption");
                    }
                }
                _caption = value;
            }
        }


        /// <summary>
        /// Gets or sets the message of the dialog.
        /// </summary>
        /// <remarks>
        /// A null value will cause a system default message to be used.
        /// </remarks>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > NativeCredentialsUI.MAX_MESSAGE_LENGTH)
                    {
                        value = value.Substring(0, NativeCredentialsUI.MAX_MESSAGE_LENGTH);
                        //string message = String.Format(
                        //    Thread.CurrentThread.CurrentUICulture,
                        //    "The message has a maximum length of {0} characters.",
                        //    NativeCredentialsUI.MAX_MESSAGE_LENGTH);
                        //throw new ArgumentException(message, "Message");
                    }
                }
                _message = value;
            }
        }

        /// <summary>
        /// Shows the credentials dialog.
        /// </summary>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show()
        {
            return Show(null, this.Name, this.Password, this.SaveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified save checkbox status.
        /// </summary>
        /// <param name="saveChecked">True if the save checkbox is checked.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(bool saveChecked)
        {
            return Show(null, this.Name, this.Password, saveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified name.
        /// </summary>
        /// <param name="name">The name for the credentials.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(string name)
        {
            return Show(null, name, this.Password, this.SaveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified name and password.
        /// </summary>
        /// <param name="name">The name for the credentials.</param>
        /// <param name="password">The password for the credentials.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(string name, SecureString password)
        {
            return Show(null, name, password, this.SaveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified name, password and save checkbox status.
        /// </summary>
        /// <param name="name">The name for the credentials.</param>
        /// <param name="password">The password for the credentials.</param>
        /// <param name="saveChecked">True if the save checkbox is checked.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(string name, SecureString password, bool saveChecked)
        {
            return Show(null, name, password, saveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified owner.
        /// </summary>
        /// <param name="owner">The System.Windows.Forms.IWin32Window the dialog will display in front of.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(IWin32Window owner)
        {
            return Show(owner, this.Name, this.Password, this.SaveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified owner and save checkbox status.
        /// </summary>
        /// <param name="owner">The System.Windows.Forms.IWin32Window the dialog will display in front of.</param>
        /// <param name="saveChecked">True if the save checkbox is checked.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(IWin32Window owner, bool saveChecked)
        {
            return Show(owner, this.Name, this.Password, saveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified owner, name and password.
        /// </summary>
        /// <param name="owner">The System.Windows.Forms.IWin32Window the dialog will display in front of.</param>
        /// <param name="name">The name for the credentials.</param>
        /// <param name="password">The password for the credentials.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(IWin32Window owner, string name, SecureString password)
        {
            return Show(owner, name, password, this.SaveChecked);
        }

        /// <summary>
        /// Shows the credentials dialog with the specified owner, name, password and save checkbox status.
        /// </summary>
        /// <param name="owner">The System.Windows.Forms.IWin32Window the dialog will display in front of.</param>
        /// <param name="name">The name for the credentials.</param>
        /// <param name="password">The password for the credentials.</param>
        /// <param name="saveChecked">True if the save checkbox is checked.</param>
        /// <returns>Returns a DialogResult indicating the user action.</returns>
        public DialogResult Show(IWin32Window owner, string name, SecureString password, bool saveChecked)
        {
            if (Environment.OSVersion.Version.Major < 5)
            {
                throw new ApplicationException("The Credential Management API requires Windows XP / Windows Server 2003 or later.");
            }
            this.Name = name;
            this.Password = password;
            this.SaveChecked = saveChecked;

            return ShowDialog(owner);
        }

        /// <summary>
        /// Returns a DialogResult indicating the user action.
        /// </summary>
        /// <param name="owner">The System.Windows.Forms.IWin32Window the dialog will display in front of.</param>
        /// <remarks>
        /// Sets the name, password and SaveChecked accessors to the state of the dialog as it was dismissed by the user.
        /// </remarks>
        private DialogResult ShowDialog(IWin32Window owner)
        {
            // set the api call parameters
            StringBuilder name = new StringBuilder(NativeCredentialsUI.MAX_USERNAME_LENGTH);
            name.Append(this.Name);

            StringBuilder password = new StringBuilder(NativeCredentialsUI.MAX_PASSWORD_LENGTH);

            int saveChecked = Convert.ToInt32(this.SaveChecked);

            NativeCredentialsUI.INFO info = GetInfo(owner);
            NativeCredentialsUI.FLAGS flags = GetFlags();

            // make the api call
            NativeCredentialsUI.ReturnCodes code = NativeCredentialsUI.PromptForCredentials(
                ref info,
                this.Target,
                IntPtr.Zero, 0,
                name, NativeCredentialsUI.MAX_USERNAME_LENGTH,
                password, NativeCredentialsUI.MAX_PASSWORD_LENGTH,
                ref saveChecked,
                flags
                );

            // set the accessors from the api call parameters
            this.Name = name.ToString();
            foreach (char c in password.ToString())
            {
                this.Password.AppendChar(c);
            }
            this.Password.MakeReadOnly();
            this.SaveChecked = Convert.ToBoolean(saveChecked);

            return GetDialogResult(code);
        }

        /// <summary>
        /// Returns the info structure for dialog display settings.
        /// </summary>
        /// <param name="owner">The System.Windows.Forms.IWin32Window the dialog will display in front of.</param>
        private NativeCredentialsUI.INFO GetInfo(IWin32Window owner)
        {
            NativeCredentialsUI.INFO info = new NativeCredentialsUI.INFO();
            if (owner != null) info.hwndParent = owner.Handle;
            info.pszCaptionText = this.Caption;
            info.pszMessageText = this.Message;
            info.cbSize = Marshal.SizeOf(info);
            return info;
        }

        /// <summary>
        /// Returns the flags for dialog display options.
        /// </summary>
        private NativeCredentialsUI.FLAGS GetFlags()
        {
            NativeCredentialsUI.FLAGS flags = NativeCredentialsUI.FLAGS.GENERIC_CREDENTIALS;

            if (this.AlwaysDisplay) flags = flags | NativeCredentialsUI.FLAGS.ALWAYS_SHOW_UI;

            if (this.ExcludeCertificates) flags = flags | NativeCredentialsUI.FLAGS.EXCLUDE_CERTIFICATES;

            if (this.Persist)
            {
                flags = flags | NativeCredentialsUI.FLAGS.PERSIST;
                flags = flags | NativeCredentialsUI.FLAGS.EXPECT_CONFIRMATION;
            }
            else
            {
                flags = flags | NativeCredentialsUI.FLAGS.DO_NOT_PERSIST;
            }

            if (this.KeepName) flags = flags | NativeCredentialsUI.FLAGS.KEEP_USERNAME;

            if (this.ValidName) flags = flags | NativeCredentialsUI.FLAGS.VALIDATE_USERNAME;

            return flags;
        }

        /// <summary>
        /// Returns a DialogResult from the specified code.
        /// </summary>
        /// <param name="code">The credential return code.</param>
        private DialogResult GetDialogResult(NativeCredentialsUI.ReturnCodes code)
        {
            DialogResult result;
            switch (code)
            {
                case NativeCredentialsUI.ReturnCodes.NO_ERROR:
                    result = DialogResult.OK;
                    break;

                case NativeCredentialsUI.ReturnCodes.ERROR_CANCELLED:
                    result = DialogResult.Cancel;
                    break;

                case NativeCredentialsUI.ReturnCodes.ERROR_NO_SUCH_LOGON_SESSION:
                    throw new ApplicationException("No such logon session.");

                case NativeCredentialsUI.ReturnCodes.ERROR_NOT_FOUND:
                    throw new ApplicationException("Not found.");

                case NativeCredentialsUI.ReturnCodes.ERROR_INVALID_ACCOUNT_NAME:
                    throw new ApplicationException("Invalid account name.");

                case NativeCredentialsUI.ReturnCodes.ERROR_INSUFFICIENT_BUFFER:
                    throw new ApplicationException("Insufficient buffer.");

                case NativeCredentialsUI.ReturnCodes.ERROR_INVALID_PARAMETER:
                    throw new ApplicationException("Invalid parameter.");

                case NativeCredentialsUI.ReturnCodes.ERROR_INVALID_FLAGS:
                    throw new ApplicationException("Invalid flags.");

                default:
                    throw new ApplicationException("Unknown credential result encountered.");
            }
            return result;
        }
    }
}
