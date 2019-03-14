using System;
using System.IO;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace SpreadEvenly
{
	[Transaction(TransactionMode.Manual)]
	public class SpreadEvenlyClass : IExternalCommand
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

				ScriptEngine engine = Python.CreateEngine();
				ScriptScope scope = engine.CreateScope();
				scope.SetVariable("doc", doc);
				scope.SetVariable("uidoc", ui_doc);
				//engine.ExecuteFile("C:/ProgramData/Autodesk/Revit/Addins/2018/SuElProgs/SimilarParams.py", scope);

				string DetailLinesLength = Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + "SpreadEvenly.py";
				Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(DetailLinesLength);
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