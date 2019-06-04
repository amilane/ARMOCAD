using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace ARMOCAD
{
  public class ConPickFilter : ISelectionFilter
  {
    Autodesk.Revit.DB.Document doc = null;
    public ConPickFilter(Document document)
    {
      doc = document;
    }
    public bool AllowElement(Element element)
    {
      return element.Category.Id.IntegerValue == -2001352 ? true : false;
    }
    public bool AllowReference(Reference reference, XYZ point)
    {
      return false;
    }
  }
}
