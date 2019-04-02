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
  public class TagOVClass : IExternalCommand
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
      MEPModel mepModel = fi.MEPModel as MechanicalFitting;
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
      MEPModel mepModel = fi.MEPModel as MechanicalFitting;
      if ((fi.MEPModel as MechanicalFitting).PartType == PartType.SpudAdjustable ||
          (fi.MEPModel as MechanicalFitting).PartType == PartType.TapAdjustable) {
        length = i.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_TAKEOFF_LENGTH).AsDouble();
      } else {
        length = 0.0;
      }
      return length;
    }

    private string size = "";
    // Размер из текста
    public string GetCalcSize(Element i)
    {
      try {
        if (i.Category.Id.IntegerValue != (int)BuiltInCategory.OST_MechanicalEquipment) {
          size = i.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
        } else {
          size = "";
        }
      }
      catch {
        size = "";
      }
      return size;
    }

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

    // Очистка старых значений PL_Pos и TAG
    public void CleanTAG(IEnumerable<Element> elems)
    {
      foreach (Element i in elems) {
        Parameter parTag = i.LookupParameter("TAG");
        Parameter parPos = i.LookupParameter("PL_Pos");
        parTag.Set("");
        parPos.Set("");
      }
    }

    public string getSystemCode(Element e, IEnumerable<Element> mepSystems)
    {
      string sysCode;
      string currentSystem;
      string systems = e.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
      if (systems != null && systems != "") {
        if (systems.Contains(",")) {
          currentSystem = systems.Split(',').First();
        } else {
          currentSystem = systems;
        }

        Element system;
        var filterSystems = mepSystems.Where(i => i.Name == currentSystem | i.Name.StartsWith(currentSystem));
        if (filterSystems.Count() > 0) {
          system = filterSystems.First();
          string parSystemCode = system.LookupParameter("Система - Номер для TAG").AsString();
          if (parSystemCode != null && parSystemCode != "") {
            sysCode = parSystemCode;
          } else {
            sysCode = "??";
          }
        } else {
          sysCode = "??";
        }

      } else {
        sysCode = "??";
      }

      return sysCode;
    }
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;
      ProjectInfo projInfo = doc.ProjectInformation;
      string t1Val = projInfo.LookupParameter("TagCode1").AsString();
      string t2Val = projInfo.LookupParameter("TagCode2").AsString();

      try {
        using (Transaction t = new Transaction(doc, "Set TAGs")) {

          t.Start();
          //Механические системы
          IEnumerable<Element> ductSystems = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctSystem)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> pipeSystems = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipingSystem)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> mepSystems = ductSystems.Union(pipeSystems);

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


          // Воздухораспределители, арматура воздуховодов и труб, оборудование
          // Не перетагирует элементы, у которых стоит галка "Ручной TAG" (даже если TAG пустой)
          IEnumerable<Element> terminal = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctTerminal)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> ductAccessory = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctAccessory)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> pipeAccessory = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeAccessory)
            .WhereElementIsNotElementType()
            .ToElements();
          IEnumerable<Element> equipment = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .WhereElementIsNotElementType()
            .ToElements();

          IEnumerable<Element> elements2 = terminal.Union(ductAccessory).Union(pipeAccessory).Union(equipment);

          // Проверка на дублирующиеся РУЧНЫЕ таги
          string AlertEqualTagMsg = "";
          string AlertDifferentTagMsg = "";
          string AlertEqualTagInDifferentTypes = "";
          string msg;

          var handleElements = elements2.Where(i =>
            i.LookupParameter("Ручной TAG").AsInteger() == 1 &&
            i.LookupParameter("TAG")?.AsString() != null &&
            i.LookupParameter("TAG").AsString() != "");

          var EqualHandleTagElements = handleElements.GroupBy(i => i.LookupParameter("TAG").AsString());
          foreach (var g in EqualHandleTagElements) {
            if (g.Count() > 1) {
              msg = g.Key + '\n';
              foreach (var i in g) {
                msg += i.Id.ToString() + '\n';
              }
              AlertEqualTagMsg += msg;
            }
          }

          //Проверка, вдруг пользователь в одном типоразмере семейства заблокировал ТАГи с разными номерами "0001, 0003"
          //Шаблон для перетагируемых элементов 1-20-CA0000-V-FC-0001A
          Regex rgxTag = new Regex(@"^[0-9]{1}-[0-9.]{1,}-[A-Z]{2}[0-9]{4}-[A-Z]{1}-[A-Z]{1,}-[0-9]{4}[A-Z]?$");

          var handleElemsBySize = handleElements.Where(i =>
            i.LookupParameter("Ручной TAG")?.AsInteger() == 1 &
            rgxTag.IsMatch(i.LookupParameter("TAG")?.AsString() ?? "")).GroupBy(i => new {
              Key1 = i.LookupParameter("TagCode3").AsString(),
              Key2 = i.LookupParameter("TagCode4").AsString(),
              Key3 = i.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
              Key4 = GetCalcSize(i)
            });
          foreach (var g in handleElemsBySize) {
            var differentNumbers = g.GroupBy(i => i.LookupParameter("TAG").AsString().Split('-').Last().Substring(2, 2));
            if (differentNumbers.Count() > 1) {
              msg = "";
              foreach (var gr in differentNumbers) {
                var eFirst = gr.First();
                msg += String.Format("{0} - {1}\n", eFirst.Id.ToString(), eFirst.LookupParameter("TAG").AsString());
              }
              msg += "----------------------------\n";
              AlertDifferentTagMsg += msg;
            }
          }

          //Проверка на заблокированные ОДИНАКОВЫЕ номера 0001.0002.0003 среди разных типоразмеров
          var handleElemsByT3T4 = handleElements.Where(i =>
            i.LookupParameter("Ручной TAG")?.AsInteger() == 1 &
            rgxTag.IsMatch(i.LookupParameter("TAG")?.AsString() ?? "")).GroupBy(i => new {
              Key1 = i.LookupParameter("TagCode3").AsString(),
              Key2 = i.LookupParameter("TagCode4").AsString(),
              Key3 = i.LookupParameter("TAG").AsString().Split('-').Last().Substring(0, 2)
            });

          foreach (var g1 in handleElemsByT3T4) {
            var groupBySize = g1.GroupBy(i => new {
              Key1 = i.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
              Key2 = GetCalcSize(i)
            });

            List<string> numCheckAll = new List<string>();
            foreach (var g2 in groupBySize) {
              List<string> numCheck = new List<string>();
              foreach (var e in g2) {
                numCheck.Add(e.LookupParameter("TAG").AsString().Split('-').Last().Substring(2, 2));
              }

              var numCheckUniq = numCheck.Distinct();
              foreach (var i in numCheckUniq) {
                numCheckAll.Add(i);
              }
            }

            string code = String.Format("{0}-{1}-", g1.Key.Key1, g1.Key.Key2);
            var grNumCheckAll = numCheckAll.GroupBy(i => i);
            foreach (var g in grNumCheckAll) {
              msg = "";
              if (g.Count() > 1) {
                msg += code + g.Key + "\n";
              }

              AlertEqualTagInDifferentTypes += msg;
            }

          }






          //Сообщение, если есть дублирующиеся РУЧНЫЕ таги или разные номера в тагах идентичных элементов, прога ничего не делает дальше
          if (AlertEqualTagMsg != "" | AlertDifferentTagMsg != "" | AlertEqualTagInDifferentTypes != "") {
            if (AlertEqualTagMsg != "") { TaskDialog.Show("Error 01", "Дублирующиеся ручные TAG:\n\n" + AlertEqualTagMsg); }
            if (AlertDifferentTagMsg != "") { TaskDialog.Show("Error 02", "Разный порядковый номер в ручных TAG идентичных элементов:\n\n" + AlertDifferentTagMsg); }
            if (AlertEqualTagInDifferentTypes != "") { TaskDialog.Show("Error 03", "Одинаковый порядковый номер в ручных TAG разных типоразмеров:\n\n" + AlertEqualTagInDifferentTypes); }

          } else {
            //если дубликатов среди ручных тагов нет:
            //группировка по Tag3, Tag4
            var groupByTag3Tag4 = elements2.GroupBy(i => new {
              Key1 = i.LookupParameter("TagCode3").AsString(),
              Key2 = i.LookupParameter("TagCode4").AsString()
            });



            foreach (var g2 in groupByTag3Tag4) {
              //int n = 0;
              string t5Val = "";
              string t6Val;
              string handleTag;
              List<int> usedNumbers = new List<int>();

              //Сбор элементов с ручными тагами среди группы Tag3 Tag4, чтобы выявить все "использованные" номера 0001. 0002. 0003


              var handleTagT3T4 = g2.Where(i =>
                i.LookupParameter("Ручной TAG")?.AsInteger() == 1 &&
                rgxTag.IsMatch(i.LookupParameter("TAG")?.AsString() ?? ""));


              if (handleTagT3T4.Count() > 0) {
                foreach (var i in handleTagT3T4) {
                  handleTag = i.LookupParameter("TAG").AsString();

                  t5Val = handleTag.Split('-').Last().Substring(2, 2);
                  int t5ValInt = Convert.ToInt32(t5Val);
                  //Собирает использованные параметры (0001) в заблокированных элементах
                  if (!usedNumbers.Contains(t5ValInt)) {
                    usedNumbers.Add(t5ValInt);
                  }
                }
              }

              //группировка по Семейству и типоразмеру
              var groupByFamilyAndSize = g2.GroupBy(i => new {
                Key1 = i.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                Key2 = GetCalcSize(i),
              });
              var sortByFamilyAndSize = groupByFamilyAndSize.OrderBy(i => i.Key.Key1).ThenBy(i => i.Key.Key2);

              List<string> availableInts = new List<string>();
              string ii;
              for (int i = 1; i <= groupByFamilyAndSize.Count(); i++) {
                if (!usedNumbers.Contains(i)) {
                  if (i < 10) {
                    ii = i.ToString().PadLeft(2, '0');
                  } else {
                    ii = i.ToString();
                  }

                  availableInts.Add(ii);
                }
              }

              int n = 0;
              //Генерация доступных чисел для нумерации 0001 0002 0003
              foreach (var g3 in sortByFamilyAndSize) {

                List<string> usedLetters = new List<string>();
                //обработка элементов для ручного тагирования (не собираются элементы с пустым TAG и несоответствующим шаблону 1-20-CA0000-V-FC-0001A)
                var handleTagElements = g3.Where(i =>
                  i.LookupParameter("Ручной TAG")?.AsInteger() == 1 &&
                  rgxTag.IsMatch(i.LookupParameter("TAG")?.AsString() ?? ""));

                if (handleTagElements.Count() > 0) {
                  foreach (var i in handleTagElements) {
                    handleTag = i.LookupParameter("TAG").AsString();
                    t5Val = handleTag.Split('-').Last().Substring(2, 2);
                    t6Val = handleTag.Split('-').Last().Remove(0, 4);
                    usedLetters.Add(t6Val);
                  }
                } else {
                  t5Val = availableInts[n];
                  n++;
                }
                //Собирает доступные буквы для списка элементов (не включая использованные)
                List<string> availableLetters = new List<string>();
                CharacterIncrement letter = new CharacterIncrement();
                for (int i = 0; i < g3.Count(); i++) {
                  t6Val = letter.СharacterIncrement(i);
                  if (!usedLetters.Contains(t6Val)) {
                    availableLetters.Add(t6Val);
                  }
                }

                //Собирает ТЭГ и назначает элементам
                int k = 0;
                foreach (var i in g3) {
                  string t3Val = g2.Key.Key1;
                  string t4Val = g2.Key.Key2;

                  //Не трогает элементы с галкой "Ручной TAG"
                  if (i.LookupParameter("Ручной TAG").AsInteger() != 1) {

                    string t7Val = getSystemCode(i, mepSystems);

                    if (g3.Count() == 1) {
                      t6Val = "";
                    } else {
                      t6Val = availableLetters[k];
                    }

                    string tag = String.Format("{0}-{1}-{2}-{3}-{4}{5}{6}", t1Val, t2Val, t3Val, t4Val, t7Val, t5Val, t6Val);
                    Parameter parTag = i.LookupParameter("TAG");
                    parTag.Set(tag);
                    k++;
                  }

                }

              }

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
