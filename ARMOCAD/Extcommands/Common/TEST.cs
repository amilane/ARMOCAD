using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class TEST : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp?.ActiveUIDocument;
      Document doc = uidoc?.Document;
      Document docAR = null;

      ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

      if (0 == selectedIds.Count)
      {
        TaskDialog.Show("Предупреждение", "Выделите модель в АР");
        return Result.Cancelled;
      }
      else
      {
        foreach (ElementId id in selectedIds)
        {
          var link = doc.GetElement(id);
          if (link.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RvtLinks)
          {
            docAR = ((RevitLinkInstance)link).GetLinkDocument();
          }
        }
      }

      Reference refElemLinked = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Выберите стену в связанной модели АР");
      Element wall = docAR.GetElement(refElemLinked.LinkedElementId);
      Reference mep = uidoc.Selection.PickObject(ObjectType.Element, "Выберите коммуникацию");

      

      //var kek = refElemLinked.
      try
      {
        using (Transaction t = new Transaction(doc, "TEST"))
        {
          t.Start();
          //Reference refElemLinked = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Выберите стену в связанной модели АР");
          //Element elem = docAR.GetElement(refElemLinked.LinkedElementId);


          t.Commit();
        }



        TaskDialog.Show("Готово", "ОК");
        return Result.Succeeded;
      }
      // This is where we "catch" potential errors and define how to deal with them
      catch (Autodesk.Revit.Exceptions.OperationCanceledException)
      {
        // If user decided to cancel the operation return Result.Canceled
        return Result.Cancelled;
      }
      catch (Exception ex)
      {
        // If something went wrong return Result.Failed
        message = ex.Message;
        return Result.Failed;
      }

    }

    



  }
}
