using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class SocketNumbering
  {
    /// <summary>
    /// группировка розеток для Кроссовых шкафов, для Серверных(с функцией Кроссовых) группировать не надо
    /// </summary>
    /// <param name="shelfAndSockets"></param>
    /// <returns> группы розеток по параметру "Розетка - Система" </returns>
    public static List<IEnumerable<Socket>> groupingSocketsByPurpose(ShelfAndSockets shelfAndSockets)
    {
      List<Socket> socList = shelfAndSockets.socketList;
      Element shelf = shelfAndSockets.shelf;
      string panelName = shelf.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

      //дополнительно - марку щита в розетки
      foreach (var i in socList) {
        i.shelfNumber = panelName;
      }

      List<IEnumerable<Socket>> socketPurposes = new List<IEnumerable<Socket>>();

      // Разделение розеток на системы, если шкаф Кроссовый
      if (shelf.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() == "СКС_Шкаф_[серверный, кроссовый] : Кроссовый") {

        var groupSockets = socList.GroupBy(s => s.system);

        foreach (var g in groupSockets) {
          socketPurposes.Add(g.AsEnumerable());
        }

      }
      
      return socketPurposes;
    }

    /// <summary>
    /// метод возвращает количество портов для группы розеток
    /// </summary>
    /// <param name="socketGroup"></param>
    /// <returns></returns>
    public static int countPorts(IEnumerable<Socket> socketGroup)
    {
      int c = 0;
      if (socketGroup.Count() > 0) {
        foreach (var s in socketGroup) {
          c += s.countOfPorts;
        }
      }

      return c;
    }

    /// <summary>
    /// метод создает порядковые номера розеток, маркирует их и возвращает список марок розеток
    /// </summary>
    /// <param name="sockets"></param>
    /// <param name="countOfPorts"></param>
    /// <param name="shelf"></param>
    public static List<string> socketMarking(IEnumerable<Socket> sockets, int countOfPorts)
    {
      int currentPort = 0;

      string numberPatch1;
      string numberPatch2;
      string markSocket1;
      string markSocket2;
      string mark1Value = String.Empty;
      string mark2Value = String.Empty;
      string prefixPurpose = String.Empty;
      string shelfNumber = String.Empty;

      List<string> socketMarks = new List<string>();


      //Определяем префикс для розеток
      //S - security (СБ) / U - user (СП)
      if (sockets.Count() > 0) {
        prefixPurpose = sockets.First().system;
        shelfNumber = sockets.First().shelfNumber;
      }

      //двойные розетки
      var doubleSockets = sockets.Where(i => i.countOfPorts == 2);

      foreach (var s in doubleSockets) {
        currentPort += 1;
        numberPatch1 = currentPatch(countOfPorts, currentPort);
        mark1Value = currentPlaceInPatch(currentPort);

        currentPort += 1;
        numberPatch2 = currentPatch(countOfPorts, currentPort);
        mark2Value = currentPlaceInPatch(currentPort);

        markSocket1 = String.Format("{0}.{1}.{2}", shelfNumber, numberPatch1, mark1Value);
        markSocket2 = String.Format("{0}.{1}.{2}", shelfNumber, numberPatch2, mark2Value);

        if (!string.IsNullOrWhiteSpace(prefixPurpose)) {
          markSocket1 = String.Format("{0}.{1}", prefixPurpose, markSocket1);
          markSocket2 = String.Format("{0}.{1}", prefixPurpose, markSocket2);
        }

        s.mark1 = markSocket1;
        s.mark2 = markSocket2;

        socketMarks.Add(markSocket1);
        socketMarks.Add(markSocket2);

      }


      //одинарные розетки

      var singleSockets = sockets.Where(i => i.countOfPorts == 1);
      foreach (var s in singleSockets) {
        currentPort += 1;
        numberPatch1 = currentPatch(countOfPorts, currentPort);
        mark1Value = currentPlaceInPatch(currentPort);

        markSocket1 = String.Format("{0}.{1}.{2}.{3}", prefixPurpose, shelfNumber, numberPatch1, mark1Value);

        s.mark1 = markSocket1;
        s.mark2 = String.Empty;

        socketMarks.Add(markSocket1);
      }

      return socketMarks;
    }

    /// <summary>
    /// метод возвращает номер патч панели и коммутатора для розетки
    /// </summary>
    /// <param name="ports"></param>
    /// <param name="commonPort"></param>
    /// <returns></returns>

    public static string currentPatch(int ports, int commonPort)
    {
      int commonPatch;
      string numberPatch = String.Empty;
      string literals = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";



      if (commonPort % 24 != 0) {
        commonPatch = commonPort / 24 + 1;
      } else {
        commonPatch = commonPort / 24;
      }

      numberPatch = literals[commonPatch - 1].ToString();


      return numberPatch;
    }

    public static string currentPlaceInPatch(int currentPort)
    {
      int placeInPatch;
      if (currentPort > 24) {
        placeInPatch = currentPort % 24;
        if (placeInPatch == 0) {
          placeInPatch = 24;
        }
      } else {
        placeInPatch = currentPort;
      }

      return placeInPatch.ToString();
    }












  }
}
