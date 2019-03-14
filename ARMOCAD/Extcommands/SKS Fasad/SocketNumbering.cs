using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class SocketNumbering
  {
    /// <summary>
    /// метод возвращает группы розеток по параметру Розетка - Система
    /// </summary>
    /// <param name="shelfAndSockets"></param>
    /// <returns></returns>
    public static List<IEnumerable<Socket>> groupingSocketsByPurpose(ShelfAndSockets shelfAndSockets)
    {
      List<Socket> socList = shelfAndSockets.socketList;
      Element shelf = shelfAndSockets.shelf;
      string panelName = shelf.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

      //дополнительно - марку щита в розетки
      foreach (var i in socList)
      {
        i.shelfNumber = panelName;
      }

      var serviceSockets = socList.Where(i =>
        i.system == "СБ");
      var userSockets = socList.Where(i =>
        i.system == "СП");

      //var socketPurposes = new List<IEnumerable<Socket>> { serviceSockets, userSockets };

      List<IEnumerable<Socket>> socketPurposes = new List<IEnumerable<Socket>>();

      if (serviceSockets.Count() > 0)
      {
        socketPurposes.Add(serviceSockets);
      }
      if (userSockets.Count() > 0)
      {
        socketPurposes.Add(userSockets);
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
      if (socketGroup.Count() > 0)
      {
        foreach (var s in socketGroup)
        {
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
      int nWifi = 1;
      int nNet = 1;
      int nCam = 1;
      int nPhone = 1;
      int currentPort = 0;

      string numberPatchAndCommut1;
      string numberPatchAndCommut2;
      string markSocket1;
      string markSocket2;
      string mark1Value = "";
      string mark2Value = "";
      string comment;
      string fullMark1Value;
      string fullMark2Value;
      string prefixPurpose = "";
      string prefixComment1 = "";
      string prefixComment2 = "";
      string prefixCheck = "";
      string shelfNumber = "";

      List<string> socketMarks = new List<string>();


      //Определяем префикс для розеток
      //S - security (СБ) / U - user (СП)
      if (sockets.Count() > 0)
      {
        prefixCheck = sockets.First().system;
        shelfNumber = sockets.First().shelfNumber;
      }

      if (prefixCheck == "СБ")
      {
        prefixPurpose = "S";
      }
      else if (prefixCheck == "СП")
      {
        prefixPurpose = "U";
      }

      //двойные розетки
      var doubleSockets = sockets.Where(i => i.countOfPorts == 2);

      foreach (var s in doubleSockets)
      {
        currentPort += 1;
        numberPatchAndCommut1 = currentPatchAndCommut(countOfPorts, currentPort);

        currentPort += 1;
        numberPatchAndCommut2 = currentPatchAndCommut(countOfPorts, currentPort);


        comment = s.socketComment;
        if (comment == null || comment.Trim() == "")
        {
          prefixComment1 = "RG";
          mark1Value = nNet.ToString();
          nNet++;
          prefixComment2 = "TL";
          mark2Value = nPhone.ToString();
          nPhone++;

        }
        else if (comment.Trim() != "")
        {
          switch (comment)
          {
            case "Интернет":
              prefixComment1 = "RG";
              prefixComment2 = "RG";
              mark1Value = nNet.ToString();
              nNet++;
              mark2Value = nNet.ToString();
              nNet++;
              break;
            case "Телефон":
              prefixComment1 = "TL";
              prefixComment2 = "TL";
              mark1Value = nPhone.ToString();
              nPhone++;
              mark2Value = nPhone.ToString();
              nPhone++;
              break;
            case "Камера":
              prefixComment1 = "AC";
              prefixComment2 = "AC";
              mark1Value = nCam.ToString();
              nCam++;
              mark2Value = nCam.ToString();
              nCam++;
              break;
            case "WiFi":
              prefixComment1 = "WF";
              prefixComment2 = "WF";
              mark1Value = nWifi.ToString();
              nWifi++;
              mark2Value = nWifi.ToString();
              nWifi++;
              break;
          }
        }

        markSocket1 = String.Format("{0}.{1}.{2}.{3}.", prefixPurpose, prefixComment1, shelfNumber, numberPatchAndCommut1);
        markSocket2 = String.Format("{0}.{1}.{2}.{3}.", prefixPurpose, prefixComment2, shelfNumber, numberPatchAndCommut2);

        fullMark1Value = markSocket1 + mark1Value;
        fullMark2Value = markSocket2 + mark2Value;

        s.mark1 = fullMark1Value;
        s.mark2 = fullMark2Value;

        socketMarks.Add(fullMark1Value);
        socketMarks.Add(fullMark2Value);

      }


      //одинарные розетки

      var singleSockets = sockets.Where(i => i.countOfPorts == 1);
      foreach (var s in singleSockets)
      {
        currentPort += 1;
        numberPatchAndCommut1 = currentPatchAndCommut(countOfPorts, currentPort);

        comment = s.socketComment;

        switch (comment)
        {
          case "Интернет":
            prefixComment1 = "RG";
            mark1Value = nNet.ToString();
            nNet++;
            break;
          case "Телефон":
            prefixComment1 = "TL";
            mark1Value = nPhone.ToString();
            nPhone++;
            break;
          case "Камера":
            prefixComment1 = "AC";
            mark1Value = nCam.ToString();
            nCam++;
            break;
          case "WiFi":
            prefixComment1 = "WF";
            mark1Value = nWifi.ToString();
            nWifi++;
            break;
        }

        markSocket1 = String.Format("{0}.{1}.{2}.{3}.", prefixPurpose, prefixComment1, shelfNumber, numberPatchAndCommut1);

        fullMark1Value = markSocket1 + mark1Value;

        s.mark1 = fullMark1Value;
        s.mark2 = "";

        socketMarks.Add(fullMark1Value);
      }

      var rgSocketMarks = socketMarks.Where(i => i.Contains("RG"));
      var tlSocketMarks = socketMarks.Where(i => i.Contains("TL"));
      var acSocketMarks = socketMarks.Where(i => i.Contains("AC"));
      var wfSocketMarks = socketMarks.Where(i => i.Contains("WF"));

      var sorteredSocketMarks = rgSocketMarks.Concat(tlSocketMarks).Concat(acSocketMarks).Concat(wfSocketMarks).ToList();


      return sorteredSocketMarks;
    }

    /// <summary>
    /// метод возвращает номер патч панели и коммутатора для розетки
    /// </summary>
    /// <param name="ports"></param>
    /// <param name="commonPort"></param>
    /// <returns></returns>

    public static string currentPatchAndCommut(int ports, int commonPort)
    {
      int commonCommut;
      int commonPatch;
      string numberPatchAndCommut;

      if (ports > 24)
      {
        if (commonPort % 48 != 0)
        {
          commonCommut = commonPort / 48 + 1;
        }
        else
        {
          commonCommut = commonPort / 48;
        }
      }
      else
      {
        commonCommut = 1;
      }

      if (commonPort % 24 != 0)
      {
        commonPatch = commonPort / 24 + 1;
      }
      else
      {
        commonPatch = commonPort / 24;
      }


      if (commonPatch % 2 == 0)
      {
        commonPatch = 2;
      }
      else
      {
        commonPatch = 1;
      }

      numberPatchAndCommut = String.Format("{0}.{1}", commonPatch.ToString(), commonCommut.ToString());

      return numberPatchAndCommut;
    }















  }
}
