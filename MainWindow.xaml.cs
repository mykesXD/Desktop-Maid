using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using HWND = System.IntPtr;
using System.Windows.Interop;
using System.Threading;

namespace Maid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public struct Rect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }
    public partial class MainWindow : Window
    {
        int SCREEN_WIDTH = Convert.ToInt32(SystemParameters.WorkArea.Width);
        int SCREEN_HEIGHT = Convert.ToInt32(SystemParameters.WorkArea.Height);
        int fullscreenHeight = Convert.ToInt32(SystemParameters.FullPrimaryScreenHeight);
        bool climbing = false;
        bool standing = false;
        Animations anim = new Animations();
        Messages messageList = new Messages();
        public List<IntPtr> coveredWindow = new List<IntPtr>();
        public List<IntPtr> windowLst = new List<IntPtr>();
        public List<Rect> windowRect = new List<Rect>();
        List<Point> mousePositions = new List<Point>();
        IDictionary<IntPtr, Rect> visibleWindows = new Dictionary<IntPtr, Rect>();
        IDictionary<IntPtr, Rect> desktopWindows = new Dictionary<IntPtr, Rect>();
        IntPtr maidWindowHandle;
        bool dragged = false;
        bool mouseUp = false;
        int avatarHeight = 150;
        int avatarCanvasTop = 220;
        int mainWinHeight = 350;
        int gravity = 1;
        int taskbarHeight = 843;
        IntPtr TaskBarHandle;
        int run = 3;
        int mov = 0;
        int up = 5;
        int down = 0;
        int standingTop = 0;
        public static bool settingsWindowOpen = false;
        IntPtr standingWindows;
        string avatarState = "idle";
        IntPtr standingWinHandle;
        public bool _IsDragInProgress { get; set; }
        public System.Windows.Point _FormMousePosition { get; set; }
        // Creates list of all open windows with Handle,Name
        public static IDictionary<HWND, string> GetOpenWindows()
        {
            HWND shellWindow = Windows.GetShellWindow();
            Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

            Windows.EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!Windows.IsWindowVisible(hWnd)) return true;

                int length = Windows.GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                Windows.GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);
            return windows;
        }

        internal static void GetMain()
        {
            var mainHandle = Windows.GetActiveWindow();
            //Main = new Window(mainHandle, GetTitle(mainHandle));
        }


        // Get state of window (Minimized, Maximized etc)
        static uint GetStyle(IntPtr handle)
        {
            return Windows.GetWindowLong(handle, Windows.GWL_STYLE);
        }
        static string GetTitle(IntPtr handle, int length = 128)
        {
            StringBuilder builder = new StringBuilder(length);
            Windows.GetWindowText(handle, builder, length + 1);
            return builder.ToString();
        }
        // Initilaze the window

        DispatcherTimer randomTimer = new DispatcherTimer();
        DispatcherTimer actionTimer = new DispatcherTimer();
        DispatcherTimer mouseTimer = new DispatcherTimer();
        DispatcherTimer windowTimer = new DispatcherTimer();

        Task task1;
        Task task2;
        public MainWindow()
        {
            Rect rectanglee = new Rect();
            InitializeComponent();
            mainWinHeight = Convert.ToInt32(this.Height);
            Trace.WriteLine("SHEESH");
            //Console.WriteLine(avatarCanvasTop);
            avatarHeight = Convert.ToInt32(Player.Height);

            randomTimer.Tick += randomTimerTick;
            randomTimer.Interval = new TimeSpan(0, 0, 6);
            randomTimer.Start();

            actionTimer.Tick += actionTimerTick;
            actionTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            actionTimer.Start();

            windowTimer.Tick += windowTimerTick;
            windowTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            windowTimer.Start();



            var AnimationTimer = new DispatcherTimer();
            AnimationTimer.Tick += AnimationTimerTick;
            AnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            task1 = new Task(AnimationTimer.Start);

            var FastAnimationTimer = new DispatcherTimer();
            FastAnimationTimer.Tick += FastAnimationTimerTick;
            FastAnimationTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            task2 = new Task(FastAnimationTimer.Start);

            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;
                Windows.GetWindowRect(handle, ref rectanglee);
                windowLst.Add(handle);
                windowRect.Add(rectanglee);
                if(title.Contains("Desktop Maid"))
                {
                    maidWindowHandle = handle;
                }
                Console.WriteLine("{0}: {1}", handle, title);

            }
            for (int i = 0; i < windowLst.Count(); i++)
            {
                //if (!(windowRect[i].Right == 1928 && windowRect[i].Bottom == 1048 && windowRect[i].Left == -8 && windowRect[i].Top == -8))
                {
                    desktopWindows.Add(windowLst[i], windowRect[i]);
                    Console.WriteLine("VISIBLE {0} -> {1}, {2}", i, windowLst[i], windowRect[i].Top);
                }
            }
            task1.Start();
            CompositionTarget.Rendering += MainEventTimer;
        }

        private void FastAnimationTimerTick(object sender, EventArgs e)
        {
            if (dragged)
            {
                anim.PlayPlayerAnimation(Player, avatarState);
            }
        }

        private void mouseTimerTick(object sender, EventArgs e)
        {
        }

        private void AnimationTimerTick(object sender, EventArgs e)
        {
            anim.PlayPlayerAnimation(Player, avatarState);
        }

        private void windowTimerTick(object sender, EventArgs e)
        {

        }
        private void MainEventTimer(object sender, EventArgs e)
        {
            
            // Accelaration on mouse release (WARNIN: DONT TRY VELOCITY. PHYSICS IS OUT OF QUESTION I TRIED)
            if (dragged)
            {
                Point shit = Windows.GetMousePosition();
                
                //Console.WriteLine("MOUSE {0}", Windows.GetMousePosition());
                mousePositions.Add(shit);
                if (mousePositions.Count > 5)
                {
                    if(mousePositions.ElementAt(mousePositions.Count - 1).X > mousePositions.ElementAt(mousePositions.Count - 5).X)
                    {
                        if(Flip.ScaleX == 1)
                        {
                            avatarState = "dragRight";
                        }
                        else
                        {
                            avatarState = "dragLeft";
                        }
                        //Console.WriteLine("RIGHT");
                    }
                    else if(mousePositions.ElementAt(mousePositions.Count - 1).X < mousePositions.ElementAt(mousePositions.Count - 5).X)
                    {
                        if (Flip.ScaleX == 1)
                        {
                            avatarState = "dragLeft";

                        }
                        else
                        {
                            avatarState = "dragRight";
                        }
                        //Console.WriteLine("LEFT");
                    }
                    else
                    {

                        avatarState = "drag";
                        //Console.WriteLine("NOT MOVING");

                    }
                }
            }
            if (mouseUp)
            {
                avatarState = "idle";
                int mouseCount = 5;
                //Console.WriteLine(mousePositions.Count);
                double speedX = 0;
                double speedY = 0;
                int speedXAvg = 0;
                int speedYAvg = 0;
                if (mousePositions.Count < mouseCount)
                {
                    mouseCount = mousePositions.Count;
                }
                if (mouseCount > 1)
                {
                    //dragged = true;
                    for (int i = 1; i < mouseCount; i++)
                    {
                        speedX += mousePositions[mousePositions.Count - i].X - mousePositions[mousePositions.Count - i - 1].X;
                        speedY += mousePositions[mousePositions.Count - i].Y - mousePositions[mousePositions.Count - i - 1].Y;
                    }
                    speedXAvg = Convert.ToInt32(speedX / mouseCount);
                    speedYAvg = Convert.ToInt32(speedY / mouseCount);
                    if (mov < 10)
                    {
                        if (speedXAvg > 0)
                        {
                            GameWindow.Left += 2;
                        }
                        else if (speedXAvg < 0)
                        {
                            GameWindow.Left -= 2;
                        }
                        if (speedYAvg > 0)
                        {
                            GameWindow.Top -= 2;
                        }
                        else if (speedYAvg < 0)
                        {
                            GameWindow.Top += 2;
                        }
                        mov += 1;
                    }
                    else
                    {
                        mov = 0;
                        up = 5;
                        down = 0;
                        mousePositions.Clear();
                        mouseUp = false;
                        dragged = false;
                    }
                }
            }
            Rect rectangle1 = new Rect();
            Rect rectangleMaid = new Rect(); // Main window

            //Get the taskbar window rect in screen coordinates
            TaskBarHandle = Windows.FindWindow("Shell_traywnd", "");
            Rect taskbarRect = new Rect();
            Windows.GetWindowRect(TaskBarHandle, ref taskbarRect);
            taskbarHeight = taskbarRect.Top - 200 + 3;

            windowLst.Clear();
            windowRect.Clear();
            desktopWindows.Clear();
            coveredWindow.Clear();
            visibleWindows.Clear();


            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            {
                IntPtr handle = window.Key;
                string title = window.Value;

                Windows.DwmGetWindowAttribute(handle, (int)Windows.DwmWindowAttribute.Cloaked, out bool isCloacked, Marshal.SizeOf(typeof(bool)));

                // Closed windows around  >900000000, Open windows >400000000 
                if (!isCloacked && (GetStyle(handle) < 400000000))
                {

                    //Console.WriteLine("{0}: {1}: {2}", handle, title, GetStyle(handle));
                    if (title.Contains("Desktop Maid"))
                    {
                        Windows.GetWindowRect(handle, ref rectangleMaid);
                        windowLst.Add(handle);
                        rectangleMaid.Top = rectangleMaid.Top + avatarCanvasTop;
                        windowRect.Add(rectangleMaid);
                        //Console.WriteLine(rectangleMaid.Left.ToString() + " " + rectangleMaid.Right.ToString() + " " + rectangleMaid.Top.ToString() + " " + rectangleMaid.Bottom.ToString());
                    }
                    else
                    {
                        Windows.GetWindowRect(handle, ref rectangle1);
                        windowLst.Add(handle);
                        windowRect.Add(rectangle1);
                        //Console.WriteLine(rectangle1.Left.ToString() + " " + rectangle1.Right.ToString() + " " + rectangle1.Top.ToString() + " " + rectangle1.Bottom.ToString());
                    }
                }
            }
            for (int i = 0; i < windowLst.Count(); i++)
            {
                desktopWindows.Add(windowLst[i], windowRect[i]);
            }
            // Game window position
            //Console.WriteLine("{0}: {1}:",GameWindow.Top.ToString(),GameWindow.Left.ToString());
            // Loop over all 
            int xIndex;
            int yIndex;
            foreach (KeyValuePair<IntPtr, Rect> x in desktopWindows)
            {
                Rect visibleRectX = x.Value;
                IntPtr visibleHandleX = x.Key;
                foreach (KeyValuePair<IntPtr, Rect> y in desktopWindows)
                {
                    xIndex = desktopWindows.Values.ToList().IndexOf(x.Value);
                    yIndex = desktopWindows.Values.ToList().IndexOf(y.Value);
                    Rect visibleRectY = y.Value;
                    IntPtr visibleHandleY = y.Key;

                    if (visibleRectY.Right == 1928 && visibleRectY.Bottom == 1048 && visibleRectY.Left == -8 && visibleRectY.Top == -8)
                    {
                        if (!coveredWindow.Contains(visibleHandleX) && xIndex > yIndex)
                        {
                            coveredWindow.Add(visibleHandleX);
                            //Console.WriteLine("{0}:Covered SHEESH {1}", GetTitle(visibleHandleX), visibleHandleX); //Geting covered screens
                        }
                    }
                    else
                    {
                        if (visibleRectX.Left > visibleRectY.Left && visibleRectX.Right < visibleRectY.Right && visibleRectX.Top > visibleRectY.Top)
                        {
                            xIndex = desktopWindows.Values.ToList().IndexOf(x.Value);
                            yIndex = desktopWindows.Values.ToList().IndexOf(y.Value);
                            if (!coveredWindow.Contains(visibleHandleX) && xIndex > yIndex)
                            {
                                coveredWindow.Add(visibleHandleX);
                                //Console.WriteLine("{0}:Covered {1}", GetTitle(visibleHandleX), xIndex); //Geting covered screens
                            }
                        }
                    }
                }
            }
            // Returns visible windows that're not fullscreen
            foreach (KeyValuePair<IntPtr, Rect> x in desktopWindows)
            {
                IntPtr deskHandle = x.Key;
                Rect deskRect = x.Value;
                
                if (!coveredWindow.Contains(deskHandle) && deskRect.Left < 1865 && deskRect.Right > 20 && deskRect.Top > avatarHeight && deskHandle != maidWindowHandle && GetTitle(deskHandle) != "Desktop Maid")
                {
                    visibleWindows.Add(deskHandle, deskRect);
                    //Console.WriteLine("{0}:{1}:Open", deskHandle, GetTitle(deskHandle));
                }

            }
            int farWindowLeft = -10;
            int farWindowRight = 2000;
            int farWindowBottom = 0;
            int farWindowTop = 2000;
            bool anotherWindowOnTop = false;
            List<Rect> farTops = new List<Rect>();
            foreach (KeyValuePair<IntPtr, Rect> visibleX in visibleWindows)
            {
                if (standing)
                {
                    IntPtr visibleHandleX = visibleX.Key;
                    Rect visibleRectX = visibleX.Value;
                    if (visibleHandleX == standingWinHandle)
                    {
                        if (farWindowTop > visibleRectX.Top)
                        {
                            foreach (KeyValuePair<IntPtr, Rect> visibleY in visibleWindows)
                            {
                                IntPtr visibleHandleY = visibleY.Key;
                                Rect visibleRectY = visibleY.Value;

                                if (visibleHandleY != visibleHandleX && visibleWindows.Values.ToList().IndexOf(visibleX.Value) > visibleWindows.Values.ToList().IndexOf(visibleY.Value))
                                {
                                    if (visibleRectY.Right > rectangleMaid.Left + avatarHeight && visibleRectY.Left < rectangleMaid.Right - avatarHeight && visibleRectY.Top < rectangleMaid.Bottom)
                                    {
                                        anotherWindowOnTop = true;
                                        break;
                                    }
                                }
                            }
                            if (!anotherWindowOnTop)
                            {
                                //Console.WriteLine("STANDING");
                                standingWinHandle = visibleHandleX;
                                farWindowTop = visibleRectX.Top;
                                farWindowLeft = visibleRectX.Left;
                                farWindowRight = visibleRectX.Right;
                                farWindowBottom = visibleRectX.Bottom;
                            }
                        }
                    }
                }
                else
                {
                    IntPtr visibleHandleX = visibleX.Key;
                    Rect visibleRectX = visibleX.Value;
                    if (farWindowTop > visibleRectX.Top && visibleRectX.Top >= rectangleMaid.Bottom && visibleRectX.Top >= rectangleMaid.Bottom - 1)
                    {
                        foreach (KeyValuePair<IntPtr, Rect> visibleY in visibleWindows)
                        {
                            IntPtr visibleHandleY = visibleY.Key;
                            Rect visibleRectY = visibleY.Value;

                            if (visibleHandleY != visibleHandleX && visibleWindows.Values.ToList().IndexOf(visibleX.Value) > visibleWindows.Values.ToList().IndexOf(visibleY.Value))
                            {
                                if (visibleRectY.Right > rectangleMaid.Left + avatarHeight && visibleRectY.Left < rectangleMaid.Right - avatarHeight && visibleRectY.Top < rectangleMaid.Bottom)
                                {
                                    anotherWindowOnTop = true;
                                    break;
                                }
                            }
                        }
                        if (!anotherWindowOnTop)
                        {
                            //Console.WriteLine("NOT STANDING");
                            standingWinHandle = visibleHandleX;
                            farWindowTop = visibleRectX.Top;
                            farWindowLeft = visibleRectX.Left;
                            farWindowRight = visibleRectX.Right;
                            farWindowBottom = visibleRectX.Bottom;
                        }
                    }
                }
            }
            //Console.WriteLine("FAR:{0}", farWindowTop);
            //Console.WriteLine("GameWindow:{0}", standingRect.Top);
            if (GameWindow.Top < taskbarHeight && !dragged)
            {
                

                if (rectangleMaid.Left + 150 > farWindowLeft && rectangleMaid.Right - 150 < farWindowRight)
                {
                    if (rectangleMaid.Bottom != farWindowTop && rectangleMaid.Bottom != farWindowTop + 1 && !climbing && !standing)
                    {
                        GameWindow.Top += gravity;
                    }
                    else
                    {
                        standingWindows = standingWinHandle;
                        standing = true;
                    }
                }
                else if (!climbing && !standing)
                {
                    GameWindow.Top += gravity;
                }
                else
                {
                    standing = false;
                }
                // Moves the avatar when window position goes up
                if (GameWindow.Top > farWindowTop - 200 && standing && farWindowTop < 1800 && standing && GameWindow.Top > farWindowTop - 199)
                {
                    GameWindow.Top = farWindowTop - 200;
                    //Console.WriteLine("{0}  {1}", GameWindow.Top, farWindowTop - 200);
                }
                else if((farWindowTop - 200 - GameWindow.Top) > 30 && standing)
                {
                    standing = false;
                }
            }
            //Console.WriteLine("{0}  {1} FUUCK", GameWindow.Top, farWindowTop - 200);
            if (GameWindow.Top > taskbarHeight && !dragged)
            {
                GameWindow.Top = taskbarHeight;
            }
            
        }




        // Player Drag function

        public void Player_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            avatarState = "drag";
            dragged = true;
            mouseUp = false;
            //Console.WriteLine("SHEESH");
            DragMove();
            base.OnMouseLeftButtonDown(e);
            if (ResizeMode != ResizeMode.NoResize)
            {
                ResizeMode = ResizeMode.NoResize;
                UpdateLayout();
            }
            //Console.WriteLine("{0}:MOUSE", e.GetPosition(GameWindow).X);//
            avatarState = "idle";
            dragged = false;
            mouseUp = true;
        }

        // Menu

        private void Player_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(MenuGrid.Visibility == Visibility.Hidden)
            {
                MenuGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MenuGrid.Visibility = Visibility.Hidden;
            }
        }

        // Avatar AI
        public void randomTimerTick(object sender, EventArgs e)
        {
            Random rnd = new Random();
            run = rnd.Next(0, 3);
            //Console.WriteLine("HELLO {0}", run);
        }



        public void actionTimerTick(object sender, EventArgs e)
        {
            
            if (!dragged)
            {
                Rect rectangle1 = new Rect();
                Rect rectangleMaid = new Rect(); // Main window

                windowLst.Clear();
                windowRect.Clear();
                desktopWindows.Clear();
                coveredWindow.Clear();
                visibleWindows.Clear();


                foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
                {
                    IntPtr handle = window.Key;
                    string title = window.Value;

                    Windows.DwmGetWindowAttribute(handle, (int)Windows.DwmWindowAttribute.Cloaked, out bool isCloacked, Marshal.SizeOf(typeof(bool)));

                    // Closed windows around  >900000000, Open windows > 400000000 
                    if (!isCloacked && (GetStyle(handle) < 400000000))
                    {
                        //Console.WriteLine("{0}: {1}: {2}", handle, title, Convert.ToUInt32(GetStyle(handle)), 16);

                        //Console.WriteLine("{0}: {1}: {2}", handle, title, GetStyle(handle));
                        if (title.Contains("Desktop Maid"))
                        {
                            Windows.GetWindowRect(handle, ref rectangleMaid);
                            windowLst.Add(handle);
                            rectangleMaid.Top = rectangleMaid.Top + avatarCanvasTop;
                            windowRect.Add(rectangleMaid);
                            //Console.WriteLine(rectangleMaid.Left.ToString() + " " + rectangleMaid.Right.ToString() + " " + rectangleMaid.Top.ToString() + " " + rectangleMaid.Bottom.ToString());
                        }
                        else
                        {
                            Windows.GetWindowRect(handle, ref rectangle1);
                            windowLst.Add(handle);
                            windowRect.Add(rectangle1);
                            //Console.WriteLine(rectangle1.Left.ToString() + " " + rectangle1.Right.ToString() + " " + rectangle1.Top.ToString() + " " + rectangle1.Bottom.ToString());
                        }
                    }
                }
                if (run == 0)
                {
                    avatarState = "idle";
                    climbing = false;
                }
                else if (run == 1)
                {
                    if (rectangleMaid.Left + avatarHeight - 25 < 0)
                    {
                        GameWindow.Top -= 2;
                        //Console.WriteLine(rectangleMaid.Left);
                        avatarState = "climb";
                        climbing = true;
                    }
                    else
                    {
                        GameWindow.Left -= 2;
                        Flip.ScaleX = 1;
                        avatarState = "run";
                        climbing = false;
                    }
                }
                else if (run == 2)
                {
                    if (rectangleMaid.Right - avatarHeight + 20 > SCREEN_WIDTH)
                    {
                        GameWindow.Top -= 2;
                        //Console.WriteLine(rectangleMaid.Right);
                        avatarState = "climb";
                        climbing = true;
                    }
                    else
                    {
                        GameWindow.Left += 2;
                        Flip.ScaleX = -1;
                        avatarState = "run";
                        climbing = false;
                    }
                }
            }
        }

        // Move on key event
        protected override void OnKeyDown(KeyEventArgs e)
        {
            Rect rectangle1 = new Rect();
            Rect rectangleMaid = new Rect(); // Main window

            windowLst.Clear();
            windowRect.Clear();
            desktopWindows.Clear();
            coveredWindow.Clear();
            visibleWindows.Clear();


            foreach (KeyValuePair<IntPtr, string> window in GetOpenWindows())
            { 
                IntPtr handle = window.Key;
                string title = window.Value;

                Windows.DwmGetWindowAttribute(handle, (int)Windows.DwmWindowAttribute.Cloaked, out bool isCloacked, Marshal.SizeOf(typeof(bool)));

                // Closed windows around  >900000000, Open windows >400000000 
                if ((GetStyle(handle) < 400000000))
                {
                    //Console.WriteLine("{0}: {1}: {2}", handle, title, Convert.ToUInt32(GetStyle(handle)), 16);

                    //Console.WriteLine("{0}: {1}: {2}", handle, title, GetStyle(handle));
                    if (title.Contains("Desktop Maid"))
                    {
                        Windows.GetWindowRect(handle, ref rectangleMaid);
                        windowLst.Add(handle);
                        rectangleMaid.Top = rectangleMaid.Top + avatarCanvasTop;
                        windowRect.Add(rectangleMaid);
                        //Console.WriteLine(rectangleMaid.Left.ToString() + " " + rectangleMaid.Right.ToString() + " " + rectangleMaid.Top.ToString() + " " + rectangleMaid.Bottom.ToString());
                    }
                    else
                    {
                        Windows.GetWindowRect(handle, ref rectangle1);
                        windowLst.Add(handle);
                        windowRect.Add(rectangle1);
                        //Console.WriteLine(rectangle1.Left.ToString() + " " + rectangle1.Right.ToString() + " " + rectangle1.Top.ToString() + " " + rectangle1.Bottom.ToString());
                    }
                }
            }
            for (int i = 0; i < windowLst.Count(); i++)
            {
                desktopWindows.Add(windowLst[i], windowRect[i]);
            }
            // Game window position
            //Console.WriteLine("{0}: {1}:",GameWindow.Top.ToString(),GameWindow.Left.ToString());
            //Console.WriteLine("SHEESH");
            // Loop over all 
            int xIndex;
            int yIndex;
            foreach (KeyValuePair<IntPtr, Rect> x in desktopWindows)
            {
                Rect visibleRectX = x.Value;
                IntPtr visibleHandleX = x.Key;

                foreach (KeyValuePair<IntPtr, Rect> y in desktopWindows)
                {
                    Rect visibleRectY = y.Value;
                    IntPtr visibleHandleY = y.Key;
                    xIndex = desktopWindows.Values.ToList().IndexOf(x.Value);
                    yIndex = desktopWindows.Values.ToList().IndexOf(y.Value);
                    //Console.WriteLine("{0}:ALL {1}", GetTitle(visibleHandleX), xIndex);
                    if (visibleRectX.Left > visibleRectY.Left && visibleRectX.Right < visibleRectY.Right && visibleRectX.Top > visibleRectY.Top)
                    {
                        xIndex = desktopWindows.Values.ToList().IndexOf(x.Value);
                        yIndex = desktopWindows.Values.ToList().IndexOf(y.Value);
                        if (!coveredWindow.Contains(visibleHandleX) && xIndex > yIndex) {
                            coveredWindow.Add(visibleHandleX);
                            //Console.WriteLine("{0}:Covered {1}", GetTitle(visibleHandleX),xIndex); //Geting covered screens
                        }
                    }
                }
            }
            // Returns visible windows that're not fullscreen
            foreach (KeyValuePair<IntPtr, Rect> x in desktopWindows)
            {
                IntPtr deskHandle = x.Key;
                Rect deskRect = x.Value;
                if (!coveredWindow.Contains(deskHandle) && deskRect.Left < 1865 && deskRect.Right > 20 && deskRect.Top > avatarHeight && deskHandle != maidWindowHandle && GetTitle(deskHandle) != "Desktop Maid")
                {
                    //Console.WriteLine("{0}:{1} Open", deskRect.Bottom, GetTitle(deskHandle));
                    visibleWindows.Add(deskHandle, deskRect);
                    
                }
            }


                switch (e.Key)
            {
                case Key.Left:
                    //placeholder = rectangleMaid.Left - 20; // Checks if its gonna go overboard
                    //Console.WriteLine("{0}:{1}", farWindowRight, placeholder);
                    //if (placeholder > 0 && placeholder > farWindowRight )
                    if (rectangleMaid.Left+avatarHeight-25 < 0)
                    {
                        GameWindow.Top -= 3;
                        Console.WriteLine(rectangleMaid.Left);
                        avatarState = "climb";
                        climbing = true;
                    }
                    else
                    {
                        GameWindow.Left -= 3;
                        Flip.ScaleX = 1;
                        avatarState = "run";
                    }
                    break;
                case Key.Right:
                    if (rectangleMaid.Right-avatarHeight+20 > SCREEN_WIDTH)
                    {
                        GameWindow.Top -= 3;
                        Console.WriteLine(rectangleMaid.Right);
                        avatarState = "climb";
                        climbing = true;
                    }
                    else
                    {
                        GameWindow.Left += 3;
                        Flip.ScaleX = -1;
                        avatarState = "run";
                    }
                    break;
                case Key.Up:
                    RotateTransform rotateTransform1 = new RotateTransform(45);
                    if (SpeechGrid.Visibility == Visibility.Hidden)
                    {
                        SpeechGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SpeechGrid.Visibility = Visibility.Hidden;
                    }
                    break;
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            avatarState = "idle";
            climbing = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!settingsWindowOpen)
            {
                SettingsMenu settingsMenu = new SettingsMenu();
                settingsMenu.Show();
                settingsWindowOpen = true;
                MenuGrid.Visibility = Visibility.Hidden;
            }
        }







        /*private void Player_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragged = true;
            if (ResizeMode != System.Windows.ResizeMode.NoResize)
            {
                ResizeMode = System.Windows.ResizeMode.NoResize;
                UpdateLayout();
            }
            var pos = this.PointToScreen(Mouse.GetPosition(this));

            Windows.ReleaseCapture();
            //Console.WriteLine("{0}:MOUSE", pos.X);
            Windows.SendMessage(Windows.GetActiveWindow(), Windows.WM_NCLBUTTONDOWN, Windows.HT_CAPTION, 0);
            dragged = false;
        }
        */
    }
}
