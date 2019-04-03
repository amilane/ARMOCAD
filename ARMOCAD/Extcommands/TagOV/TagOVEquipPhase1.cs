using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace ARMOCAD
{
  [Transaction(TransactionMode.Manual)]
  public class TagOVEquipPhase1 : IExternalCommand
  {

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
            var differentNumbers = g.GroupBy(i => i.LookupParameter("TAG").AsString().Split('-').Last().Substring(0, 4));
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
                numCheck.Add(e.LookupParameter("TAG").AsString().Split('-').Last().Substring(0, 4));
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

                  t5Val = handleTag.Split('-').Last().Substring(0, 4);
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
              for (int i = 1; i <= groupByFamilyAndSize.Count(); i++) {
                if (!usedNumbers.Contains(i)) {
                  availableInts.Add(i.ToString().PadLeft(4, '0'));
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
                    t5Val = handleTag.Split('-').Last().Substring(0, 4);
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

                    if (g3.Count() == 1) {
                      t6Val = "";
                    } else {
                      t6Val = availableLetters[k];
                    }

                    string tag = String.Format("{0}-{1}-{2}-{3}-{4}{5}", t1Val, t2Val, t3Val, t4Val, t5Val, t6Val);
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
