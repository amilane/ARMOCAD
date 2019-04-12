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
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp?.ActiveUIDocument;
      Document doc = uidoc?.Document;

      //Сбор точек доступа среди выбранных элементов
      ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
      List<Element> elems = new List<Element>();

      if (0 == selectedIds.Count) {
        TaskDialog.Show("Предупреждение", "Ничего не выбрано");
      } else {
        foreach (ElementId id in selectedIds) {
          var el = doc.GetElement(id);
          if (((FamilyInstance)el).Symbol.FamilyName == "СКУД_ТД") {
            elems.Add(el);
          }

        }

      }
      //Сбор семейств
      BuiltInCategory[] bics = new BuiltInCategory[]
      {
        BuiltInCategory.OST_CommunicationDevices,
        BuiltInCategory.OST_ElectricalEquipment,
        BuiltInCategory.OST_DataDevices,

      };

      IList<ElementFilter> a = new List<ElementFilter>(bics.Count());

      foreach (BuiltInCategory bic in bics) {
        a.Add(new ElementCategoryFilter(bic));
      }
      LogicalOrFilter categoryFilter = new LogicalOrFilter(a);

      var famTypes = new FilteredElementCollector(doc).WherePasses(categoryFilter).WhereElementIsElementType();


      try {
        using (Transaction t = new Transaction(doc, "CreateAccessPoints")) {
          t.Start();

          foreach (var td in elems) {
            Level level = (Level)doc.GetElement(td.LevelId);
            XYZ tdLocation = ((LocationPoint)td.Location).Point;
            XYZ tdFO = ((FamilyInstance)td).FacingOrientation;
            XYZ perpFO = new XYZ(-1 * tdFO.Y, tdFO.X, tdFO.Z);

            double tdW = td.LookupParameter("Ширина двери").AsDouble();
            double tdH = td.LookupParameter("Высота двери").AsDouble();
            double tdD = td.LookupParameter("Толщина стены").AsDouble();

            if (((FamilyInstance)td).Symbol.LookupParameter("Блок питания").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -100.0, 0.0, 2500.0, true, "EEQ_Оборудование_[Блок бесперебойного питания]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Вызывная панель видеодомофона").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, 150.0, 0.0, 1600.0, false, "СКУД_Оборудование_[Вызывная панель видеодомофона]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Дверной доводчик").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -200.0, 20.0, tdH * 304.8 - 100.0, true, "СКУД_Оборудование_[Дверной доводчик]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Замок электромагнитный").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 + 200.0, 0.0, tdH * 304.8 - 100.0, true, "СКУД_Замок_[Накладной электромагнитный]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Замок электромеханический").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 + 100.0, 0.0, 1000.0, true, "СКУД_Замок_[Накладной электромеханический]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Защелка электромеханическая").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8, -1 * tdD * 304.8 / 2, 1000.0, true, "СКУД_Замок_[Защелка электромеханическая]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Кнопка выхода").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 - 200.0, 0.0, 1100.0, true, "СКУД_Оборудование_[Кнопка выхода]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Модуль контроля доступа").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -400.0, 0.0, 2500.0, true, "СКУД_Контроллеры_[Модуль контроля доступа]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Магнитоконтактный извещатель").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 + 50.0, 0.0, tdH * 304.8, true, "СОТС_Датчики_[Магнитоконтактный]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Монитор видеодомофона").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 - 300.0, 0.0, 1600.0, true, "СКУД_Оборудование_[Монитор видеодомофона]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Прибор приемно-контрольный").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 - 300.0, 0.0, 1500.0, true, "EEQ_Контроллеры_[Прибор приемно-контрольный]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Считыватель вход").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 - 200.0, 0.0, 1100.0, false, "СКУД_Считыватель_[Для карт доступа]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Считыватель выход").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 - 200.0, 0.0, 1100.0, true, "СКУД_Считыватель_[Для карт доступа]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Турникет").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, 0.0, -1 * tdD * 304.8 / 2, 0.0, true, "СКУД_Оборудование_[Турникет]");
            }

            if (((FamilyInstance)td).Symbol.LookupParameter("Устройство разблокировки двери").AsInteger() == 1) {
              CreateEquip(doc, level, famTypes, tdLocation, tdFO, perpFO, tdW, tdH, tdD, -1 * tdW * 304.8 - 200.0, 0.0, 1500.0, true, "СКУД_Оборудование_[Устройство разблокировки двери]");
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
      Document doc,
      Level level,
      FilteredElementCollector famTypes,
      XYZ tdLocation,
      XYZ tdFO,
      XYZ perpFO,
      double tdW,
      double tdH,
      double tdD,
      double xOffset,
      double yOffset,
      double zOffset,
      bool inRoom,
      string erFamName)
    {
      if (inRoom) {
        tdFO = tdFO.Negate();
      }
      var _fs = famTypes.Where(i => ((FamilySymbol)i).FamilyName == erFamName);

      if (_fs.Count() == 0) {
        TaskDialog.Show("Предупреждение", "В проект не загружено семейство: \n" + erFamName);
      } else {
        var fs = _fs.First() as FamilySymbol;

        if (!fs.IsActive) {
          fs.Activate();
        }

        XYZ eqLocation = tdLocation.Add(tdFO.Multiply(tdD / 2 + yOffset / 304.8)).Add(perpFO.Multiply(tdW / 2 + xOffset / 304.8));
        var rotAxis = Line.CreateBound(eqLocation, new XYZ(eqLocation.X, eqLocation.Y, eqLocation.Z + 1.0));
        var angle = tdFO.Negate().AngleTo(new XYZ(0, 1, 0));

        var eq = doc.Create.NewFamilyInstance(eqLocation, fs, level, StructuralType.NonStructural);
        ElementTransformUtils.RotateElement(doc, eq.Id, rotAxis, angle);
        eq.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(zOffset / 304.8);
      }



    }

  }
}
