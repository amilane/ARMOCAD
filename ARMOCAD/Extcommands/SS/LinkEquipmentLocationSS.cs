using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ARMOCAD
{
  [TransactionAttribute(TransactionMode.Manual)]
  [RegenerationAttribute(RegenerationOption.Manual)]


  public class LinkEquipmentLoc : IExternalCommand
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
            ["Задание СС"] = "d30b8343-4a2d-4457-9137-e34e511d7233",
            ["Новый"] = "bf28d2b7-3b97-4f90-8c2a-590a92a654c6",
            ["Перемещен"] = "7ad4d050-bedd-42fa-98c4-6cff9e953460",
            ["Удален"] = "f2b4ac9f-4ae4-49c3-9c84-e043de20d814",
            ["Этаж"] = "4857fa3b-e80e-4167-9b66-f40cd5992680"
          };
          FilteredElementCollector collector = new FilteredElementCollector(doc);
          IList<Element> links = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType().ToElements();
          var linkName = PlaceEquip.SelectLink(links);
          var link = links.Where(i => i.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString() == linkName);
          RevitLinkInstance linkInstance = link.First() as RevitLinkInstance;
          Document docLinked = linkInstance.GetLinkDocument();
          string famname1 = "Задание для СС";
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
          var elems = CatsElems.Where(f =>
            f.get_Parameter(new Guid(param["Задание СС"])) != null &&
            f.get_Parameter(new Guid(param["Задание СС"])).AsInteger() == 1); //фильтр по параметру "Задание ЭМ"

          FilteredElementCollector collfams = collector.OfClass(typeof(Family));
          Family fam1 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname1)) as Family;
          FilteredElementCollector MEcollector = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType();
          sm = new SchemaMethods(SchemaGuid, "Ag_Schema");
          sch = sm.Schema;
          var targetElems = MEcollector.Where(i =>
            i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname1 &&
            (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Element_UniqueId) == i.UniqueId &&
            (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Link_Name) == LinkName);

          var NSE = 0;
          var PSE = 0;
          var countId = 0;
          var Del = 0;
          string tdelems = "Этажи на которых есть неправильные элементы:" + "\n";
          int countLink = elems.Count();
          foreach (Element targEL in targetElems)
          {
            var cntEl = 0; //удаленный элемент
            foreach (Element origElement in elems)
            {
              LocationPoint pPoint = origElement.Location as LocationPoint;
              XYZ pointLink = pPoint.Point;
              var ElemUniq = origElement.UniqueId;
              string LinkUniq =
                (string)sm.getSchemaDictValue<string>(targEL, "Dict_String", (int)Keys.Linked_Element_UniqueId);
              if (LinkUniq == null || LinkUniq == string.Empty)
              {
                continue;
              }

              if (LinkUniq == ElemUniq)
              {
                countId++;
                cntEl++;
                LocationPoint locEl = targEL.Location as LocationPoint;
                XYZ pointEl = locEl.Point;
                if (pointEl.ToString() == pointLink.ToString())
                {
                  PSE++;
                }

                if (pointEl.ToString() != pointLink.ToString())
                {
                  NSE++;
                  var lvl = targEL.get_Parameter(new Guid(param["Этаж"])).AsString();
                  if (!tdelems.Contains(lvl))
                  {
                    tdelems = tdelems + "[" + targEL.get_Parameter(new Guid(param["Этаж"])).AsString() + "]" + "\n";
                  }
                }
              }

            }

            if (cntEl == 0)
            {
              Del++;
              var lvl = targEL.get_Parameter(new Guid(param["Этаж"])).AsString();
              if (!tdelems.Contains(lvl))
              {
                tdelems = tdelems + "[" + targEL.get_Parameter(new Guid(param["Этаж"])).AsString() + "]" + "\n";
              }
            }
          }
          string U1, U2, U3, U5;
          U1 = string.Empty;
          U2 = string.Empty;
          U3 = string.Empty;
          U5 = string.Empty;
          TaskDialog td = new TaskDialog("Информация");
          bool Not = true;
          if (countLink > countId)
          {
            var q = countLink - countId;
            U1 = "Не размещено " + q + " элементов" + " \n";
            Not = false;
          }
          if (NSE > 0)
          {
            U2 = "Неправильно размещенных элементов: " + NSE + " \n";
          }

          if (NSE == 0)
          {
            U3 = "Нет неправильно размещенных элементов \n";
          }
          if (Del!=0)
          {
            U5 = "Удаленных элементов: " + Del + " \n";
          }
          var U4 = "На своих местах: " + PSE + " из " + countLink ;
          if (Del == 0 && NSE == 0)
          {
            tdelems = String.Empty;
          }
          td.Id = "ID_TaskDialog_Demonstration_by_Spiderinnet";
          td.Title = "Информация";
          td.TitleAutoPrefix = false;
          td.AllowCancellation = true;
          td.MainInstruction = "Связь: " + LinkName;
          td.MainContent = U1 + U2 + U3 + U5 + U4;
          td.FooterText = "Количество элементов в связи: " + elems.Count().ToString();
          td.ExpandedContent =  tdelems;
          if (NSE != 0)
          {
            td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Отметить неправильно размещенные");
            td.VerificationText = "Переместить неправильные на свои места";
            if (Not == false)
            {
              td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Отметить неправильные и добавить новые");
            }
          }
          else
          {
            if (Not == false)
            {
              td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Добавить новые элементы");
            }
            td.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
          }

          td.CommonButtons = TaskDialogCommonButtons.Close;
          TaskDialogResult tdRes = td.Show();
          if (tdRes == TaskDialogResult.CommandLink1 || tdRes == TaskDialogResult.CommandLink2||tdRes==TaskDialogResult.CommandLink3)
          {
            using (Transaction tr = new Transaction(doc, "Проверка элементов из связи"))
            {
              tr.Start();

              foreach (Element targEL in targetElems) // перебираем элементы проекта
              {
                var cntEl = 0; //удаленный элемент
                foreach (Element origElement in elems) //перебираем элементы связи
                {
                  LocationPoint pPoint = origElement.Location as LocationPoint;
                  XYZ pointLink = pPoint.Point;
                  var ElemUniq = origElement.UniqueId;
                  string LinkUniq =
                    (string)sm.getSchemaDictValue<string>(targEL, "Dict_String", (int)Keys.Linked_Element_UniqueId);
                  if (LinkUniq == null || LinkUniq == string.Empty)
                  {
                    continue;
                  }
                  if (LinkUniq == ElemUniq) //уник номер проекта и связи равны
                  {
                    cntEl++;
                    LocationPoint locEl = targEL.Location as LocationPoint;
                    XYZ pointEl = locEl.Point;
                    if (pointEl.ToString() != pointLink.ToString()) //координаты не равны
                    {
                      targEL.get_Parameter(new Guid(param["Перемещен"])).Set(1);
                      targEL.get_Parameter(new Guid(param["Новый"])).Set(0);
                      if (td.WasVerificationChecked())
                      {
                        XYZ newPoint = pointLink - pointEl;
                        ElementTransformUtils.MoveElement(doc,targEL.Id,newPoint); 
                      }
                    }
                  }
                }

                if (cntEl == 0)
                {
                  targEL.get_Parameter(new Guid(param["Удален"])).Set(1);
                  targEL.get_Parameter(new Guid(param["Перемещен"])).Set(0);
                  targEL.get_Parameter(new Guid(param["Новый"])).Set(0);
                }
              }
              tr.Commit();
            }
            if (tdRes == TaskDialogResult.CommandLink2)
            {
              PlaceEquip ss = new PlaceEquip(doc,linkName);
            }
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
  }
}
