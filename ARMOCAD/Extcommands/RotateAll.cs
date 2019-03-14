using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace RotateAll
{
  [Transaction(TransactionMode.Manual)]
  public class RotateAllClass : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;
      try
      {
        ScriptEngine engine = Python.CreateEngine();
        ScriptScope scope = engine.CreateScope();
        scope.SetVariable("doc", doc);
        scope.SetVariable("uidoc", ui_doc);
        //engine.ExecuteFile("D:/RotateAll.py", scope);
        
        string scriptName = Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + "RotateAll.py";
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptName);
        if (stream != null)
        {
          string script = new StreamReader(stream).ReadToEnd();
          engine.Execute(script, scope);
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