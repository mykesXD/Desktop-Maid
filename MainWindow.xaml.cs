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
        readonly AvatarActions avatarAction = new AvatarActions();
        public List<IntPtr> coveredWindow = new List<IntPtr>();
        public List<IntPtr> windowLst = new List<IntPtr>();
        public List<Rect> windowRect = new List<Rect>();
        public static List<int> actionList = new List<int>();
        List<Point> mousePositions = new List<Point>();
        IDictionary<IntPtr, Rect> visibleWindows = new Dictionary<IntPtr, Rect>();
        IDictionary<IntPtr, Rect> desktopWindows = new Dictionary<IntPtr, Rect>();
        IntPtr maidWindowHandle;
        public static bool dragged = false;
        bool mouseUp = false;
        int avatarHeight = 150;
        int avatarCanvasTop = 220;
        int mainWinHeight = 350;
        int gravity = 1;
        int taskbarHeight = 843;
        IntPtr TaskBarHandle;
        int action = 0;
        int actionTime;
        int actionCount = 100;
        int mov = 0;
        int up = 5;
        int down = 0;
        int standingTop = 0;
        public static bool settingsWindowOpen = false;
        IntPtr standingWindows;
        public static string avatarState = "idle";
        IntPtr standingWinHandle;
        public bool _IsDragInProgress { get; set; }
        public System.Windows.Point _FormMousePosition { get; set; }

        double speedX = 0;
        double speedY = 0;
        int speedXAvg = 0;
        int speedYAvg = 0;
        double forceX = 0;
        double forceY = 0;
        string throwX, throwY;
        bool force = false;
        bool falling = false;


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
            randomTimer.Interval = new TimeSpan(0, 0, 1);
            randomTimer.Start();

            actionTimer.Tick += actionTimerTick;
            actionTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            actionTimer.Start();

            windowTimer.Tick += windowTimerTick;
            windowTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            windowTimer.Start();

            actionList.Add(1);
            actionList.Add(2);
            actionList.Add(3);
            actionList.Add(4);
            actionList.Add(5);
            foreach (int action in actionList)
            {
                Console.WriteLine(action);
            }

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
                    //Console.WriteLine("{0} {1} {2} {3} {4}", windowRect[i].Left, windowRect[i].Right, windowRect[i].Top, windowRect[i].Bottom, GetTitle(windowLst[i]));
                }
            }
            task1.Start();
            CompositionTarget.Rendering += MainEventTimer;
        }

        //Drag Animation
        private void FastAnimationTimerTick(object sender, EventArgs e)
        {
            if (dragged)
            {
                anim.PlayPlayerAnimation(Player, avatarState,climbing);
            }
        }

        private void mouseTimerTick(object sender, EventArgs e)
        {
        }

        private void AnimationTimerTick(object sender, EventArgs e)
        {
            anim.PlayPlayerAnimation(Player, avatarState,climbing);
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

                // Rotate avatar opposite of the direction being dragged
                mousePositions.Add(shit);
                if (mousePositions.Count > 10)
                {
                    if (mousePositions.ElementAt(mousePositions.Count - 1).X > mousePositions.ElementAt(mousePositions.Count - 10).X && mousePositions.ElementAt(mousePositions.Count - 1).X - mousePositions.ElementAt(mousePositions.Count - 10).X > 2)
                    {
                        //Console.WriteLine(mousePositions.ElementAt(mousePositions.Count - 1).X - mousePositions.ElementAt(mousePositions.Count - 10).X);
                        //avatarState = "dragRight";
                        if (Rotate.Angle < 34)
                        {
                            Rotate.Angle += 2;
                        }
                        else
                        {
                            //avatarState = "dragLeft";
                            if (Rotate.Angle > -34)
                            {
                                Rotate.Angle -= 2;
                            }
                        }
                        //Console.WriteLine("RIGHT");
                    }
                    else if (mousePositions.ElementAt(mousePositions.Count - 1).X < mousePositions.ElementAt(mousePositions.Count - 10).X && mousePositions.ElementAt(mousePositions.Count - 10).X - mousePositions.ElementAt(mousePositions.Count - 1).X > 2)
                    {
                        //Console.WriteLine(mousePositions.ElementAt(mousePositions.Count - 10).X - mousePositions.ElementAt(mousePositions.Count - 1).X);
                        //avatarState = "dragLeft";
                        if (Rotate.Angle > -34)
                        {
                            Rotate.Angle -= 1;
                        }
                        else
                        {
                            //avatarState = "dragRight";
                            if (Rotate.Angle < 34)
                            {
                                Rotate.Angle += 1;
                            }
                        }
                        //Console.WriteLine("LEFT");
                    }
                    else
                    {
                        if (Rotate.Angle > 0)
                        {
                            if(Rotate.Angle - 2 < 0)
                            {
                                Rotate.Angle -= 1;
                            }
                            else
                            {
                                Rotate.Angle -= 2;
                            }
                            
                            //Console.WriteLine(Rotate.Angle);
                        }
                        else if (Rotate.Angle < 0)
                        {
                            //Console.WriteLine(Rotate.Angle);
                            if (Rotate.Angle + 2 > 0)
                            {
                                Rotate.Angle += 1;
                            }
                            else
                            {
                                Rotate.Angle += 2;
                            }
                        }
                        //avatarState = "idle";
                        //Console.WriteLine("NOT MOVING");
                    }
    
                }
            }
            if (mouseUp)
            {
                // Stop rotation
                if(avatarState == "idle" || avatarState == "runRight" || climbing || avatarState == "runLeft")
                {
                    Rotate.Angle = 0;
                }

                // Apply force to avatar slightly after drag mouse released, giving illusion of physic
                int mouseCount = 5;
                //Console.WriteLine(mousePositions.Count);
                /*double speedX = 0;
                double speedY = 0;
                int speedXAvg = 0;
                int speedYAvg = 0;
                double forceX = 0;
                double forceY = 0;
                string throwX, throwY;
                bool force = false;*/
                // Set maximum amount for last positions of mouse
                if (mousePositions.Count < mouseCount)
                {
                    mouseCount = mousePositions.Count;
                }
                if (mouseCount > 0)
                {
                    mouseCount = mousePositions.Count;
                    
                    //dragged = true;
                    //for (int i = mouseCount; i > 0; i--)
                    if (mousePositions[mouseCount - 1].X > mousePositions[mouseCount - 3].X)
                    {
                        speedX = mousePositions[mouseCount - 1].X - mousePositions[mouseCount - 3].X;
                        throwX = "right";
                    }
                    else
                    {
                        speedX = mousePositions[mouseCount - 3].X - mousePositions[mouseCount - 1].X;
                        throwX = "left";
                    }
                    if (mousePositions[mouseCount - 1].Y > mousePositions[mouseCount - 3].Y)
                    {
                        speedY = mousePositions[mouseCount - 1].Y - mousePositions[mouseCount - 3].Y;
                        throwY = "up";
                    }
                    else
                    {
                        speedY = mousePositions[mouseCount - 3].Y - mousePositions[mouseCount - 1].Y;
                        throwY = "down";
                    }
                    if(speedX > 4)
                    {
                        speedX = 4;
                    }
                    if(speedY > 8)
                    {
                        speedY = 8;
                    }
                    if (forceX < speedX && falling == false)
                    {
                        forceX = speedX;
                    }
                    if (forceY < speedY && falling == false)
                        forceY = speedY;
                    if ((int)forceX == speedX) {
                        falling = true;
                        Console.WriteLine("BRUHHHHHHHHHHHHH");
                    }
                    if (falling == true)
                    {
                        forceX -= 0.025;
                        forceY -= 0.1;
                    }
                    if (forceX <= 0 && forceY <= 0)
                    {
                        forceX = 0;
                        forceY = 0;
                        falling = false;
                        mouseUp = false;
                        dragged = false;
                        mousePositions.Clear();
                    }


                    if (forceY < 0 || forceY == 0.5)
                    {
                        forceY = 0;
                    }

                    //Console.WriteLine(falling);
                    Console.WriteLine("speedX: {0}", speedX);
                    Console.WriteLine("speedY: {0}", speedY);
                    Console.WriteLine("forceX: {0}", forceX);
                    Console.WriteLine("forceY: {0}", forceY);

                    if (throwX == "left")
                    {
                        GameWindow.Left -= forceX;
                    }
                    if (throwX == "right")
                    {
                        GameWindow.Left += forceX;
                    }
                    if (throwY == "down")
                    {
                        GameWindow.Top -= forceY;
                    }
                    if (throwY == "up")
                    {
                        GameWindow.Top += forceY;
                    }
                    
                    }
                    /*speedXAvg = Convert.ToInt32(speedX / mouseCount);
                    speedYAvg = Convert.ToInt32(speedY / mouseCount);

                    if (speedXAvg > 0)
                    {
                        GameWindow.Left += 5;
                    }
                    else if (speedXAvg < 0)
                    {
                        GameWindow.Left -= 5;
                    }
                    if (speedYAvg > 0)
                    {
                        GameWindow.Top -= 5;
                    }
                    else if (speedYAvg < 0)
                    {
                        GameWindow.Top += 5;
                    }
                    mov += 1;
                }*/
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
                else if ((farWindowTop - 200 - GameWindow.Top) > 30 && standing)
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
            //avatarState = "drag";
            dragged = true;
            mouseUp = false;
            //Console.WriteLine("SHEESH");
            DragMove();
            base.OnMouseLeftButtonDown(e);
            if (ResizeMode != ResizeMode.NoResize)
            {
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Normal;
                UpdateLayout();
            }
            //Console.WriteLine("{0}:MOUSE", e.GetPosition(GameWindow).X);//
            //avatarState = "idle";
            dragged = false;
            mouseUp = true;
        }
        public void Player_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ResizeMode != ResizeMode.NoResize)
            {
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Normal;
                UpdateLayout();
            }
        }

        // Menu

        private void Player_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(MenuGrid.Visibility == Visibility.Hidden)
            {
                MenuGrid.Visibility = Visibility.Visible;
                avatarAction.avatarMove(0);
            }
            else
            {
                MenuGrid.Visibility = Visibility.Hidden;
                Console.WriteLine(action);
                avatarAction.avatarMove(action);
            }
        }

        // Avatar AI
        public void randomTimerTick(object sender, EventArgs e)
        {
            Random rnd = new Random();

            if (!dragged && MenuGrid.Visibility == Visibility.Hidden)
            {
                if (actionCount >= actionTime)
                {
                    List<int> myList = actionList.GetRange(actionList.Count - 1, 1);
                    foreach (int shit in actionList)
                    {
                        Console.Write("{0},", shit);
                    }
                    Console.WriteLine();
                    if (myList.Contains(action))
                    {
                        if(action+1 >= 3)
                        {
                            action = 0;
                        }
                        else
                        {
                            action += 1;
                        }
                        if ((myList.Contains(1) || myList.Contains(2)) && (action == 1 || action == 2))
                        {
                            avatarAction.avatarMove(0);
                            Thread.Sleep(2000);
                        }
                        avatarAction.avatarMove(action);
                        actionList.Add(action);
                        actionList.RemoveAt(0);
                        Console.WriteLine("{0}:{1} contain", action, avatarState);
                    }
                    else
                    {
                        action = rnd.Next(0, 3);
                        if ((myList.Contains(1) || myList.Contains(2)) && (action == 1 || action == 2))
                        {
                            avatarAction.avatarMove(0);
                            Thread.Sleep(2000);
                        }
                        avatarAction.avatarMove(action);
                        actionList.Add(action);
                        actionList.RemoveAt(0);
                        Console.WriteLine("{0}:{1}", action, avatarState);
                    }
                    if (action == 0)
                    {
                        actionTime = rnd.Next(10, 30);
                    }
                    else
                    {
                        actionTime = rnd.Next(15, 30);
                    }
                    actionCount = 0;
                }
                actionCount += 1;
            }
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
                if (avatarState == "idle")
                {
                    //avatarState = "idle";
                    climbing = false;
                }
                else if (avatarState == "runLeft")
                {
                    if (rectangleMaid.Left + avatarHeight - 25 < 0)
                    {
                        GameWindow.Top -= 2;
                        //Console.WriteLine(rectangleMaid.Left);
                        //avatarState = "climb";
                        climbing = true;
                    }
                    else
                    {
                        GameWindow.Left -= 2;
                        Flip.ScaleX = 1;
                        //avatarState = "run";
                        climbing = false;
                    }
                }
                else if (avatarState == "runRight")
                {
                    if (rectangleMaid.Right - avatarHeight + 20 > SCREEN_WIDTH)
                    {
                        GameWindow.Top -= 2;
                        //Console.WriteLine(rectangleMaid.Right);
                        climbing = true;
                    }
                    else
                    {
                        GameWindow.Left += 2;
                        Flip.ScaleX = -1;
                        //avatarState = "run";
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
                Console.WriteLine("{0} {1} {2} {3} {4}",GetTitle(windowLst[i]), windowRect[i].Left, windowRect[i].Right, windowRect[i].Top, windowRect[i].Bottom);
            }
            // Game window position
            // Console.WriteLine("{0}: {1}:",GameWindow.Top.ToString(),GameWindow.Left.ToString());
            // Console.WriteLine("SHEESH");
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
                        //Console.WriteLine(rectangleMaid.Left);
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
                        //Console.WriteLine(rectangleMaid.Right);
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
                case Key.Space:
                    GameWindow.Top -= 10;
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
        public static string ActionModify
        {
            get { return avatarState; }
            set { avatarState = value; }
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
