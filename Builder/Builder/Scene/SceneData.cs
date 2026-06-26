using Builder.Background;
using Builder.Dialog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Builder.Scene
{
    internal class SceneData
    {
        public string pathBackgorund {  get; set; }
        public DialogLine[] lines { get; set; }
        public bool hasChoice{  get; set; }
        public Choice[] choices { get; set; }
        public string nextScene { get; set; }
        public bool transiton { get; set; }

        //public string pathSound { get; set; }
        public SceneData()
        {
        }

        public SceneData(string pathBackgorund, DialogLine[] lines, string nextScene, bool transiton = false)
        {
            this.pathBackgorund = pathBackgorund;
            this.lines = lines;
            this.nextScene = nextScene;
            this.hasChoice = false;
            this.transiton = transiton;
        }

        public SceneData(string pathBackgorund, DialogLine[] lines, Choice[] choices, bool transiton = false)
        {
            this.pathBackgorund = pathBackgorund;
            this.lines = lines;

            this.choices = choices;
            this.hasChoice = true;
            this.transiton = transiton;
        }
    }
}
