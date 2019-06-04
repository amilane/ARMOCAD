using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;


namespace ARMOCAD
{
  public class PickFilter : ISelectionFilter
  {
    Autodesk.Revit.DB.Document doc = null;
    public PickFilter(Document document)
    {
      doc = document;
    }
    public bool AllowElement(Element element)
    {
      return true;
    }
    public bool AllowReference(Reference reference, XYZ point)
    {
      RevitLinkInstance revitlinkinstance = doc.GetElement(reference) as RevitLinkInstance;
      Autodesk.Revit.DB.Document docLink = revitlinkinstance.GetLinkDocument();
      Element eLink = docLink.GetElement(reference.LinkedElementId);
      string name = eLink.LookupParameter("Семейство").AsValueString();
      if (name == "DA_Клапан_[ОЗК_КДУ, Сечение прямоугольное]" || name == "DA_Клапан_[ОЗК_КДУ, Сечение круглое]")
      {
        return true;
      }
      return false;
    }
  }
}
