using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorsInImage
{
    public class RowProcessedEventArgs
    {
        /*** Properties ***/
        #region
        public int Row { get; }

        public int TotalRows { get; }
        #endregion

        /*** Constructor & Initialization ***/
        #region
        public RowProcessedEventArgs(int Row, int TotalRows)
        {
            this.Row = Row;
            this.TotalRows = TotalRows;
        }
        #endregion
    }
}
