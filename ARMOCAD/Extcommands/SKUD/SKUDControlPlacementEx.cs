using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class SKUDControlPlacementEx : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp?.ActiveUIDocument;
      Document doc = uidoc?.Document;
      Document docAR = null;
      FamilySymbol accessPoint = null;

      var accessPoints = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_CommunicationDevices)
        .WhereElementIsElementType().Where(i => ((FamilySymbol)i).FamilyName == "СКУД_Точка доступа");
      if (accessPoints.Count() != 0) {
        accessPoint = (FamilySymbol)accessPoints.First();
        if (!accessPoint.IsActive) {
          accessPoint.Activate();
        }
      } else {
        TaskDialog.Show("Предупреждение", "Не загружено семейство:\n СКУД_Точка доступа");
        return Result.Cancelled;
      }


      var levels = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements();
      XYZ yVect = new XYZ(0, 1, 0);

      ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

      if (0 == selectedIds.Count) {
        TaskDialog.Show("Предупреждение", "Выделите модель в АР");
        return Result.Cancelled;
      } else {
        foreach (ElementId id in selectedIds) {
          var link = doc.GetElement(id);
          if (link.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RvtLinks) {
            docAR = ((RevitLinkInstance)link).GetLinkDocument();
          }
        }
      }

      while (true) {
        try {
          using (Transaction t = new Transaction(doc, "CreateAccessPoint")) {
            t.Start();

            Reference refElemLinked = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Выберите дверь в связанной модели АР");
            Element elem = docAR.GetElement(refElemLinked.LinkedElementId);
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors |
                elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels) {
              var elev = Math.Round(((Level)docAR.GetElement(elem.LevelId)).Elevation, 2);
              var currentLevels = levels.Where(i => Math.Round(((Level)i).Elevation, 2) == elev);
              if (currentLevels.Count() > 0) {
                Level level = (Level)currentLevels.First();
                XYZ loc = ((FamilyInstance)elem).GetTotalTransform().Origin;
                var rotAxis = Line.CreateBound(loc, new XYZ(loc.X, loc.Y, loc.Z + 1.0));
                var orient = ((FamilyInstance)elem).FacingOrientation;
                var angle = orient.AngleTo(yVect);

                var doorHost = (Wall)((FamilyInstance)elem).Host;
                var wallDepth = doorHost.Width;

                double doorW = 0.0;
                double doorH = 0.0;

                if (doorHost.WallType.Kind == WallKind.Curtain) {
                  if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors) {
                    doorW = elem.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();
                    doorH = elem.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsDouble();
                  } else if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels) {
                    doorW = elem.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANELS_WIDTH).AsDouble();
                    doorH = elem.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANELS_HEIGHT).AsDouble();
                  }

                } else {
                  var doorSymbol = ((FamilyInstance)elem).Symbol;
                  doorW = doorSymbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();
                  doorH = doorSymbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsDouble();
                }

                var accPoint = doc.Create.NewFamilyInstance(loc, accessPoint, level, StructuralType.NonStructural);
                ElementTransformUtils.RotateElement(doc, accPoint.Id, rotAxis, angle);

                accPoint.LookupParameter("Ширина двери").Set(doorW);
                accPoint.LookupParameter("Высота двери").Set(doorH);
                accPoint.LookupParameter("Толщина стены").Set(wallDepth);
                accPoint.LookupParameter("Смещение").Set(0.0);
              }

            }

            t.Commit();

          }


        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
          break;
        }
        catch (Exception ex) {
          message = ex.Message;
          return Result.Failed;
        }
      }



      return Result.Succeeded;
    }

  }
}
