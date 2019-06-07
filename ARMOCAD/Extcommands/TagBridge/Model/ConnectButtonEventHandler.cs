using System;
using System.Collections.Generic;
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
        
        using (Transaction t = new Transaction(doc, "Связь 2х экземпляров"))
        {
          t.Start();

          Element eModel = RevitModel.EModel;
          Element eDraft = RevitModel.EDraft;

          setValueToEntity<ElementId>(eModel, "DictElemId", 0, eDraft.Id);

          
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

    public void setValueToEntity<T>(Element e, string fieldName, int key, T value)
    {
      Entity entity;
      IDictionary<int, T> dict = null;
      Field field = schema.GetField(fieldName);

      entity = e.GetEntity(schema);

      if (entity.Schema == null)
      {
        entity = new Entity(schema);
        dict = new Dictionary<int, T> { [key] = value };
      }
      else
      {
        dict = entity.Get<IDictionary<int, T>>(field);
        if (dict != null)
        {
          if (dict.ContainsKey(key))
          {
            dict[key] = value;
          }
          else
          {
            dict.Add(key, value);
          }
        }

      }
      
      e.SetEntity(entity);

    }



  }
}
