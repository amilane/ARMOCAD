using System;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Newtonsoft.Json;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class ArmocadSettingsCommand : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      try
      {

        //Проверка наличия файла ArmocadConfig.json, создание файла
        var myDocumentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        var armocadSettingsPath = $"{myDocumentDirectory}\\ArmocadSettings\\ArmocadConfig.json";

        if (!File.Exists(armocadSettingsPath))
        {
          File.WriteAllText(armocadSettingsPath, JsonConvert.SerializeObject(new Configs()));
        }

        var Configs = JsonConvert.DeserializeObject<Configs>(File.ReadAllText(armocadSettingsPath));

        ArmocadSettingsModel model = new ArmocadSettingsModel(uiapp);

        ArmocadSettingsViewModel vmod = new ArmocadSettingsViewModel();
        ArmocadSettingsViewModel.Path = armocadSettingsPath;
        ArmocadSettingsViewModel.Configs = Configs;
        vmod.RevitModel = model;

        ArmocadSettings view = new ArmocadSettings();
        view.DataContext = vmod;
        view.Show();


        return Result.Succeeded;
      }
      catch (Exception ex)
      {
        message = ex.Message;
        return Result.Failed;
      }
    }


  }


}

