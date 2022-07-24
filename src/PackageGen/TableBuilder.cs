using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageGen
{
    public readonly struct Row
    {
        public string[] Cells { get; init; }

        public Row()
        {
            Cells = Array.Empty<string>();
        }

        public Row(params string[] cells)
        {
            Cells = cells;
        }
    }

    public class TableBuilder
    {
        private const string TOP_LEFT = "┌";
        private const string TOP_RIGHT = "┐";
        private const string BOTTOM_RIGHT = "┘";
        private const string BOTTOM_LEFT = "┌";
        private const string HORIZONTAL = "─";
        private const string VERTICAL = "│";
        private const string INTERSECTION = "┼";
        private const string INTERSECTION_RIGHT = "├";
        private const string INTERSECTION_LEFT = "┤";

        public List<Row> Rows { get; init; }
        public int MaxLineWidth { get; set; }

        private List<CellInformation>? _cellInfo;
        private int _minimumChopLength;

        public TableBuilder()
        {
            Rows = new List<Row>();
            MaxLineWidth = Console.BufferWidth - 1;
        }

        public void Print()
        {

        }

        public override string ToString()
        {
            PopulateCellInfo();
            GetRowString(Rows.First());
            return "";
        }

        private string GetRowString(Row row)
        {
            var builder = new StringBuilder();
            int lineCount = _cellInfo.Max(c => c.LineWidths.Count);

            for (int i = 0; i < lineCount; i++)
            {

                for (int j = 0; j < row.Cells.Length; j++)
                {
                    var curField = row.Cells[j];
                    var curInfo = _cellInfo[j];

                    curField = curField.Replace("\t", "").Replace("\r", "").Replace("\n", "");

                    if(curInfo.LineWidths.Count <= i)
                    {
                        builder.Append(new string(' ', curInfo.LongestLine));
                        break;
                    }
                    int lineStart = 0;
                    if (i > 0)
                    {
                        lineStart = curInfo.LineWidths[i - 1];
                    }

                    builder.Append(curField, lineStart, curInfo.LineWidths[i]);
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }


        private void PopulateCellInfo()
        {
            _cellInfo = new List<CellInformation>();
            _cellInfo.Clear();
            foreach (var row in Rows)
            {
                for (int i = 0; i < row.Cells.Length; i++)
                {
                    CellInformation? curCellInfo = null;
                    if(i >= _cellInfo.Count)
                    {
                        curCellInfo = new CellInformation(i);
                        _cellInfo.Add(curCellInfo);
                    }
                    else
                    {
                        curCellInfo = _cellInfo[i];
                    }

                    if (row.Cells[i].Length >= curCellInfo.TotalWidth)
                    {
                        curCellInfo.LineWidths.Clear();
                        curCellInfo.TotalWidth = row.Cells[i].Length;
                        curCellInfo.LineWidths.Add(curCellInfo.TotalWidth);
                    }
                }
            }
            _minimumChopLength = (MaxLineWidth - FindSpecialCharsRowLength()) / _cellInfo.Count;

            var cellsOrderedBySize = _cellInfo.OrderBy(c => -c.LongestLine);
            int nextCellToChop = 0;
            int longestRowByCells = FindLongestRowByCells();
            while (longestRowByCells > MaxLineWidth)
            {
                if(nextCellToChop >= _cellInfo.Count)
                {
                    nextCellToChop = 0;
                }

                var cellToChop = cellsOrderedBySize.ElementAt(nextCellToChop);

                // Where do we need to chop it to make others fit?
                int chopPos = longestRowByCells - MaxLineWidth;
                if(chopPos < _minimumChopLength)
                {
                    chopPos = _minimumChopLength;
                }

                int lastLineWidthIndex = cellToChop.LineWidths.Count - 1;
                int nextWidth = cellToChop.LineWidths[lastLineWidthIndex] - chopPos;
                cellToChop.LineWidths[lastLineWidthIndex] = chopPos;
                cellToChop.LineWidths.Add(nextWidth);

                longestRowByCells = FindLongestRowByCells();
                nextCellToChop++;
            }
        }

        private int FindSpecialCharsRowLength()
        {
            return VERTICAL.Length + (_cellInfo.Count * VERTICAL.Length - 1) + VERTICAL.Length;
        }

        private int FindLongestRowByCells()
        {
            var rowLen = 0;
            for (int i = 0; i < _cellInfo.Count; i++)
            {
                CellInformation? item = _cellInfo[i];
                rowLen += item.LongestLine;
            }
            return rowLen + FindSpecialCharsRowLength();
        }


        private int FindRowWidth(Row row)
        {
            return VERTICAL.Length + row.Cells.Select(c => c.Length).Aggregate((x, y) => x + y) + row.Cells.Length - 1 + VERTICAL.Length;
        }

        private class CellInformation
        {
            public int Index;
            public int TotalWidth;
            public int LongestLine
            {
                get
                {
                    if(LineWidths.Count == 0)
                    {
                        return TotalWidth;
                    }
                    return LineWidths.Max(x => x);
                }
            }
            public List<int> LineWidths { get; private set; }

            public CellInformation(int cellIndex)
            {
                Index = cellIndex;
                TotalWidth = -1;
                LineWidths = new List<int>();
            }
        }
    }
}
