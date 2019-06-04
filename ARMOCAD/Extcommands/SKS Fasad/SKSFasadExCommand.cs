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
    public Document doc;

    public ViewDrafting viewFasade;
    public ViewDrafting viewScheme;

    public XYZ pointFacade;
    public XYZ pointScheme;

    public string shelfName;

    public int countCommuts;
    public double lengthYShkos;

    public List<string> portNamesForPatch1;
    public List<ElementId> socketSymbolsIdsPatch1;
    public List<string> socketRoomsForPatch1;
    public char patch1Number;

    public List<string> portNamesForPatch2;
    public List<ElementId> socketSymbolsIdsPatch2;
    public List<string> socketRoomsForPatch2;
    public char patch2Number;


    public static FamilySymbol frameSymbol;//Ф_Шкаф
    public static FamilySymbol orgSymbol;//Ф_Органайзер
    public static FamilySymbol patch24Symbol;//Ф_Патч-панель 24 RG45
    public static FamilySymbol commut24Symbol;//Ф_Коммутатор 24 RG45
    public static FamilySymbol commut48Symbol;//Ф_Коммутатор 48 RG45
    public static FamilySymbol shkos1U32Symbol;//Ф_Шкос 1U 32 волокон
    public static FamilySymbol shkos2U96Symbol;//Ф_Шкос 2U 96 волокон
    public static FamilySymbol core24sSymbol;//Ф_Коммутатор Cisco WS-C3750X-24S-S
    public static FamilySymbol core24tSymbol;//Ф_Коммутатор Cisco WS-C3750X-24T-S
    public static FamilySymbol routerSymbol;//Ф_Маршрутизатор CISCO2921
    public static FamilySymbol patch24SchemeSymbol;//С_Патч-панель 24 RG45
    public static FamilySymbol commut48SchemeSymbol;//С_Коммутатор 48 RG45
    public static FamilySymbol commut24SchemeSymbol;//С_Коммутатор 24 RG45
    public static FamilySymbol shkosSchemeSymbol;//С_Шкос 1U 32 волокон

    public static FamilySymbol socketInBox; //Розетка RJ45 в коробе
    public static FamilySymbol socketInHatch; //Розетка RJ45 в лючке
    public static FamilySymbol socketInInstallBox; //Розетка RJ45 в установочной коробке
    public static FamilySymbol socketMortise;//Розетка RJ45 врезная
    public static FamilySymbol socketPatch;//Розетка RJ45 накладная
    public static FamilySymbol socketPhone;//Розетка телефонная
    public static FamilySymbol socketUnknown;//Розетка неизвестная

    //словарь типоразмеров розеток и их символов
    public Dictionary<string, ElementId> socketSymbolDict;

    
   

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {


      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      doc = ui_doc?.Document;

      string[] symbolNames =
      {
        "Ф_Шкаф",
        "Ф_Органайзер",
        "Ф_Патч-панель 24 RG45",
        "Ф_Коммутатор 24 RG45",
        "Ф_Коммутатор 48 RG45",
        "Ф_Шкос 1U 32 волокон",
        "Ф_Шкос 2U 96 волокон",
        "Ф_Коммутатор Cisco WS-C3750X-24S-S",
        "Ф_Коммутатор Cisco WS-C3750X-24T-S",
        "Ф_Маршрутизатор CISCO2921",
        "С_Патч-панель 24 RG45",
        "С_Коммутатор 48 RG45",
        "С_Коммутатор 24 RG45",
        "С_Шкос 1U 32 волокон",

        "Розетка RJ45 в коробе",
        "Розетка RJ45 в лючке",
        "Розетка RJ45 в установочной коробке",
        "Розетка RJ45 врезная",
        "Розетка RJ45 накладная",
        "Розетка телефонная",
        "Розетка неизвестная"
      };

      List<FamilySymbol> symbols2 = new List<FamilySymbol>();

      //собираем FamilySymbol'ы проверяем их наличие в проекте
      string alert = String.Empty;
      foreach (var name in symbolNames)
      {
        var x = Util.GetFamilySymbolByFamilyName(doc, name);
        if (x == null)
        {
          alert += $"{name};\n";
        }
        else
        {
          symbols2.Add(x);
        }
      }

      if (alert != String.Empty)
      {
        Util.InfoMsg2("В модели отсутствуют семейства:", alert);
        return Result.Cancelled;
      }

      //если семейства в проект загружены, мы получили список семейств symbols2
      frameSymbol = symbols2[0];
      orgSymbol = symbols2[1];
      patch24Symbol = symbols2[2];
      commut24Symbol = symbols2[3];
      commut48Symbol = symbols2[4];
      shkos1U32Symbol = symbols2[5];
      shkos2U96Symbol = symbols2[6];
      core24sSymbol = symbols2[7];
      core24tSymbol = symbols2[8];
      routerSymbol = symbols2[9];
      patch24SchemeSymbol = symbols2[10];
      commut48SchemeSymbol = symbols2[11];
      commut24SchemeSymbol = symbols2[12];
      shkosSchemeSymbol = symbols2[13];

      socketInBox = symbols2[14];
      socketInHatch = symbols2[15];
      socketInInstallBox = symbols2[16];
      socketMortise = symbols2[17];
      socketPatch = symbols2[18];
      socketPhone = symbols2[19];
      socketUnknown = symbols2[20];

      socketSymbolDict = new Dictionary<string, ElementId>
      {
        ["1xRJ-45, Врезная"] = socketMortise.Id,
        ["2xRJ-45, Врезная"] = socketMortise.Id,
        ["1xRJ-45, Лючок"] = socketInHatch.Id,
        ["2xRJ-45, Лючок"] = socketInHatch.Id,
        ["1xRJ-45, Накладная"] = socketPatch.Id,
        ["2xRJ-45, Накладная"] = socketPatch.Id,
        ["1xRJ-45, Размещение в коробе"] = socketInBox.Id,
        ["2xRJ-45, Размещение в коробе"] = socketInBox.Id,
      };
      

      frameSymbol.Activate();
      orgSymbol.Activate();
      patch24Symbol.Activate();
      commut24Symbol.Activate();
      commut24Symbol.Activate();
      commut48Symbol.Activate();
      shkos1U32Symbol.Activate();
      shkos2U96Symbol.Activate();
      patch24SchemeSymbol.Activate();
      commut48SchemeSymbol.Activate();
      commut24SchemeSymbol.Activate();
      shkosSchemeSymbol.Activate();
      core24sSymbol.Activate();
      core24tSymbol.Activate();
      routerSymbol.Activate();

      
      #region try


      try
      {


        using (Transaction t = new Transaction(doc, "SKSFasad"))
        {
          t.Start();



          // Кроссовые шкафы, в которые собираются розетки
          IList<Element> shelfs = Util.GetElementsByStringParameter(
            doc,
            BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
            "СКС_Шкаф_[серверный, кроссовый] : Кроссовый");


          // розетки типоразмеров из списка socketTypeNames
          IList<Element> sockets = Util.GetElementsByStringParameter(
            doc,
            BuiltInParameter.ELEM_FAMILY_PARAM,
            "СКС_Розетка_[1xRJ45, 2xRJ45, TV, radio]")
            .Where(i => socketSymbolDict.Keys.Contains(i.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString()))
            .ToList();


          // собираем розетки в шкафы по параметру "Розетка - Шкаф"
          if (shelfs.Count > 0 && sockets.Count > 0)
          {

            List<ShelfAndSockets> shelfAndSockets = new List<ShelfAndSockets>();
            foreach (var i in shelfs)
            {
              ShelfAndSockets sas = new ShelfAndSockets();
              sas.shelf = i;

              string shelfNumber = i.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

              var socketsForShelf = sockets.Where(s => s.LookupParameter("Розетка - Шкаф").AsString() == shelfNumber);

              foreach (var s in socketsForShelf)
              {
                Socket socket = new Socket();
                socket.socket = s;

                string socketTypeName = s.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();

                //символ розетки
                socket.symbolId = socketSymbolDict[socketTypeName];

                //количество портов
                if (socketTypeName.Contains("1xRJ-45"))
                {
                  socket.countOfPorts = 1;
                }
                else if (socketTypeName.Contains("2xRJ-45"))
                {
                  socket.countOfPorts = 2;
                }

                //система
                socket.system = s.LookupParameter("Розетка - Система").AsString();

                var space = ((FamilyInstance)s).Space;
                if (space != null)
                {
                  socket.roomNumber = space.Number;
                }
                else
                {
                  socket.roomNumber = string.Empty;
                }

                sas.socketList.Add(socket);      

              }

              if (sas.socketList.Count > 0)
              {
                shelfAndSockets.Add(sas);
              }

            }




            //имена существующих чертежных видов
            var viewCreatedNames = ViewDraftingCreate.viewDraftingNames(doc);

            
            string viewFasadeName;
            string viewSchemeName;

            int countPorts;
            int commutNumber;
            
            int typeOfCommutators;

            foreach (var i in shelfAndSockets)
            {
              var socketGroups = SocketNumbering.groupingSocketsByPurpose(i);

              Element shelf = i.shelf;
              shelfName = shelf.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
              typeOfCommutators = shelf.LookupParameter("Коммутаторы по 24 порта").AsInteger();

              viewFasadeName = createViewName("СКС Фасад", shelfName, viewCreatedNames);
              viewSchemeName = createViewName("СКС Схема", shelfName, viewCreatedNames);

              viewFasade = ViewDraftingCreate.viewDraftingCreate(doc, viewFasadeName);
              viewScheme = ViewDraftingCreate.viewDraftingCreate(doc, viewSchemeName);

              viewFasade.Scale = 10;
              viewScheme.Scale = 2;

              createFrame();

              pointFacade = new XYZ(0, 0, 0);
              pointScheme = new XYZ(0, 0, 0);

              foreach (var group in socketGroups)
              {
                countPorts = SocketNumbering.countPorts(group);
                countCommuts = countOfCommuts(countPorts, typeOfCommutators);


                List<string> portsNames = SocketNumbering.socketMarking(group, countPorts);

                //Создаем шкосы если шкаф Кроссовый
                if (shelf.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() ==
                    "Шкаф СКС: Кроссовый")
                {
                  pointFacade = createShkos();
                }

                string literals = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                commutNumber = 1;

                //алгоритм для 48-портовых коммутаторов
                if (typeOfCommutators == 0)
                {

                  lengthYShkos = (countCommuts * (260 + 150) - 130) / 304.8; // длина проводка у шкоса на схемах
                  pointScheme = createShkosScheme();

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

                      patch1Number = literals[0];
                      literals = literals.Remove(0, 1);
                      patch2Number = literals[0];
                      literals = literals.Remove(0, 1);

                      pointFacade = createPatch48();

                      pointScheme = createPatch48Scheme();

                      countPorts -= 48;
                    }
                    else
                    {
                      pointFacade = createPatch24();

                      portNamesForPatch1 = portsNames;
                      socketSymbolsIdsPatch1 = SocketGraphicElementIds(group, portNamesForPatch1);
                      socketRoomsForPatch1 = SocketRoomNumbers(group, portNamesForPatch1);
                      patch1Number = literals[0];
                      literals = literals.Remove(0, 1);

                      pointScheme = createPatch24Scheme();


                      countPorts -= 24;
                    }
                    commutNumber++;
                  }
                }
                //алгоритм для 24-портовых
                else if (typeOfCommutators == 1)
                {

                  lengthYShkos = countCommuts * 280 / 304.8; // длина проводка у шкоса на схемах
                  pointScheme = createShkosScheme();

                  while (countPorts > 0)
                  {
                    pointFacade = createPatch24();
                    if (countPorts > 24)
                    {
                      portNamesForPatch1 = portsNames.GetRange(0, 24);
                      portsNames.RemoveRange(0, 24);
                    }
                    else
                    {
                      portNamesForPatch1 = portsNames;
                    }
                    socketSymbolsIdsPatch1 = SocketGraphicElementIds(group, portNamesForPatch1);
                    socketRoomsForPatch1 = SocketRoomNumbers(group, portNamesForPatch1);
                    patch1Number = literals[0];
                    literals = literals.Remove(0, 1);

                    pointScheme = createPatch24Scheme();

                    countPorts -= 24;
                    commutNumber++;
                  }

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



      #endregion

     
















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


    public int countOfCommuts(int ports, int typeOfCommutators)
    {
      int commuts;
      int totalOnCommutator = 0;

      if (typeOfCommutators == 1)
      {
        totalOnCommutator = 24;
      }
      else if (typeOfCommutators == 0)
      {
        totalOnCommutator = 48;
      }

      if (ports % totalOnCommutator > 0)
      {
        commuts = ports / totalOnCommutator + 1;
      }
      else
      {
        commuts = ports / totalOnCommutator;
      }

      return commuts;
    }


    public void checkCorrectSockets(IList<Element> shelfs, IList<Element> sockets)
    {
      string alert = String.Empty;

      List<string> namesOfShelfs = new List<string>();
      foreach (var s in shelfs)
      {
        namesOfShelfs.Add(s.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString());
      }

      var uncorrectSockets = sockets.Where(i => !namesOfShelfs.Contains(i.LookupParameter("Розетка - Шкаф").AsString()));

      if (uncorrectSockets.Count() > 0)
      {
        alert =
          "Проверьте параметр на данных розетках. Или он пустой, или в модели нет шкафа, соответствующего параметру в розетке:\n" +
          "ID розетки - Номер шкафа\n";
        foreach (var s in uncorrectSockets)
        {
          alert += String.Format("{0} {1}\n", s.Id.ToString(), s.LookupParameter("Розетка - Шкаф").AsString());
        }

        TaskDialog.Show("Некорректный параметр \"Розетка - Шкаф\"", alert);
      }
    }


    #region CreatePatch Methods

    public double createFasadeDetail(
      Document doc,
      double y,
      FamilySymbol symbol,
      ViewDrafting view,
      string position,
      string name,
      double h)
    {
      var detail = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), symbol, view);
      detail.LookupParameter("Панель - Имя шкафа").Set(shelfName);
      detail.LookupParameter("AG_Spc_Позиция").Set(position);
      detail.LookupParameter("AG_Spc_Наименование").Set(name);
      y -= h / 304.8;
      return y;
    }
    
    /// создание шкоса для фасадов
    public XYZ createShkos()
    {
      double y = pointFacade.Y - 45.0 / 304.8;

      y = createFasadeDetail(doc, y, shkos1U32Symbol, viewFasade, "3", "Оптическая патч-панель", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, viewFasade, "5", "Кабельный организатор", 45.0);

      pointFacade = new XYZ(0, y, 0);
      return pointFacade;
    }

    public XYZ createShkosScheme()
    {
      double y = pointScheme.Y;

      FamilyInstance shkos = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), shkosSchemeSymbol, viewScheme);
      y -= 150.0 / 304.8;

      Parameter parCountCommut = shkos.LookupParameter("Шкос - Количество коммутаторов");
      Parameter parLengthY = shkos.LookupParameter("Длина Y");
      Parameter parPatchNumber = shkos.LookupParameter("Панель - Номер");

      parCountCommut.Set(countCommuts);
      parLengthY.Set(lengthYShkos);
      parPatchNumber.Set("Оптическая п/п");

      pointScheme = new XYZ(0, y, 0);
      return pointScheme;

    }


    public XYZ createPatch24Scheme()
    {
      double y = pointScheme.Y;

      Parameter parCalloutsUp;
      Parameter parPortsCount24;
      Parameter parCountOfPorts;
      Parameter parPatchNumber;


      FamilyInstance patch = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), patch24SchemeSymbol, viewScheme);
      y -= 130.0 / 304.8;

      parCalloutsUp = patch.LookupParameter("Выноски сверху");
      parPortsCount24 = patch.LookupParameter("Портов на проводнике 24");
      parCountOfPorts = patch.LookupParameter("Порт - Количество занятых");
      parPatchNumber = patch.LookupParameter("Панель - Номер");


      parCalloutsUp.Set(0);
      parPortsCount24.Set(1);
      parCountOfPorts.Set(portNamesForPatch1.Count);
      parPatchNumber.Set(patch1Number.ToString());


      SetParameter(patch, "П", portNamesForPatch1);
      SetParameter(patch, "Тип розетки ", socketSymbolsIdsPatch1);
      SetParameter(patch, "Помещение ", socketRoomsForPatch1);

      FamilyInstance commutator = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), commut24SchemeSymbol, viewScheme);
      y -= 150.0 / 304.8;

      parCountOfPorts = commutator.LookupParameter("Порт - Количество занятых");
      parPatchNumber = commutator.LookupParameter("Панель - Номер");

      parCountOfPorts.Set(portNamesForPatch1.Count);
      parPatchNumber.Set(countCommuts);

      pointScheme = new XYZ(0, y, 0);
      return pointScheme;
    }

    public void SetParameter(FamilyInstance patch, string prefixParameterName, List<string> values)
    {
      int n = 1;
      foreach (var value in values)
      {
        string paramName = String.Format("{0}{1}", prefixParameterName, n.ToString());
        Parameter param = patch.LookupParameter(paramName);
        param.Set(value);
        n++;
      }
    }
    public void SetParameter(FamilyInstance patch, string prefixParameterName, List<ElementId> values)
    {
      int n = 1;
      foreach (var value in values)
      {
        string paramName = String.Format("{0}{1}", prefixParameterName, n.ToString());
        Parameter param = patch.LookupParameter(paramName);
        param.Set(value);
        n++;
      }
    }

    public XYZ createPatch48Scheme()
    {
      double y = pointScheme.Y;

      Parameter parCalloutsUp;
      Parameter parPortsCount24;
      Parameter parCountOfPorts;
      Parameter parPatchNumber;


      FamilyInstance patch1 = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), patch24SchemeSymbol, viewScheme);
      y -= 130.0 / 304.8;

      parCalloutsUp = patch1.LookupParameter("Выноски сверху");
      parPortsCount24 = patch1.LookupParameter("Портов на проводнике 24");
      parCountOfPorts = patch1.LookupParameter("Порт - Количество занятых");
      parPatchNumber = patch1.LookupParameter("Панель - Номер");


      parCalloutsUp.Set(0);
      parPortsCount24.Set(0);
      parCountOfPorts.Set(24);
      parPatchNumber.Set(patch1Number.ToString());


      SetParameter(patch1, "П", portNamesForPatch1);
      SetParameter(patch1, "Тип розетки ", socketSymbolsIdsPatch1);
      SetParameter(patch1, "Помещение ", socketRoomsForPatch1);

      FamilyInstance commutator = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), commut48SchemeSymbol, viewScheme);
      y -= 130.0 / 304.8;

      parCountOfPorts = commutator.LookupParameter("Порт - Количество занятых");
      parPatchNumber = commutator.LookupParameter("Панель - Номер");

      parCountOfPorts.Set(24 + socketSymbolsIdsPatch2.Count);
      parPatchNumber.Set(countCommuts);


      FamilyInstance patch2 = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), patch24SchemeSymbol, viewScheme);
      y -= 150.0 / 304.8;

      parCalloutsUp = patch2.LookupParameter("Выноски сверху");
      parPortsCount24 = patch2.LookupParameter("Портов на проводнике 24");
      parCountOfPorts = patch2.LookupParameter("Порт - Количество занятых");
      parPatchNumber = patch2.LookupParameter("Панель - Номер");

      parCalloutsUp.Set(1);
      parPortsCount24.Set(0);
      parCountOfPorts.Set(socketSymbolsIdsPatch2.Count);
      parPatchNumber.Set(patch2Number.ToString());


      SetParameter(patch2, "П", portNamesForPatch2);
      SetParameter(patch2, "Тип розетки ", socketSymbolsIdsPatch2);
      SetParameter(patch2, "Помещение ", socketRoomsForPatch2);

      pointScheme = new XYZ(0, y, 0);
      return pointScheme;
    }

    public  XYZ createPatch24()
    {
      double y = pointFacade.Y;

      y = createFasadeDetail(doc, y, patch24Symbol, viewFasade, shelfName, "4", "Патч-панель RG 45", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, viewFasade, shelfName, "5", "Кабельный организатор", 45.0);
      y = createFasadeDetail(doc, y, commut24Symbol, viewFasade, shelfName, "2", "Коммутатор 24", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, viewFasade, shelfName, "5", "Кабельный организатор", 45.0);

      pointFacade = new XYZ(0, y, 0);
      return pointFacade;
    }

    public static double createFasadeDetail(
      Document doc,
      double y,
      FamilySymbol symbol,
      ViewDrafting view,
      string shelfName,
      string position,
      string name,
      double h)
    {
      var detail = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), symbol, view);
      detail.LookupParameter("Панель - Имя шкафа").Set(shelfName);
      detail.LookupParameter("AG_Spc_Позиция").Set(position);
      detail.LookupParameter("AG_Spc_Наименование").Set(name);
      y -= h / 304.8;
      return y;
    }


    /// создание стойки для фасадов на фасаде
    public void createFrame()
    {
      createFasadeDetail(doc, 0, frameSymbol, viewFasade, shelfName, "б/н", "Сетевой шкаф", 0.0);
    }

    // на фасаде
    public XYZ createPatch48()
    {
      double y = pointFacade.Y;

      y = createFasadeDetail(doc, y, patch24Symbol, viewFasade, shelfName, "4", "Патч-панель RG 45", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, viewFasade, shelfName, "5", "Кабельный организатор", 45.0);
      y = createFasadeDetail(doc, y, commut48Symbol, viewFasade, shelfName, "1", "Коммутатор 48", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, viewFasade, shelfName, "5", "Кабельный организатор", 45.0);
      y = createFasadeDetail(doc, y, patch24Symbol, viewFasade, shelfName, "4", "Патч-панель RG 45", 45.0);

      pointFacade = new XYZ(0, y, 0);
      return pointFacade;
    }

    #endregion//CreatePatch Methods


  }
}
