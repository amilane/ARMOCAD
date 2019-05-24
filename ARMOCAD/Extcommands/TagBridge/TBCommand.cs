using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class TBCommand : IExternalCommand
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
        ConnectButtonEventHandler connectButtonEventHandler = new ConnectButtonEventHandler();
        ExternalEvent connectExEvent = ExternalEvent.Create(connectButtonEventHandler);

        TBModel model = new TBModel(uiapp);
        connectButtonEventHandler.RevitModel = model;
        

        TBViewModel vmod = new TBViewModel();
        vmod.RevitModel = model;
        vmod.connectEvent = connectExEvent;

        TBView view = new TBView();
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

