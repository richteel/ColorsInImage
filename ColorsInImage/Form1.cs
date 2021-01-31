using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace ColorsInImage
{
    public partial class Form1 : Form
    {
        /*** Fields and Constants ***/
        #region
        private const int MAXCOLORSTOLIST = 256;

        private int imgWidth = 0;
        private int imgHeight = 0;
        private ImageColorInformation imageColorInformation;
        private ListViewColumnSorter lvwColumnSorter;
        #endregion

        /*** Constructor & Initialization ***/
        #region
        public Form1()
        {
            InitializeComponent();

            imageColorInformation = null;
            lvwColumnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = lvwColumnSorter;
        }
        #endregion

        /*** Event Handlers ***/
        #region
        private void ColorInformation_DoneProcessing(object sender, EventArgs e)
        {
            UpdateStatus(imgHeight * imgWidth, imgHeight * imgWidth);

            UpdateImageInformation();

            List<KeyValuePair<string, ImageColor>> myList = imageColorInformation.ColorsList.ToList();

            myList.Sort((pair1, pair2) => pair2.Value.Count.CompareTo(pair1.Value.Count));

            LoadListViewAsync(myList);

            
        }

        private void ColorInformation_RowProcessed(object sender, RowProcessedEventArgs e)
        {
            UpdateStatus(e.Row * imgWidth, e.TotalRows * imgWidth);
        }

        private void cmdAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach(ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = true;
            }
        }

        private void cmdInverse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = !lvi.Checked;
            }
        }

        private void cmdNone_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = false;
            }
        }

        private void cmdOpenImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                ResetControls();
                Image img = Image.FromFile(openFileDialog1.FileName);

                imgWidth = img.Width;
                imgHeight = img.Height;

                pictureBox1.Image = img;

                imageColorInformation = new ImageColorInformation();
                imageColorInformation.RowProcessed += ColorInformation_RowProcessed;
                imageColorInformation.DoneProcessing += ColorInformation_DoneProcessing;

                imageColorInformation.GetColorsListAsync(img);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadWebBrowserFromResourceFile(webBrowser1, "about");

            openFileDialog1.Title = "Open Image";

            LayoutControls();
            UpdateStatus(0, 0);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
            LayoutControls();
        }

        #endregion

        /*** Private Methods ***/
        #region
        private void LayoutControls()
        {
            Control panParent = cmdAll.Parent;
            int padSpace = 5;
            int nextTop = padSpace;
            int nextLeft = padSpace;

            cmdAll.Top = nextTop;
            cmdAll.Left = nextLeft;
            nextLeft = cmdAll.Left + cmdAll.Width + padSpace;

            if (nextLeft + cmdNone.Width > panParent.Width)
            {
                nextTop = nextTop + cmdAll.Height + padSpace;
                nextLeft = padSpace;
            }

            cmdNone.Top = nextTop;
            cmdNone.Left = nextLeft;
            nextLeft = cmdNone.Left + cmdNone.Width + padSpace;

            if (nextLeft + cmdInverse.Width > panParent.Width)
            {
                nextTop = nextTop + cmdInverse.Height + padSpace;
                nextLeft = padSpace;
            }

            cmdInverse.Top = nextTop;
            cmdInverse.Left = nextLeft;
            nextLeft = cmdInverse.Left + cmdInverse.Width + padSpace;

            panParent.Height = cmdInverse.Top + cmdInverse.Height + padSpace;
        }

        private void LoadHtmlColors()
        {
            LoadWebBrowserFromResourceFile(webBrowser1, "template");
            string webText = webBrowser1.DocumentText;

            for (int i=0; i<6; i++)
            {
                string forecolor = "";
                string backcolor = "";
                string count = "";

                if (listView1.CheckedItems.Count > i)
                {
                    backcolor = listView1.CheckedItems[i].SubItems[0].Text;
                    forecolor = ColorTranslator.ToHtml(listView1.CheckedItems[i].ForeColor);
                    count = listView1.CheckedItems[i].SubItems[1].Text;
                }


                webText = webText.Replace(string.Format("<color{0} />", i), backcolor);
                webText = webText.Replace(string.Format("<colortext{0} />", i), forecolor);
            }

            webBrowser1.DocumentText = webText;

            SaveHtmlFile("Sample1.html", webText);
        }

        private async void LoadListViewAsync(List<KeyValuePair<string, ImageColor>> sortedList)
        {
            listView1.Columns.Clear();
            listView1.Columns.Add("Color");
            listView1.Columns.Add("Count");

            await Task.Run(() =>
            {
                int q = 0;
                for (int i = 0; i < sortedList.Count && i < MAXCOLORSTOLIST; i++)
                {
                    ImageColor imgColor = sortedList[i].Value;

                    ListViewItem newItem = new ListViewItem(new string[] { imgColor.HtmlColor, imgColor.Count.ToString() }, null, imgColor.TextColor, imgColor.ColorValue, null);
                    newItem.Checked = i < 6;
                    LoadListViewAddItem(newItem);
                }
            });
        }

        private delegate void LoadListViewAddItemDelegate(ListViewItem newItem);

        private void LoadListViewAddItem(ListViewItem newItem)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new LoadListViewAddItemDelegate(LoadListViewAddItem), new object[] { newItem });
                return;
            }

            listView1.Items.Add(newItem);
        }

        private void LoadWebBrowserFromResourceFile(WebBrowser webBrowser, string htmlFile)
        {
            Type myType = this.GetType();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("{0}.html.{1}.html", myType.Namespace, htmlFile);
            Stream htmlFileStream = assembly.GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(htmlFileStream);
            string htmltext = reader.ReadToEnd();

            webBrowser.Navigate("about:blank");
            HtmlDocument doc = webBrowser.Document;
            doc.Write(htmltext);
            //webBrowser.DocumentText = htmltext;
        }

        private void ResetControls()
        {
            richTextBox1.Clear();
            listView1.Items.Clear();
            UpdateStatus(0, 0);
            pictureBox1.Image = null;
            LoadWebBrowserFromResourceFile(webBrowser1, "about");
        }

        private void SaveHtmlFile(string filename, string htmlText, bool showInBrowser = true)
        {
            string htmlFileFullName = System.Reflection.Assembly.GetExecutingAssembly().Location;

            htmlFileFullName = Path.GetDirectoryName(htmlFileFullName);
            htmlFileFullName = Path.Combine(htmlFileFullName, filename);

            File.WriteAllText(htmlFileFullName, htmlText);

            if (showInBrowser)
            {
                System.Diagnostics.Process.Start(htmlFileFullName);
            }
        }

        private void UpdateImageInformation()
        {
            richTextBox1.Text = string.Format("Image Information\r\n" +
                "------------------------------------\r\n" +
                "Width: {0:#,##0}\r\n" +
                "Height: {1:#,##0}\r\n" +
                "Pixels: {2:#,##0}\r\n" +
                "Distinct Colors: {3:#,##0}", 
                imgWidth, imgHeight, imgWidth * imgHeight, imageColorInformation.ColorsList.Count);
        }

        private delegate void UpdateStatusDelegate(int CurrentCount, int TotalCount);

        private void UpdateStatus(int CurrentCount, int TotalCount)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateStatusDelegate(UpdateStatus), new object[] { CurrentCount, TotalCount });
                return;
            }

            // Handle special cases
            if(TotalCount == 0)
            {
                toolStripStatusLabel1.Text = "Open an image to start";
                toolStripProgressBar2.Value = toolStripProgressBar2.Minimum;
            }
            else if(CurrentCount == TotalCount)
            {
                toolStripStatusLabel1.Text = "Done";
                toolStripProgressBar2.Value = toolStripProgressBar2.Maximum;
            }
            else
            {
                toolStripStatusLabel1.Text = string.Format("{0:#,##0} of {1:#,##0} pixels", CurrentCount, TotalCount);
                toolStripProgressBar2.Value = toolStripProgressBar2.Minimum + (int)((toolStripProgressBar2.Maximum - toolStripProgressBar2.Minimum) * ((Double)CurrentCount / (Double)TotalCount));
            }
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            LoadHtmlColors();
        }
    }
}
