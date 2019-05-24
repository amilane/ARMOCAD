using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ARMOCAD
{
  class ConnectButtonEventHandler : IExternalEventHandler
  {
    public TBModel RevitModel;

    public Schema schema;

    public void Execute(UIApplication uiapp)
    {
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      try
      {
        //TODO: Write Revit API code here
        schema = RevitModel.schema;

        using (Transaction t = new Transaction(doc, "lolkek"))
        {
          t.Start();

          ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

          List<Element> elems = new List<Element>();
          if (selectedIds.Count != 2)
          {
            TaskDialog.Show("Ошибка", "Выберите 2 элемента.");
          }
          else
          {
            foreach (var i in selectedIds)
            {
              elems.Add(doc.GetElement(i));
            }

            if (elems.Any(i => i.Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents) &&
                elems.Any(i => i.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents))
            {
              Element eModel = elems.Where(i => i.Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents).First();
              Element eDraft = elems.Where(i => i.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents).First();

              // create an entity object (object) for this schema (class)
              Entity entity = new Entity(schema);

              // get the field from schema
              Field field = schema.GetField("DraftElemFromScheme");

              // set the value for entity
              entity.Set(field, eDraft.Id);

              // store the entity on the element
              eModel.SetEntity(entity);

              Parameter parTagDraft = eDraft.LookupParameter("TAG");
              Parameter parTagModel = eModel.LookupParameter("TAG");

              if (parTagDraft != null && parTagDraft != null && !string.IsNullOrWhiteSpace(parTagModel.AsString()))
              {
                parTagDraft.Set(parTagModel.AsString());

                TagItem tag = new TagItem();
                tag.ModelId = eModel.Id;
                tag.DraftId = eDraft.Id;
                tag.ModelTag = parTagModel.AsString();
                tag.DraftTag = parTagModel.AsString();

                RevitModel.NewTag = tag;



              }

              
              

            }
            else
            {
              TaskDialog.Show("Ошибка", "Выберите 1 элемент модели и 1 элемент узла на чертежном виде.");
            }

          }

          t.Commit();
        }




      }
      catch (Exception exception)
      {

        System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }


      //WpfWindowController.Instance.Window.Close(); //uncomment it to close the app
    }
    public string GetName()
    {
      return "Revit Addin";
    }

  }
}
