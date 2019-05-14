using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class CreateMepSpaces : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp?.ActiveUIDocument;
      Document doc = uidoc?.Document;

      var activeView = doc.ActiveView;

      if (activeView.ViewType != ViewType.FloorPlan)
      {
        TaskDialog.Show("Предупреждение", "Откройте план этажа!");
        return Result.Cancelled;
      }

      int newSpaceCount = 0;


      Level level = activeView.GenLevel;
      var levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).ToElements();

      // уже размещенные пространства на уровне
      var oldSpaces = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MEPSpaces)
        .WhereElementIsNotElementType().ToElements().Where(i => ((Space)i).LevelId == level.Id && ((Space)i).Area > 0);

      var links = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).ToElements();
      List<Document> linkdocs = new List<Document>();
      foreach (RevitLinkInstance i in links)
      {
        linkdocs.Add(i.GetLinkDocument());
      }

      // сбор помещений из связей на уровне активного вида
      List<Element> linkrooms = new List<Element>();
      foreach (Document linkdoc in linkdocs)
      {
        var roomsInOneLink = new FilteredElementCollector(linkdoc).OfCategory(BuiltInCategory.OST_Rooms)
          .WhereElementIsNotElementType().ToElements();

        foreach (var i in roomsInOneLink)
        {
          if (((Room)i).Area > 0 && Math.Round(((Room)i).Level.Elevation, 2) == Math.Round(level.Elevation, 2))
          {
            linkrooms.Add(i);
          }

        }

      }

      if (linkrooms.Count == 0)
      {
        TaskDialog.Show("Предупреждение", "На уровне открытого плана нет помещений.");
        return Result.Cancelled;
      }



      try
      {
        using (Transaction t = new Transaction(doc, "Создание пространств"))
        {

          t.Start();



          //создаю новые пространства
          foreach (Room room in linkrooms)
          {

            
            //если пространства нет - оно создается, если есть просто обновляются его параметры
            var spaceFromRoom = SpaceFromRoom(room);
            if (spaceFromRoom == null)
            {
              var space = CreateSpace(room);
              SetSpaceParameters(space, room);
            }
            else
            {
              SetSpaceParameters(spaceFromRoom, room);
            }


          }

          t.Commit();
          TaskDialog.Show("Отчет", $"Создано пространств: {newSpaceCount.ToString()}, на уровне: {level.Name}.");
          return Result.Succeeded;
        }

      }
      // This is where we "catch" potential errors and define how to deal with them
      catch (Exception ex)
      {
        // If something went wrong return Result.Failed
        message = ex.Message;
        return Result.Failed;
        
      }

      #region localMethods
      // заполнение пространств помещений
      void SetSpaceParameters(Space space, Room room)
      {
        space.get_Parameter(BuiltInParameter.ROOM_NAME).Set(room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString());
        space.get_Parameter(BuiltInParameter.ROOM_NUMBER)
          .Set(room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString());
        space.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL)
          .Set(level.Id);

        //высота помещения
        var volume = room.get_Parameter(BuiltInParameter.ROOM_VOLUME).AsDouble();

        double height = 0;
        if (volume == 0)
        {
          height = room.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).AsDouble();
        }
        else
        {
          height = volume / room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
        }

        space.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET)
          .Set(height);
      }

      //вытаскивание пространства по помещению
      Space SpaceFromRoom(Room room)
      {
        XYZ loc = ((LocationPoint)room.Location).Point;
        var spaces = oldSpaces.Where(i => ((Space)i).IsPointInSpace(loc));
        if (spaces.Any())
        {
          return (Space) spaces.First();
        }
        else
        {
          return null;
        }
      }

      // создание нового пространства
      Space CreateSpace(Room room)
      {
        XYZ pt = ((LocationPoint)room.Location).Point;
        UV uv = new UV(pt.X, pt.Y);
        var space = doc.Create.NewSpace(level, uv);
        newSpaceCount++;
        return space;
      }

      #endregion localMethods














    }


  }
}
