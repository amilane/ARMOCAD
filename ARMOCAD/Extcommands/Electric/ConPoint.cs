using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Structure;

namespace ARMOCAD
{
  [TransactionAttribute(TransactionMode.Manual)]
  [RegenerationAttribute(RegenerationOption.Manual)]


  public class ConPoint : IExternalCommand
  {
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
          FilteredElementCollector collector = new FilteredElementCollector(doc);
          ISelectionFilter selectionFilter = new ConPickFilter(doc);
          refElemLinked = uidoc.Selection.PickObject(obt, selectionFilter, "Выберите связь");
          RevitLinkInstance linkInstance = doc.GetElement(refElemLinked.ElementId) as RevitLinkInstance;
          Document docLinked = linkInstance.GetLinkDocument();
          string taskguid = "893e72a1-b208-4d12-bb26-6bcc4a444d0c";
          string famname1 = "ME_Точка_подключения_(1 фазная сеть)";
          string famname2 = "ME_Точка_подключения_(2 коннектора, 3 фазная сеть)";
          string famname3 = "ME_Точка_подключения_(3 фазная сеть)";
          var LinkUniq = linkInstance.UniqueId;
          var LinkName = docLinked.Title;
          var LinkPath = docLinked.PathName;
          TaskDialog.Show("Информация ", "Связь:  " + LinkName + "\r\n");
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
          var ElemsWithParam = CatsElems.Where(f => f.get_Parameter(new Guid(taskguid)) != null);
          var elems = ElemsWithParam.Where(f => f.get_Parameter(new Guid(taskguid)).AsInteger() == 1);  //фильтр по параметру "Задание ЭМ"
          FilteredElementCollector collfams = collector.OfClass(typeof(Family));
          Family fam1 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname1)) as Family;
          Family fam2 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname2)) as Family;
          Family fam3 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname3)) as Family;
          if (fam1 == null || fam2 == null || fam3 == null)
          {
            string n1, n2, n3; n1 = ""; n2 = ""; n3 = "";
            if (fam1 == null) { n1 = famname1 + "\n"; }
            if (fam2 == null) { n2 = famname2 + "\n"; }
            if (fam3 == null) { n3 = famname3 + "\n"; }
            TaskDialog.Show("Предупреждение", "Не загружены семейства:\n " + n1 + n3 + n2);
            return Result.Cancelled;
          }
          ISet<ElementId> elementSet1 = fam1.GetFamilySymbolIds();
          ISet<ElementId> elementSet2 = fam2.GetFamilySymbolIds();
          ISet<ElementId> elementSet3 = fam3.GetFamilySymbolIds();
          FamilySymbol type1 = doc.GetElement(elementSet1.First()) as FamilySymbol;
          FamilySymbol type2 = doc.GetElement(elementSet2.First()) as FamilySymbol;
          FamilySymbol type3 = doc.GetElement(elementSet3.First()) as FamilySymbol;
          FilteredElementCollector MEcollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType();
          var targetElems = MEcollector.WhereElementIsNotElementType().Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname1
          || i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname3);
          var targetElems2 = MEcollector.WhereElementIsNotElementType().Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname2);
          using (Transaction t = new Transaction(doc, "trans"))
          {
            t.Start();
            type1.Activate();
            type2.Activate();
            type3.Activate();
            string poles = "d182b385-9e45-4e8b-b8da-725396848493"; // параметр "Количетсво полюсов"
            string ozk = "ce22f60b-9ae0-4c79-a624-873f39099510";   // параметр "Нормально отк/закр." для клапанов озк
            string Nname = "b4cfdcbd-5668-4572-bcd6-3d504043bd65"; // параметр "Наименование"
            string KM = "e3c1a4b0-78c8-49f5-b3c7-01869252c30e";// параметр "Коэф. мощности"
            IList<string> Guids = new List<string> // общие параметры
          {
            "303f67e6-3fd6-469b-9356-dccb116a3277", // параметр "Имя системы"
            //"e3c1a4b0-78c8-49f5-b3c7-01869252c30e", // параметр "Коэф. мощности"
            "7e243149-8b16-4c8b-8161-cd7780048c99", // параметр "Ток"
            "be29221e-5b74-4a61-a253-4eb5f3b532d9", // параметр "Напряжение"
            "b4d13aad-0763-4481-b015-63137342d077"  // параметр "Номинальная мощность"     
          };
            IList<string> NamesParam = new List<string> // параметры семейства
          {
            "Ввод1_Номинальная_мощность",
            "Ввод1_Коэффициент_мощности",
            "Ввод1_Напряжение",
            "Ввод2_Номинальная_мощность",
            "Ввод2_Коэффициент_мощности",
            "Ввод2_Напряжение"
          };
            bool check1 = CheckStorageExists(fam1, SchemaGuid);
            bool check2 = CheckStorageExists(fam2, SchemaGuid);
            bool check3 = CheckStorageExists(fam3, SchemaGuid);
            if (check1 == false || check3 == false || check2 == false)
            {
              SchemaBuilder sb = new SchemaBuilder(new Guid(SchemaGuid));
              sb.SetReadAccessLevel(AccessLevel.Public);
              FieldBuilder fbName = sb.AddSimpleField("Link_Name", typeof(string));
              FieldBuilder fbUniq = sb.AddSimpleField("Link_Instance_UniqueId", typeof(string));
              FieldBuilder fbLUniq = sb.AddSimpleField("Link_Element_UniqueId", typeof(string));
              FieldBuilder fbLinkPath = sb.AddSimpleField("Link_Path", typeof(string));
              sb.SetSchemaName("Elements_from_Link");
              sch = sb.Finish();
            }
            else
            {
              sch = Schema.Lookup(new Guid(SchemaGuid));
            }
            foreach (Element origElement in elems)
            {
              LocationPoint pPoint = origElement.Location as LocationPoint;
              XYZ coords = pPoint.Point;
              bool Uniq = false;
              var FamSymbol = (origElement as FamilyInstance).Symbol; // FamilySymbol элементов связи
              var ElemUniq = origElement.UniqueId;
              var famname = origElement.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
              string typename = famname + "_/" + origElement.Name; typename = typename.Replace("[", "("); typename = typename.Replace("]", ")");
              if (targetElems2 != null)
              {
                foreach (Element targEL in targetElems2)
                {
                  Entity entity = targEL.GetEntity(sch);
                  Field fLuniq = sch.GetField("Link_Element_UniqueId");
                  string LinkUniqid = entity.Get<string>(fLuniq);
                  if (LinkUniqid == ElemUniq)
                  {
                    Uniq = true;
                    continue;
                  }
                }
              }
              if (Uniq == true)
              { continue; }
              if (typename.Contains("(380-2)"))
              {
                FamilySymbol Newtype = Utils.FindElementByName(doc, typeof(FamilySymbol), typename) as FamilySymbol ?? CreateNewType(type2, typename);
                var targElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
                foreach (string nameparam in NamesParam)
                {
                  if (FamSymbol.LookupParameter(nameparam) != null)
                  {
                    var origParam = FamSymbol.LookupParameter(nameparam).AsDouble();
                    targElement.LookupParameter(nameparam).Set(origParam);
                  }
                  if (origElement.LookupParameter(nameparam) != null)
                  {
                    var origParam = origElement.LookupParameter(nameparam).AsDouble();
                    targElement.LookupParameter(nameparam).Set(origParam);
                  }
                }
                NameSystemParameter(origElement, targElement, Guids[0]);
                targElement.get_Parameter(new Guid(Nname)).Set(origElement.Name);
                SetEntity(targElement, sch, LinkName, LinkUniq, ElemUniq, LinkPath);
                continue;
              }
              foreach (Element targEL in targetElems)
              {
                Entity entity = targEL.GetEntity(sch);
                Field fLuniq = sch.GetField("Link_Element_UniqueId");
                string LinkUniqid = entity.Get<string>(fLuniq);
                if (LinkUniqid == ElemUniq) { Uniq = true; continue; }
              }
              if (Uniq == true) { continue; }
              FamilyInstance targetElement = null;
              if (FamSymbol.get_Parameter(new Guid(poles)) != null || origElement.get_Parameter(new Guid(poles)) != null)
              {
                if (origElement.get_Parameter(new Guid(poles))?.AsInteger() == 1 || FamSymbol.get_Parameter(new Guid(poles))?.AsInteger() == 1)
                {
                  FamilySymbol Newtype = Utils.FindElementByName(doc, typeof(FamilySymbol), typename) as FamilySymbol ?? CreateNewType(type1, typename);
                  targetElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
                }
                if (origElement.get_Parameter(new Guid(poles))?.AsInteger() == 3 || FamSymbol.get_Parameter(new Guid(poles))?.AsInteger() == 3)
                {
                  FamilySymbol Newtype = Utils.FindElementByName(doc, typeof(FamilySymbol), typename) as FamilySymbol ?? CreateNewType(type3, typename);
                  targetElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
                }
              }
              else { continue; }
              SetEntity(targetElement, sch, LinkName, LinkUniq, ElemUniq, LinkPath);
              if (origElement.get_Parameter(new Guid(ozk)) != null)
              {
                var origPar = origElement.get_Parameter(new Guid(ozk)).AsString();
                targetElement.get_Parameter(new Guid(ozk)).Set(origPar);
                if (origPar == "НЗ")
                {
                  targetElement.Symbol.LookupParameter("УГО_ОЗК_НЗ").Set(1);
                  targetElement.Symbol.LookupParameter("УГО_Кабельный вывод").Set(0);
                }
                if (origPar == "НО")
                {
                  targetElement.Symbol.LookupParameter("УГО_ОЗК_НО").Set(1);
                  targetElement.Symbol.LookupParameter("УГО_Кабельный вывод").Set(0);
                }
              }
              foreach (string guid in Guids)
              {

                if (guid == Guids[0])
                {
                  NameSystemParameter(origElement, targetElement, guid);
                  continue;
                }
                if (origElement.get_Parameter(new Guid(guid)) != null)
                {

                  var origParam = FamSymbol.get_Parameter(new Guid(guid)).AsDouble();
                  targetElement.get_Parameter(new Guid(guid)).Set(origParam);
                }
                if (origElement.get_Parameter(new Guid(guid)) != null)
                {
                  var origParam = origElement.get_Parameter(new Guid(guid)).AsDouble();
                  targetElement.get_Parameter(new Guid(guid)).Set(origParam);
                }
              }
              if (origElement.get_Parameter(new Guid(KM)) != null)
              {
                targetElement.get_Parameter(new Guid(KM)).Set(origElement.get_Parameter(new Guid(KM)).AsDouble());
              }
              targetElement.get_Parameter(new Guid(Nname)).Set(origElement.Name);
              if (typename.Contains("Вентилятор"))
              {
                targetElement.Symbol.LookupParameter("УГО_Двигатель").Set(1);
                targetElement.Symbol.LookupParameter("УГО_Кабельный вывод").Set(0);
              }
              if (typename.Contains("МДУ"))
              {
                targetElement.Symbol.LookupParameter("УГО_МДУ_(клапан ОЗК)").Set(1);
                targetElement.Symbol.LookupParameter("УГО_Кабельный вывод").Set(0);
              }
              if (typename.Contains("Щит_автоматики"))
              {
                targetElement.Symbol.LookupParameter("УГО_ЩА").Set(1);
                targetElement.Symbol.LookupParameter("УГО_Кабельный вывод").Set(0);
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

    private void SetEntity(Element targetElement, Schema sch, string LinkName, string LinkUniq, string ElemUniq, string LinkPath)
    {
      Entity ent = new Entity(sch);
      Field fdName = sch.GetField("Link_Name");
      Field fdUniq = sch.GetField("Link_Instance_UniqueId");
      Field fdLuniq = sch.GetField("Link_Element_UniqueId");
      Field fdLPath = sch.GetField("Link_Path");
      ent.Set<string>(fdName, LinkName.ToString());
      ent.Set<string>(fdUniq, LinkUniq.ToString());
      ent.Set<string>(fdLuniq, ElemUniq.ToString());
      ent.Set<string>(fdLPath, LinkPath.ToString());
      targetElement.SetEntity(ent);
      return;
    }
    private void NameSystemParameter(Element origElement, Element targetElement, string guid)
    {
      if (origElement.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM) != null)
      {
        var origParam = origElement.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
        targetElement.get_Parameter(new Guid(guid)).Set(origParam);
      }
      if (origElement.get_Parameter(new Guid(guid)) != null)
      {
        var origParam = origElement.get_Parameter(new Guid(guid)).AsString();
        targetElement.get_Parameter(new Guid(guid)).Set(origParam);
      }
    }
    private bool CheckStorageExists(Family fam, string sGuid)
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
    public static FamilySymbol CreateNewType(FamilySymbol Type, string Typename)
    {
      FamilySymbol newtype = Type.Duplicate(Typename) as FamilySymbol;
      return newtype;
    }
    public static class Utils
    {
      public static Element FindElementByName(Document doc, Type targetType, string targetName)
      {
        return new FilteredElementCollector(doc).OfClass(targetType).FirstOrDefault<Element>(e => e.Name.Equals(targetName));
      }
    }
    
  }
}