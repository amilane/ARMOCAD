using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class CreatePatch
  {
    /// <summary>
    /// создание стойки для фасадов
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="view"></param>
    /// <param name="frameSymbol"></param>
    public static void createFrame(
                                    Document doc,
                                    ViewDrafting view,
                                    FamilySymbol frameSymbol,
                                    string shelfName)
    {
      createFasadeDetail(doc, 0, frameSymbol, view, shelfName, "б/н", "Сетевой шкаф", 0.0);
     
    }

    /// <summary>
    /// создание группы для патч-панели на 24 порта (фасады)
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="view"></param>
    /// <param name="point"></param>
    /// <param name="patch24Symbol"></param>
    /// <param name="orgSymbol"></param>
    /// <param name="commut24Symbol"></param>
    /// <returns></returns>
    public static XYZ createPatch24(
                                    Document doc,
                                    ViewDrafting view,
                                    XYZ point,
                                    FamilySymbol patch24Symbol,
                                    FamilySymbol orgSymbol,
                                    FamilySymbol commut24Symbol,
                                    string shelfName)
    {
      double y = point.Y;

      y = createFasadeDetail(doc, y, patch24Symbol, view, shelfName, "4", "Патч-панель RG 45", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, view, shelfName, "5", "Кабельный организатор", 45.0);
      y = createFasadeDetail(doc, y, commut24Symbol, view, shelfName, "2", "Коммутатор 24", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, view, shelfName, "5", "Кабельный организатор", 45.0);
      
      point = new XYZ(0, y, 0);
      return point;
    }

    /// <summary>
    /// создание группы для патч-панели на 48 порта (фасады)
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="view"></param>
    /// <param name="point"></param>
    /// <param name="patch24Symbol"></param>
    /// <param name="orgSymbol"></param>
    /// <param name="commut48Symbol"></param>
    /// <returns></returns>
    public static XYZ createPatch48(
                                      Document doc,
                                      ViewDrafting view,
                                      XYZ point,
                                      FamilySymbol patch24Symbol,
                                      FamilySymbol orgSymbol,
                                      FamilySymbol commut48Symbol,
                                      string shelfName)
    {
      double y = point.Y;

      y = createFasadeDetail(doc, y, patch24Symbol, view, shelfName, "4", "Патч-панель RG 45", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, view, shelfName, "5", "Кабельный организатор", 45.0);
      y = createFasadeDetail(doc, y, commut48Symbol, view, shelfName, "1", "Коммутатор 48", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, view, shelfName, "5", "Кабельный организатор", 45.0);
      y = createFasadeDetail(doc, y, patch24Symbol, view, shelfName, "4", "Патч-панель RG 45", 45.0);

      point = new XYZ(0, y, 0);
      return point;
    }

    /// <summary>
    /// создание шкоса для фасадов
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="view"></param>
    /// <param name="point"></param>
    /// <param name="shkos1U32Symbol"></param>
    /// <param name="orgSymbol"></param>
    /// <returns></returns>
    public static XYZ createShkos(
                                    Document doc,
                                    ViewDrafting view,
                                    XYZ point,
                                    FamilySymbol shkos1U32Symbol,
                                    FamilySymbol orgSymbol,
                                    string shelfName)
    {
      double y = point.Y - 45.0 / 304.8;


      y = createFasadeDetail(doc, y, shkos1U32Symbol, view, shelfName, "3", "Оптическая патч-панель", 45.0);
      y = createFasadeDetail(doc, y, orgSymbol, view, shelfName, "5", "Кабельный организатор", 45.0);

      point = new XYZ(0, y, 0);
      return point;
    }

    public static XYZ createServer(
      Document doc,
      ViewDrafting view,
      XYZ point,
      FamilySymbol shkos2U96Symbol,
      FamilySymbol core24sSymbol,
      FamilySymbol core24tSymbol,
      FamilySymbol routerSymbol,
      string shelfName)
    {
      double y = point.Y - 45.0 / 304.8;

      y = createFasadeDetail(doc, y, shkos2U96Symbol, view, shelfName, "3", "Оптическая патч-панель", 90.0);
      y = createFasadeDetail(doc, y, shkos2U96Symbol, view, shelfName, "3", "Оптическая патч-панель", 90.0);
      y = createFasadeDetail(doc, y, shkos2U96Symbol, view, shelfName, "3", "Оптическая патч-панель", 90.0);

      y = createFasadeDetail(doc, y, core24sSymbol, view, shelfName, "6", "Ядро сети Cisco WS-C3750X-24S-S", 45.0);
      y = createFasadeDetail(doc, y, core24tSymbol, view, shelfName, "7", "Ядро сети Cisco WS-C3750X-24T-S", 45.0);

      y -= 45.0 / 304.8;

      y = createFasadeDetail(doc, y, routerSymbol, view, shelfName, "8", "Маршрутизатор CISCO", 90.0);

      y -= 45.0 / 304.8;

      point = new XYZ(0, y, 0);
      return point;
    }


    public static XYZ createPatch24Scheme(
      Document doc,
      ViewDrafting view,
      XYZ point,
      string commutNumber,
      FamilySymbol patch24SchemeSymbol,
      FamilySymbol commut24SchemeSymbol,
      List<string> portNamesForPatch1,
      List<ElementId> socketSymbolsIdsPatch1,
      List<string> socketRoomsForPatch1)
    {
      double y = point.Y;
      string patchNumber;

      Parameter parCalloutsUp;
      Parameter parPortsCount24;
      Parameter parCountOfPorts;
      Parameter parPatchNumber;


      patchNumber = String.Format("{0}.1", commutNumber);

      FamilyInstance patch = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), patch24SchemeSymbol, view);
      y -= 130.0 / 304.8;

      parCalloutsUp = patch.LookupParameter("Выноски сверху");
      parPortsCount24 = patch.LookupParameter("Портов на проводнике 24");
      parCountOfPorts = patch.LookupParameter("Порт - Количество занятых");
      parPatchNumber = patch.LookupParameter("Панель - Номер");


      parCalloutsUp.Set(0);
      parPortsCount24.Set(1);
      parCountOfPorts.Set(portNamesForPatch1.Count);
      parPatchNumber.Set(patchNumber);


      SetParameter(patch, "П", portNamesForPatch1);
      SetParameter(patch, "Тип розетки ", socketSymbolsIdsPatch1);
      SetParameter(patch, "Помещение ", socketRoomsForPatch1);

      FamilyInstance commutator = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), commut24SchemeSymbol, view);
      y -= 150.0 / 304.8;

      parCountOfPorts = commutator.LookupParameter("Порт - Количество занятых");
      parPatchNumber = commutator.LookupParameter("Панель - Номер");

      parCountOfPorts.Set(portNamesForPatch1.Count);
      parPatchNumber.Set(commutNumber);

      point = new XYZ(0, y, 0);
      return point;
    }

    public static XYZ createPatch48Scheme(
      Document doc,
      ViewDrafting view,
      XYZ point,
      string commutNumber,
      FamilySymbol patch24SchemeSymbol,
      FamilySymbol commut48SchemeSymbol,
      List<string> portNamesForPatch1,
      List<string> portNamesForPatch2,
      List<ElementId> socketSymbolsIdsPatch1,
      List<ElementId> socketSymbolsIdsPatch2,
      List<string> socketRoomsForPatch1,
      List<string> socketRoomsForPatch2)
    {
      double y = point.Y;
      string patchNumber1;
      string patchNumber2;

      Parameter parCalloutsUp;
      Parameter parPortsCount24;
      Parameter parCountOfPorts;
      Parameter parPatchNumber;


      patchNumber1 = String.Format("{0}.1", commutNumber);
      patchNumber2 = String.Format("{0}.2", commutNumber);

      FamilyInstance patch1 = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), patch24SchemeSymbol, view);
      y -= 130.0 / 304.8;

      parCalloutsUp = patch1.LookupParameter("Выноски сверху");
      parPortsCount24 = patch1.LookupParameter("Портов на проводнике 24");
      parCountOfPorts = patch1.LookupParameter("Порт - Количество занятых");
      parPatchNumber = patch1.LookupParameter("Панель - Номер");


      parCalloutsUp.Set(0);
      parPortsCount24.Set(0);
      parCountOfPorts.Set(24);
      parPatchNumber.Set(patchNumber1);


      SetParameter(patch1, "П", portNamesForPatch1);
      SetParameter(patch1, "Тип розетки ", socketSymbolsIdsPatch1);
      SetParameter(patch1, "Помещение ", socketRoomsForPatch1);

      FamilyInstance commutator = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), commut48SchemeSymbol, view);
      y -= 130.0 / 304.8;

      parCountOfPorts = commutator.LookupParameter("Порт - Количество занятых");
      parPatchNumber = commutator.LookupParameter("Панель - Номер");

      parCountOfPorts.Set(24 + socketSymbolsIdsPatch2.Count);
      parPatchNumber.Set(commutNumber);


      FamilyInstance patch2 = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), patch24SchemeSymbol, view);
      y -= 150.0 / 304.8;

      parCalloutsUp = patch2.LookupParameter("Выноски сверху");
      parPortsCount24 = patch2.LookupParameter("Портов на проводнике 24");
      parCountOfPorts = patch2.LookupParameter("Порт - Количество занятых");
      parPatchNumber = patch2.LookupParameter("Панель - Номер");

      parCalloutsUp.Set(1);
      parPortsCount24.Set(0);
      parCountOfPorts.Set(socketSymbolsIdsPatch2.Count);
      parPatchNumber.Set(patchNumber2);


      SetParameter(patch2, "П", portNamesForPatch2);
      SetParameter(patch2, "Тип розетки ", socketSymbolsIdsPatch2);
      SetParameter(patch2, "Помещение ", socketRoomsForPatch2);

      point = new XYZ(0, y, 0);
      return point;
    }


    public static void SetParameter(FamilyInstance patch, string prefixParameterName, List<string> values)
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
    public static void SetParameter(FamilyInstance patch, string prefixParameterName, List<ElementId> values)
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

    public static XYZ createShkosScheme(Document doc,
      ViewDrafting view,
      XYZ point,
      int commutCount,
      double lengthY,
      FamilySymbol shkosSchemeSymbol)
    {
      double y = point.Y;

      FamilyInstance shkos = doc.Create.NewFamilyInstance(new XYZ(0, y, 0), shkosSchemeSymbol, view);
      y -= 150.0 / 304.8;

      Parameter parCountCommut = shkos.LookupParameter("Шкос - Количество коммутаторов");
      Parameter parLengthY = shkos.LookupParameter("Длина Y");
      Parameter parPatchNumber = shkos.LookupParameter("Панель - Номер");

      parCountCommut.Set(commutCount);
      parLengthY.Set(lengthY);
      parPatchNumber.Set("Оптическая п/п");

      point = new XYZ(0, y, 0);
      return point;

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







  }
}
