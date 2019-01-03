using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace KeyboardHeatmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KeyboardListener KListener = new KeyboardListener();

        //Dictionary<int, KeyStroke> keyDict = new Dictionary<int, KeyStroke>();
        public ObservableCollection<KeyStroke> keyList = new ObservableCollection<KeyStroke>();
        int keyMax;
        Page currentLayout = new Page();

        LayoutAZERTY azerty = new LayoutAZERTY();
        LayoutENGB engb = new LayoutENGB();
        LayoutENUS enus = new LayoutENUS();
        public MainWindow()
        {
            InitializeComponent();
            // Find some better way to do all this ?
            lblInfo.Content = $"{Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            List<string> layoutList = new List<string>();
            List<string> keyCapture = new List<string>();
            currentLayout = azerty;
            layoutFrame.Content = currentLayout;
            layoutList.Add("AZERTY");
            layoutList.Add("QWERTY (en-GB)");
            layoutList.Add("QWERTY (en-US)");
            layoutList.Add("QWERTZ");
            cbLayout.SelectedValue = "AZERTY";
            cbLayout.ItemsSource = layoutList;
            keyCapture.Add("KeyUp");
            keyCapture.Add("KeyDown");
            cbKeyCapture.ItemsSource = keyCapture;
            cbKeyCapture.SelectedValue = "KeyUp";
            // Pretty ugly, huh ?

            var formLang = InputLanguageManager.Current.CurrentInputLanguage;
            if (formLang.ToString() == "fr-FR")
            {
                Console.WriteLine("Honhon bonjour");
            }
            else if (formLang.ToString() == "en-GB")
            {
                Console.WriteLine($"Aye lad !");
                cbLayout.SelectedValue = "QWERTY (en-GB)";
                currentLayout = engb;
                layoutFrame.Content = currentLayout;
            }
            else if (formLang.ToString() == "en-US")
            {
                Console.WriteLine($"Hello !");
                cbLayout.SelectedValue = "QWERTY (en-US)";
                currentLayout = enus;
                layoutFrame.Content = currentLayout;
            }

            // Init our keyList to make sure everything is at 0
            for (int i = 0; i <= 254; i++)
            {
                //keyDict.Add(i, new KeyStroke(i));
                keyList.Add(new KeyStroke(i));
            }

            // Here we set up our default listener on KeyUp
            KListener.KeyUp += new RawKeyEventHandler(KListener_KeyHandler);
            //KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);

            // Binding keyList display and filter
            keyListView.ItemsSource = keyList;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(keyListView.ItemsSource);
            view.Filter = UserFilter;

        }

        private bool UserFilter(object item)
        {
            return ((item as KeyStroke).NumPress >= 1); // Only show the values above 1 in the list
        }
        

        private void LayoutSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Layout switcher logic
            switch (cbLayout.SelectedItem.ToString())
            {
                case "AZERTY":
                    Console.WriteLine("Switching to AZERTY");
                    currentLayout = azerty;
                    lblWarning.Content = "You are not supposed to see this";
                    lblWarning.Visibility = Visibility.Hidden;
                    layoutFrame.Content = currentLayout;
                    break;
                case "QWERTY (en-GB)":
                    Console.WriteLine("Switching to QWERTY");
                    currentLayout = engb;
                    lblWarning.Content = "You are not supposed to see this";
                    lblWarning.Visibility = Visibility.Hidden;
                    layoutFrame.Content = currentLayout;
                    break;
                case "QWERTY (en-US)":
                    Console.WriteLine("Switching to QWERTY");
                    currentLayout = enus;
                    lblWarning.Content = "You are not supposed to see this";
                    lblWarning.Visibility = Visibility.Hidden;
                    layoutFrame.Content = currentLayout;
                    break;
                case "QWERTZ":
                    Console.WriteLine("Switching to QWERTZ");
                    lblWarning.Content = "Warning : QWERTZ is not available yet.";
                    lblWarning.Visibility = Visibility.Visible;
                    break;
            }
            layoutFrame.Focus();                // Focus elsewhere otherwise some keys change the bindings (A, Q, Arrow Up and down etc...)
            new Thread(() => UpdateKeymap());   // Update keymap to show the previsously typed keys
        }

        private void KListener_KeyHandler(object sender, RawKeyEventArgs args)
        {
            //Console.WriteLine(args.VKCode);
            keyList[args.VKCode].NumPress += 1;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(keyListView.ItemsSource);
            view.Filter = UserFilter;                   // gotta update everytime we press a key
            keyMax = keyList.Max(ke => ke.NumPress);    // Get maximum of key presses
            new Thread(() => UpdateKeymap());           // Update keymap again 
            //Console.WriteLine(args.ToString()); // Prints the text of pressed button, takes in account big and small letters. E.g. "Shift+a" => "A"
            //txtText.Text += args.ToString();
        }
        private int[] GetHeatMapColor(float value)
        {
            float[,] color = { { 0, 0, 255 }, { 0, 255, 0 }, { 255, 255, 0 }, { 255, 0, 0 } };
            int NUM_COLORS = color.Length/3;    // Number of colors in our array
            int idx1;                           // |-- Our desired color will be between these two indexes in "color".
            int idx2;                           // |
            float fractBetween = 0;             // Fraction between "idx1" and "idx2" where our value is.
            
            if (value <= 0) { idx1 = idx2 = 0; }                        // accounts for an input <=0
            else if (value >= 255) { idx1 = idx2 = NUM_COLORS - 1; }    // accounts for an input >=255
            else
            {
                value = value * (NUM_COLORS - 1);           // Will multiply value by 3.
                idx1 = (int)Math.Floor(value/255);          // Our desired color will be after this index.
                idx2 = idx1 + 1;                            // ... and before this index (inclusive).
                fractBetween = value/255 - (float)idx1;     // Distance between the two indexes (0-1).
            }
            // Color selector logic
            int red = (int)Math.Ceiling((color[idx2, 0] - color[idx1, 0]) * fractBetween + color[idx1, 0]);
            int green = (int)Math.Ceiling((color[idx2, 1] - color[idx1, 1]) * fractBetween + color[idx1, 1]);
            int blue = (int)Math.Ceiling((color[idx2, 2] - color[idx1, 2]) * fractBetween + color[idx1, 2]);
            int[] returned = { red, green, blue };
            return returned;
        }

        public void UpdateKeymap()
        {
            // The invoke is important to update the GUI. Otherwise it crashes.
            this.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i <= 254; i++)
                {
                    try
                    {
                        if (keyList[i].NumPress > 0) // Update only pressed keys to avoid blocking the GUI
                        {
                            var col = (int)Math.Ceiling(keyList[i].NumPress * (double)255 / keyMax);    // Get our key color between 0 and 255
                            int[] rgb = GetHeatMapColor(col);                                           // Generate RGB values for our color
                                try
                                {
                                    Label keyTile = (Label)currentLayout.FindName($"_{i}");     // Only way to get the label we want
                                    //Label keyTile = keyLbl as Label;
                                    keyTile.ToolTip = $"Key {keyList[i].Character}({i}) pressed {keyList[i].NumPress} times";
                                    keyTile.Background = new SolidColorBrush(Color.FromArgb(255, (byte)rgb[0], (byte)rgb[1], (byte)rgb[2]));
                                }
                                catch
                                { Console.WriteLine($"No key _{i} in view"); }
                        }
                    }
                    catch (Exception ex)
                    { Console.WriteLine($"Error : {ex}"); }
                }
            });
        }
        private void Btn_Reset(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i <= 254; i++)
            {
                if (keyList[i].NumPress > 0)
                {
                    keyList[i].NumPress = 0;
                    try
                    {
                        Label keyTile = (Label)currentLayout.FindName($"_{i}");
                        keyTile.ToolTip = $"Key {keyList[i].Character}({i}) pressed {keyList[i].NumPress} times";
                        //keyTile.Background = new SolidColorBrush(Color.FromArgb(255, (byte)col, (byte)(-col + 255), (byte)0));
                        keyTile.Background = new SolidColorBrush(Color.FromArgb(255, (byte)171, (byte)171, (byte)171));
                    }
                    catch
                    { Console.WriteLine($"No key _{i} in view"); }
                }
            }
            Console.WriteLine("Keypresses cleared");
        }

        private void CbLayout_DropDownClosed(object sender, EventArgs e)
        {
            if (cbLayout.IsDropDownOpen == false)
            { Console.WriteLine("Closed Selector"); }
        }

        private void CbKeyCapture_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                KListener.KeyUp -= KListener_KeyHandler;    // |-- Remove actual listeners
                KListener.KeyDown -= KListener_KeyHandler;  // | 
            }
            catch
            { throw; }
            try
            {
                if (cbKeyCapture.SelectedValue.ToString() == "KeyUp")
                { KListener.KeyUp += new RawKeyEventHandler(KListener_KeyHandler); }
                else
                { KListener.KeyDown += new RawKeyEventHandler(KListener_KeyHandler); }
                Console.WriteLine("Switched KListener");
            }
            catch (Exception ex)
            { Console.WriteLine(ex); }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KListener.Dispose();
        }


        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private void KeyListView_Click(object sender, RoutedEventArgs e)
        {
            // List View Order logic
            //var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (e.OriginalSource is GridViewColumnHeader)
            {
                var headerClicked = (GridViewColumnHeader)e.OriginalSource;
                ListSortDirection direction;
                if (headerClicked != null)
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != _lastHeaderClicked)
                        { direction = ListSortDirection.Ascending; }
                        else
                        {
                            if (_lastDirection == ListSortDirection.Ascending)
                            { direction = ListSortDirection.Descending; }
                            else
                            { direction = ListSortDirection.Ascending; }
                        }

                        var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                        var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                        Sort(sortBy, direction);

                        if (direction == ListSortDirection.Ascending)
                        { headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowUp"] as DataTemplate; }
                        else
                        { headerClicked.Column.HeaderTemplate = Resources["HeaderTemplateArrowDown"] as DataTemplate; }

                        // Remove arrow from previously sorted header  
                        if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                        { _lastHeaderClicked.Column.HeaderTemplate = null; }

                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
            
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(keyListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }

    public class KeyStroke : INotifyPropertyChanged
    {
        // KeyStroke class storing data about each key and how many types it received
        private int id;
        private int numPress;
        private string character;
        public KeyStroke(int id, int numPress)
        {
            Id = id;
            NumPress = numPress;
            Character = System.Windows.Input.KeyInterop.KeyFromVirtualKey(Id).ToString();
        }
        public KeyStroke(int id)
        {
            Id = id;
            NumPress = 0;
            Character = System.Windows.Input.KeyInterop.KeyFromVirtualKey(Id).ToString();
        }
        public int Id
        {
            get => id;
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }
        public int NumPress
        {
            get { return this.numPress; }
            set
            {
                this.numPress = value;
                NotifyPropertyChanged("NumPress");
            }
        }

        public string Character
        {
            get => character;
            set
            {
                character = value;
                NotifyPropertyChanged("Character");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class KeyboardListener : IDisposable
    {
        /// <summary>
        /// Creates global keyboard listener.
        /// </summary>
        public KeyboardListener()
        {
            // Dispatcher thread handling the KeyDown/KeyUp events.
            this.dispatcher = Dispatcher.CurrentDispatcher;

            // We have to store the LowLevelKeyboardProc, so that it is not garbage collected runtime
            hookedLowLevelKeyboardProc = (NativeMethods.LowLevelKeyboardProc)LowLevelKeyboardProc;

            // Set the hook
            hookId = NativeMethods.SetHook(hookedLowLevelKeyboardProc);

            // Assign the asynchronous callback event
            hookedKeyboardCallbackAsync = new KeyboardCallbackAsync(KeyboardListener_KeyboardCallbackAsync);
        }

        private Dispatcher dispatcher;

        /// <summary>
        /// Destroys global keyboard listener.
        /// </summary>
        ~KeyboardListener()
        {
            Dispose();
        }

        /// <summary>
        /// Fired when any of the keys is pressed down.
        /// </summary>
        public event RawKeyEventHandler KeyDown;

        /// <summary>
        /// Fired when any of the keys is released.
        /// </summary>
        public event RawKeyEventHandler KeyUp;

        #region Inner workings

        /// <summary>
        /// Hook ID
        /// </summary>
        private readonly IntPtr hookId = IntPtr.Zero;

        /// <summary>
        /// Asynchronous callback hook.
        /// </summary>
        /// <param name="character">Character</param>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        private delegate void KeyboardCallbackAsync(NativeMethods.KeyEvent keyEvent, int vkCode, string character);

        /// <summary>
        /// Actual callback hook.
        ///
        /// <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            string chars = "";
            //Console.WriteLine($"LowLevelKeyboardProc {nCode}");
            if (nCode >= 0)
                if (wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_KEYDOWN ||
                    wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_KEYUP ||
                    wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_SYSKEYDOWN ||
                    wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_SYSKEYUP)
                {
                    // Captures the character(s) pressed only on WM_KEYDOWN
                    chars = NativeMethods.VKCodeToString((uint)Marshal.ReadInt32(lParam),
                        (wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_KEYDOWN ||
                        wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_SYSKEYDOWN));

                    hookedKeyboardCallbackAsync.BeginInvoke((NativeMethods.KeyEvent)wParam.ToUInt32(), Marshal.ReadInt32(lParam), chars, null, null);
                }

            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Event to be invoked asynchronously (BeginInvoke) each time key is pressed.
        /// </summary>
        private KeyboardCallbackAsync hookedKeyboardCallbackAsync;

        /// <summary>
        /// Contains the hooked callback in runtime.
        /// </summary>
        private readonly NativeMethods.LowLevelKeyboardProc hookedLowLevelKeyboardProc;

        /// <summary>
        /// HookCallbackAsync procedure that calls accordingly the KeyDown or KeyUp events.
        /// </summary>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        /// <param name="character">Character as string.</param>
        private void KeyboardListener_KeyboardCallbackAsync(NativeMethods.KeyEvent keyEvent, int vkCode, string character)
        {
            //Console.WriteLine($"LowLevelKeyboardProc {vkCode} - {keyEvent}");

            switch (keyEvent)
            {
                // KeyDown events
                case NativeMethods.KeyEvent.WM_KEYDOWN:
                    if (KeyDown != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this, new RawKeyEventArgs(vkCode, false, character));
                    break;
                case NativeMethods.KeyEvent.WM_SYSKEYDOWN:
                    if (KeyDown != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this, new RawKeyEventArgs(vkCode, true, character));
                    break;

                // KeyUp events
                case NativeMethods.KeyEvent.WM_KEYUP:
                    if (KeyUp != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this, new RawKeyEventArgs(vkCode, false, character));
                    break;
                case NativeMethods.KeyEvent.WM_SYSKEYUP:
                    if (KeyUp != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this, new RawKeyEventArgs(vkCode, true, character));
                    break;

                default:
                    break;
            }
        }

        #endregion Inner workings

        #region IDisposable Members

        /// <summary>
        /// Disposes the hook.
        /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        public void Dispose()
        {
            NativeMethods.UnhookWindowsHookEx(hookId);
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// Raw KeyEvent arguments.
    /// </summary>
    public class RawKeyEventArgs : EventArgs
    {
        /// <summary>
        /// VKCode of the key.
        /// </summary>
        public int VKCode;

        /// <summary>
        /// WPF Key of the key.
        /// </summary>
        public Key Key;

        /// <summary>
        /// Is the hitted key system key.
        /// </summary>
        public bool IsSysKey;

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns>Returns string representation of this key, if not possible empty string is returned.</returns>
        public override string ToString()
        {
            return Key.ToString();
        }

        /// <summary>
        /// Unicode character of key pressed.
        /// </summary>
        public string Character;

        /// <summary>
        /// Create raw keyevent arguments.
        /// </summary>
        /// <param name="VKCode"></param>
        /// <param name="isSysKey"></param>
        /// <param name="Character">Character</param>
        public RawKeyEventArgs(int VKCode, bool isSysKey, string Character)
        {
            this.VKCode = VKCode;
            this.IsSysKey = isSysKey;
            this.Character = Character;
            this.Key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(VKCode);
        }
    }

    /// <summary>
    /// Raw keyevent handler.
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="args">raw keyevent arguments</param>
    public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

    #region WINAPI Helper class

    /// <summary>
    /// Winapi Key interception helper class.
    /// </summary>
    internal static class NativeMethods
    {
        public delegate IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam);
        public static int WH_KEYBOARD_LL = 13;

        /// <summary>
        /// Key event
        /// </summary>
        public enum KeyEvent : int
        {
            /// <summary>
            /// Key down
            /// </summary>
            WM_KEYDOWN = 256,

            /// <summary>
            /// Key up
            /// </summary>
            WM_KEYUP = 257,

            /// <summary>
            /// System key up
            /// </summary>
            WM_SYSKEYUP = 261,

            /// <summary>
            /// System key down
            /// </summary>
            WM_SYSKEYDOWN = 260
        }

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);




        #region Convert VKCode to string

        // Note: Sometimes single VKCode represents multiple chars, thus string.
        // E.g. typing "^1" (notice that when pressing 1 the both characters appear,
        // because of this behavior, "^" is called dead key)

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetKeyboardLayout(uint dwLayout);

        [DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private static uint lastVKCode = 0;
        private static uint lastScanCode = 0;
        private static byte[] lastKeyState = new byte[255];
        private static bool lastIsDead = false;

        /// <summary>
        /// Convert VKCode to Unicode.
        /// <remarks>isKeyDown is required for because of keyboard state inconsistencies!</remarks>
        /// </summary>
        /// <param name="VKCode">VKCode</param>
        /// <param name="isKeyDown">Is the key down event?</param>
        /// <returns>String representing single unicode character.</returns>
        public static string VKCodeToString(uint VKCode, bool isKeyDown)
        {
            // ToUnicodeEx needs StringBuilder, it populates that during execution.
            System.Text.StringBuilder sbString = new System.Text.StringBuilder(5);

            byte[] bKeyState = new byte[255];
            bool bKeyStateStatus;
            bool isDead = false;

            // Gets the current windows window handle, threadID, processID
            IntPtr currentHWnd = GetForegroundWindow();
            uint currentWindowThreadID = GetWindowThreadProcessId(currentHWnd, out uint currentProcessID);

            // This programs Thread ID
            uint thisProgramThreadId = GetCurrentThreadId();

            // Attach to active thread so we can get that keyboard state
            if (AttachThreadInput(thisProgramThreadId, currentWindowThreadID, true))
            {
                // Current state of the modifiers in keyboard
                bKeyStateStatus = GetKeyboardState(bKeyState);

                // Detach
                AttachThreadInput(thisProgramThreadId, currentWindowThreadID, false);
            }
            else
            {
                // Could not attach, perhaps it is this process?
                bKeyStateStatus = GetKeyboardState(bKeyState);
            }

            // On failure we return empty string.
            if (!bKeyStateStatus)
                return "";

            // Gets the layout of keyboard
            IntPtr HKL = GetKeyboardLayout(currentWindowThreadID);

            // Maps the virtual keycode
            uint lScanCode = MapVirtualKeyEx(VKCode, 0, HKL);

            // Keyboard state goes inconsistent if this is not in place. In other words, we need to call above commands in UP events also.
            if (!isKeyDown)
                return "";

            // Converts the VKCode to unicode
            int relevantKeyCountInBuffer = ToUnicodeEx(VKCode, lScanCode, bKeyState, sbString, sbString.Capacity, (uint)0, HKL);

            string ret = "";

            switch (relevantKeyCountInBuffer)
            {
                // Dead keys (^,`...)
                case -1:
                    isDead = true;

                    // We must clear the buffer because ToUnicodeEx messed it up, see below.
                    ClearKeyboardBuffer(VKCode, lScanCode, HKL);
                    break;

                case 0:
                    break;

                // Single character in buffer
                case 1:
                    ret = sbString[0].ToString();
                    break;

                // Two or more (only two of them is relevant)
                case 2:
                default:
                    ret = sbString.ToString().Substring(0, 2);
                    break;
            }

            // We inject the last dead key back, since ToUnicodeEx removed it.
            // More about this peculiar behavior see e.g:
            //   http://www.experts-exchange.com/Programming/System/Windows__Programming/Q_23453780.html
            //   http://blogs.msdn.com/michkap/archive/2005/01/19/355870.aspx
            //   http://blogs.msdn.com/michkap/archive/2007/10/27/5717859.aspx
            if (lastVKCode != 0 && lastIsDead)
            {
                System.Text.StringBuilder sbTemp = new System.Text.StringBuilder(5);
                ToUnicodeEx(lastVKCode, lastScanCode, lastKeyState, sbTemp, sbTemp.Capacity, (uint)0, HKL);
                lastVKCode = 0;

                return ret;
            }

            // Save these
            lastScanCode = lScanCode;
            lastVKCode = VKCode;
            lastIsDead = isDead;
            lastKeyState = (byte[])bKeyState.Clone();

            return ret;
        }

        private static void ClearKeyboardBuffer(uint vk, uint sc, IntPtr hkl)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(10);

            int rc;
            do
            {
                byte[] lpKeyStateNull = new Byte[255];
                rc = ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }

        #endregion Convert VKCode to string
    }

    #endregion WINAPI Helper class
}
