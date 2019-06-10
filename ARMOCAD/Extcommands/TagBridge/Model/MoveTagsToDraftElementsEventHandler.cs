using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ARMOCAD
{
  class MoveTagsToDraftElementsEventHandler : IExternalEventHandler
  {
    public TBModel RevitModel;
    public IEnumerable<Element> elems;
    public Schema schema;

    public void Execute(UIApplication uiapp)
    {
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      try
      {
        //ТРАНЗАКЦИЯ
        elems = RevitModel.elems;
        schema = RevitModel.schema;

        using (Transaction t = new Transaction(doc, "Перенос тэгов на схемы"))
        {
          t.Start();

          foreach (var e in elems)
          {
            try
            {
              Parameter parTagModel = e.LookupParameter("TAG");
              if (parTagModel != null)
              {
                ElementId draftId = getSchemaDictValue<ElementId>(e, "DictElemId", 0) as ElementId;
                if (draftId != null && draftId.IntegerValue != -1)
                {
                  Element draftElement = doc.GetElement(draftId);
                  Parameter parTagDraft = draftElement.LookupParameter("TAG");
                  if (parTagDraft != null)
                  {
                    string tagModel = parTagModel.AsString();
                    if (!string.IsNullOrWhiteSpace(tagModel))
                    {
                      parTagDraft.Set(tagModel);
                    }
                  }
                }
              }
            }
            catch (Exception exception)
            {
              continue;
            }
            
            
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

    public object getSchemaDictValue<T>(Element e, string fieldName, int key)
    {
      object result = null;
      IDictionary<int, T> dict;

      var ent = e.GetEntity(schema);
      if (ent.Schema != null)
      {

        dict = ent.Get<IDictionary<int, T>>(schema.GetField(fieldName));
        if (dict != null && dict.ContainsKey(key))
        {
          result = dict[key];
        }


      }

      return result;
    }



  }
}
