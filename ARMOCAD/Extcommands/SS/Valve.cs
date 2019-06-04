using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ARMOCAD
{
  [TransactionAttribute(TransactionMode.Manual)]
  [RegenerationAttribute(RegenerationOption.Manual)]

  public class Valve : IExternalCommand
  {
    private FamilyInstance elems;

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      UIApplication uiApp = commandData.Application;
      UIDocument uidoc = uiApp.ActiveUIDocument;
      Application app = uiApp.Application;
      Document doc = uidoc.Document;
      ObjectType obt = ObjectType.LinkedElement;
      Element linkedelement = null;
      Reference refElemLinked;
      Schema sch = null;
      string SchemaGuid = "4718e9f1-71eb-4a59-9ef1-12ca32788c30";

      while (true)
      {
        try
        {
          ISelectionFilter selectionFilter = new PickFilter(doc);  //фильтр выбора
          refElemLinked = uidoc.Selection.PickObject(obt, selectionFilter, "Выберите элемент в связанной модели");
          RevitLinkInstance el = doc.GetElement(refElemLinked.ElementId) as RevitLinkInstance;
          Document docLinked = el.GetLinkDocument(); //получаем документ
          linkedelement = docLinked.GetElement(refElemLinked.LinkedElementId);
          var LinkUniq = el.UniqueId;
          var LinkName = docLinked.Title;
          var LinkPath = docLinked.PathName;
          string nameTargFam = "ME_Клапан ОЗК,КДУ_[СС]";
          FilteredElementCollector collector = new FilteredElementCollector(docLinked);
          var elem = collector.OfCategory(BuiltInCategory.OST_DuctAccessory).WhereElementIsNotElementType().Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "DA_Клапан_[ОЗК_КДУ, Сечение круглое]" || i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "DA_Клапан_[ОЗК_КДУ, Сечение прямоугольное]"); //элементы категории
          TaskDialog.Show("Информация ", "Семейство:  " + linkedelement.Name + "\n" + "Связь:  " + docLinked.Title + "\r\n");
          FilteredElementCollector a = new FilteredElementCollector(doc).OfClass(typeof(Family));
          Family family = a.FirstOrDefault<Element>(e => e.Name.Equals(nameTargFam)) as Family;
          if (family == null)
          {
            TaskDialog.Show("Предупреждение", "Не загружено семейство:\n " + nameTargFam);
            return Result.Cancelled;
          }
          ISet<ElementId> elementSet = family.GetFamilySymbolIds();
          FamilySymbol type = doc.GetElement(elementSet.First()) as FamilySymbol;
          //имена параметров
          string parameter1 = "Нормально отк/закр.";
          string parameter2 = "Имя системы";
          string parameter3 = "Комментарии";
          FilteredElementCollector MEcollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment);
          var targetElems = MEcollector.WhereElementIsNotElementType().Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == nameTargFam);
          using (Transaction t = new Transaction(doc, "trans"))
          {
            t.Start();
            type.Activate();
            foreach (Element e in elem)
            {
              bool check = CheckStorageExists(family, SchemaGuid);
              //TaskDialog.Show("1", check.ToString());
              if (check == false)
              {
                SchemaBuilder sb = new SchemaBuilder(new Guid(SchemaGuid));
                sb.SetReadAccessLevel(AccessLevel.Public);
                FieldBuilder fbName = sb.AddSimpleField("Link_Name", typeof(string));
                FieldBuilder fbUniq = sb.AddSimpleField("Link_Instance_UniqueId", typeof(string));
                FieldBuilder fbLUniq = sb.AddSimpleField("Link_Element_UniqueId", typeof(string));
                FieldBuilder fbLinkPath = sb.AddSimpleField("Link_Path", typeof(string));
                sb.SetSchemaName("Elements_from_Link_OZK");
                sch = sb.Finish();
              }
              else
              {
                sch = Schema.Lookup(new Guid(SchemaGuid));
              }
              bool Uniq = false;
              string linkparam1 = e.LookupParameter(parameter1).AsString();
              string linkparam2 = e.LookupParameter(parameter2).AsString();
              string linkparam3 = e.LookupParameter(parameter3).AsString();
              var ElemUniq = e.UniqueId;
              if (targetElems.Count() != 0 && targetElems != null)
              {
                foreach (Element targEL in targetElems)
                {
                  Entity entity = targEL.GetEntity(sch);
                  Field fLuniq = sch.GetField("Link_Element_UniqueId");
                  string LinkUniqid = entity.Get<string>(fLuniq);
                  if (LinkUniqid == ElemUniq) { Uniq = true; continue; }
                }
              }
              if (Uniq == true) { continue; }
              if (e.LookupParameter("Семейство").AsValueString() == "DA_Клапан_[ОЗК_КДУ, Сечение прямоугольное]")
              {
                LocationPoint pPoint = e.Location as LocationPoint;
                XYZ coords = pPoint.Point;
                elems = doc.Create.NewFamilyInstance(coords, type, StructuralType.NonStructural);
              }
              else
              {
                if (e.LookupParameter("Семейство").AsValueString() == "DA_Клапан_[ОЗК_КДУ, Сечение круглое]")
                {
                  BoundingBoxXYZ bounding = e.get_BoundingBox(null);
                  //LocationPoint pPoint = e.Location as LocationPoint;
                  XYZ coords = (bounding.Min + bounding.Max) * 0.5;
                  elems = doc.Create.NewFamilyInstance(coords, type, StructuralType.NonStructural);
                }
              }
              if (Uniq == true) { continue; }
              SetEntity(elems, sch, LinkName, LinkUniq, ElemUniq, LinkPath);
              Parameter param = elems.get_Parameter(new Guid("b4cfdcbd-5668-4572-bcd6-3d504043bd65"));
              param.Set(e.Name);
              Parameter param1 = elems.LookupParameter(parameter1);
              param1.Set(linkparam1);
              Parameter param2 = elems.LookupParameter(parameter2);
              param2.Set(linkparam2);
              Parameter param3 = elems.LookupParameter(parameter3);
              param3.Set(linkparam3);
              Parameter NZ = elems.LookupParameter("НЗ");
              Parameter NO = elems.LookupParameter("НО");
              if (linkparam1 == "НЗ")
              {
                NZ.Set(1);
                NO.Set(0);
              }
              else
              {
                if (linkparam1 == "НО")
                {
                  NZ.Set(0);
                  NO.Set(1);
                }
              }
            }
            t.Commit();
            break;
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
      }
      return Result.Succeeded;
    }
    public bool CheckStorageExists(Family fam, string sGuid)
    {
      try
      {
        Schema sch = Schema.Lookup(new Guid(sGuid));
        Entity ent = fam.GetEntity(sch);
        if (sch != null) return true;
      }
      catch { }
      return false;
    }
    void SetEntity(Element targetElement, Schema sch, string LinkName, string LinkUniq, string ElemUniq, string LinkPath)
    {
      Field fdName = sch.GetField("Link_Name");
      Field fdUniq = sch.GetField("Link_Instance_UniqueId");
      Field fdLuniq = sch.GetField("Link_Element_UniqueId");
      Field fdLPath = sch.GetField("Link_Path");
      Entity ent = new Entity(sch);
      ent.Set<string>(fdName, LinkName.ToString());
      ent.Set<string>(fdUniq, LinkUniq.ToString());
      ent.Set<string>(fdLuniq, ElemUniq.ToString());
      ent.Set<string>(fdLPath, LinkPath.ToString());
      targetElement.SetEntity(ent);
      return;
    }
  }

  

}
