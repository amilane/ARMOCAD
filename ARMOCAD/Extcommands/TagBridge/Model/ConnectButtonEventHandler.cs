using System;
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
        //ТРАНЗАКЦИЯ
        //накидывает схему на элемент модели (с Id элемента узла), переносит тэг из модели в элемент узла
        schema = RevitModel.schema;

        using (Transaction t = new Transaction(doc, "lolkek"))
        {
          t.Start();

          Element eModel = RevitModel.EModel;
          Element eDraft = RevitModel.EDraft;

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
          }

          t.Commit();
        }




      }
      catch (Exception exception)
      {
        System.Windows.MessageBox.Show("Some error has occured. \n" + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }

    }
    public string GetName()
    {
      return "Revit Addin";
    }

  }
}
