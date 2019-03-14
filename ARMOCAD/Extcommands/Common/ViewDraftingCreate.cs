using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class ViewDraftingCreate
  {
    public static ViewDrafting viewDraftingCreate(Document doc, string viewName)
    {
      FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));
      ViewFamilyType viewFamilyType = collector.Cast<ViewFamilyType>().First(vft => vft.ViewFamily == ViewFamily.Drafting);
      ViewDrafting view = ViewDrafting.Create(doc, viewFamilyType.Id);
      view.ViewName = viewName;

      return view;
    }

    public static IEnumerable<string> viewDraftingNames(Document doc)
    {
      var views = new FilteredElementCollector(doc).OfClass(typeof(ViewDrafting)).ToElements();

      IList<string>names = new List<string>();
      foreach (var v in views)
      {
        names.Add(v.Name);
      }
      return names;
    }
  }
}
