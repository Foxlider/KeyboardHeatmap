using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace KeyboardHeatmapUE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        KeyboardListener KListener = new KeyboardListener();
        
        public ObservableCollection<KeyStroke> keyList = new ObservableCollection<KeyStroke>();
        uint keyMax;
        bool honhon = true;
        Thread honhonT;
        Thread huhuT;
        Page currentLayout = new Page();
        LayoutAZERTY azerty = null;
        LayoutENGB engb = null;
        LayoutENUS enus = null;

        public MainWindow()
        {
            InitializeComponent();
            // Find some better way to do all this ?
            lblInfo.Content = $"{Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version.ToString()} :)";
            List<string> layoutList = new List<string>();
            List<string> keyCapture = new List<string>();
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
                if (azerty == null)
                    azerty = new LayoutAZERTY();
                currentLayout = azerty;
                Console.WriteLine("Honhon bonjour");
            }
            else if (formLang.ToString() == "en-GB")
            {
                Console.WriteLine($"Aye lad !");
                cbLayout.SelectedValue = "QWERTY (en-GB)";
                if (engb == null)
                    engb = new LayoutENGB();
                currentLayout = engb;
            }
            else if (formLang.ToString() == "en-US")
            {
                Console.WriteLine($"Hello !");
                cbLayout.SelectedValue = "QWERTY (en-US)";
                if (enus == null)
                    enus = new LayoutENUS();
                currentLayout = enus;
            }
            layoutFrame.Content = currentLayout;
            // Init our keyList to make sure everything is at 0
            for (byte i = 0; i <= 254; i++)
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
            honhonT = new Thread(() => HonhonThread())
            {
                Name = "Honhonhon !"
            };
            honhonT.Start();
            huhuT = new Thread(() => HuhuThread())
            {
                Name = "Huhu !"
            };
            huhuT.Start();
        }

        private void HonhonThread()
        {
            while (true)
            {
                if (honhon)
                {
                    Random r = new Random();
                    int rMvX = r.Next(-(int)keyMax, (int)keyMax) / 25;
                    int rMvY = r.Next(-(int)keyMax, (int)keyMax) / 25;
                    NativeMethods.Move(rMvX, rMvY);
                    //Console.WriteLine($"Moving mouse {rMvX}px {rMvY}px");
                }
                Thread.Sleep(100);
            }
        }


        private void HuhuThread()
        {
            while (true)
            {
                Random r = new Random();
                int rWait = r.Next(5000, 120000);
                int width = Screen.PrimaryScreen.WorkingArea.Width;
                int height = Screen.PrimaryScreen.WorkingArea.Height;
                //rWait = 5000;
                Console.WriteLine($"Next ping in {rWait}ms");
                Thread.Sleep(rWait);
                if (honhon)
                {
                    Point mousepos = NativeMethods.MousePos();
                    Console.WriteLine($"Moving to {width},{height}px");
                    NativeMethods.MoveTo(width, height);
                    NativeMethods.LeftClick();
                    Console.WriteLine($"Moving back to {mousepos.X},{mousepos.Y}px");
                    NativeMethods.MoveTo((int)mousepos.X, (int)mousepos.Y);
                }
            }
        }

        private bool UserFilter(object item)
        {
            return ((item as KeyStroke).NumPress >= 1); // Only show the values above 1 in the list
        }
        

        private void LayoutSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Layout switcher logic
            if (currentLayout is LayoutAZERTY)
                azerty = null;
            else if (currentLayout is LayoutENGB)
                engb = null;
            else if (currentLayout is LayoutENUS)
                enus = null;
            switch (cbLayout.SelectedItem.ToString())
            {
                case "AZERTY":
                    Console.WriteLine("Switching to AZERTY");
                    if (azerty == null)
                        azerty = new LayoutAZERTY();
                    currentLayout = azerty;
                    lblWarning.Content = "You are not supposed to see this";
                    lblWarning.Visibility = Visibility.Hidden;
                    layoutFrame.Content = currentLayout;
                    break;
                case "QWERTY (en-GB)":
                    Console.WriteLine("Switching to QWERTY");
                    if (engb == null)
                        engb = new LayoutENGB();
                    currentLayout = engb;
                    lblWarning.Content = "You are not supposed to see this";
                    lblWarning.Visibility = Visibility.Hidden;
                    layoutFrame.Content = currentLayout;
                    break;
                case "QWERTY (en-US)":
                    Console.WriteLine("Switching to QWERTY");
                    if (enus == null)
                        enus = new LayoutENUS();
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
            layoutFrame.Focus();                        // Focus elsewhere otherwise some keys change the bindings (A, Q, Arrow Up and down etc...)
            Keyboard.ClearFocus();                      // And clear all focus hopefully
            new Thread(() => UpdateKeymap()).Start();   // Update keymap to show the previsously typed keys
        }

        private void KListener_KeyHandler(object sender, RawKeyEventArgs args)
        {
            //Console.WriteLine(args.VKCode);
            keyList[args.VKCode].NumPress += 1;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(keyListView.ItemsSource);
            view.Filter = UserFilter;                   // gotta update everytime we press a key
            keyMax = keyList.Max(ke => ke.NumPress);    // Get maximum of key presses
            new Thread(() => UpdateKeymap()).Start();   // Update keymap again 
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
                                    System.Windows.Controls.Label keyTile = (System.Windows.Controls.Label)currentLayout.FindName($"_{i}");     // Only way to get the label we want
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
            if (honhon)
            {
                honhon = false;
                lblInfo.Content = $"{Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version.ToString()} :(";
            }
            else
            {
                honhon = true;
                lblInfo.Content = $"{Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version.ToString()} :)";
            }

            for (int i = 0; i <= 254; i++)
            {
                if (keyList[i].NumPress > 0)
                {
                    keyList[i].NumPress = 0;
                    try
                    {
                        System.Windows.Controls.Label keyTile = (System.Windows.Controls.Label)currentLayout.FindName($"_{i}");
                        keyTile.ToolTip = $"Key {keyList[i].Character}({i}) pressed {keyList[i].NumPress} times";
                        //keyTile.Background = new SolidColorBrush(Color.FromArgb(255, (byte)col, (byte)(-col + 255), (byte)0));
                        keyTile.Background = new SolidColorBrush(Color.FromArgb(255, (byte)171, (byte)171, (byte)171));
                    }
                    catch
                    { Console.WriteLine($"No key _{i} in view"); }
                }
            }
            keyMax = 0;
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
            layoutFrame.Focus();    // Focus elsewhere otherwise some keys change the bindings (A, Q, Arrow Up and down etc...)
            Keyboard.ClearFocus();  // And clear all focus hopefully
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MainWindow()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                KListener.Dispose();
                honhonT.Abort();
                huhuT.Abort();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            honhon = false;
            this.Dispose();
        }


        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private void KeyListView_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = new GridViewColumnHeader();
            if (e.OriginalSource is GridViewColumnHeader)
            {
                headerClicked = (GridViewColumnHeader)e.OriginalSource;

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

                        var columnBinding = headerClicked.Column.DisplayMemberBinding as System.Windows.Data.Binding;
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
        private byte id;
        private uint numPress;
        private string character;
        public KeyStroke(byte id, uint numPress)
        {
            Id = id;
            NumPress = numPress;
            Character = System.Windows.Input.KeyInterop.KeyFromVirtualKey(Id).ToString();
        }
        public KeyStroke(byte id)
        {
            Id = id;
            NumPress = 0;
            Character = System.Windows.Input.KeyInterop.KeyFromVirtualKey(Id).ToString();
        }
        public byte Id
        {
            get => id;
            set
            {
                id = value;
                NotifyPropertyChanged("Id");
            }
        }
        public uint NumPress
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
        private delegate void KeyboardCallbackAsync(NativeMethods.KeyEvent keyEvent, int vkCode);

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
            //string chars = "";
            //Console.WriteLine($"LowLevelKeyboardProc {nCode}");
            if (nCode >= 0)
                if (wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_KEYDOWN ||
                    wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_KEYUP ||
                    wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_SYSKEYDOWN ||
                    wParam.ToUInt32() == (int)NativeMethods.KeyEvent.WM_SYSKEYUP)
                {
                    hookedKeyboardCallbackAsync.BeginInvoke((NativeMethods.KeyEvent)wParam.ToUInt32(), Marshal.ReadInt32(lParam), null, null);
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
        private void KeyboardListener_KeyboardCallbackAsync(NativeMethods.KeyEvent keyEvent, int vkCode)
        {
            //Console.WriteLine($"LowLevelKeyboardProc {vkCode} - {keyEvent}");

            switch (keyEvent)
            {
                // KeyDown events
                case NativeMethods.KeyEvent.WM_KEYDOWN:
                    if (KeyDown != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this, new RawKeyEventArgs(vkCode, false));
                    break;
                case NativeMethods.KeyEvent.WM_SYSKEYDOWN:
                    if (KeyDown != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this, new RawKeyEventArgs(vkCode, true));
                    break;

                // KeyUp events
                case NativeMethods.KeyEvent.WM_KEYUP:
                    if (KeyUp != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this, new RawKeyEventArgs(vkCode, false));
                    break;
                case NativeMethods.KeyEvent.WM_SYSKEYUP:
                    if (KeyUp != null)
                        dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this, new RawKeyEventArgs(vkCode, true));
                    break;

                default:
                    break;
            }
        }

        #endregion Inner workings

        #region IDisposable Members

        /// <summary>
        /// Destroys global keyboard listener.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~KeyboardListener()
        {
            Dispose(false);
        }
        /// <summary>
        /// Disposes the hook.
        /// <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        protected virtual void Dispose(bool disposing)
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
        /// Unicode character of key pressed.
        /// </summary>
        public string Character;

        /// <summary>
        /// Create raw keyevent arguments.
        /// </summary>
        /// <param name="VKCode"></param>
        /// <param name="isSysKey"></param>
        /// <param name="Character">Character</param>
        public RawKeyEventArgs(int VKCode, bool isSysKey)
        {
            this.VKCode = VKCode;
            this.IsSysKey = isSysKey;
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

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, ulong dwExtraInfo);
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        
        public static void Move(int xDelta, int yDelta)
        {
            float min = 0;
            float max = 65535;
            int width = Screen.PrimaryScreen.WorkingArea.Width;
            int height = Screen.PrimaryScreen.WorkingArea.Height;
            int mappedX = (int)Remap(xDelta, 0.0f, (float)width, min, max);
            int mappedY = (int)Remap(yDelta, 0.0f, (float)height, min, max);
            mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }
        public static void MoveTo(int x, int y)
        {
            float min = 0;
            float max = 65535;
            int width = Screen.PrimaryScreen.WorkingArea.Width;
            int height = Screen.PrimaryScreen.WorkingArea.Height;
            int mappedX = (int)Remap(x, 0.0f, (float)width, min, max);
            int mappedY = (int)Remap(y, 0.0f, (float)height, min, max);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, mappedX, mappedY, 0, 0);
        }
        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y, 0, 0);
        }
        public static Point MousePos()
        {
            return new Point(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
        }

        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }

    #endregion WINAPI Helper class
}
