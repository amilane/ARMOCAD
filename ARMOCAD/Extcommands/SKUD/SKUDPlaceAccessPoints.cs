using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ARMOCAD
{
  static class SKUDPlaceAccessPoints
  {
    public static void SkudPlaceAccessPoints(Document docAR, Document doc)
    {
      var doors = new FilteredElementCollector(docAR).OfCategory(BuiltInCategory.OST_Doors)
        .WhereElementIsNotElementType().ToElements();

      FamilySymbol accessPoint = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_CommunicationDevices)
        .WhereElementIsElementType().First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "СКУД_ТД") as FamilySymbol;
      accessPoint.Activate();

      var levels = new FilteredElementCollector(doc)
        .OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements();

      XYZ yVect = new XYZ(0,1,0);
      
      var doorsByLevel = doors.GroupBy(i => i.LevelId);

      foreach (var group in doorsByLevel) {
        var elev = Math.Round(((Level)docAR.GetElement(group.First().LevelId)).Elevation, 2);
        var currentLevels = levels.Where(i => Math.Round(((Level)i).Elevation,2) == elev);
        if (currentLevels.Count() > 0) {
          Level level = (Level)currentLevels.First();

          foreach (var d in group) {
            XYZ loc = ((LocationPoint)d.Location).Point;
            var rotAxis = Line.CreateBound(loc, new XYZ(loc.X, loc.Y, loc.Z + 1.0));
            var orient = ((FamilyInstance)d).FacingOrientation;
            var angle = orient.AngleTo(yVect);

            var wallDepth = ((Wall)((FamilyInstance)d).Host).Width;
            var doorSymbol = ((FamilyInstance) d).Symbol;
            var doorW = doorSymbol.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();
            var doorH = doorSymbol.get_Parameter(BuiltInParameter.DOOR_HEIGHT).AsDouble();

            var accPoint = doc.Create.NewFamilyInstance(loc, accessPoint, level, StructuralType.NonStructural);
            ElementTransformUtils.RotateElement(doc, accPoint.Id, rotAxis, angle);

            accPoint.LookupParameter("Ширина двери").Set(doorW);
            accPoint.LookupParameter("Высота двери").Set(doorH);
            accPoint.LookupParameter("Толщина стены").Set(wallDepth);
            accPoint.LookupParameter("Смещение").Set(0.0);

          }
        }


      }



    }
  }
}
