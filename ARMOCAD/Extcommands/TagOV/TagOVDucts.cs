using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;

namespace ARMOCAD
{
  [Transaction(TransactionMode.Manual)]
  public class TagOVDucts : IExternalCommand
  {

    // Тип детали соединительных деталей воздуховода
    public PartType GetPartType(Element i)
    {
      FamilyInstance fi = i as FamilyInstance;
      return (fi.MEPModel as MechanicalFitting).PartType;
    }


    double angle = 0.0;
    // Угол отводов
    public double GetAngle(Element i)
    {
      FamilyInstance fi = i as FamilyInstance;
      MEPModel mepModel = fi.MEPModel as MechanicalFitting;
      if ((fi.MEPModel as MechanicalFitting).PartType == PartType.Elbow) {
        ConnectorSet conSet = mepModel.ConnectorManager.Connectors;
        List<Connector> connectors = new List<Connector>();
        foreach (Connector c in conSet) {
          connectors.Add(c);
        }
        angle = Math.Round(connectors[0].Angle, 2);
      } else {
        angle = 0.0;
      }
      return angle;
    }

    // Длина заглушки
    double length = 0.0;
    public double GetCapLength(Element i)
    {
      FamilyInstance fi = i as FamilyInstance;
      if ((fi.MEPModel as MechanicalFitting).PartType == PartType.Cap) {

        string[] parameters = { "Length", "Длина воздуховода", "L" };

        foreach (string p in parameters) {
          Parameter parLength = i.LookupParameter(p);
          if (parLength != null) {
            length = parLength.AsDouble();
          } else {
            length = 0.0;
          }
        }

      } else {
        length = 0.0;
      }
      return length;
    }

    //Длина перехода
    public double GetTakeoffLength(Element i)
    {
      FamilyInstance fi = i as FamilyInstance;
      if ((fi.MEPModel as MechanicalFitting).PartType == PartType.SpudAdjustable ||
          (fi.MEPModel as MechanicalFitting).PartType == PartType.TapAdjustable) {
        length = i.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_TAKEOFF_LENGTH).AsDouble();
      } else {
        length = 0.0;
      }
      return length;
    }

    private string size = "";


    //Диаметр, Ширина, Высота соединительных деталей
    private int d;
    private int h;
    private int w;
    public int[] GetSizeFitting(Element i)
    {
      string size = i.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
      string[] sizeSplit1 = size.Split('-');
      if (sizeSplit1[0].Contains("ø")) {
        d = Int32.Parse(Regex.Match(sizeSplit1[0], @"\d+").Value);
        w = 0;
        h = 0;
      } else {
        d = 0;
        string[] sizeSplit2 = sizeSplit1[0].Split('x');
        w = Int32.Parse(Regex.Match(sizeSplit2[0], @"\d+").Value);
        h = Int32.Parse(Regex.Match(sizeSplit2[1], @"\d+").Value);
      }

      int[] res = { d, w, h };
      return res;
    }

    //Диаметр, Ширина и высота воздуховодов
    private double dDuct;
    private double wDuct;
    private double hDuct;
    private Parameter parDDuct, parWDuct, parHDuct;
    public double[] GetSizeDuct(Element i)
    {

      parDDuct = i.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
      if (parDDuct != null) {
        dDuct = Math.Round(parDDuct.AsDouble(), 2);
      } else {
        dDuct = 0.0;
      }

      parWDuct = i.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
      if (parWDuct != null) {
        wDuct = Math.Round(parWDuct.AsDouble(), 2);
      } else {
        wDuct = 0.0;
      }

      parHDuct = i.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
      if (parHDuct != null) {
        hDuct = Math.Round(parHDuct.AsDouble(), 2);
      } else {
        hDuct = 0.0;
      }


      double[] res = { dDuct, wDuct, hDuct };
      return res;
    }


    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;
      ProjectInfo projInfo = doc.ProjectInformation;
      string t1Val = projInfo.LookupParameter("TagCode1").AsString();

      try {
        using (Transaction t = new Transaction(doc, "Set TAGs")) {

          t.Start();

          // Воздуховоды и соединительные детали воздуховодов
          IEnumerable<Element> ducts = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctCurves)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> filterDucts = ducts.Where(
            i => i.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * 304.8 > 10.0);

          IEnumerable<Element> ductFittings = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_DuctFitting)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> ductsAndFittings = filterDucts.Union(ductFittings);

          // Группировка по системам
          var ductsFittingsBySystem =
            ductsAndFittings.GroupBy(i => i.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString());


          foreach (var g1 in ductsFittingsBySystem) {
            // Воздуховоды
            var sysDucts = g1.Where(i => i.Category.Id.IntegerValue ==
                                         (int)BuiltInCategory.OST_DuctCurves);
            var groupDucts = sysDucts.GroupBy(i => new {
              Key1 = GetSizeDuct(i)[0],
              Key2 = GetSizeDuct(i)[1],
              Key3 = GetSizeDuct(i)[2],
              Key4 = Math.Round(i.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble(), 2),
              Key5 = i.LookupParameter("AG_Thickness").AsValueString()
            });

            //Обработка воздуховодов, которые совпадают с заблокированными воздуховодами
            List<int> positions = new List<int>(); // Список использованных номеров позиций среди заблокированных элементов

            var blockedDucts = groupDucts.Where(g => g.Any(i => i.LookupParameter("Ручной TAG").AsInteger() == 1.0));

            foreach (var g in blockedDucts) {
              Element blockedElem = g.Where(i => i.LookupParameter("Ручной TAG").AsInteger() == 1.0 &&
                                                 i.LookupParameter("TAG").AsString() != "" &&
                                                 i.LookupParameter("PL_Pos").AsString() != "").First();

              string blTag = blockedElem.LookupParameter("TAG").AsString();
              string blPos = blockedElem.LookupParameter("PL_Pos").AsString();
              positions.Add(Convert.ToInt32(blPos));

              foreach (var e in g) {
                Parameter parTag = e.LookupParameter("TAG");
                Parameter parPos = e.LookupParameter("PL_Pos");
                parTag.Set(blTag);
                parPos.Set(blPos);
              }
            }

            // Соединительные детали
            var sysFittings =
              g1.Where(i => i.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting);
            var groupFittings = sysFittings.GroupBy(i => new {
              Key1 = GetPartType(i),
              Key2 = GetAngle(i),
              Key3 = GetSizeFitting(i)[0],
              Key4 = GetSizeFitting(i)[1],
              Key5 = GetSizeFitting(i)[2],
              Key6 = GetCapLength(i),
              Key7 = i.LookupParameter("AG_Thickness").AsDouble(),
              Key8 = GetTakeoffLength(i)
            });

            //Обработка соед деталей, которые совпадают с заблокированными соед деталями

            var blockedFits = groupFittings.Where(g => g.Any(i => i.LookupParameter("Ручной TAG").AsInteger() == 1.0));

            foreach (var g in blockedFits) {
              Element blockedElem = g.Where(i => i.LookupParameter("Ручной TAG").AsInteger() == 1.0 &&
                                                 i.LookupParameter("TAG").AsString() != "" &&
                                                 i.LookupParameter("PL_Pos").AsString() != "").First();

              string blTag = blockedElem.LookupParameter("TAG").AsString();
              string blPos = blockedElem.LookupParameter("PL_Pos").AsString();
              positions.Add(Convert.ToInt32(blPos));

              foreach (var e in g) {

                Parameter parTag = e.LookupParameter("TAG");
                Parameter parPos = e.LookupParameter("PL_Pos");
                parTag.Set(blTag);
                parPos.Set(blPos);

              }
            }

            // Собираем пропущенные позиции среди заблокированных элементов
            int maxUsedPos;
            if (positions.Count > 0) {
              maxUsedPos = positions.Max();
            } else {
              maxUsedPos = 0;
            }

            List<int> freePositions = new List<int>();

            for (int i = 1; i < maxUsedPos; i++) {
              if (!positions.Contains(i)) {
                freePositions.Add(i);
              }
            }
            // Незаблокированные воздуховоды
            var unblockedDucts = groupDucts.
              Where(g => g.All(i => i.LookupParameter("Ручной TAG").AsInteger() == 0)).
              OrderBy(i => i.Key.Key1).
              ThenBy(i => i.Key.Key2).
              ThenBy(i => i.Key.Key3).
              ThenBy(i => i.Key.Key4);


            int fpCount = freePositions.Count;
            // Дополняем список свобод
            if (unblockedDucts.Count() > fpCount) {
              for (int i = 0; i < unblockedDucts.Count() - fpCount; i++) {
                freePositions.Add(++maxUsedPos);
              }
            }


            foreach (var g in unblockedDucts) {
              string sys = g1.Key;
              string t5Val = freePositions[0].ToString().PadLeft(3, '0');
              string tag = string.Format("{0}/{1}/{2}", t1Val, sys, t5Val);

              foreach (var e in g) {
                Parameter parTag = e.LookupParameter("TAG");
                Parameter parPos = e.LookupParameter("PL_Pos");
                parTag.Set(tag);
                parPos.Set(t5Val);
              }

              freePositions.RemoveAt(0);

            }

            var unblockedFits = groupFittings.
              Where(g => g.Any(i => i.LookupParameter("Ручной TAG").AsInteger() == 0)).
              OrderBy(i => i.Key.Key1).
              ThenBy(i => i.Key.Key2).
              ThenBy(i => i.Key.Key3).
              ThenBy(i => i.Key.Key4).
              ThenBy(i => i.Key.Key5);

            fpCount = freePositions.Count;
            if (unblockedFits.Count() > fpCount) {
              for (int i = 0; i < unblockedFits.Count() - fpCount; i++) {
                freePositions.Add(++maxUsedPos);
              }
            }


            foreach (var g in unblockedFits) {
              string sys = g1.Key;
              string t5Val = freePositions[0].ToString().PadLeft(3, '0');
              string tag = string.Format("{0}/{1}/{2}", t1Val, sys, t5Val);

              foreach (var e in g) {
                Parameter parTag = e.LookupParameter("TAG");
                Parameter parPos = e.LookupParameter("PL_Pos");
                parTag.Set(tag);
                parPos.Set(t5Val);
              }
              freePositions.RemoveAt(0);
            }

          }

          t.Commit();
          TaskDialog.Show("Всё хорошо", "ОК");
          return Result.Succeeded;
        }

      }
      // This is where we "catch" potential errors and define how to deal with them
      catch (Exception ex) {
        // If something went wrong return Result.Failed
        message = ex.Message;
        return Result.Failed;
      }

    }

  }
}
