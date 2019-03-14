using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public class ShelfAndSockets
  {
    public Element shelf;
    public List<Socket> socketList = new List<Socket>();
  }

  public class Socket
  {
    public Element socket;
    public ElementId symbolId;
    public int countOfPorts;
    public string system;
    public string shelfNumber;
    public string socketComment;
    public string roomNumber;
    public string mark1;
    public string mark2;
  }


  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class SKSFasadExCommand : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;
      try
      {
        using (Transaction t = new Transaction(doc, "SKSFasad"))
        {
          t.Start();

          var levels = new FilteredElementCollector(doc)
          .OfCategory(BuiltInCategory.OST_Levels)
          .WhereElementIsNotElementType()
          .ToElements().OrderBy(i => ((Level)i).Elevation);

          FilteredElementCollector symbols = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_DetailComponents)
            .WhereElementIsElementType();

          FamilySymbol frameSymbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Ф_Шкаф") as FamilySymbol;
          FamilySymbol orgSymbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Ф_Органайзер") as FamilySymbol;
          FamilySymbol patch24Symbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Ф_Патч-панель 24 RG45") as FamilySymbol;
          FamilySymbol commut24Symbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Ф_Коммутатор 24 RG45") as FamilySymbol;
          FamilySymbol commut48Symbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Ф_Коммутатор 48 RG45") as FamilySymbol;
          FamilySymbol shkos1U32Symbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Ф_Шкос 1U 32 волокон") as FamilySymbol;

          FamilySymbol patch24SchemeSymbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "С_Патч-панель 24 RG45") as FamilySymbol;
          FamilySymbol commut48SchemeSymbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "С_Коммутатор 48 RG45") as FamilySymbol;
          FamilySymbol commut24SchemeSymbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "С_Коммутатор 24 RG45") as FamilySymbol;
          FamilySymbol shkosSchemeSymbol = symbols.First(i => i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "С_Шкос 1U 32 волокон") as FamilySymbol;

          var socketSymbols = symbols.Where(i =>
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка RJ45 в коробе" |
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка RJ45 в лючке" |
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка RJ45 в установочной коробке" |
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка RJ45 врезная" |
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка RJ45 накладная" |
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка телефонная" |
            i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == "Розетка неизвестная").Cast<FamilySymbol>();


          frameSymbol.Activate();
          orgSymbol.Activate();
          patch24Symbol.Activate();
          commut24Symbol.Activate();
          commut24Symbol.Activate();
          commut48Symbol.Activate();
          shkos1U32Symbol.Activate();
          patch24SchemeSymbol.Activate();
          commut48SchemeSymbol.Activate();
          commut24SchemeSymbol.Activate();
          shkosSchemeSymbol.Activate();

          foreach (Level level in levels)
          {
            // шкафы СКС на данном уровне
            IList<Element> shelfs = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_CommunicationDevices)
              .WhereElementIsNotElementType()
              .Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "Стойка СКС" &&
                          i.LevelId == level.Id).ToList();

            // розетки на данном уровне
            IList<Element> sockets = new FilteredElementCollector(doc)
              .OfCategory(BuiltInCategory.OST_CommunicationDevices)
              .WhereElementIsNotElementType()
              .Where(i => ((FamilyInstance)i).Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString() == "Розетка СКС" &&
                          i.LevelId == level.Id &&
                          i.LookupParameter("Розетка - Система")?.AsString() != null).ToList();


            if (shelfs.Count > 0 && sockets.Count > 0)
            {
              // собираем шкафы и соответствующие им розетки в класс ShelfAndSockets
              List<ShelfAndSockets> shelfAndSockets = new List<ShelfAndSockets>();
              foreach (var i in shelfs)
              {
                ShelfAndSockets sas = new ShelfAndSockets();
                sas.shelf = i;
                shelfAndSockets.Add(sas);
              }

              List<Element> errorSockets = new List<Element>(); //розетки, которые не входят в радиус ни одного шкафа (90 м)

              foreach (var s in sockets)
              {
                XYZ locSocket = ((LocationPoint)s.Location).Point;

                //ближайший шкаф к розетке
                var nearestShelf =
                  shelfAndSockets.OrderBy(i => ((LocationPoint)i.shelf.Location).Point.DistanceTo(locSocket)).First();

                XYZ locNearestShelf = ((LocationPoint)nearestShelf.shelf.Location).Point;

                if (locSocket.DistanceTo(locNearestShelf) < 90000 / 304.8)
                {
                  // сбор информации о розетке
                  Socket socket = new Socket();
                  socket.socket = s;

                  string nameOfSocketSymbol = ((FamilyInstance)s).Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS)
                    .AsString();
                  if (nameOfSocketSymbol == null || nameOfSocketSymbol == "")
                  {
                    nameOfSocketSymbol = "Розетка неизвестная";
                  }
                  socket.symbolId = socketSymbols.Where(i => i.Name == nameOfSocketSymbol).First().Id;

                  if (((FamilyInstance) s).Symbol.LookupParameter("2xRJ45").AsInteger() == 1)
                  {
                    socket.countOfPorts = 2;
                  }
                  else
                  {
                    socket.countOfPorts = 1;
                  }

                  socket.system = s.LookupParameter("Розетка - Система").AsString();

                  socket.socketComment = s.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();

                  var space = ((FamilyInstance)s).Space;
                  if (space != null)
                  {
                    socket.roomNumber = space.Number;
                  }
                  else
                  {
                    socket.roomNumber = "";
                  }

                  nearestShelf.socketList.Add(socket);
                }
                else
                {
                  errorSockets.Add(s);
                }
              }

              //имена существующих чертежных видов
              var viewCreatedNames = ViewDraftingCreate.viewDraftingNames(doc);

              List<string> portNamesForPatch1;
              List<string> portNamesForPatch2;

              List<string> socketRoomsForPatch1;
              List<string> socketRoomsForPatch2;

              List<ElementId> socketSymbolsIdsPatch1;
              List<ElementId> socketSymbolsIdsPatch2;


              string viewFasadeName;
              string viewSchemeName;
              string shelfName;

              int countPorts;
              int countCommuts;
              int commutNumber;
              double lengthYShkos;

              foreach (var i in shelfAndSockets)
              {
                var socketGroups = SocketNumbering.groupingSocketsByPurpose(i);

                Element shelf = i.shelf;
                shelfName = shelf.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

                viewFasadeName = createViewName("СКС Фасад", shelfName, viewCreatedNames);
                viewSchemeName = createViewName("СКС Схема", shelfName, viewCreatedNames);

                ViewDrafting viewFasade = ViewDraftingCreate.viewDraftingCreate(doc, viewFasadeName);
                ViewDrafting viewScheme = ViewDraftingCreate.viewDraftingCreate(doc, viewSchemeName);

                viewFasade.Scale = 10;
                viewScheme.Scale = 2;

                CreatePatch.createFrame(doc, viewFasade, frameSymbol, shelfName);

                XYZ pointFacade = new XYZ(0, 0, 0);
                XYZ pointScheme = new XYZ(0, 0, 0);


                foreach (var group in socketGroups)
                {
                  countPorts = SocketNumbering.countPorts(group);
                  countCommuts = countOfCommuts(countPorts);
                  lengthYShkos = (countCommuts * (260 + 150) - 130) / 304.8; // длина проводка у шкоса на схемах

                  List<string> portsNames = SocketNumbering.socketMarking(group, countPorts);

                  pointFacade = CreatePatch.createShkos(doc, viewFasade, pointFacade, shkos1U32Symbol, orgSymbol, shelfName);
                  pointScheme = CreatePatch.createShkosScheme(doc, viewScheme, pointScheme, countCommuts, lengthYShkos,
                    shkosSchemeSymbol);
                  commutNumber = 1;
                  
                  while (countPorts > 0)
                  {
                    if (countPorts > 24)
                    {
                      portNamesForPatch1 = portsNames.GetRange(0, 24);
                      portsNames.RemoveRange(0, 24);
                      if (countPorts > 48)
                      {
                        portNamesForPatch2 = portsNames.GetRange(0, 24);
                        portsNames.RemoveRange(0, 24);
                      }
                      else
                      {
                        portNamesForPatch2 = portsNames;
                      }

                      socketSymbolsIdsPatch1 = SocketGraphicElementIds(group, portNamesForPatch1);
                      socketSymbolsIdsPatch2 = SocketGraphicElementIds(group, portNamesForPatch2);

                      socketRoomsForPatch1 = SocketRoomNumbers(group, portNamesForPatch1);
                      socketRoomsForPatch2 = SocketRoomNumbers(group, portNamesForPatch2);

                      pointFacade = CreatePatch.createPatch48(doc,
                        viewFasade,
                        pointFacade,
                        patch24Symbol,
                        orgSymbol,
                        commut48Symbol,
                        shelfName);

                      pointScheme = CreatePatch.createPatch48Scheme(doc,
                        viewScheme,
                        pointScheme,
                        commutNumber.ToString(),
                        patch24SchemeSymbol,
                        commut48SchemeSymbol,
                        portNamesForPatch1,
                        portNamesForPatch2,
                        socketSymbolsIdsPatch1,
                        socketSymbolsIdsPatch2,
                        socketRoomsForPatch1,
                        socketRoomsForPatch2);

                      countPorts -= 48;
                    }
                    else
                    {
                      pointFacade = CreatePatch.createPatch24(doc,
                        viewFasade,
                        pointFacade,
                        patch24Symbol,
                        orgSymbol,
                        commut24Symbol,
                        shelfName);

                      portNamesForPatch1 = portsNames;
                      socketSymbolsIdsPatch1 = SocketGraphicElementIds(group, portNamesForPatch1);
                      socketRoomsForPatch1 = SocketRoomNumbers(group, portNamesForPatch1);

                      pointScheme = CreatePatch.createPatch24Scheme(doc,
                        viewScheme,
                        pointScheme,
                        commutNumber.ToString(),
                        patch24SchemeSymbol,
                        commut24SchemeSymbol,
                        portNamesForPatch1,
                        socketSymbolsIdsPatch1,
                        socketRoomsForPatch1);


                      countPorts -= 24;
                    }
                    commutNumber++;
                  }


                  foreach (var s in group)
                  {
                    Parameter parMark1 = s.socket.LookupParameter("Розетка - Марка 1");
                    Parameter parMark2 = s.socket.LookupParameter("Розетка - Марка 2");

                    parMark1.Set(s.mark1);
                    parMark2.Set(s.mark2);

                  }













                }
                
              }

            }

          }

          t.Commit();
        }



        TaskDialog.Show("Готово", "ОК");
        return Result.Succeeded;
      }
      // This is where we "catch" potential errors and define how to deal with them
      catch (Autodesk.Revit.Exceptions.OperationCanceledException)
      {
        // If user decided to cancel the operation return Result.Canceled
        return Result.Cancelled;
      }
      catch (Exception ex)
      {
        // If something went wrong return Result.Failed
        message = ex.Message;
        return Result.Failed;
      }

    }

    public string createViewName(string prefix, string shelfName, IEnumerable<string> viewCreatedNames)
    {
      string viewName;
      string viewNameCore;
      string maxNumView;
      string viewNamePart1;
      int viewNamePart2;

      //имя чертежного вида для шкафа (если вид существует - добавит порядковый номер к нему)
      viewName = String.Format("{0} {1}_1", prefix, shelfName);
      viewNameCore = String.Format("{0} {1}", prefix, shelfName);

      if (viewCreatedNames.Contains(viewName))
      {
        maxNumView = viewCreatedNames.Where(a => a.Contains(viewNameCore)).OrderBy(a => Convert.ToInt32(a.Split('_').Last())).Last().Split('_').Last();
        var viewNameSplit = viewName.Split('_');
        viewNamePart1 = viewNameSplit[0];
        viewNamePart2 = Convert.ToInt32(maxNumView) + 1;
        viewName = String.Format("{0}_{1}", viewNamePart1, viewNamePart2.ToString());
      }

      return viewName;
    }


    public static List<ElementId> SocketGraphicElementIds(IEnumerable<Socket> sockets, List<string> portNamesForPatch)
    {
      List<ElementId> idList = new List<ElementId>();

      foreach (var port in portNamesForPatch)
      {
        var s = sockets.Where(i => i.mark1 == port | i.mark2 == port).First();
        idList.Add(s.symbolId);
      }

      return idList;
    }

    public static List<string> SocketRoomNumbers(IEnumerable<Socket> sockets, List<string> portNamesForPatch)
    {
      List<string> roomList = new List<string>();

      foreach (var port in portNamesForPatch)
      {
        var s = sockets.Where(i => i.mark1 == port | i.mark2 == port).First();
        roomList.Add(s.roomNumber);
      }

      return roomList;
    }


    public int countOfCommuts(int ports)
    {
      int commuts;
      if (ports % 48 > 0)
      {
        commuts = ports / 48 + 1;
      }
      else
      {
        commuts = ports / 48;
      }

      return commuts;
    }

    

    
  }
}
