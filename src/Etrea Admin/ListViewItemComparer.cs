using System.Collections;
using System.Windows.Forms;

namespace Etrea_Admin
{
    public class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder sortOrder;
        private CaseInsensitiveComparer objectComparer;

        public ListViewItemComparer()
        {
            col = 0;
            sortOrder = SortOrder.Ascending;
            objectComparer = new CaseInsensitiveComparer();
        }

        public ListViewItemComparer(int column, SortOrder order)
        {
            this.col = column;
            this.sortOrder = order;
            objectComparer = new CaseInsensitiveComparer();
        }

        public int SortColumn
        {
            set { col = value; }
            get { return col; }
        }

        public SortOrder SortOrder
        {
            set { sortOrder = value; }
            get { return sortOrder; }
        }

        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listViewX, listViewY;
            listViewX = (ListViewItem)x;
            listViewY = (ListViewItem)y;
            if (listViewX.Checked && !listViewY.Checked)
            {
                return -1;
            }
            if (!listViewX.Checked && listViewY.Checked)
            {
                return 1;
            }
            compareResult = objectComparer.Compare(listViewX.SubItems[col].Text, listViewY.SubItems[col].Text);
            switch(sortOrder)
            {
                case SortOrder.Ascending:
                    return compareResult;

                case SortOrder.Descending:
                    return (-compareResult);

                default:
                    return 0;
            }
        }
    }
}
