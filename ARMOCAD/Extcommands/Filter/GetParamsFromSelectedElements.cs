using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class GetParamsFromSelectedElements
  {
    class ParameterComparer : IEqualityComparer<Parameter>
    {
      public bool Equals(Parameter x, Parameter y)
      {
        return x.Id == y.Id;
      }
      public int GetHashCode(Parameter parameter)
      {
        return parameter.Id.GetHashCode();
      }

    }

    public static List<ParameterData> getParamsFromSelectedElements(FilteredElementCollector collector)
    {

      var families = collector.GroupBy(e => e.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString());
      List<ParameterSet> listOfPsets = new List<ParameterSet>();
      foreach (var f in families)
      {
        var parameters = f.First().Parameters;
        listOfPsets.Add(parameters);
      }


      List<ParameterData> outList = new List<ParameterData>();
      if (listOfPsets.Count > 0)
      {
        var fset = listOfPsets.First().Cast<Parameter>();
        foreach (var i in listOfPsets)
        {
          var merge = fset.Intersect(i.Cast<Parameter>(), new ParameterComparer());
          fset = merge;
        }

        foreach (Parameter p in fset)
        {
          if (p.StorageType != StorageType.ElementId | (p.StorageType == StorageType.ElementId & p.Definition.Name == "Уровень"))
          {
            ParameterData pd = new ParameterData
            {
              Id = p.Id,
              StorageType = p.StorageType,
              ParameterName = p.Definition.Name
            };

            outList.Add(pd);
          }
        }
        return outList.OrderBy(i => i.ParameterName).ToList();
      }
      else
      {
        return outList;
      }


    }

  }
}
