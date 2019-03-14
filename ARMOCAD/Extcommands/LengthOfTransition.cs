using System;
using System.IO;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;

namespace LengthOfTransition
{
  [Transaction(TransactionMode.Manual)]
  public class LengthOfTransitionClass : IExternalCommand
  {

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;

      try
      {
        var opts = new Dictionary<string, object>();
        if (System.Diagnostics.Debugger.IsAttached)
          opts["Debug"] = true;

        ScriptEngine engine = Python.CreateEngine(opts);
        ScriptScope scope = engine.CreateScope();
        scope.SetVariable("doc", doc);
        scope.SetVariable("uidoc", ui_doc);
        //engine.ExecuteFile(@"D:\Drive\ARMOPlug\ARMOCAD\ARMOCAD\Resources\LengthOfTransition.py", scope);

        string DetailLinesLength = Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + "LengthOfTransition.py";
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(DetailLinesLength);
        if (stream != null)
        {
          string script = new StreamReader(stream).ReadToEnd();
          engine.Execute(script, scope);
        }

        TaskDialog.Show("Всё хорошо", "ОК");

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