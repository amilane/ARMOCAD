using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class SKUDControlPlacementEx : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;

      var docAR = ((RevitLinkInstance)new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks)
        .WhereElementIsNotElementType().Where(i =>
          i.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString() ==
          "Связанная модель Revit: 4115_ARM_MDL_1-200_AR.rvt").First()).GetLinkDocument();



      try {
        using (Transaction t = new Transaction(doc, "CreateAccessPoints")) {
          t.Start();

          SKUDPlaceAccessPoints.SkudPlaceAccessPoints(docAR, doc);



          t.Commit();
        }



        TaskDialog.Show("Готово", "ОК");
        return Result.Succeeded;
      }
      // This is where we "catch" potential errors and define how to deal with them
      catch (Autodesk.Revit.Exceptions.OperationCanceledException) {
        // If user decided to cancel the operation return Result.Canceled
        return Result.Cancelled;
      }
      catch (Exception ex) {
        // If something went wrong return Result.Failed
        message = ex.Message;
        return Result.Failed;
      }

    }





  }
}
