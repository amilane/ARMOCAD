using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  class modelRevitBridge
  {
    // Just like what you do when creating a Revit command, declare the necessary variable such as below.
    private UIApplication UIAPP = null;
    private Application APP = null;
    private UIDocument UIDOC = null;
    private Document DOC = null;

    // The model constructor. Include a UIApplication argument and do all the assignments here.
    public modelRevitBridge(UIApplication uiapp)
    {
      UIAPP = uiapp;
      APP = UIAPP.Application;
      UIDOC = UIAPP.ActiveUIDocument;
      DOC = UIDOC.Document;
    }

    // This function will be called by the Action function in the view model, so it must be public.
    public List<string> GenerateParametersAndValues(int idIntegerValue)
    {
      List<string> resstr = new List<string>();

      Element el = DOC.GetElement(new ElementId(idIntegerValue));
      if (el != null)
      {
        foreach (Parameter prm in el.Parameters)
        {
          string str = prm.Definition.Name;
          str += " : ";
          str += prm.AsValueString();

          resstr.Add(str);
        }
      }

      return resstr.OrderBy(x => x).ToList();
    }
  }
}
