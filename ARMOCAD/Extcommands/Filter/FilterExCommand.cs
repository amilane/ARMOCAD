using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace ARMOCAD
{
  [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
  [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
  public class FilterExCommand : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      try
      {
        UIApplication uiapp = commandData.Application;
        UIDocument uidoc = uiapp.ActiveUIDocument;
        Application app = uiapp.Application;
        Document doc = uidoc.Document;

        System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();

        using (FilterView window = new FilterView(doc))
        {
          System.Windows.Interop.WindowInteropHelper helper =
            new System.Windows.Interop.WindowInteropHelper(window);
          helper.Owner = proc.MainWindowHandle;

          window.DOC = doc;
          window.UIDOC = uidoc;

          window.ShowDialog();

        }
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
