using System;
using System.Windows.Forms;

namespace BipChecker
{
  public partial class BuiltInParamsCheckerForm : Form
  {
    SortableBindingList<ParameterData> _data;

    public BuiltInParamsCheckerForm(
      string description,
      SortableBindingList<ParameterData> data )
    {
      _data = data;
      InitializeComponent();
      Text = description + " " + Text;
    }

    void BuiltInParamsCheckerForm_Load( object sender, EventArgs e )
    {
      dataGridView1.DataSource = _data;
      dataGridView1.Columns[0].HeaderText = "BuiltInParameter";
      dataGridView1.Columns[1].HeaderText = "Parameter Name";
      dataGridView1.Columns[2].HeaderText = "Type";
      dataGridView1.Columns[3].HeaderText = "Read/Write";
      dataGridView1.Columns[4].HeaderText = "String Value";
      dataGridView1.Columns[5].HeaderText = "Database Value";
      dataGridView1.Columns[6].HeaderText = "Parameter Group";
      dataGridView1.Columns[7].HeaderText = "Group Name";
      dataGridView1.Columns[8].HeaderText = "Shared";
      int w = dataGridView1.Width / dataGridView1.Columns.Count;
      foreach( DataGridViewColumn c in dataGridView1.Columns )
      {
        c.Width = w;
      }
    }

    void CopyToClipboardToolStripMenuItem_Click( object sender, EventArgs e )
    {
      string s = Text + "\r\n";
      foreach( ParameterData a in _data )
      {
        s += "\r\n" + a.Enum + "\t" + a.Name + "\t" + a.Type + "\t" + a.ReadWrite + "\t" + a.ValueString + "\t" + a.Value;
      }
      if( 0 < s.Length )
      {
        Clipboard.SetDataObject( s );
      }
    }
  }
}
