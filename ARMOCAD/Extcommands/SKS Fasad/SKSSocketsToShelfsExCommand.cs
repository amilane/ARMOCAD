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

      #region Проверка наличия шкафов, розеток и параметров

      var shelfsAll = Util.GetElementsByStringParameter(
        doc,
        BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
        "СКС_Шкаф_[серверный, кроссовый] : Кроссовый").ToList();

      IList<Element> socketsAll = Util.GetElementsByStringParameter(
        doc,
        BuiltInParameter.ELEM_FAMILY_PARAM,
        "СКС_Розетка_[1xRJ45, 2xRJ45, TV, radio]").ToList();

      if (shelfsAll.Count == 0)
      {
        Util.InfoMsg2("В модели не расставлены шкафы:", "СКС_Шкаф_[серверный, кроссовый] : Кроссовый");
        return Result.Cancelled;
      }
      if (socketsAll.Count == 0)
      {
        Util.InfoMsg2("В модели не расставлены розетки:", "СКС_Розетка_[1xRJ45, 2xRJ45, TV, radio]");
        return Result.Cancelled;
      }

      if (socketsAll.Any(i => i.LookupParameter("Розетка - Шкаф") == null))
      {
        Util.InfoMsg2("У розеток нет текстового параметра (по экземпляру):", "Розетка - Шкаф");
        return Result.Cancelled;
      }

      #endregion






      try {
        using (Transaction t = new Transaction(doc, "Марка шкафа в розетки")) {
          t.Start();

          foreach (var shelfsAtLevel in shelfsAll.GroupBy(i=> i.LevelId.IntegerValue))
          {
            IList<Element> socketsAtLevel = socketsAll
              .Where(i => i.LevelId.IntegerValue == shelfsAtLevel.Key)
              .ToList();

            if (socketsAtLevel.Count != 0)
            {
              List<Element> errorSockets = new List<Element>(); //розетки, которые не входят в радиус ни одного шкафа (60 м)

              foreach (var s in socketsAtLevel)
              {
                XYZ locSocket = ((LocationPoint)s.Location).Point;

                //ближайший шкаф к розетке
                var nearestShelf =
                  shelfsAtLevel.OrderBy(i => ((LocationPoint)i.Location).Point.DistanceTo(locSocket)).First();

                XYZ locNearestShelf = ((LocationPoint)nearestShelf.Location).Point;

                if (locSocket.DistanceTo(locNearestShelf) < 60000 / 304.8)
                {

                  Parameter parSocShelf = s.LookupParameter("Розетка - Шкаф");
                  string shelfNumber = nearestShelf.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                  parSocShelf.Set(shelfNumber);

                }
                else
                {
                  errorSockets.Add(s);
                }
              }


              string alert = String.Empty;

              if (errorSockets.Count > 0)
              {
                alert = "Розетки, не попавшие в шкаф:\n";
                foreach (var s in errorSockets)
                {
                  alert += s.Id.ToString() + "\n";
                }
              }

              if (alert != String.Empty)
              {
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
