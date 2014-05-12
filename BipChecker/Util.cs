#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion // Namespaces

namespace BipChecker
{
  /// <summary>
  /// A collection of utility methods reused in several labs.
  /// </summary>
  static class Util
  {
    #region Formatting and message handlers
    public const string Caption = "Built-in Parameter Checker";

    /// <summary>
    /// Return an English plural suffix 's' or
    /// nothing for the given number of items.
    /// </summary>
    public static string PluralSuffix( int n )
    {
      return 1 == n ? "" : "s";
    }

    /// <summary>
    /// Return a dot for zero items, or a colon for more.
    /// </summary>
    public static string DotOrColon( int n )
    {
      return 0 < n ? ":" : ".";
    }

    /// <summary>
    /// Format a real number and return its string representation.
    /// </summary>
    public static string RealString( double a )
    {
      return a.ToString( "0.##" );
    }

    /// <summary>
    /// Format a point or vector and return its string representation.
    /// </summary>
    public static string PointString( XYZ p )
    {
      return string.Format( "({0},{1},{2})",
        RealString( p.X ), RealString( p.Y ), RealString( p.Z ) );
    }

    /// <summary>
    /// Return a description string for a given element.
    /// </summary>
    public static string ElementDescription( Element e )
    {
      string description = ( null == e.Category )
        ? e.GetType().Name
        : e.Category.Name;

      FamilyInstance fi = e as FamilyInstance;

      if( null != fi )
      {
        description += " '" + fi.Symbol.Family.Name + "'";
      }

      if( null != e.Name )
      {
        description += " '" + e.Name + "'";
      }
      return description;
    }

    /// <summary>
    /// Return a description string including element id for a given element.
    /// </summary>
    public static string ElementDescription( Element e, bool includeId )
    {
      string description = ElementDescription( e );
      if( includeId )
      {
        description += " " + e.Id.IntegerValue.ToString();
      }
      return description;
    }

    /// <summary>
    /// Revit TaskDialog wrapper for a short informational message.
    /// </summary>
    public static void InfoMsg( string msg )
    {
      Debug.WriteLine( msg );

      //WinForms.MessageBox.Show( msg, Caption, WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information );

      TaskDialog.Show( Caption, msg, TaskDialogCommonButtons.Ok );
    }

    /// <summary>
    /// Revit TaskDialog wrapper for a message
    /// with separate main instruction and content.
    /// </summary>
    public static void InfoMsg( string msg, string content )
    {
      Debug.WriteLine( msg );
      Debug.WriteLine( content );
      TaskDialog d = new TaskDialog( Caption );
      d.MainInstruction = msg;
      d.MainContent = content;
      d.Show();
    }

    /// <summary>
    /// MessageBox wrapper for error message.
    /// </summary>
    public static void ErrorMsg( string msg )
    {
      Debug.WriteLine( msg );

      //WinForms.MessageBox.Show( msg, Caption, WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error );

      TaskDialog d = new TaskDialog( Caption );
      d.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
      d.MainInstruction = msg;
      d.Show();
    }

    /// <summary>
    /// MessageBox wrapper for question message.
    /// </summary>
    public static bool QuestionMsg( string msg )
    {
      Debug.WriteLine( msg );

      //bool rc = WinForms.DialogResult.Yes
      //  == WinForms.MessageBox.Show( msg, Caption, WinForms.MessageBoxButtons.YesNo, WinForms.MessageBoxIcon.Question );
      //Debug.WriteLine( rc ? "Yes" : "No" );
      //return rc;

      TaskDialog d = new TaskDialog( Caption );
      d.MainIcon = TaskDialogIcon.TaskDialogIconNone;
      d.MainInstruction = msg;
      //d.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
      d.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Instance parameters");
      d.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Type parameters");
      //d.DefaultButton = TaskDialogResult.Yes;
      //return TaskDialogResult.Yes == d.Show();
      return d.Show() == TaskDialogResult.CommandLink1;
    }

    /// <summary>
    /// MessageBox wrapper for question and cancel message.
    /// </summary>
    public static TaskDialogResult QuestionCancelMsg( string msg )
    {
      Debug.WriteLine( msg );

      //WinForms.DialogResult rc = WinForms.MessageBox.Show( msg, Caption, WinForms.MessageBoxButtons.YesNoCancel, WinForms.MessageBoxIcon.Question );
      //Debug.WriteLine( rc.ToString() );
      //return rc;

      TaskDialog d = new TaskDialog( Caption );
      d.MainIcon = TaskDialogIcon.TaskDialogIconNone;
      d.MainInstruction = msg;
      d.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel;
      d.DefaultButton = TaskDialogResult.Yes;
      return d.Show();
    }
    #endregion // Formatting and message handlers

    #region Selection
    public static Element
      GetSingleSelectedElementOrPrompt(
        UIDocument uidoc )
    {
      Element e = null;

      ICollection<ElementId> ids
        = uidoc.Selection.GetElementIds(); // 2015

      if( 1 == ids.Count )
      {
        foreach( ElementId id in ids )
        {
          e = uidoc.Document.GetElement( id );
        }
      }
      else
      {
        string sid;
        DialogResult result = DialogResult.OK;
        while( null == e && DialogResult.OK == result )
        {
          using( ElementIdForm form
            = new ElementIdForm() )
          {
            result = form.ShowDialog();
            sid = form.ElementId;
          }
          if( DialogResult.OK == result )
          {
            if( 0 == sid.Length )
            {
              try
              {
                Reference r = uidoc.Selection.PickObject(
                  ObjectType.Element,
                  "Please pick an element" );

                //e = r.Element; // 2011
                e = uidoc.Document.GetElement( r ); // 2012
              }
              catch( OperationCanceledException )
              {
              }
            }
            else
            {
              int id;
              if( int.TryParse( sid, out id ) )
              {
                ElementId elementId = new ElementId(
                    id );


                e = uidoc.Document.GetElement( elementId );
                if( null == e )
                {
                  ErrorMsg( string.Format(
                      "Invalid element id '{0}'.",
                      sid ) );
                }
              }
              else
              {
                e = uidoc.Document.GetElement( sid );
                if( null == e )
                {
                  ErrorMsg( string.Format(
                      "Invalid element id '{0}'.",
                      sid ) );
                }
              }
            }
          }
        }
      }
      return e;
    }

    /// <summary>
    /// A selection filter for a specific System.Type.
    /// </summary>
    class TypeSelectionFilter : ISelectionFilter
    {
      Type _type;

      public TypeSelectionFilter( Type type )
      {
        _type = type;
      }

      /// <summary>
      /// Allow an element of the specified System.Type to be selected.
      /// </summary>
      /// <param name="element">A candidate element in selection operation.</param>
      /// <returns>Return true for specified System.Type, false for all other elements.</returns>
      public bool AllowElement( Element e )
      {
        //return null != e.Category
        // && e.Category.Id.IntegerValue == ( int ) _bic;

        return e.GetType().Equals( _type );
      }

      /// <summary>
      /// Allow all the reference to be selected
      /// </summary>
      /// <param name="refer">A candidate reference in selection operation.</param>
      /// <param name="point">The 3D position of the mouse on the candidate reference.</param>
      /// <returns>Return true to allow the user to select this candidate reference.</returns>
      public bool AllowReference( Reference r, XYZ p )
      {
        return true;
      }
    }

    public static Element GetSingleSelectedElementOrPrompt(
      UIDocument uidoc,
      Type type )
    {
      Element e = null;

      ICollection<ElementId> ids
        = uidoc.Selection.GetElementIds(); // 2015

      if( 1 == ids.Count )
      {
        Element e2 = null;

        foreach( ElementId id in ids )
        {
          e2 = uidoc.Document.GetElement( id );
        }
        Type t = e2.GetType();

        if( t.Equals( type ) || t.IsSubclassOf( type ) )
        {
          e = e2;
        }
      }

      if( null == e )
      {
        try
        {
          Reference r = uidoc.Selection.PickObject(
            ObjectType.Element,
            new TypeSelectionFilter( type ),
            string.Format( "Please pick a {0} element", type.Name ) );

          //e = r.Element; // 2011
          e = uidoc.Document.GetElement( r ); // 2012
        }
        catch( OperationCanceledException )
        {
        }
      }
      return e;
    }
    #endregion // Selection

    #region Helpers for parameters
    /// <summary>
    /// Helper to return parameter value as string.
    /// One can also use param.AsValueString() to
    /// get the user interface representation.
    /// </summary>
    public static string GetParameterValue( Parameter param )
    {
      string s;
      switch( param.StorageType )
      {
        case StorageType.Double:
          //
          // the internal database unit for all lengths is feet.
          // for instance, if a given room perimeter is returned as
          // 102.36 as a double and the display unit is millimeters,
          // then the length will be displayed as
          // peri = 102.36220472440
          // peri * 12 * 25.4
          // 31200 mm
          //
          //s = param.AsValueString(); // value seen by user, in display units
          //s = param.AsDouble().ToString(); // if not using not using LabUtils.RealString()
          s = RealString( param.AsDouble() ); // raw database value in internal units, e.g. feet
          break;

        case StorageType.Integer:
          s = param.AsInteger().ToString();
          break;

        case StorageType.String:
          s = param.AsString();
          break;

        case StorageType.ElementId:
          s = param.AsElementId().IntegerValue.ToString();
          break;

        case StorageType.None:
          s = "?NONE?";
          break;

        default:
          s = "?ELSE?";
          break;
      }
      return s;
    }

    static int _min_bic = 0;
    static int _max_bic = 0;

    static void SetMinAndMaxBuiltInCategory()
    {
      Array a = Enum.GetValues( typeof( BuiltInCategory ) );
      _max_bic = a.Cast<int>().Max();
      _min_bic = a.Cast<int>().Min();
    }

    static string BuiltInCategoryString( int i )
    {
      if( 0 == _min_bic )
      {
        SetMinAndMaxBuiltInCategory();
      }
      return ( _min_bic < i && i < _max_bic )
        ? " " + ( (BuiltInCategory) i ).ToString()
        : string.Empty;
    }

    /// <summary>
    /// Helper to return parameter value as string, with additional
    /// support for element id to display the element type referred to.
    /// </summary>
    public static string GetParameterValue2( Parameter param, Document doc )
    {
      string s;
      if( StorageType.ElementId == param.StorageType
        && null != doc )
      {
        ElementId id = param.AsElementId();

        int i = id.IntegerValue;

        if( 0 > i )
        {
          s = i.ToString()
            + BuiltInCategoryString( i );
        }
        else
        {
          Element e = doc.GetElement( id );
          s = ElementDescription( e, true );
        }
      }
      else
      {
        s = GetParameterValue( param );
      }
      return s;
    }
    #endregion // Helpers for parameters
  }
}
