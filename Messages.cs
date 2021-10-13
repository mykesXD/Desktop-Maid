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
        List<string> morning = new List<string>
        {
            "Pretty early today huh?",
            "Morning! How’s it going?",
            "Ohayou Sekai!",
        };
        List<string> demo = new List<string>
        {
            "Hello it's my first time here",
            "My name is- Sorry seems like I forgot my name$Will you give me a new name?",
            "Great! Since I'm a bot, coming up with new ideas is pretty hard..",
            "I'll be renting your screen for the time being so let's get along :)"
        };
        public void sendMessage(TextBox SpeechBox, int index)
        {
            SpeechBox.Text = demo.ElementAt(index);
            
        }

    }
}
