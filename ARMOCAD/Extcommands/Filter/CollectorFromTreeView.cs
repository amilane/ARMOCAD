using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class CollectorFromTreeView
  {
    public static FilteredElementCollector collectorFromTreeView(Document doc, ObservableCollection<Node> nodes)
    {
      IList<ElementFilter> familyAndTypeFilters = new List<ElementFilter>();
      foreach (var category in nodes)
      {
        foreach (var family in category.Children)
        {
          foreach (var familyType in family.Children)
          {
            if (familyType.IsChecked == true)
            {

              ParameterValueProvider fvp = new ParameterValueProvider(new ElementId(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM));
              FilterStringRuleEvaluator fsre = new FilterStringEquals();
              string ruleString = String.Format("{0}: {1}", family.Text, familyType.Text);
              FilterRule fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              ElementParameterFilter filter = new ElementParameterFilter(fRule);

              familyAndTypeFilters.Add(filter);

            }
          }
        }
      }

      LogicalOrFilter familyAndTypeFilter = new LogicalOrFilter(familyAndTypeFilters);

      FilteredElementCollector collector = new FilteredElementCollector(doc).WherePasses(familyAndTypeFilter);

      return collector;
    }
  }
}
