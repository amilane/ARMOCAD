using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ARMOCAD
{
  static class TagListData
  {
    public static ObservableCollection<TagItem> tagListData(Document doc, Schema schema)
    {
      TagItem.doc = doc;
      TagItem.schema = schema;
      ObservableCollection<TagItem> tagItems = new ObservableCollection<TagItem>();
      
      IEnumerable<Element> terminal = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctTerminal)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> ductAccessory = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctAccessory)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> pipeAccessory = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeAccessory)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> equipment = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> elems = terminal.Union(ductAccessory).Union(pipeAccessory).Union(equipment);

      foreach (var e in elems)
      {
        TagItem t = new TagItem();
        t.element = e;

        
        string modelTag = e.LookupParameter("TAG")?.AsString();

        if (!string.IsNullOrWhiteSpace(modelTag))
        {
          tagItems.Add(t);
        }
      }

      return tagItems;


    }
  }
}
