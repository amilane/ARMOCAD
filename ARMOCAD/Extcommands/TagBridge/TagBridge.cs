using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class TagBridge : IExternalCommand
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

        /*
        using (TagBridgeWPF window = new TagBridgeWPF())
        {
          System.Windows.Interop.WindowInteropHelper helper =
            new System.Windows.Interop.WindowInteropHelper(window);
          helper.Owner = proc.MainWindowHandle;

          window.DOC = doc;
          window.UIDOC = uidoc;

          window.ShowDialog();
        }
        */

        



        TagBridgeWPF window = new TagBridgeWPF(doc);
        System.Windows.Interop.WindowInteropHelper helper =
            new System.Windows.Interop.WindowInteropHelper(window);
        helper.Owner = proc.MainWindowHandle;

        window.doc = doc;
        window.uidoc = uidoc;
        window.Show();




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

