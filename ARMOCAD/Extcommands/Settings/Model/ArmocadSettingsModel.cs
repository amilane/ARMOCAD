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
  class ArmocadSettingsModel
  {
    private static UIApplication UIAPP = null;
    private static Application APP = null;
    private static UIDocument UIDOC = null;
    private static Document DOC = null;


    public ArmocadSettingsModel(UIApplication uiapp)
    {
      UIAPP = uiapp;
      APP = UIAPP.Application;
      UIDOC = UIAPP.ActiveUIDocument;
      DOC = UIDOC.Document;

      

    }

    public List<string> collectFamilyNames()
    {
      var electricEquip = Util.GetFamilySymbolsOfCategory(DOC, BuiltInCategory.OST_ElectricalEquipment);
      var communicationDev = Util.GetFamilySymbolsOfCategory(DOC, BuiltInCategory.OST_CommunicationDevices);
      var dataDev = Util.GetFamilySymbolsOfCategory(DOC, BuiltInCategory.OST_DataDevices);

      ///////////////////////////////////////////
      return null;
    }


  }
}
