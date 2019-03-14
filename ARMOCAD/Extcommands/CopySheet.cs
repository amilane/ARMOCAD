using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace CopySheet
{
  [Transaction(TransactionMode.Manual)]
  public class CopySheetClass : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {

      // Get application and document objects
      UIApplication ui_app = commandData.Application;
      UIDocument ui_doc = ui_app?.ActiveUIDocument;
      Document doc = ui_doc?.Document;
      ViewSheet vs = doc.ActiveView as ViewSheet;
      try
      {
        using (Transaction t = new Transaction(doc, "Duplicate Sheet"))
        {
          t.Start();
          FamilyInstance titleblock = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_TitleBlocks).Cast<FamilyInstance>().First(q => q.OwnerViewId == vs.Id);

          ViewSheet newsheet = ViewSheet.Create(doc, titleblock.GetTypeId());
          newsheet.SheetNumber = vs.SheetNumber + "-COPY";
          newsheet.Name = vs.Name;
          // all views but schedules
          foreach (ElementId eid in vs.GetAllPlacedViews())
          {
            View ev = doc.GetElement(eid) as View;

            View newview = null;

            // legends
            if (ev.ViewType == ViewType.Legend)
            {
              newview = ev;
            }
            // all non-legend and non-schedule views
            else
            {
              if (ev.CanViewBeDuplicated(ViewDuplicateOption.AsDependent))
              {
                ElementId newviewid = ev.Duplicate(ViewDuplicateOption.AsDependent);
                newview = doc.GetElement(newviewid) as View;
                //newview.Name = ev.Name + "-COPY";
              }


            }

            foreach (Viewport vp in new FilteredElementCollector(doc).OfClass(typeof(Viewport)))
            {

              if (vp.SheetId == vs.Id && vp.ViewId == ev.Id)
              {
                BoundingBoxXYZ vpbb = vp.get_BoundingBox(vs);
                XYZ initialCenter = (vpbb.Max + vpbb.Min) / 2;

                Viewport newvp = Viewport.Create(doc, newsheet.Id, newview.Id, XYZ.Zero);

                BoundingBoxXYZ newvpbb = newvp.get_BoundingBox(newsheet);
                XYZ newCenter = (newvpbb.Max + newvpbb.Min) / 2;

                ElementTransformUtils.MoveElement(doc, newvp.Id, new XYZ(
                initialCenter.X - newCenter.X,
                initialCenter.Y - newCenter.Y,
                0));
              }

            }// end for each

          }// end for each

          // schedules

          foreach (ScheduleSheetInstance si in (new FilteredElementCollector(doc).OfClass(typeof(ScheduleSheetInstance))))
          {
            if (si.OwnerViewId == vs.Id)
            {
              if (!si.IsTitleblockRevisionSchedule)
              {
                foreach (ViewSchedule vsc in new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule)))
                {
                  if (si.ScheduleId == vsc.Id)
                  {
                    BoundingBoxXYZ sibb = si.get_BoundingBox(vs);
                    XYZ initialCenter = (sibb.Max + sibb.Min) / 2;

                    ScheduleSheetInstance newssi = ScheduleSheetInstance.Create(doc, newsheet.Id, vsc.Id, XYZ.Zero);

                    BoundingBoxXYZ newsibb = newssi.get_BoundingBox(newsheet);
                    XYZ newCenter = (newsibb.Max + newsibb.Min) / 2;

                    ElementTransformUtils.MoveElement(doc, newssi.Id, new XYZ(
                        initialCenter.X - newCenter.X,
                        initialCenter.Y - newCenter.Y,
                        0));
                  }
                }
              }

            }
          }// end foreach

          t.Commit();

        }// end using

        // Implement Selection Filter to select curves

        // Measure their total length

        // Return a message window that displays total length to user

        // Assuming that everything went right return Result.Succeeded
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
