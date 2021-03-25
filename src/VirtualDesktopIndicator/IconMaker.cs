using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace VirtualDesktopIndicator
{
    public class IconMaker
    {
        private Font font;
        private int borderThinkness;
        private Size size;
        private Color color;
        private Color backgroundColor;


        public IconMaker(Font font, int borderThinkness, Size size, Color color, Color backgroundColor)
        {
            this.font = font;
            this.borderThinkness = borderThinkness;
            this.size = size;
            this.color = color;
            this.backgroundColor = backgroundColor;
        }


        public Icon GenerateIcon(int i, bool negative = false)
        {
            var textColor = negative ? backgroundColor : color;

            var bitmap = new Bitmap(size.Width, size.Height);

            var g = Graphics.FromImage(bitmap);

            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            if (!negative)
            {
                // Draw only border
                g.Clear(Color.Transparent);

                // The g.DrawRectangle always uses anti-aliasing and border looks very poor at such small resolutions
                // Implement own hack!
                var pen = new Pen(color, 1);
                for (int o = 0; o < borderThinkness; o++)
                {
                    // Top
                    g.DrawLine(pen, 0, o, size.Width - 1, o);

                    // Right
                    g.DrawLine(pen, o, 0, o, size.Height - 1);

                    // Left
                    g.DrawLine(pen, size.Width - 1 - o, 0, size.Width - 1 - o, size.Height - 1);

                    // Bottom
                    g.DrawLine(pen, 0, size.Height - 1 - o, size.Width - 1, size.Height - 1 - o);
                }
            }
            else 
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, size.Width, size.Height);
            }


            // Draw text
            var textSize = g.MeasureString(i.ToString(), font);

            // Сalculate padding to center the text
            // We can't assume that g.DrawString will round the coordinates correctly, so we do it manually
            var offsetX = (float)Math.Ceiling((size.Width - textSize.Width) / 2);
            var offsetY = (float)Math.Ceiling((size.Height - textSize.Height) / 2);

            g.DrawString(i.ToString(), font, new SolidBrush(textColor), offsetX, offsetY);

            // Create icon from bitmap and return it
            // bitmapText.GetHicon() can throw exception
            try
            {
                return Icon.FromHandle(bitmap.GetHicon());
            }
            catch
            {
                return null;
            }
        }


    }
}
