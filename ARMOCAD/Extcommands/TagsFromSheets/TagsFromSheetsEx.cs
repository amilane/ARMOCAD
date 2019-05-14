using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{

  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  public class TagsFromSheetsEx : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp?.ActiveUIDocument;
      Document doc = uidoc?.Document;


      SortedDictionary<string, List<string>[]> outDict = new SortedDictionary<string, List<string>[]>();

      var sheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).ToElements();
      if (sheets.Count > 0)
      {
        foreach (ViewSheet s in sheets)
        {
          var originalSheetNumber = s.SheetNumber;

          var sheetNumber = string.Empty;
          var revision = string.Empty;

          if (originalSheetNumber.Contains("_"))
          {
            sheetNumber = originalSheetNumber.Split('_')[0];
            revision = originalSheetNumber.Split('_')[1];
          }


          var viewsIds = s.GetAllPlacedViews();
          if (viewsIds.Count > 0)
          {
            foreach (var id in viewsIds)
            {
              Element view = doc.GetElement(id);
              if (view.GetType() == typeof(ViewPlan) |
                  view.GetType() == typeof(View3D) |
                  view.GetType() == typeof(ViewSection))
              {
                var tags = new FilteredElementCollector(doc, id).OfClass(typeof(IndependentTag)).ToElements();
                if (tags.Count > 0)
                {
                  foreach (IndependentTag t in tags)
                  {
                    var element = doc.GetElement(t.TaggedLocalElementId);
                    var tagText = t.TagText;
                    var tagFromElement = element.LookupParameter("TAG")?.AsString();
                    if (!string.IsNullOrWhiteSpace(tagFromElement) && tagText.Contains(tagFromElement))
                    {
                      if (!outDict.ContainsKey(tagFromElement))
                      {
                        outDict.Add(tagFromElement, new [] { new List<string> { revision }, new List<string> { sheetNumber } });
                      }
                      else
                      {
                        if (!outDict[tagFromElement][1].Contains(sheetNumber))
                        {
                          outDict[tagFromElement][0].Add(revision);
                          outDict[tagFromElement][1].Add(sheetNumber);
                        }
                        
                      }
                    }

                  }
                }
              }
            }
          }

        }
      }


      StringBuilder sb = new StringBuilder();
      if (outDict.Count > 0)
      {
        foreach (var d in outDict)
        {
          for (int i = 0; i < d.Value[0].Count; i++)
          {
            sb.Append(d.Key);
            sb.Append("\t");
            sb.Append(d.Value[0][i]);
            sb.Append("\t");
            sb.Append(d.Value[1][i]);
            sb.AppendLine();
          }
          
        }
      }

      System.Windows.Clipboard.Clear();
      System.Windows.Clipboard.SetText(sb.ToString());


      TaskDialog resultsDialog = new TaskDialog(
        "Данные в буфере обмена");

      resultsDialog.MainInstruction = "Данные в буфере обмена";

      resultsDialog.MainContent = "Вставьте данные из буфера обмена в Excel";

      resultsDialog.Show();

      return Result.Succeeded;
    }

  }
}
