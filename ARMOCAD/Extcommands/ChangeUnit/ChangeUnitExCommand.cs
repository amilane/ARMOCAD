using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ARMOCAD
{
  [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
  [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
  public class ChangeUnitExCommand : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      try
      {
        UIApplication uiapp = commandData.Application;
        UIDocument uidoc = uiapp.ActiveUIDocument;
        Document doc = uidoc.Document;

        // Get Data from Excel from sheet 
        ReadDataFromExcel dataFromExcel = new ReadDataFromExcel();
        var dataListFromExcel = dataFromExcel.readDataTable(@"\\arena\ARMO-GROUP\ОБЪЕКТЫ\В_РАБОТЕ\41XX_AGPZ\60-BIM\040-Database\030-EXCEL\0055-CPC-4.0.0.00.000.xlsx", "UnitList");

        //Create DataDict from excel
        Dictionary<string, List<string>> dataDict = new Dictionary<string, List<string>>();
        foreach (List<string> row in dataListFromExcel)
        {
          string k = row[0];
          List<string> v = row;
          v.RemoveAt(0);

          dataDict.Add(k, v);
        }


        System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();

        
        using (ChangeUnitWPF window = new ChangeUnitWPF())
        {
          System.Windows.Interop.WindowInteropHelper helper =
            new System.Windows.Interop.WindowInteropHelper(window);
          helper.Owner = proc.MainWindowHandle;
          
          
          window.DOC = doc;
          window.BuildingNameBox.ItemsSource = dataDict;
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
