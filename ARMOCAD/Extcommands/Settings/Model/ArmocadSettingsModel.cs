using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace ARMOCAD
{
  class ArmocadSettingsModel
  {
    private static UIApplication UIAPP = null;
    private static Application APP = null;
    private static UIDocument UIDOC = null;
    private static Document DOC = null;

    public List<string> FamilySymbolsNames { get; set; }
    public Configs Configs { get; set; }
    public string Path { get; set; }
    public SkudTdEquipment SkudTD { get; set; }

    public string Power { get; set; }
    public string VideoPanel { get; set; }
    public string DoorCloser { get; set; }
    public string LockMagnet { get; set; }
    public string LockMechanical { get; set; }
    public string LatchMechanical { get; set; }
    public string ExitButton { get; set; }
    public string InputControl { get; set; }
    public string Siren { get; set; }
    public string Monitor { get; set; }
    public string ReaderIn { get; set; }
    public string ReaderOut { get; set; }
    public string Turnstile { get; set; }
    public string DoorUnlocking { get; set; }


    public ArmocadSettingsModel(UIApplication uiapp, string path)
    {
      UIAPP = uiapp;
      APP = UIAPP.Application;
      UIDOC = UIAPP.ActiveUIDocument;
      DOC = UIDOC.Document;

      FamilySymbolsNames = collectFamilyNames();

      // Десериализация класса Config
      Path = path;
      Configs = JsonConvert.DeserializeObject<Configs>(File.ReadAllText(Path));
      SkudTD = Configs.SkudTd;

      Power = SkudTD.Power;
      VideoPanel = SkudTD.VideoPanel;
      DoorCloser = SkudTD.DoorCloser;
      LockMagnet = SkudTD.LockMagnet;
      LockMechanical = SkudTD.LockMechanical;
      LatchMechanical = SkudTD.LatchMechanical;
      ExitButton = SkudTD.ExitButton;
      InputControl = SkudTD.InputControl;
      Siren = SkudTD.Siren;
      Monitor = SkudTD.Monitor;
      ReaderIn = SkudTD.ReaderIn;
      ReaderOut = SkudTD.ReaderOut;
      Turnstile = SkudTD.Turnstile;
      DoorUnlocking = SkudTD.DoorUnlocking;


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
