using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;



namespace ARMOCAD
{
  [TransactionAttribute(TransactionMode.Manual)]
  [RegenerationAttribute(RegenerationOption.Manual)]
  public class ValvelocSS : IExternalCommand
  {

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      UIApplication uiApp = commandData.Application;
      UIDocument uidoc = uiApp.ActiveUIDocument;
      Application app = uiApp.Application;
      Document doc = uidoc.Document;
      ObjectType obt = ObjectType.LinkedElement;
      Reference refElemLinked;
      while (true)
      {
        try
        {
          //ISelectionFilter selectionFilter = new PickFilter(doc);
          refElemLinked = uidoc.Selection.PickObject(obt, "Выберите элемент в связанной модели");
          RevitLinkInstance el = doc.GetElement(refElemLinked.ElementId) as RevitLinkInstance;
          Document docLinked = el.GetLinkDocument();
          FilteredElementCollector collectorlink = new FilteredElementCollector(docLinked);
          FilteredElementCollector collectordoc = new FilteredElementCollector(doc);
          var elemlinkrect = collectorlink.OfCategory(BuiltInCategory.OST_DuctAccessory).WhereElementIsNotElementType().Where(i => i.LookupParameter("Семейство").AsValueString() == "DA_Клапан_[ОЗК_КДУ, Сечение прямоугольное]");
          var elemlinkcirc = collectorlink.OfCategory(BuiltInCategory.OST_DuctAccessory).WhereElementIsNotElementType().Where(i => i.LookupParameter("Семейство").AsValueString() == "DA_Клапан_[ОЗК_КДУ, Сечение круглое]");
          var elem = collectordoc.OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().Where(i => i.LookupParameter("Семейство").AsValueString() == "ME_Клапан ОЗК,КДУ_[СС]");
          if ((elemlinkrect.Count() + elemlinkcirc.Count()) < elem.Count())
          {
            TaskDialog.Show("Предупреждение", "В проекте больше экземпляров чем в связи");
          }
          if ((elemlinkrect.Count() + elemlinkcirc.Count()) > elem.Count())
          {
            TaskDialog.Show("Предупреждение", "В связи больше экземпляров чем в проекте");
          }
          var t = 0;
          var p = 0;
          foreach (Element e in elem)
          {
            var y = 0;
            LocationPoint lPoint = e.Location as LocationPoint;
            XYZ locp = lPoint.Point;
            foreach (Element elink in elemlinkrect)
            {
              LocationPoint ePoint = elink.Location as LocationPoint;
              XYZ ep = ePoint.Point;
              if (locp.ToString() == ep.ToString())
              {
                y++;
                t++;
                break;
              }
              else
              {
                continue;
              }

            }
            foreach (Element ecirc in elemlinkcirc)
            {
              BoundingBoxXYZ bounding = ecirc.get_BoundingBox(null);
              XYZ ecp = (bounding.Max + bounding.Min) * 0.5;
              if (locp.ToString() == ecp.ToString())
              {
                y++;
                t++;
                break;
              }
              else
              {
                continue;
              }
            }
            if (y == 0)
            {
              //TaskDialog.Show("2", "element 0");
              p++;
              OverrideGraphicSettings ogs = new OverrideGraphicSettings();
              ogs.SetProjectionLineColor(new Color(255, 0, 0));
              ElementId id = e.Id;
              using (Transaction tr = new Transaction(doc, "Set Element Override"))
              {
                tr.Start();
                doc.ActiveView.SetElementOverrides(id, ogs);
                Parameter r = e.LookupParameter("Комментарии");
                r.Set("Размещен неправильно!");
                tr.Commit();
              }
            }
          }
          TaskDialog.Show("Предупреждение", "Неправильно размещенных элементов: " + p);
          //TaskDialog.Show("2", elemlink.Count().ToString() + " " + elem.Count().ToString() );

          TaskDialog.Show("Предупреждение", "На своих местах: " + t + " из " + (elemlinkrect.Count() + elemlinkcirc.Count()));

        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
          break;
        }
        catch (Exception ex)
        {
          message = ex.Message;
          return Result.Failed;
        }
        break;
      }
      return Result.Succeeded;
    }
  }

  
}
