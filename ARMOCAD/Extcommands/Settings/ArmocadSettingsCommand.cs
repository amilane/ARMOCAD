using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

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



        ArmocadSettingsModel model = new ArmocadSettingsModel(uiapp);
       
        ArmocadSettingsViewModel vmod = new ArmocadSettingsViewModel();
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

