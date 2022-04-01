using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Maid
{
    class Messages
    {
        List<string> messagesMorning = new List<string>
        {
            "Pretty early today huh?",
            "Morning! How’s it going?",
            "Today weather seems to be really chilly. Be sure to dress up well if you decide to go outside",
            "I neeed caffiene",
            "Ohayou Sekai!.. It means good morning in japanese. The more you know :)", 
            "Did you ate your breakfast yet?"
        };
        List<string> pm12to6 = new List<string>
        {
            "Didn't you had a class today?",
            "Getting tired already lol"
        };
        List<string> demo = new List<string>
        {
            "Hello it's my first time here",
            "My name is- Sorry seems like I forgot my name$Will you give me a new name?",
            "Great! Since I'm a bot, coming up with new ideas is pretty hard..",
            "I'll be renting your screen for the time being so let's get along :)"
        };
        List<string> random = new List<string>
        {

        };
        public void sendMessage(TextBox SpeechBox, int index)
        {
            SpeechBox.Text = demo.ElementAt(index);
            
        }

    }
}
