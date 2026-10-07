using DesktopPerformance.UI;

namespace DesktopPerformance.Tests;

public class ListViewColumnSorterTests
{
    [Fact]
    public void ListViewColumnSorter_SortsStatesAscendingAndDescending()
    {
        var sorter = new ListViewColumnSorter
        {
            SortColumn = 2,
            Order = SortOrder.Ascending
        };

        var item1 = new ListViewItem("Discord");
        item1.SubItems.Add("Discord");
        item1.SubItems.Add("RunningNormal");

        var item2 = new ListViewItem("OneDrive");
        item2.SubItems.Add("OneDrive");
        item2.SubItems.Add("NotRunning");

        var item3 = new ListViewItem("Spotify");
        item3.SubItems.Add("Spotify");
        item3.SubItems.Add("Suspended");

        var item4 = new ListViewItem("Steam Game Client");
        item4.SubItems.Add("steam");
        item4.SubItems.Add("Whitelisted");

        // Ascending comparison: NotRunning < RunningNormal < Suspended < Whitelisted
        Assert.True(sorter.Compare(item2, item1) < 0); // NotRunning < RunningNormal
        Assert.True(sorter.Compare(item1, item3) < 0); // RunningNormal < Suspended
        Assert.True(sorter.Compare(item3, item4) < 0); // Suspended < Whitelisted

        // Descending comparison
        sorter.Order = SortOrder.Descending;
        Assert.True(sorter.Compare(item2, item1) > 0);
        Assert.True(sorter.Compare(item4, item3) < 0); // Whitelisted comes before Suspended in descending
    }

    [Fact]
    public void ListViewColumnSorter_NaturalNumericComparison()
    {
        var sorter = new ListViewColumnSorter
        {
            SortColumn = 0,
            Order = SortOrder.Ascending
        };

        var item6 = new ListViewItem("6 instance(s) running (PIDs: 25500, 1788)");
        var item46 = new ListViewItem("46 instance(s) running (PIDs: 32396, 18744)");
        var item1 = new ListViewItem("1 instance(s) running (PIDs: 22908)");

        // Numeric extract: 1 < 6 < 46
        Assert.True(sorter.Compare(item1, item6) < 0);
        Assert.True(sorter.Compare(item6, item46) < 0);
        Assert.True(sorter.Compare(item1, item46) < 0);
    }

    [Fact]
    public void ListViewColumnSorter_RefreshRatesNumericComparison()
    {
        var sorter = new ListViewColumnSorter
        {
            SortColumn = 0,
            Order = SortOrder.Ascending
        };

        var hz60 = new ListViewItem("60 Hz");
        var hz144 = new ListViewItem("144 Hz");
        var hz240 = new ListViewItem("240 Hz");

        Assert.True(sorter.Compare(hz60, hz144) < 0);
        Assert.True(sorter.Compare(hz144, hz240) < 0);
    }

    [Fact]
    public void ListViewColumnSorter_TieBreakerUsesColumnZero()
    {
        var sorter = new ListViewColumnSorter
        {
            SortColumn = 2,
            Order = SortOrder.Ascending
        };

        var itemA = new ListViewItem("Adobe CC Process");
        itemA.SubItems.Add("CCXProcess");
        itemA.SubItems.Add("NotRunning");

        var itemB = new ListViewItem("Dropbox");
        itemB.SubItems.Add("Dropbox");
        itemB.SubItems.Add("NotRunning");

        // Both are "NotRunning", so column 0 ("Adobe CC Process" vs "Dropbox") determines order
        Assert.True(sorter.Compare(itemA, itemB) < 0);
    }

    [Fact]
    public void ListViewColumnSorter_UpdateHeaderIndicators_SetsGlyphsCorrectly()
    {
        using var lv = new ListView();
        lv.Columns.Add("Application");
        lv.Columns.Add("Process");
        lv.Columns.Add("State");

        var sorter = ListViewColumnSorter.Attach(lv);

        // Click State (Column index 2)
        sorter.SortColumn = 2;
        sorter.Order = SortOrder.Ascending;
        ListViewColumnSorter.UpdateHeaderIndicators(lv, sorter);

        Assert.Equal("Application", lv.Columns[0].Text);
        Assert.Equal("Process", lv.Columns[1].Text);
        Assert.Equal("State ▲", lv.Columns[2].Text);

        // Toggle to Descending
        sorter.Order = SortOrder.Descending;
        ListViewColumnSorter.UpdateHeaderIndicators(lv, sorter);
        Assert.Equal("State ▼", lv.Columns[2].Text);

        // Click Application (Column index 0)
        sorter.SortColumn = 0;
        sorter.Order = SortOrder.Ascending;
        ListViewColumnSorter.UpdateHeaderIndicators(lv, sorter);
        Assert.Equal("Application ▲", lv.Columns[0].Text);
        Assert.Equal("State", lv.Columns[2].Text);
    }
}
