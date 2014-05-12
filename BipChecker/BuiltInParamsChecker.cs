#define USE_LIST_VIEW

#region Namespaces
using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Electrical;
#endregion // Namespaces

namespace BipChecker
{
  /// <summary>
  /// List all accessible built-in parameters on 
  /// a selected element in a DataGridView.
  /// Todo: add support for shared parameters also.
  /// </summary>
  [Transaction( TransactionMode.ReadOnly )]
  public class BuiltInParamsChecker : IExternalCommand
  {
    const string _type_prompt =
      "This element {0}, so it has both type and"
      + " instance parameters. By default, the instance"
      + " parameters are displayed."
      //+ " If you select 'No',"
      //+ " the type parameters will be displayed instead."
      + " Would you like to see the instance parameters or the type parameters?";

    #region Contained in ParameterSet Collection

    #region Unnecessarily complicated first approach
    /// <summary>
    /// Return BuiltInParameter id for a given parameter,
    /// assuming it is a built-in parameter.
    /// </summary>
    static BuiltInParameter BipOf( Parameter p )
    {
      return ( p.Definition as InternalDefinition )
        .BuiltInParameter;
    }

    /// <summary>
    /// Check whether two given parameters represent
    /// the same parameter, i.e. shared parameters
    /// have the same GUID, others the same built-in
    /// parameter id.
    /// </summary>
    static bool IsSameParameter( Parameter p, Parameter q )
    {
      return ( p.IsShared == q.IsShared )
        && ( p.IsShared
          ? p.GUID.Equals( q.GUID )
          : BipOf( p ) == BipOf( q ) );
    }

    /// <summary>
    /// Return true if the given element parameter 
    /// retrieved by  get_parameter( BuiltInParameter ) 
    /// is contained in the element Parameters collection.
    /// Workaround to replace ParameterSet.Contains.
    /// Why does this not work?
    /// return _parameter.Element.Parameters.Contains(_parameter);
    /// </summary>
    bool ContainedInCollectionUnnecessarilyComplicated(
      Parameter p,
      ParameterSet set )
    {
      bool rc = false;

      foreach( Parameter q in set )
      {
        rc = IsSameParameter( p, q );

        if( rc )
        {
          break;
        }
      }
      return rc;
    }
    #endregion // Unnecessarily complicated first approach

    /// <summary>
    /// Return true if the given element parameter 
    /// retrieved by get_Parameter( BuiltInParameter ) 
    /// is contained in the element Parameters collection.
    /// Workaround to replace ParameterSet.Contains.
    /// Why does the following statement not work?
    /// return _parameter.Element.Parameters.Contains(_parameter);
    /// </summary>
    bool ContainedInCollection(
      Parameter p,
      ParameterSet set )
    {
      return set
        .OfType<Parameter>()
        .Any( x => x.Id == p.Id );
    }
    #endregion // Contained in ParameterSet Collection

    /// <summary>
    /// Revit external command to list all valid
    /// built-in parameters for a given selected
    /// element.
    /// </summary>
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      // Select element

      Element e
        = Util.GetSingleSelectedElementOrPrompt(
          uidoc );

      if( null == e )
      {
        return Result.Cancelled;
      }

      bool isSymbol = false;

      // For a family instance, ask user whether to
      // display instance or type parameters; in a
      // similar manner, we could add dedicated
      // switches for Wall --> WallType,
      // Floor --> FloorType etc. ...

      if( e is FamilyInstance )
      {
        FamilyInstance inst = e as FamilyInstance;
        if( null != inst.Symbol )
        {
          string symbol_name = Util.ElementDescription(
            inst.Symbol, true );

          string family_name = Util.ElementDescription(
            inst.Symbol.Family, true );

          string msg = string.Format( _type_prompt,
            "is a family instance" );

          if( !Util.QuestionMsg( msg ) )
          {
            e = inst.Symbol;
            isSymbol = true;
          }
        }
      }
      else if( e.CanHaveTypeAssigned() )
      {
        ElementId typeId = e.GetTypeId();
        if( null == typeId )
        {
          Util.InfoMsg( "Element can have a type,"
            + " but the current type is null." );
        }
        else if( ElementId.InvalidElementId == typeId )
        {
          Util.InfoMsg( "Element can have a type,"
            + " but the current type id is the"
            + " invalid element id." );
        }
        else
        {
          Element type = doc.GetElement( typeId );

          if( null == type )
          {
            Util.InfoMsg( "Element has a type,"
              + " but it cannot be accessed." );
          }
          else
          {
            string msg = string.Format( _type_prompt,
              "has an element type" );

            if( !Util.QuestionMsg( msg ) )
            {
              e = type;
              isSymbol = true;
            }
          }
        }
      }

      // Retrieve parameter data

      SortableBindingList<ParameterData> data
        = new SortableBindingList<ParameterData>();

      {
        WaitCursor waitCursor = new WaitCursor();

        ParameterSet set = e.Parameters;
        bool containedInCollection;

        /* 
         * Edited by Chekalin Victor 13.12.2012
         * !!! This implemention does not work properly
         * if enum has the same integer value
         * For example, BuiltInParameter.All_MODEL_COST and
         * BuiltInParameter.DOOR_COST have -1001205 integer value
         * 
           Array bips = Enum.GetValues(
             typeof( BuiltInParameter ) );
         
           int n = bips.Length;
         * 
         */

        /*
         * Edited by Chekalin Victor 13.12.2012
         */
        var bipNames =
            Enum.GetNames( typeof( BuiltInParameter ) );

        Parameter p;

        /*
         * Edited by Chekalin Victor 13.12.2012
         */
        //foreach( BuiltInParameter a in bips )
        foreach( var bipName in bipNames )
        {
          BuiltInParameter a;

          if( !Enum.TryParse( bipName, out a ) )
            continue;

          try
          {
            p = e.get_Parameter( a );

            #region Check for external definition
#if CHECK_FOR_EXTERNAL_DEFINITION
            Definition d = p.Definition;
            ExternalDefinition e = d as ExternalDefinition; // this is never possible
            string guid = ( null == e ) ? null : e.GUID.ToString();
#endif // CHECK_FOR_EXTERNAL_DEFINITION
            #endregion // Check for external definition

            if( null != p )
            {
              string valueString =
                ( StorageType.ElementId == p.StorageType )
                  ? Util.GetParameterValue2( p, doc )
                  : p.AsValueString();

              //containedInCollection = set.Contains( p ); // this does not work
              containedInCollection = ContainedInCollection( p, set );

              data.Add( new ParameterData( a, p,
                valueString,
                containedInCollection,
                bipName ) );
            }
          }
          catch( Exception ex )
          {
            Debug.Print(
              "Exception retrieving built-in parameter {0}: {1}",
              a, ex );
          }
        }
      }

      // Retrieve parameters from Element.Parameters collection

      foreach( Parameter p in e.Parameters )
      {
        string valueString =
          ( StorageType.ElementId == p.StorageType )
            ? Util.GetParameterValue2( p, doc )
            : p.AsValueString();

        ParameterData parameterData = new ParameterData(
          ( p.Definition as InternalDefinition ).BuiltInParameter,
          p,
          valueString,
          true,
          null );

        if( !data.Contains( parameterData ) )
          data.Add( parameterData );
      }

      // Display form

      string description
        = Util.ElementDescription( e, true )
        + ( isSymbol
          ? " Type"
          : " Instance" );

#if USE_LIST_VIEW
      using( BuiltInParamsCheckerFormListView form
        = new BuiltInParamsCheckerFormListView( e,
          description, data ) )
#else
      using (BuiltInParamsCheckerForm form
        = new BuiltInParamsCheckerForm(
          description, data))
#endif // USE_LIST_VIEW
      {
        form.ShowDialog();
      }
      return Result.Succeeded;
    }
  }
}
