using System.Drawing;

namespace ColorsInImage
{
    public struct ImageColor
    {
        public Color ColorValue { get; }

        public int Count { get; set; }

        public string HtmlColor { get; }

        public Color TextColor { get; }

        public ImageColor(Color ColorValue)
        {
            this.ColorValue = ColorValue;
            Count = 1;
            HtmlColor = ColorTranslator.ToHtml(ColorValue);

            if (ColorValue.GetBrightness() < 0.55)
                TextColor = Color.White;
            else
                TextColor = Color.Black;
        }
    }
}
