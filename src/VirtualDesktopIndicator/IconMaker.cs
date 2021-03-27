using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using VirtualDesktopIndicator.Helpers;

namespace VirtualDesktopIndicator
{
    public class IconMaker
    {
        private Color color;
        private Color backgroundColor;

        // Default windows tray icon size
        private const int BaseHeight = 16;
        private const int BaseWidth = 16;

        // We use half the size, because otherwise the image is rendered with incorrect anti-aliasing
        private int Height
        {
            get
            {
                var height = SystemMetricsApi.GetSystemMetrics(SystemMetric.SM_CYICON) / 2;
                return height < BaseHeight ? BaseHeight : height;
            }
        }

        private int Width
        {
            get
            {
                var width = SystemMetricsApi.GetSystemMetrics(SystemMetric.SM_CXICON) / 2;
                return width < BaseWidth ? BaseWidth : width;
            }
        }

        private int BorderThinkness => Width / BaseWidth;

        private const string FontName = "Tahoma";
        private int FontSize => (int)Math.Ceiling(Width / 1.5);
        private FontStyle FontStyle = FontStyle.Regular;

        private Font Font => new Font(FontName, FontSize, FontStyle, GraphicsUnit.Pixel);


        public IconMaker(Color color, Color backgroundColor)
        {
            this.color = color;
            this.backgroundColor = backgroundColor;
        }


        public Icon GenerateIcon(int i, bool negative = false)
        {
            var textColor = negative ? backgroundColor : color;

            var bitmap = new Bitmap(Width, Height);

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
                for (int o = 0; o < BorderThinkness; o++)
                {
                    // Top
                    g.DrawLine(pen, 0, o, Width - 1, o);

                    // Right
                    g.DrawLine(pen, o, 0, o, Height - 1);

                    // Left
                    g.DrawLine(pen, Width - 1 - o, 0, Width - 1 - o, Height - 1);

                    // Bottom
                    g.DrawLine(pen, 0, Height - 1 - o, Width - 1, Height - 1 - o);
                }
            }
            else 
            {
                g.FillRectangle(new SolidBrush(color), 0, 0, Width, Height);
            }


            // Draw text
            var textSize = g.MeasureString(i.ToString(), Font);

            // Сalculate padding to center the text
            // We can't assume that g.DrawString will round the coordinates correctly, so we do it manually
            var offsetX = (float)Math.Ceiling((Width - textSize.Width) / 2);
            var offsetY = (float)Math.Ceiling((Height - textSize.Height) / 2);

            g.DrawString(i.ToString(), Font, new SolidBrush(textColor), offsetX, offsetY);

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
