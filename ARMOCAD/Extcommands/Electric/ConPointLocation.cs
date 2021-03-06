using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinForms = System.Windows.Forms;

namespace ARMOCAD
{
  [TransactionAttribute(TransactionMode.Manual)]
  [RegenerationAttribute(RegenerationOption.Manual)]


  public class ConPointLocation : IExternalCommand
  {
    private SchemaMethods sm;    
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      UIApplication uiApp = commandData.Application;
      UIDocument uidoc = uiApp.ActiveUIDocument;
      Application app = uiApp.Application;
      Document doc = uidoc.Document;
      Schema sch = null;
      string SchemaGuid = "ea07dfeb-9c7f-4233-b516-6621abc6744e";
      ObjectType obt = ObjectType.Element;
      Reference refElemLinked;
      while (true)
      {
        try
        {
          Dictionary<string, string> param = new Dictionary<string, string>
          {
            ["Задание ЭМ"] = "893e72a1-b208-4d12-bb26-6bcc4a444d0c",
            ["Новый"] = "bf28d2b7-3b97-4f90-8c2a-590a92a654c6",
            ["Перемещен"] = "7ad4d050-bedd-42fa-98c4-6cff9e953460",
            ["Удален"]= "f2b4ac9f-4ae4-49c3-9c84-e043de20d814"
          };
          FilteredElementCollector collector = new FilteredElementCollector(doc);
          ISelectionFilter selectionFilter = new LinkPickFilter(doc);
          refElemLinked = uidoc.Selection.PickObject(obt, selectionFilter, "Выберите связь");
          RevitLinkInstance linkInstance = doc.GetElement(refElemLinked.ElementId) as RevitLinkInstance;
          Document docLinked = linkInstance.GetLinkDocument();
          string famname1 = "ME_Точка_подключения_(1 фазная сеть)";
          string famname2 = "ME_Точка_подключения_(2 коннектора, 3 фазная сеть)";
          string famname3 = "ME_Точка_подключения_(3 фазная сеть)";
          var LinkInstUniq = linkInstance.UniqueId;
          var LinkName = docLinked.Title;
          var LinkPath = docLinked.PathName;
          //TaskDialog.Show("Информация ", "Связь:  " + LinkName + "\r\n");

          FilteredElementCollector collectorlink = new FilteredElementCollector(docLinked);
          IList<Element> CatsElems = new List<Element>();
          collectorlink.WherePasses(new LogicalOrFilter(new List<ElementFilter>
                {
                new ElementCategoryFilter(BuiltInCategory.OST_DuctAccessory),
                new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory),
                new ElementCategoryFilter(BuiltInCategory.OST_Furniture),
                new ElementCategoryFilter(BuiltInCategory.OST_GenericModel),
                new ElementCategoryFilter(BuiltInCategory.OST_LightingFixtures),
                new ElementCategoryFilter(BuiltInCategory.OST_ElectricalFixtures),
                new ElementCategoryFilter(BuiltInCategory.OST_SecurityDevices),
                new ElementCategoryFilter(BuiltInCategory.OST_FireAlarmDevices),
                new ElementCategoryFilter(BuiltInCategory.OST_CommunicationDevices),
                new ElementCategoryFilter(BuiltInCategory.OST_ElectricalEquipment),
                new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment),
                new ElementCategoryFilter(BuiltInCategory.OST_Casework)
                }));
          CatsElems = collectorlink.WhereElementIsNotElementType().ToElements(); //элементы по категориям
          var elems = CatsElems.Where(f => f.get_Parameter(new Guid(param["Задание ЭМ"])) != null && f.get_Parameter(new Guid(param["Задание ЭМ"])).AsInteger() == 1); //фильтр по параметру "Задание ЭМ"

          FilteredElementCollector collfams = collector.OfClass(typeof(Family));
          Family fam1 = collfams.FirstOrDefault(e => e.Name.Equals(famname1)) as Family;
          Family fam2 = collfams.FirstOrDefault(e => e.Name.Equals(famname2)) as Family;
          Family fam3 = collfams.FirstOrDefault(e => e.Name.Equals(famname3)) as Family;
          FilteredElementCollector MEcollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType();
          sm = new SchemaMethods(SchemaGuid, "Ag_Schema");
          sch = sm.Schema;
          var targetElems = MEcollector.Where(i => (i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname1
          || i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname2 || i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname3) && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Element_UniqueId) == i.UniqueId && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Link_Name) == LinkName);
                    
          var NSE = 0;
          var PSE = 0;
          var countId = 0;
          int countLink = elems.Count();
          using (Transaction tr = new Transaction(doc, "Проверка элементов из связи"))
          {
            tr.Start();
            foreach (Element targEL in targetElems)
            {
              var cntEl = 0;
              foreach (Element origElement in elems)
              {
                LocationPoint pPoint = origElement.Location as LocationPoint;
                XYZ pointLink = pPoint.Point;
                var ElemUniq = origElement.UniqueId;
                string LinkUniq = (string)sm.getSchemaDictValue<string>(targEL, "Dict_String", (int)Keys.Linked_Element_UniqueId);                
                if (LinkUniq == null || LinkUniq == string.Empty) { continue; }              
                if (LinkUniq == ElemUniq)
                {
                  cntEl++;
                  countId++;
                  LocationPoint locEl = targEL.Location as LocationPoint;
                  XYZ pointEl = locEl.Point;
                  if (pointEl.ToString() == pointLink.ToString())
                  {
                    PSE++;
                  }
                  if (pointEl.ToString() != pointLink.ToString())
                  {
                    NSE++;
                    targEL.get_Parameter(new Guid(param["Перемещен"])).Set(1);
                    targEL.get_Parameter(new Guid(param["Новый"])).Set(0);
                    //targEL.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("Элемент перемещен!!!");
                  }
                }
               
              }
              if (cntEl == 0)
              {
                NSE++;
                targEL.get_Parameter(new Guid(param["Удален"])).Set(1);
                targEL.get_Parameter(new Guid(param["Перемещен"])).Set(0);
                targEL.get_Parameter(new Guid(param["Новый"])).Set(0);
                //targEL.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("Элемент удален!!!");
              }
            }
            string U1, U2, U3; U1 = string.Empty; U2 = string.Empty; U3 = string.Empty;           
            if (countLink > countId)
            {
              var q = countLink - countId;
              U1 = "Не размещено " + q + " элементов" + " \n";
            }
            if (NSE > 0)
            {
              U2 = "Неправильно размещенных элементов: " + NSE + " \n";
            }
            if (NSE == 0)
            {
              U3 = "Нет неправильно размещенных элементов \n";
            }
            var U4 = "На своих местах: " + PSE + " из " + countLink;
            InfoMsg("Связь: " + LinkName + "\n" + "Количество элементов в связи: " + elems.Count().ToString() + " \n \n" + U1 + U2 + U3 + U4);
            tr.Commit();
          }
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
          break;
        }
        catch (Exception ex)
        {
          message = ex.Message;
          return Result.Failed;
        }
        break;
      }

      return Result.Succeeded;

    }
    public static void InfoMsg(string msg)
    {
      Debug.WriteLine(msg);
      WinForms.MessageBox.Show(msg, "Информация", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
    }
  }

}
