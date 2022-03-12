using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Maid
{
    public class AvatarActions
    {
        public void avatarMove(int action)
        {
            switch (action)
            {
                case 0:
                    if (!MainWindow.actionList.GetRange(MainWindow.actionList.Count - 1, 1).Contains(action))
                    {
                        MainWindow.avatarState = "idle";
                        Console.WriteLine("IDLE");
                    }
                    break;
                case 1:
                    if (!MainWindow.actionList.GetRange(MainWindow.actionList.Count - 2, 2).Contains(action))
                    {
                        MainWindow.avatarState = "runRight";
                        Console.WriteLine("RIGHT");
                    }
                    break;
                case 2:
                    if (!MainWindow.actionList.GetRange(MainWindow.actionList.Count - 2, 2).Contains(action))
                    {
                        MainWindow.avatarState = "runLeft";
                        Console.WriteLine("LEFT");
                    }
                    break;
            }
        }
        public void avatarTalk()
        {
            Console.WriteLine("LEFT");
        }
        public void avatarSearch()
        {

        }
        public void avatarSleep()
        {

        }
    }
}
