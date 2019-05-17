using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public class ExternalEventMy : IExternalEventHandler
  {
    public delegate void ButtonAction();

    public ButtonAction act;
    public string transactionName;
    

    public void Execute(UIApplication uiapp)
    {
      UIDocument uidoc = uiapp.ActiveUIDocument;
      if (null == uidoc)
      {
        return; // no document, nothing to do
      }
      Document doc = uidoc.Document;
      using (Transaction tx = new Transaction(doc))
      {
        tx.Start(transactionName);

        // Action within valid Revit API context thread
        act();


        tx.Commit();
      }
    }
    public string GetName()
    {
      return transactionName;
    }

  }
}
