using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
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
        TBViewModel vmod = new TBViewModel();
        vmod.RevitModel = new TBModel(uiapp);

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

