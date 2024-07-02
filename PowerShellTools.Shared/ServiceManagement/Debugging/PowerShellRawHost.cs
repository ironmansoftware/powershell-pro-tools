using System;
using System.Management.Automation.Host;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Management.Automation;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{

    public class ConsoleRawUI : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor
        {
            get { return System.Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }
        public override Size BufferSize
        {
            get
            {
                return new Size(Console.BufferWidth, Console.BufferHeight);
            }
            set
            {
                Console.SetBufferSize(value.Width, value.Height);
            }
        }
        public override Coordinates CursorPosition
        {
            get { return new Coordinates(Console.CursorLeft, Console.CursorTop); }
            set
            {
                Console.SetCursorPosition(value.X < 0 ? 0 : value.X,
                                          value.Y < 0 ? 0 : value.Y);
            }
        }
        public override int CursorSize
        {
            get { return Console.CursorSize; }
            set { Console.CursorSize = value; }
        }
        public override ConsoleColor ForegroundColor
        {
            get { return System.Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public override bool KeyAvailable => Console.KeyAvailable;

        public override Size MaxPhysicalWindowSize => new Size(Console.WindowWidth, Console.WindowHeight);

        public override Size MaxWindowSize => new Size(Console.WindowWidth, Console.WindowHeight);

        public override Coordinates WindowPosition
        {
            get { return new Coordinates(Console.WindowLeft, Console.WindowTop); }
            set { Console.SetWindowPosition(value.X, value.Y); }
        }
        public override Size WindowSize
        {
            get { return new Size(Console.WindowWidth, Console.WindowHeight); }
            set { Console.SetWindowSize(value.Width, value.Height); }
        }
        public override string WindowTitle
        {
            get { return Console.Title; }
            set { Console.Title = value; }
        }

        public override void FlushInputBuffer()
        {
            if (!Console.IsInputRedirected)
            {
                Console.OpenStandardInput().Flush();
            }
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            ConsoleKeyInfo key = Console.ReadKey((options & ReadKeyOptions.NoEcho) != 0);
            return new KeyInfo((int)key.Key, key.KeyChar, new ControlKeyStates(), true);
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public void ScrollBuffer(int lines)
        {
            for (int i = 0; i < lines; ++i)
            {
                Console.Out.Write('\n');
            }
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            // if there are no contents, there is nothing to set the buffer to
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
            // if the cursor is on the last line, we need to make more space to print the specified buffer
            if (origin.Y == BufferSize.Height - 1 && origin.X >= BufferSize.Width)
            {
                // for each row in the buffer, create a new line
                int rows = contents.GetLength(0);
                ScrollBuffer(rows);
                // for each row in the buffer, move the cursor y up to the beginning of the created blank space
                // but not above zero
                if (origin.Y >= rows)
                {
                    origin.Y -= rows;
                }
            }

            // iterate through the buffer to set
            foreach (var charitem in contents)
            {
                // set the cursor to false to prevent cursor flicker
                Console.CursorVisible = false;

                // if x is exceeding buffer width, reset to the next line
                if (origin.X >= BufferSize.Width)
                {
                    origin.X = 0;
                }

                // write the character from contents
                Console.Out.Write(charitem.Character);
            }

            // reset the cursor to the original position
            CursorPosition = origin;
            // reset the cursor to visible
            Console.CursorVisible = true;
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            Console.Clear();
        }
    }

    /// <summary>
    /// Implementation of the PSHostRawUserInterface.
    /// </summary>
    internal sealed class PowerShellRawHost : PSHostRawUserInterface
    {
        private readonly PowerShellDebuggingService _debuggingService;

        public PowerShellRawHost(PowerShellDebuggingService debugger)
        {
            _debuggingService = debugger;
        }

        /// <summary>
        /// Gets or sets the foreground color of the displayed text.
        /// </summary>
        public override ConsoleColor ForegroundColor
        {
            get
            {
                return _debuggingService.RawHostOptions.ForegroundColor;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the background color of the displayed text.
        /// </summary>
        public override ConsoleColor BackgroundColor
        {
            get
            {
                return _debuggingService.RawHostOptions.BackgroundColor;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the cursor position. 
        /// </summary>
        public override Coordinates CursorPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the displayed cursor. 
        /// </summary>
        public override int CursorSize
        {
            get
            {
                return _debuggingService.RawHostOptions.CursorSize;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the size of the host buffer.
        /// </summary>
        public override Size BufferSize
        {
            get
            {
                var result = _debuggingService.CallbackService.GetREPLWindowWidth();
                return new Size(result, 0);
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the position of the displayed window. 
        /// </summary>
        public override Coordinates WindowPosition
        {
            get
            {
                return _debuggingService.RawHostOptions.WindowPosition;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the size of the displayed window. 
        /// </summary>
        public override Size WindowSize
        {
            get
            {
                var result = _debuggingService.CallbackService.GetREPLWindowWidth();
                return new Size(result, 0);
            }
            set { }
        }

        /// <summary>
        /// Gets the dimentions of the largest window size that can be displayed.
        /// </summary>
        public override Size MaxWindowSize
        {
            get
            {
                var result = _debuggingService.CallbackService.GetREPLWindowWidth();
                return new Size(result, 0);
            }
        }

        /// <summary>
        /// Gets the dimensions of the largest window that could be 
        /// rendered in the current display, if the buffer was at the least that large.
        /// </summary>
        public override Size MaxPhysicalWindowSize
        {
            get
            {
                var result = _debuggingService.CallbackService.GetREPLWindowWidth();
                return new Size(result, 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has pressed a key. 
        /// </summary>
        public override bool KeyAvailable
        {
            get
            {
                return _debuggingService.CallbackService.IsKeyAvailable();
            }
        }

        /// <summary>
        /// Gets or sets the title of the displayed window.
        /// </summary>
        public override string WindowTitle
        {
            get
            {
                return _debuggingService.RawHostOptions.WindowTitle;
            }
            set { }
        }

        /// <summary>
        /// This API resets the input buffer. 
        /// </summary>
        public override void FlushInputBuffer()
        {
            throw new NotImplementedException(
                     "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API returns a rectangular region of the screen buffer.
        /// </summary>
        /// <param name="rectangle">Defines the size of the rectangle.</param>
        /// <returns>Throws a NotImplementedException exception.</returns>
        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException(
                     "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API reads a pressed, released, or pressed and released keystroke 
        /// from the keyboard device, blocking processing until a keystroke is 
        /// typed that matches the specified keystroke options. 
        /// </summary>
        /// <param name="options">Options, such as IncludeKeyDown,  used when reading the keyboard.</param>
        /// <returns>KeyInfo of the key pressed</returns>
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            VsKeyInfo keyInfo = _debuggingService.CallbackService.VsReadKey();

            if (keyInfo == null)
            {
                // abort current pipeline
                throw new PipelineStoppedException();
            }

            ControlKeyStates states = default(ControlKeyStates);
            states |= (keyInfo.CapsLockToggled ? ControlKeyStates.CapsLockOn : 0);
            states |= (keyInfo.NumLockToggled ? ControlKeyStates.NumLockOn : 0);
            states |= (keyInfo.ShiftPressed ? ControlKeyStates.ShiftPressed : 0);
            states |= (keyInfo.AltPressed ? ControlKeyStates.LeftAltPressed : 0); // assume LEFT alt
            states |= (keyInfo.ControlPressed ? ControlKeyStates.LeftCtrlPressed : 0); // assume LEFT ctrl

            return new KeyInfo(keyInfo.VirtualKey, keyInfo.KeyChar, states, keyDown: (keyInfo.KeyStates == KeyStates.Down));
        }

        /// <summary>
        /// This API crops a region of the screen buffer.
        /// </summary>
        /// <param name="source">The region of the screen to be scrolled.</param>
        /// <param name="destination">The region of the screen to receive the 
        /// source region contents.</param>
        /// <param name="clip">The region of the screen to include in the operation.</param>
        /// <param name="fill">The character and attributes to be used to fill all cell.</param>
        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        /// <summary>
        /// This method copies an array of buffer cells into the screen buffer 
        /// at a specified location.
        /// </summary>
        /// <param name="origin">The parameter is not used.</param>
        /// <param name="contents">The parameter is not used.</param>
        public override void SetBufferContents(Coordinates origin,
                                               BufferCell[,] contents)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        /// <summary>
        /// This method copies a given character, foreground color, and background 
        /// color to a region of the screen buffer. 
        /// </summary>
        /// <param name="rectangle">Defines the area to be filled. </param>
        /// <param name="fill">Defines the fill character.</param>
        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            // Reference: get-command clear-host | Select-Object -expand Definition
            if (rectangle.Top == -1 &&
                rectangle.Bottom == -1 &&
                rectangle.Left == -1 &&
                rectangle.Right == -1 &&
                fill.Character == ' ')
            {
                _debuggingService.CallbackService.ClearHostScreen();
            }
            else
            {
                throw new NotImplementedException(
                      "The method or operation is not implemented.");
            }
        }
    }
}
