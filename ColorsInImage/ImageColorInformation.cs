using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace ColorsInImage
{
    public class ImageColorInformation
    {
        /*** Fields and Constants ***/
        #region

        #endregion

        /*** Properties ***/
        #region
        public Dictionary<string, ImageColor> ColorsList { get; set; }
        #endregion

        /*** Constructor & Initialization ***/
        #region
        public ImageColorInformation()
        {
            ColorsList = null;
        }
        #endregion

        /*** Public Events ***/
        #region
        public delegate void DoneProcessingEventHandler(object sender, EventArgs args);

        public event DoneProcessingEventHandler DoneProcessing;

        void OnDoneProcessing()
        {
            DoneProcessing?.Invoke(this, new EventArgs());
        }

        /* --------------------- */

        public delegate void RowProcessedEventHandler(object sender, RowProcessedEventArgs args);

        public event RowProcessedEventHandler RowProcessed;

        void OnRowProcessed(int Row, int TotalRows)
        {
            RowProcessed?.Invoke(this, new RowProcessedEventArgs(Row, TotalRows));
        }
        #endregion

        /*** Public Methods ***/
        #region
        public async void GetColorsListAsync(Image img)
        {
            ColorsList = await Task.Run(() => ProcessImage(new Bitmap(img)));

            OnDoneProcessing();
        }
        #endregion

        /*** Private Methods ***/
        #region
        private async Task<Dictionary<string, ImageColor>> ProcessImage(Bitmap bitmap)
        {
            Dictionary<string, ImageColor> mColorsList = new Dictionary<string, ImageColor>();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    ImageColor imgColor = new ImageColor(bitmap.GetPixel(x, y));

                    if (mColorsList.ContainsKey(imgColor.HtmlColor))
                    {
                        ImageColor iColor = mColorsList[imgColor.HtmlColor];
                        iColor.Count++;
                        mColorsList[imgColor.HtmlColor] = iColor;
                    }
                    else
                    {
                        mColorsList.Add(imgColor.HtmlColor, imgColor);
                    }
                }
                OnRowProcessed(y, bitmap.Height);
            }

            return mColorsList;
        }
        #endregion
    }
}
