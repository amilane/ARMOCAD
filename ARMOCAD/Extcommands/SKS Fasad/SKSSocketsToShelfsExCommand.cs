using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class SKSSocketsToShelfsExCommand : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;

      try {
        using (Transaction t = new Transaction(doc, "SKSSocketsToShelfs")) {
          t.Start();

          var levels = new FilteredElementCollector(doc)
          .OfCategory(BuiltInCategory.OST_Levels)
          .WhereElementIsNotElementType()
          .ToElements().OrderBy(i => ((Level)i).Elevation);

          foreach (Level level in levels) {
            // шкафы СКС на данном уровне, в которые собираются розетки
            IList<Element> shelfs = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_CommunicationDevices)
              .WhereElementIsNotElementType()
              .Where(i => (i.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() == "Шкаф СКС: Кроссовый" |
                          i.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() == "Шкаф СКС: Серверно-кроссовый") &&
                          i.LevelId == level.Id).ToList();

            // розетки на данном уровне
            IList<Element> sockets = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_CommunicationDevices)
              .WhereElementIsNotElementType()
              .Where(i => ((FamilyInstance)i).Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString() == "Розетка СКС" &&
                          i.LevelId == level.Id &&
                          i.LookupParameter("Розетка - Система")?.AsString() != null).ToList();


            if (shelfs.Count > 0 && sockets.Count > 0) {

              List<Element> errorSockets = new List<Element>(); //розетки, которые не входят в радиус ни одного шкафа (60 м)

              foreach (var s in sockets) {
                XYZ locSocket = ((LocationPoint)s.Location).Point;

                //ближайший шкаф к розетке
                var nearestShelf =
                  shelfs.OrderBy(i => ((LocationPoint)i.Location).Point.DistanceTo(locSocket)).First();

                XYZ locNearestShelf = ((LocationPoint)nearestShelf.Location).Point;

                if (locSocket.DistanceTo(locNearestShelf) < 60000 / 304.8) {

                  Parameter parSocShelf = s.LookupParameter("Розетка - Шкаф");
                  string shelfNumber = nearestShelf.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                  parSocShelf.Set(shelfNumber);

                } else {
                  errorSockets.Add(s);
                }
              }


              string alert = "";

              if (errorSockets.Count > 0) {
                alert = "Розетки, не попавшие в шкаф:\n";
                foreach (var s in errorSockets) {
                  alert += s.Id.ToString() + "\n";
                }
              }

              if (alert != "") {
                TaskDialog.Show("Предупреждение", alert);
              }

            }

          }

          t.Commit();
        }



        TaskDialog.Show("Готово", "ОК");
        return Result.Succeeded;
      }
      // This is where we "catch" potential errors and define how to deal with them
      catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
        // If user decided to cancel the operation return Result.Canceled
        return Result.Cancelled;
      }
      catch (Exception ex) {
        // If something went wrong return Result.Failed
        message = ex.Message;
        return Result.Failed;
      }

    }





  }
}
