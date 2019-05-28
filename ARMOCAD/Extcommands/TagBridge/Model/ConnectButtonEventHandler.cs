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
    public Element eModelDelDraft;

    public void Execute(UIApplication uiapp)
    {
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      try
      {
        //ТРАНЗАКЦИЯ
        //удаляет ElementId из предыдущего экземпляра (eModelDelDraft)
        //накидывает схему на элемент модели (с Id элемента узла), переносит тэг из модели в элемент узла
        schema = RevitModel.schema;
        eModelDelDraft = RevitModel.EModelDelDraft;

        using (Transaction t = new Transaction(doc, "Связь 2х экземпляров"))
        {
          t.Start();

          Element eModel = RevitModel.EModel;
          Element eDraft = RevitModel.EDraft;

          SchemaMethods.setValueToEntity<ElementId>(eModel, "DictElemId", 0, eDraft.Id);
          
          //clear entity for old element
          if (eModelDelDraft != null)
          {
            SchemaMethods.setValueToEntity<ElementId>(eModelDelDraft, "DictElemId", 0, new ElementId(-1));
          }

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
