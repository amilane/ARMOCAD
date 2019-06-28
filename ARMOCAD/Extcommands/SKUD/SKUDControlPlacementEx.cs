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

      string accPointName = "СКУД_Точка доступа_[ТД]";
      var accessPoint = Util.GetFamilySymbolByFamilyName(doc, accPointName);
      if (accessPoint == null)
      {
        Util.InfoMsg2("В модели не загружено семейство:", accPointName);
        return Result.Cancelled;
      }

      

      var levels = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_Levels);

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
          using (Transaction t = new Transaction(doc, "Создание ТД")) {
            t.Start();

            if (!accessPoint.IsActive)
            {
              accessPoint.Activate();
            }

            Reference refElemLinked = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Выберите дверь в связанной модели АР");
            Element door = docAR.GetElement(refElemLinked.LinkedElementId);
            if (door.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors |
                door.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels) {
              var elev = Math.Round(((Level)docAR.GetElement(door.LevelId)).Elevation, 2);
              var currentLevels = levels.Where(i => Math.Round(((Level)i).Elevation, 2) == elev);
              if (currentLevels.Count() > 0) {
                Level level = (Level)currentLevels.First();
                XYZ loc = ((FamilyInstance)door).GetTotalTransform().Origin;
                var rotAxis = Line.CreateBound(loc, new XYZ(loc.X, loc.Y, loc.Z + 1.0));
                var orient = ((FamilyInstance)door).FacingOrientation;
                var angle = orient.AngleTo(yVect);

                var doorHost = (Wall)((FamilyInstance)door).Host;
                var wallDepth = doorHost.Width;

                double doorW = 0.0;
                double doorH = 0.0;
                double doorShift = 0.0;

                if (doorHost.WallType.Kind == WallKind.Curtain) {
                  if (door.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors) {
                    doorW = door.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();
                    doorH = door.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsDouble();
                  } else if (door.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels) {
                    doorW = door.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANELS_WIDTH).AsDouble();
                    doorH = door.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANELS_HEIGHT).AsDouble();
                  }

                } else {
                  var doorSymbol = ((FamilyInstance)door).Symbol;
                  doorW = doorSymbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();
                  doorH = doorSymbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsDouble();
                  doorShift = door.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
                }

                var accPoint = doc.Create.NewFamilyInstance(loc, accessPoint, level, StructuralType.NonStructural);
                ElementTransformUtils.RotateElement(doc, accPoint.Id, rotAxis, angle);

                accPoint.LookupParameter("Ширина двери").Set(doorW);
                accPoint.LookupParameter("Высота двери").Set(doorH);
                accPoint.LookupParameter("Толщина стены").Set(wallDepth);
                accPoint.LookupParameter("Смещение").Set(doorShift);
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
