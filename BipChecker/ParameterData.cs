#region Namespaces
using Autodesk.Revit.DB;
using System.Diagnostics;
#endregion // Namespaces

namespace BipChecker
{
    /// <summary>
    /// A class used to manage the data of an element parameter.
    /// </summary>
    public class ParameterData
    {
        BuiltInParameter _enum;
        readonly Parameter _parameter;
        private readonly string _parameterName;

        string GetValue
        {
            get
            {
                //return _value;
                string s;
                switch (_parameter.StorageType)
                {
                    // database value, internal units, e.g. feet:
                    case StorageType.Double: s = Util.RealString(_parameter.AsDouble()); break;
                    case StorageType.Integer: s = _parameter.AsInteger().ToString(); break;
                    case StorageType.String: s = _parameter.AsString(); break;
                    case StorageType.ElementId: s = _parameter.AsElementId().IntegerValue.ToString(); break;
                    case StorageType.None: s = "None"; break;
                    default: Debug.Assert(false, "unexpected storage type"); s = string.Empty; break;
                }
                return s;
            }
        }

        public ParameterData(
          BuiltInParameter bip,
          Parameter parameter,
          string valueStringOrElementDescription,
          bool containedInCollection,
            /*
             * Edited by Chekalin Victor 13.12.2012
             */
          string parameterName)
        {
            _enum = bip;
            _parameter = parameter;
            /*
             * Edited by Chekalin Victor 13.12.2012
             */
            _parameterName = parameterName;

            ValueString = valueStringOrElementDescription;
            Value = GetValue;


            Definition d = _parameter.Definition;

            ParameterGroup = d.ParameterGroup.ToString();
            GroupName = LabelUtils.GetLabelFor(d.ParameterGroup);
            ContainedInCollection = containedInCollection ? "Y" : "N";
        }

        public string Enum
        {
            get
            {
                /*
                 * Edited by Chekalin Victor 13.12.2012
                 */
                return _parameterName ?? _enum.ToString();
            }
        }



        public string Name
        {
            get { return _parameter.Definition.Name; }
        }

        public string Type
        {
            get
            {
                ParameterType pt = _parameter.Definition.ParameterType; // returns 'Invalid' for 'ElementId'
                string s = ParameterType.Invalid == pt ? "" : "/" + pt.ToString();
                return _parameter.StorageType.ToString() + s;
            }
        }

        public string ReadWrite
        {
            get { return _parameter.IsReadOnly ? "read-only" : "read-write"; }
        }

        /// <summary>
        /// Value string or element description
        /// in case of an element id.
        /// </summary>
        public string ValueString { get; set; }

        public string Value { get; set; }

        public string ParameterGroup { get; set; }
        public string GroupName { get; set; }

        /// <summary>
        /// Contained in the Element.Parameters collection?
        /// </summary>
        public string ContainedInCollection { get; set; }

        public string Shared
        {
            get { return _parameter.IsShared ? "Shared" : "Non-shared"; }
        }

        public override bool Equals(object obj)
        {
            ParameterData otherParameter = obj as ParameterData;
            if (otherParameter == null) return false;

            return _parameter.Id.Equals(otherParameter._parameter.Id);

            
        }
        public override int GetHashCode()
        {
            return _parameter.Id.IntegerValue;
        }

        public string Guid
        {
            get { return _parameter.IsShared ? _parameter.GUID.ToString() : string.Empty; }
        }
        
    }
}
