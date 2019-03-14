using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class ValuesFromParameter
  {
    public static List<string> valuesFromParameter(string parameterName, FilteredElementCollector collector)
    {
      List<string> values = new List<string>();
      foreach (var e in collector)
      {

        Parameter p = e.LookupParameter(parameterName);
        string value = "";
        StorageType storageType = p.StorageType;
        switch (storageType)
        {
          case StorageType.Double:
          case StorageType.Integer:
          case StorageType.ElementId:
            value = p.AsValueString();
            break;
          case StorageType.String:
            value = p.AsString();
            break;
        }

        if (value != "")
        {
          values.Add(value);
        }

      }

      values.Sort();
      List<string> uniqueValues = values.Distinct().ToList();
      return uniqueValues;
    }
  }
}
