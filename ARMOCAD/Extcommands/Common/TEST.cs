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

      ElementId selectedId = uidoc.Selection.GetElementIds().First();

      var elem = doc.GetElement(selectedId);
      

     

      

      
      try
      {
        using (Transaction t = new Transaction(doc, "TEST"))
        {
          t.Start();

          SchemaMethods sm = new SchemaMethods("2b6a75d7-a580-4786-9d6c-6739437c2170", "TestSchema","Схема всякой фигни");
          sm.setValueToEntity(elem, "Dict_String", 0, "lal kek cheburek");
          sm.setValueToEntity(elem, "Dict_Double", 0, 15.2);
          sm.setValueToEntity(elem, "Dict_XYZ", 0, new XYZ(10.0, 0.0, 15.3));

          XYZ ret = (XYZ)sm.getSchemaDictValue<XYZ>(elem, "Dict_XYZ", 0);
          Util.InfoMsg2("Результат:", ret.ToString());

          t.Commit();
        }



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
