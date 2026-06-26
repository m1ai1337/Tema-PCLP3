using System;
using System.Collections.Generic;
using System.Text;

namespace Builder.Scene
{
    public class Choice
    {
        public string text { get; set; }
        public string nextScene { get; set; }

        public Choice(string text, string nextScene)
        {
            this.text = text;
            this.nextScene = nextScene;
        }
    }
}
