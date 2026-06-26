using System;
using System.Collections.Generic;
using System.Text;

namespace Builder.Background
{
    public class GifAnimator
    {
        private Image gif;

        public GifAnimator(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                gif = Image.FromFile(path);

                // pornește animația internă
                ImageAnimator.Animate(gif, OnFrameChanged);
            }
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            // forțează redraw
            if (OnFrameUpdate != null)
                OnFrameUpdate();
        }

        public Action OnFrameUpdate;

        public void Draw(Graphics g, RectangleF rect)
        {

            if (gif != null)
            {
                ImageAnimator.UpdateFrames(gif);
                g.DrawImage(gif, rect);
            }
        }
    }
}
