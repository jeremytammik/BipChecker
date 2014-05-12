using System;
using System.Collections;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;
using Autodesk.Revit.DB;

namespace BipChecker
{
  public partial class BuiltInParamsCheckerFormListView : Form
  {
    private readonly SortableBindingList<ParameterData> _data;

    private readonly ListViewItemSorter _listViewItemSorter;

    public BuiltInParamsCheckerFormListView(Element element,
      string description,
      SortableBindingList<ParameterData> data )
    {
      _data = data;
      InitializeComponent();
      Text = description + " " + Text;

      _listViewItemSorter = new ListViewItemSorter();
      lvParameters.ListViewItemSorter = _listViewItemSorter;

        tslElementType.Text = element.GetType().ToString();
    }

    private void BuiltInParamsCheckerFormListView_Load( object sender, EventArgs e )
    {
      foreach( var parameterData in _data )
      {
        ListViewItem lvi = new ListViewItem( parameterData.Enum );
        lvi.SubItems.Add( parameterData.Name );
        lvi.SubItems.Add( parameterData.Type );
        lvi.SubItems.Add( parameterData.ReadWrite );
        lvi.SubItems.Add( parameterData.ValueString );
        lvi.SubItems.Add( parameterData.Value );
        lvi.SubItems.Add( parameterData.ParameterGroup );
        lvi.SubItems.Add( parameterData.GroupName );
        lvi.SubItems.Add(parameterData.Shared);
        lvi.SubItems.Add(parameterData.Guid);
      
        lvi.Tag = parameterData;

        lvParameters.Items.Add( lvi );
      }
      cboGroupPArameterType.SelectedIndex = 1;
    }

    private ListViewGroup GetListViewGroupForParameter( ListView listView, string groupName )
    {
      ListViewGroup listViewGroup = listView.Groups[groupName];
      if( listViewGroup == null )
        listViewGroup = listView.Groups.Add( groupName, groupName );

      return listViewGroup;
    }

    private void listView1_ColumnClick( object sender, ColumnClickEventArgs e )
    {
      if( _listViewItemSorter.HeaderIndex == e.Column )
      {
        _listViewItemSorter.SortOrder = _listViewItemSorter.SortOrder == SortOrder.Ascending ? 
            SortOrder.Descending : SortOrder.Ascending;
      }
      else
      {
        _listViewItemSorter.HeaderIndex = e.Column;
        _listViewItemSorter.SortOrder = SortOrder.Ascending;
      }
      lvParameters.Sort();
    }

    private void cboGroupPArameterType_SelectedIndexChanged( object sender, EventArgs e )
    {
      GroupingParameters( cboGroupPArameterType.SelectedIndex );
    }

    private void GroupingParameters( int groupingType )
    {
      lvParameters.Groups.Clear();

      foreach( ListViewItem item in lvParameters.Items )
      {
        ParameterData parameterData = item.Tag as ParameterData;
        if( parameterData == null ) continue;

        switch( groupingType )
        {
          case 1:
            if( "Y" == parameterData.ContainedInCollection )
              item.Group = GetListViewGroupForParameter( item.ListView, "Parameter in Element.Parameters collection" );
            else
              item.Group = GetListViewGroupForParameter( item.ListView, "Parameter retrieving via Element.get_Parameter(BuiltInParameter)" );
            break;
          case 2:
            item.Group = GetListViewGroupForParameter( item.ListView, parameterData.Type );
            break;
          case 3:
            item.Group = GetListViewGroupForParameter( item.ListView, parameterData.ReadWrite );
            break;
          case 4:
            item.Group = GetListViewGroupForParameter( item.ListView, parameterData.GroupName );
            break;
        }
      }
    }
  }

  public class ListViewItemSorter : IComparer
  {
    public ListViewItemSorter()
    {
      HeaderIndex = 0;
      SortOrder = SortOrder.None;
    }

    public int HeaderIndex { get; set; }

    public SortOrder SortOrder { get; set; }

    public int Compare( object x, object y )
    {
      ListViewItem lvi1 = (ListViewItem) x;
      ListViewItem lvi2 = (ListViewItem) y;

      int compareResult =
          Comparer.Default.Compare( lvi1.SubItems[HeaderIndex].Text, lvi2.SubItems[HeaderIndex].Text );

      if( compareResult == 0 )
        return compareResult;

      if( SortOrder == SortOrder.Ascending )
        return compareResult;
      if( SortOrder == SortOrder.Descending )
        return -compareResult;

      return 0;
    }
  }
}
