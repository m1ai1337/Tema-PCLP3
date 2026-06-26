using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Builder.Dialog
{
    internal class DialogSystem
    {
        private System.Windows.Forms.Timer timer;
        private Font fontName;
        private Font fontText;
        private Queue<DialogLine> queue = new Queue<DialogLine>();
        private DialogLine current;
        private int index = 0;
        private string visibleText = string.Empty;
        private bool isWaiting = false;
        public bool IsActive { get; set; } = false;
        public DialogSystem()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;
            timer.Tick += Update;
            this.fontName = SystemFonts.DefaultFont;
            this.fontText = SystemFonts.DefaultFont;
        }

        public DialogSystem(Font fontName, Font fontText)
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 20;
            timer.Tick += Update;
            this.fontName = fontName;
            this.fontText = fontText;
        }

        private void Update(object sender, EventArgs e)
        {
            if (index < current.line.Length)
            {
                visibleText += current.line[index];
                index++;
            }
            else
            {
                timer.Stop();
                isWaiting = true;
            }
        }

        public void Add(params DialogLine[] lines)
        {
            foreach (var l in lines)
                queue.Enqueue(l);
        }
        public void Start()
        {
            if (queue.Count == 0) return;

            IsActive = true;

            LoadNext();
        }
        private void LoadNext()
        {
            current = queue.Dequeue();

            visibleText = "";
            index = 0;

            isWaiting = false;

            timer.Start();
        }
        public void Next()
        {
            if (!IsActive || current == null) return;

            // skip animation
            if (!isWaiting)
            {
                visibleText = current.line;
                index = current.line.Length;

                timer.Stop();
                isWaiting = true;

                return;
            }

            // next dialog line
            if (queue.Count > 0)
            {
                LoadNext();
            }
            else
            {
                IsActive = false;
            }
        }

        public void Draw(Graphics g, RectangleF rect)
        {
            if (!IsActive) return;

            using (Brush b = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                g.FillRectangle(b, rect);

            using (Pen p = new Pen(Color.White, 1))
                g.DrawRectangle(p, rect);

            g.DrawString(
                current.name,
                fontName,
                Brushes.Gold,
                rect.X + 10,
                rect.Y + 10);


            RectangleF textRect = new RectangleF(
                rect.X + 10,
                rect.Y + 40,
                rect.Width - 20,
                rect.Height - 60);

            g.DrawString(
                visibleText,
                fontText,
                Brushes.White,
                textRect);


            if (isWaiting)
            {
                g.DrawString(
                    "▼",
                    SystemFonts.DefaultFont,
                    Brushes.White,
                    rect.Right - 25,
                    rect.Bottom - 25);
            }
        }

        public void Clear()
        {
            queue.Clear();
            index = 0;
            visibleText = string.Empty;
            isWaiting = false;
            IsActive = false;
        }


    }
}
