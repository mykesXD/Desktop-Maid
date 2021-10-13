using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Maid
{
    class Animations
    {
        List<string> idleAnimation = new List<string>
        {
            "pack://application:,,,/images/idle1.png"
        };
        List<string> runAnimation = new List<string>
        {
            "pack://application:,,,/images/run1.png",
            "pack://application:,,,/images/run2.png"
        };
        List<string> climbAnimation = new List<string>
        {
            "pack://application:,,,/images/climb1.png",
            "pack://application:,,,/images/climb2.png",
            "pack://application:,,,/images/climb3.png"
        };
        List<string> dragAnimation = new List<string>
        {
            "pack://application:,,,/images/drag.png",

        };
        List<string> dragRight = new List<string>
        {
            //"pack://application:,,,/images/dragRight1.png",
            "pack://application:,,,/images/dragRight2.png",

        };
        List<string> dragLeft = new List<string>
        {
            //"pack://application:,,,/images/dragLeft1.png",
            "pack://application:,,,/images/dragLeft2.png",

        };
        List<string> fallAnimation = new List<string>
        {
            "pack://application:,,,/images/fall1.png",
        };
        int playerCounter = 0;

        public void PlayPlayerAnimation(Rectangle avatarCanvas, string avatarState)
        {

            if (avatarState == "idle")
            {
                if (playerCounter >= idleAnimation.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(idleAnimation[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            else if (avatarState == "run")
            {
                if (playerCounter >= runAnimation.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(runAnimation[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            else if (avatarState == "drag")
            {
                if (playerCounter >= dragAnimation.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(dragAnimation[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            else if (avatarState == "dragRight")
            {
                if (playerCounter >= dragRight.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(dragRight[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            else if (avatarState == "dragLeft")
            {
                if (playerCounter >= dragLeft.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(dragLeft[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            else if (avatarState == "fall")
            {
                if (playerCounter >= fallAnimation.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(fallAnimation[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            else if (avatarState == "climb")
            {
                if (playerCounter >= climbAnimation.Count)
                {
                    playerCounter = 0;
                }
                avatarCanvas.Fill = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(climbAnimation[playerCounter], UriKind.Absolute))
                };
                playerCounter++;
            }
            
        }
    }
}
