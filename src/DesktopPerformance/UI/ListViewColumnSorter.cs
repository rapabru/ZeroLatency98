using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DesktopPerformance.UI;

/// <summary>
/// Provides intelligent multi-column sorting for WinForms ListView controls.
/// Supports natural numeric extraction (e.g. '60 Hz' vs '144 Hz', '6 instances' vs '46 instances'),
/// toggling between Ascending and Descending orders, and visual header indicators (▲ / ▼).
/// </summary>
public partial class ListViewColumnSorter : IComparer
{
    private static readonly Regex LeadingNumberRegex = new(@"^[-+]?\d+(\.\d+)?", RegexOptions.Compiled);

    public int SortColumn { get; set; } = 0;
    public SortOrder Order { get; set; } = SortOrder.None;

    public int Compare(object? x, object? y)
    {
        if (Order == SortOrder.None) return 0;
        if (x is not ListViewItem itemX || y is not ListViewItem itemY) return 0;

        string textX = itemX.SubItems.Count > SortColumn ? itemX.SubItems[SortColumn].Text : string.Empty;
        string textY = itemY.SubItems.Count > SortColumn ? itemY.SubItems[SortColumn].Text : string.Empty;

        int result = CompareValues(textX, textY);

        // Secondary tie-breaker: sort by Column 0 (name/identifier) if primary column values match
        if (result == 0 && SortColumn != 0)
        {
            string primaryX = itemX.Text;
            string primaryY = itemY.Text;
            result = CompareValues(primaryX, primaryY);
        }

        return Order == SortOrder.Descending ? -result : result;
    }

    /// <summary>
    /// Performs smart comparison: parses numbers or leading numbers if present,
    /// otherwise falls back to cultural case-insensitive string comparison.
    /// </summary>
    public static int CompareValues(string a, string b)
    {
        bool hasNumA = TryExtractNumber(a, out double numA);
        bool hasNumB = TryExtractNumber(b, out double numB);

        if (hasNumA && hasNumB)
        {
            return numA.CompareTo(numB);
        }

        return string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Attempts to extract a numeric value from the beginning of a string,
    /// handling cases like '60 Hz', '46 instance(s)', '-2.5%', '1920 x 1080'.
    /// </summary>
    public static bool TryExtractNumber(string s, out double number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;

        string trimmed = s.Trim().TrimStart('+');

        // Direct parse
        if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out number))
            return true;
        if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out number))
            return true;

        // Leading numeric pattern
        var match = LeadingNumberRegex.Match(trimmed);
        if (match.Success)
        {
            if (double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                return true;
            if (double.TryParse(match.Value, NumberStyles.Float, CultureInfo.CurrentCulture, out number))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Attaches column click sorting and visual indicator management to any ListView.
    /// </summary>
    public static ListViewColumnSorter Attach(ListView listView)
    {
        var sorter = new ListViewColumnSorter();
        listView.ListViewItemSorter = sorter;

        // Cache initial column titles
        foreach (ColumnHeader col in listView.Columns)
        {
            col.Tag ??= col.Text;
        }

        listView.ColumnClick += (sender, e) =>
        {
            if (sorter.SortColumn == e.Column)
            {
                // Toggle order
                sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                sorter.SortColumn = e.Column;
                sorter.Order = SortOrder.Ascending;
            }

            UpdateHeaderIndicators(listView, sorter);
            listView.Sort();
        };

        return sorter;
    }

    /// <summary>
    /// Updates column headers to display ▲ (Ascending) or ▼ (Descending) on the active sort column.
    /// </summary>
    public static void UpdateHeaderIndicators(ListView listView, ListViewColumnSorter sorter)
    {
        foreach (ColumnHeader col in listView.Columns)
        {
            string baseTitle = col.Tag as string ?? col.Text.TrimEnd(' ', '▲', '▼');
            col.Tag = baseTitle;

            if (col.Index == sorter.SortColumn && sorter.Order != SortOrder.None)
            {
                col.Text = sorter.Order == SortOrder.Ascending ? $"{baseTitle} ▲" : $"{baseTitle} ▼";
            }
            else
            {
                col.Text = baseTitle;
            }
        }
    }
}
