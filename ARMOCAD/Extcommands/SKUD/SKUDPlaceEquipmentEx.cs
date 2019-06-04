using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class SKUDPlaceEquipmentEx : IExternalCommand
  {
    private bool hFlipped;
    private bool fFlipped;
    private Level level;
    private Document doc;
    private FilteredElementCollector famTypes;
    private XYZ tdLocation;
    private XYZ tdFO;
    private XYZ tdHO;
    private double tdW;
    private double tdH;
    private double tdD;
    private double doubleDoor;
    public double tdZShift;

    public List<Element> elems;

    public Dictionary<string, string> paramsAndFamilies = new Dictionary<string, string>
    {
      ["Блок питания"] = "EE_Бокс_резервного_электропитания_[БР]",
      ["Вызывная панель видеодомофона"] = "СКУД_Оборудование_[Вызывная панель видеодомофона]",
      ["Дверной доводчик"] = "СКУД_Оборудование_[Дверной доводчик]",
      ["Замок электромагнитный"] = "СКУД_Замок_[Накладной электромагнитный]",
      ["Замок электромеханический"] = "СКУД_Замок_[Накладной электромеханический]",
      ["Защелка электромеханическая"] = "СКУД_Замок_[Защелка электромеханическая]",
      ["Кнопка выхода"] = "СКУД_Оборудование_[Кнопка выхода]",
      ["Модуль контроля доступа"] = "EE_СКУД_Модуль контроля доступа_[МКД]",
      ["Магнитоконтактный извещатель"] = "СОТС_Датчики_[Магнитоконтактный]",
      ["Монитор видеодомофона"] = "СКУД_Оборудование_[Монитор видеодомофона]",
      ["Считыватель вход"] = "СКУД_Считыватель_[Для карт доступа]",
      ["Считыватель выход"] = "СКУД_Считыватель_[Для карт доступа]",
      ["Турникет"] = "СКУД_Оборудование_[Турникет]",
      ["Устройство разблокировки двери"] = "СКУД_Оборудование_[Устройство разблокировки двери]",
      
    };

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp?.ActiveUIDocument;
      doc = uidoc?.Document;

      //Сбор точек доступа среди выбранных элементов
      ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
      elems = new List<Element>();

      if (0 == selectedIds.Count) {
        Util.InfoMsg2("Выберите точки доступа", "");
        return Result.Cancelled;
      } else {
        foreach (ElementId id in selectedIds) {
          var el = doc.GetElement(id);
          if (((FamilyInstance)el).Symbol.FamilyName == "СКУД_Точка доступа_[ТД]") {
            elems.Add(el);
          }

        }

      }
      

      try {
        using (Transaction t = new Transaction(doc, "Оборудование по ТД")) {
          t.Start();

          string alert = CheckFamilies();
          if (alert != String.Empty)
          {
            Util.InfoMsg2("В модели не загружены семейства:", alert);
            return Result.Cancelled;
          }
          else
          {
            foreach (var td in elems)
            {
              level = (Level)doc.GetElement(td.LevelId);
              tdLocation = ((LocationPoint)td.Location).Point;
              tdFO = ((FamilyInstance)td).FacingOrientation;
              tdHO = ((FamilyInstance)td).HandOrientation;
              hFlipped = ((FamilyInstance)td).HandFlipped;
              fFlipped = ((FamilyInstance)td).FacingFlipped;
              doubleDoor = ((FamilyInstance)td).Symbol.LookupParameter("Двойная дверь").AsInteger();
              tdZShift = td.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).AsDouble();

              tdW = td.LookupParameter("Ширина двери").AsDouble();
              tdH = td.LookupParameter("Высота двери").AsDouble();
              tdD = td.LookupParameter("Толщина стены").AsDouble();

              FamilySymbol tdSymbol = ((FamilyInstance)td).Symbol;

              if (tdSymbol.LookupParameter("Блок питания").AsInteger() == 1)
              {
                CreateEquip(-100.0, 0.0, 2500.0, true, paramsAndFamilies["Блок питания"]);
              }

              if (tdSymbol.LookupParameter("Вызывная панель видеодомофона").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8 - 200.0, 0.0, 1600.0, false, paramsAndFamilies["Вызывная панель видеодомофона"]);
              }

              if (tdSymbol.LookupParameter("Дверной доводчик").AsInteger() == 1)
              {

                if (doubleDoor == 1.0)
                {
                  CreateEquip(-200.0, 20.0, tdH * 304.8 - 100.0, true, paramsAndFamilies["Дверной доводчик"]);
                }
                else
                {
                  CreateEquip(-200.0, 20.0, tdH * 304.8 - 100.0, true, paramsAndFamilies["Дверной доводчик"]);
                }

              }

              if (tdSymbol.LookupParameter("Замок электромагнитный").AsInteger() == 1)
              {

                if (doubleDoor == 1.0)
                {
                  CreateEquip(-tdW * 304.8 / 2 + 200.0, 0.0, tdH * 304.8 - 100.0, true, paramsAndFamilies["Замок электромагнитный"]);
                }
                else
                {
                  CreateEquip(-tdW * 304.8 + 200.0, 0.0, tdH * 304.8 - 100.0, true, paramsAndFamilies["Замок электромагнитный"]);
                }

              }

              if (tdSymbol.LookupParameter("Замок электромеханический").AsInteger() == 1)
              {

                if (doubleDoor == 1.0)
                {
                  CreateEquip(-tdW * 304.8 / 2 + 100.0, 0.0, 1000.0, true, paramsAndFamilies["Замок электромеханический"]);
                }
                else
                {
                  CreateEquip(-tdW * 304.8 + 100.0, 0.0, 1000.0, true, paramsAndFamilies["Замок электромеханический"]);
                }

              }

              if (tdSymbol.LookupParameter("Защелка электромеханическая").AsInteger() == 1)
              {

                if (doubleDoor == 1.0)
                {
                  CreateEquip(-tdW * 304.8 / 2, -1 * tdD * 304.8 / 2, 1000.0, true, paramsAndFamilies["Защелка электромеханическая"]);
                }
                else
                {
                  CreateEquip(-1 * tdW * 304.8, -1 * tdD * 304.8 / 2, 1000.0, true, paramsAndFamilies["Защелка электромеханическая"]);
                }

              }

              if (tdSymbol.LookupParameter("Кнопка выхода").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8 - 200.0, 0.0, 1100.0, true, paramsAndFamilies["Кнопка выхода"]);
              }

              if (tdSymbol.LookupParameter("Модуль контроля доступа").AsInteger() == 1)
              {
                CreateEquip(-400.0, 0.0, 2500.0, true, paramsAndFamilies["Модуль контроля доступа"]);
              }

              if (tdSymbol.LookupParameter("Магнитоконтактный извещатель").AsInteger() == 1)
              {

                if (doubleDoor == 1.0)
                {
                  CreateEquip(-tdW * 304.8 / 2 + 100.0, 0.0, tdH * 304.8, true, paramsAndFamilies["Магнитоконтактный извещатель"]);
                  CreateEquip(-tdW * 304.8 / 2 - 100.0, 0.0, tdH * 304.8, true, paramsAndFamilies["Магнитоконтактный извещатель"]);
                }
                else
                {
                  CreateEquip(-1 * tdW * 304.8 + 100.0, 0.0, tdH * 304.8, true, paramsAndFamilies["Магнитоконтактный извещатель"]);
                }
              }

              if (tdSymbol.LookupParameter("Монитор видеодомофона").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8 - 300.0, 0.0, 1600.0, true, paramsAndFamilies["Монитор видеодомофона"]);
              }

              if (tdSymbol.LookupParameter("Считыватель вход").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8 - 200.0, 0.0, 1100.0, false, paramsAndFamilies["Считыватель вход"]);
              }

              if (tdSymbol.LookupParameter("Считыватель выход").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8 - 200.0, 0.0, 1100.0, true, paramsAndFamilies["Считыватель выход"]);
              }

              if (tdSymbol.LookupParameter("Турникет").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8, -1 * tdD * 304.8 / 2, 0.0, false, paramsAndFamilies["Турникет"]);
              }

              if (tdSymbol.LookupParameter("Устройство разблокировки двери").AsInteger() == 1)
              {
                CreateEquip(-1 * tdW * 304.8 - 200.0, 0.0, 1500.0, true, paramsAndFamilies["Устройство разблокировки двери"], 800.0);
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



    //Функция расстановки семейства относительно Точки доступа

    public void CreateEquip(
      double xOffset,
      double yOffset,
      double zOffset,
      bool inRoom,
      string familyName,
      double ugoY = 0.0)
    {
      double angle;
      XYZ eqLocation = null;
      Line rotAxis = null;

      var fs = Util.GetFamilySymbolByFamilyName(doc, familyName);

      if (!fs.IsActive)
      {
        fs.Activate();
      }

      if (inRoom)
      {

        if (fFlipped)
        {
          angle = tdFO.Negate().AngleTo(new XYZ(0, 1, 0)) + Math.PI;
        }
        else
        {
          angle = tdFO.AngleTo(new XYZ(0, 1, 0));
        }

        eqLocation = tdLocation.Add(tdFO.Negate().Multiply(tdD / 2 + yOffset / 304.8)).Add(tdHO.Negate().Multiply(tdW / 2 + xOffset / 304.8));

      }
      else
      {

        if (fFlipped)
        {
          angle = tdFO.Negate().AngleTo(new XYZ(0, 1, 0));
        }
        else
        {
          angle = tdFO.AngleTo(new XYZ(0, 1, 0)) + Math.PI;
        }

        eqLocation = tdLocation.Add(tdFO.Multiply(tdD / 2 + yOffset / 304.8)).Add(tdHO.Negate().Multiply(tdW / 2 + xOffset / 304.8));




      }


      rotAxis = Line.CreateBound(eqLocation, new XYZ(eqLocation.X, eqLocation.Y, eqLocation.Z + 1.0));
      var eq = doc.Create.NewFamilyInstance(eqLocation, fs, level, StructuralType.NonStructural);
      ElementTransformUtils.RotateElement(doc, eq.Id, rotAxis, angle);
      eq.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(zOffset / 304.8 + tdZShift);
      eq.LookupParameter("Смещение УГО по Y")?.Set(ugoY / 304.8);



    }

    //проверка наличия семейств
    public string CheckFamilies()
    {
      string alert = String.Empty;

      foreach (var p in paramsAndFamilies.Keys)
      {
        
        if (elems.Any(i => ((FamilyInstance)i).Symbol.LookupParameter(p).AsInteger() == 1))
        {
          string famName = paramsAndFamilies[p];

          var fs = Util.GetFamilySymbolByFamilyName(doc, famName);
          if (fs == null && !alert.Contains(famName))
          {
            alert += $"{famName};\n";
          }
        }
      }

      return alert;

    }






  }
}
