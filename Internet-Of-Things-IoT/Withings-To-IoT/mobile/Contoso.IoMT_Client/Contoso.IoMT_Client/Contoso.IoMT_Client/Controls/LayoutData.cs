// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Controls
{
    using Xamarin.Forms;

    public struct LayoutData
    {
        public LayoutData(int visibleChildCount, Size cellSize, int rows, int columns)
        {
            VisibleChildCount = visibleChildCount;
            CellSize = cellSize;
            Rows = rows;
            Columns = columns;
        }

        public int VisibleChildCount { get; private set; }

        public Size CellSize { get; private set; }

        public int Rows { get; private set; }

        public int Columns { get; private set; }
    }
}
