using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  class ArmocadSettingsModel
  {
    private static UIApplication UIAPP = null;
    private static Application APP = null;
    private static UIDocument UIDOC = null;
    private static Document DOC = null;

    public List<string> FamilySymbolsNames { get; set; }


    public ArmocadSettingsModel(UIApplication uiapp)
    {
      UIAPP = uiapp;
      APP = UIAPP.Application;
      UIDOC = UIAPP.ActiveUIDocument;
      DOC = UIDOC.Document;

      FamilySymbolsNames = collectFamilyNames();

    }

    public List<string> collectFamilyNames()
    {
      var electricEquip = Util.GetFamilySymbolsOfCategory(DOC, BuiltInCategory.OST_ElectricalEquipment);
      var communicationDev = Util.GetFamilySymbolsOfCategory(DOC, BuiltInCategory.OST_CommunicationDevices);
      var dataDev = Util.GetFamilySymbolsOfCategory(DOC, BuiltInCategory.OST_DataDevices);

      var elems = electricEquip.Union(communicationDev).Union(dataDev);

      List<string> familySymbolsNames = new List<string>();
      foreach (var e in elems)
      {
        familySymbolsNames.Add($"{e.FamilyName}: {e.Name}");
      }

      var sortedNames = familySymbolsNames.OrderBy(i => i).ToList();

      return sortedNames;
    }


  }
}
