using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

// тагирование для 2 и последующих фаз заменой части тэга на код системы -0002В => -1102B

namespace ARMOCAD
{
  [Transaction(TransactionMode.Manual)]
  public class TagOVEquipReplace : IExternalCommand
  {
    public IEnumerable<Element> mepSystems;

    public Regex rgx = new Regex(@"^(\w+-){5}\S+");


    public string getSystemCode(Element e)
    {
      string sysCode;
      string currentSystem;
      string systems = e.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();

      if (!string.IsNullOrWhiteSpace(systems)) {
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

      try {
        using (Transaction t = new Transaction(doc, "Тагирование Ф2 заменой")) {

          t.Start();
          //Механические системы
          IEnumerable<Element> ductSystems = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_DuctSystem);
          IEnumerable<Element> pipeSystems = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_PipingSystem);
          mepSystems = ductSystems.Union(pipeSystems);


          // Воздухораспределители, арматура воздуховодов и труб, оборудование

          IEnumerable<Element> terminal = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_DuctTerminal);
          IEnumerable<Element> ductAccessory = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_DuctAccessory);
          IEnumerable<Element> pipeAccessory = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_PipeAccessory);
          IEnumerable<Element> equipment = Util.GetElementsOfCategory(doc, BuiltInCategory.OST_MechanicalEquipment);

          IEnumerable<Element> elems = terminal.Union(ductAccessory).Union(pipeAccessory).Union(equipment);

          foreach (var e in elems)
          {
            string tag = e.LookupParameter("TAG").AsString();
            if (!string.IsNullOrWhiteSpace(tag) && rgx.IsMatch(tag))
            {
              int idx = tag.LastIndexOf("-");
              string sysCode = getSystemCode(e);
              string newTag = tag.Remove(idx + 1, 2).Insert(idx + 1, sysCode);
              e.LookupParameter("TAG").Set(newTag);
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
