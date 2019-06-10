using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Structure;
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

  public class ConPoint : IExternalCommand
  {
    private Schema sch;
    public SchemaMethods sm;
    private string ElemUniq;

    /// <summary>  10-Общие 20-Механические 30-Электрические </summary>
    enum Keys
    {
      Link_Instance_UniqueId = 101011101,
      Linked_Element_UniqueId = 101011102,
      Element_UniqueId = 101011103,
      Link_Name = 101011104,
      Link_Path = 101011105,
      Load_Date = 101010101,
      Linked_FamilyName = 101011106,
      Linked_Elem_Coords = 101011107
    };
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      UIApplication uiApp = commandData.Application;
      UIDocument uidoc = uiApp.ActiveUIDocument;
      Application app = uiApp.Application;
      Document doc = uidoc.Document;
      Schema sch = null;
      string SchemaGuid = "ce827518-2247-4eda-b76d-c7dfb4681f3c";
      ObjectType obt = ObjectType.Element;
      Reference refElemLinked;
     

      while (true)
      {
        try
        {
          FilteredElementCollector collector = new FilteredElementCollector(doc);
          string taskguid = "893e72a1-b208-4d12-bb26-6bcc4a444d0c";
          string famname1 = "ME_Точка_подключения_(1 фазная сеть)";
          string famname2 = "ME_Точка_подключения_(2 коннектора, 3 фазная сеть)";
          string famname3 = "ME_Точка_подключения_(3 фазная сеть)";
          FilteredElementCollector collfams = collector.OfClass(typeof(Family));
          Family fam1 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname1)) as Family;
          Family fam2 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname2)) as Family;
          Family fam3 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname3)) as Family;
          if (fam1 == null || fam2 == null || fam3 == null)
          {
            string n1, n2, n3; n1 = ""; n2 = ""; n3 = "";
            if (fam1 == null) { n1 = famname1 + "\n "; }
            if (fam2 == null) { n2 = famname2 + "\n "; }
            if (fam3 == null) { n3 = famname3 + "\n "; }
            var DiagRes = FamErrorMsg("Не загружены семейства:\n " + n1 + n3 + n2 + "\n Загрузить ?");
            if (DiagRes == true)
            {
              using (Transaction t = new Transaction(doc, "Загрузить семейство"))
              {
                t.Start();
                string path1 = @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT\02-БИБЛИОТЕКА\10-Семейства\90-Электрооборудование и освещение (ЭО)\Оборудование\ME_Точка_подключения_(1 фазная сеть).rfa";
                string path2 = @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT\02-БИБЛИОТЕКА\10-Семейства\90-Электрооборудование и освещение (ЭО)\Оборудование\ME_Точка_подключения_(2 коннектора, 3 фазная сеть).rfa";
                string path3 = @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT\02-БИБЛИОТЕКА\10-Семейства\90-Электрооборудование и освещение (ЭО)\Оборудование\ME_Точка_подключения_(3 фазная сеть).rfa";
                if (fam1 == null) { doc.LoadFamily(path1, out fam1); }
                if (fam2 == null) { doc.LoadFamily(path2, out fam2); }
                if (fam3 == null) { doc.LoadFamily(path3, out fam3); }
                t.Commit();
              }
            }
            if (DiagRes == false)
            {
              return Result.Cancelled;
            }

            //TaskDialog.Show("Предупреждение", "Не загружены семейства:\n " + n1 + n3 + n2);
            //return Result.Cancelled;
          }

          ISelectionFilter selectionFilter = new ConPickFilter(doc);
          refElemLinked = uidoc.Selection.PickObject(obt, selectionFilter, "Выберите связь");
          RevitLinkInstance linkInstance = doc.GetElement(refElemLinked.ElementId) as RevitLinkInstance;
          Document docLinked = linkInstance.GetLinkDocument();
          var checkLinkInst = false;
          var LinkUniq = linkInstance.UniqueId;
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
          var ElemsWithParam = CatsElems.Where(f => f.get_Parameter(new Guid(taskguid)) != null);
          var elems = ElemsWithParam.Where(f => f.get_Parameter(new Guid(taskguid)).AsInteger() == 1);  //фильтр по параметру "Задание ЭМ"
          InfoMsg("Связь: " + LinkName + "\n" + "Количество элементов в связи: " + elems.Count().ToString());
          ISet<ElementId> elementSet1 = fam1.GetFamilySymbolIds();
          ISet<ElementId> elementSet2 = fam2.GetFamilySymbolIds();
          ISet<ElementId> elementSet3 = fam3.GetFamilySymbolIds();
          FamilySymbol type1 = doc.GetElement(elementSet1.First()) as FamilySymbol;
          FamilySymbol type2 = doc.GetElement(elementSet2.First()) as FamilySymbol;
          FamilySymbol type3 = doc.GetElement(elementSet3.First()) as FamilySymbol;


          //sch = CreateGetSchema(SchemaGuid);
          //SchemaMethods.schema = sch;

          sm = new SchemaMethods(SchemaGuid, "Ag_Schema", "описание схемы");
          sch = sm.Schema;

          FilteredElementCollector MEcollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType();         
            var targetElems = MEcollector.Where(i => (i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname1
          || i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname3) && (string)sm.getSchemaDictValue<string>(i,"Dict_String",(int)Keys.Element_UniqueId) == i.UniqueId && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Link_Name) == LinkName);
          var targetElems2 = MEcollector.Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname2 && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Element_UniqueId) == i.UniqueId && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Link_Name) == LinkName);
          
          if (targetElems.Count() != 0 || targetElems2.Count() != 0) { checkLinkInst = true; }

          using (Transaction t = new Transaction(doc, "Размещение элементов"))
          {
            t.Start();
            type1.Activate();
            type2.Activate();
            type3.Activate();
            string poles = "d182b385-9e45-4e8b-b8da-725396848493"; // параметр "Количетсво полюсов"
            string ozk = "ce22f60b-9ae0-4c79-a624-873f39099510";   // параметр "Нормально отк/закр." для клапанов озк
            string Nname = "b4cfdcbd-5668-4572-bcd6-3d504043bd65"; // параметр "Наименование"
            string KM = "e3c1a4b0-78c8-49f5-b3c7-01869252c30e";// параметр "Коэф. мощности"
            string date = "2e2e42ce-3e29-4fac-a314-d6d5574ac27b";// параметр "Дата выгрузки"
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

            var countTarget = 0;
            foreach (Element origElement in elems)
            {
              LocationPoint pPoint = origElement.Location as LocationPoint;
              XYZ coords = pPoint.Point;
              var FamSymbol = (origElement as FamilyInstance).Symbol; // FamilySymbol элементов связи
              var linkElemUniq = origElement.UniqueId;
              var famname = origElement.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
              string typename = famname + "_/" + origElement.Name; typename = typename.Replace("[", "("); typename = typename.Replace("]", ")");
              var target = targetElems.Where(i => (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Linked_Element_UniqueId) == linkElemUniq);
              var target2 = targetElems2.Where(i => (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Linked_Element_UniqueId) == linkElemUniq);
              if (target.Count() != 0 || target2.Count() != 0) { continue; }
              if (typename.Contains("(380-2)"))
              {
                FamilySymbol Newtype = Utils.FindElementByName(doc, typeof(FamilySymbol), typename) as FamilySymbol ?? CreateNewType(type2, typename);
                var targElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
                if (checkLinkInst != false) { targElement.LookupParameter("УГО_Новый").Set(1); }
                ElemUniq = targElement.UniqueId;
                SetValueToFields(targElement, ElemUniq, linkElemUniq, LinkUniq, LinkName, LinkPath, typename, coords, sch);
                countTarget++;
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
                targElement.get_Parameter(new Guid(date)).Set(DateTime.Now.ToShortDateString());

                continue;
              }
              FamilyInstance targetElement = null;
              if (FamSymbol.get_Parameter(new Guid(poles)) != null || origElement.get_Parameter(new Guid(poles)) != null)
              {
                if (origElement.get_Parameter(new Guid(poles))?.AsInteger() == 1 || FamSymbol.get_Parameter(new Guid(poles))?.AsInteger() == 1)
                {
                  FamilySymbol Newtype = Utils.FindElementByName(doc, typeof(FamilySymbol), typename) as FamilySymbol ?? CreateNewType(type1, typename);
                  targetElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
                  targetElement.get_Parameter(new Guid(date)).Set(DateTime.Now.ToShortDateString());

                  if (checkLinkInst != false) { targetElement.LookupParameter("УГО_Новый").Set(1); }
                  ElemUniq = targetElement.UniqueId;
                  countTarget++;
                }
                if (origElement.get_Parameter(new Guid(poles))?.AsInteger() == 3 || FamSymbol.get_Parameter(new Guid(poles))?.AsInteger() == 3)
                {
                  FamilySymbol Newtype = Utils.FindElementByName(doc, typeof(FamilySymbol), typename) as FamilySymbol ?? CreateNewType(type3, typename);
                  targetElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
                  targetElement.get_Parameter(new Guid(date)).Set(DateTime.Now.ToShortDateString());
                  if (checkLinkInst != false) { targetElement.LookupParameter("УГО_Новый").Set(1); }
                  ElemUniq = targetElement.UniqueId;
                  countTarget++;
                }
              }
              else { continue; }
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
              SetValueToFields(targetElement, ElemUniq, linkElemUniq, LinkUniq, LinkName, LinkPath, typename, coords, sch);
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
            if (countTarget == 0)
            {
              WinForms.MessageBox.Show("Нет элементов для размещения!", "Предупреждение", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
              break;
            }
            InfoMsg("Элементов размещено в проекте: " + countTarget);
            //TaskDialog.Show("Информация ", "Размещено в проекте элементов: " + countTarget);
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
    //public static Schema CreateSchema(string guid)
    //{
    //  Guid schemaGuid = new Guid(guid);
    //  SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);
    //  // set read access
    //  schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    //  // set write access
    //  schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    //  // set schema name
    //  schemaBuilder.SetSchemaName("AgSchema");
    //  // set documentation
    //  //schemaBuilder.SetDocumentation(
    //  //  "Хранение ElementId элементов узлов из принципиальной схемы внутри экземпляров семейств модели");
    //  // create a field to store the bool value      
    //  FieldBuilder elemIdField = schemaBuilder.AddMapField("DictElemId", typeof(Int32), typeof(ElementId));
    //  FieldBuilder elemStringField = schemaBuilder.AddMapField("DictString", typeof(Int32), typeof(string));
    //  FieldBuilder elemIntField = schemaBuilder.AddMapField("DictInt", typeof(Int32), typeof(Int32));
    //  // register the schema
    //  Schema schema = schemaBuilder.Finish();
    //  return schema;
    //}
    public void SetValueToFields(Element e, string ElemUniq ,string linkElemUniq,string LinkUniq, string LinkName,string LinkPath,string typename,XYZ coords,Schema sch)
    {
      //Entity ent;
      //ent = e.GetEntity(sch);
      //if (ent.Schema == null)
      //{
      //  ent = new Entity(sch);
      //}
      //IDictionary<int, double> dict = new Dictionary<int, double>();
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Linked_Element_UniqueId,linkElemUniq);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Element_UniqueId, ElemUniq);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Link_Instance_UniqueId, LinkUniq);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Link_Name, LinkName);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Link_Path, LinkPath);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Linked_FamilyName, typename);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Element_UniqueId, e.UniqueId.ToString());
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Load_Date, DateTime.Now.ToShortDateString());
      sm.setValueToEntity(e, "Dict_XYZ", (int)Keys.Linked_Elem_Coords, coords);
      
      //ent.Set("Elem_Uniq", ElemUniq);
      //ent.Set("LinkElem_Uniq", linkElemUniq);
      //ent.Set("LinkInstance_Uniq", LinkUniq);
      //ent.Set("Link_Name", LinkName);
      //ent.Set("Dict_XYZ", coords, DisplayUnitType.DUT_DECIMAL_FEET);
      //e.SetEntity(ent);
    }
    public bool FamErrorMsg(string msg)
    {
      Debug.WriteLine(msg);
      var dialogResult = WinForms.MessageBox.Show(msg, "Предупреждение", WinForms.MessageBoxButtons.YesNo, WinForms.MessageBoxIcon.Warning);
      if (dialogResult == WinForms.DialogResult.Yes)
      {
        return true;
      }
      if (dialogResult == WinForms.DialogResult.No)
      {
        return false;
      }
      return true;
    }
    public static void InfoMsg(string msg)
    {
      Debug.WriteLine(msg);
      WinForms.MessageBox.Show(msg, "Информация", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
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

    public Schema CreateGetSchema(string sGuid)
    {
      Schema sch = Schema.Lookup(new Guid(sGuid));
      if (sch != null) { return sch; }
      else
      {
        SchemaBuilder sb = new SchemaBuilder(new Guid(sGuid));
        sb.SetReadAccessLevel(AccessLevel.Public);
        sb.SetWriteAccessLevel(AccessLevel.Public);
        sb.AddSimpleField("Elem_Uniq",typeof(string));
        sb.AddSimpleField("LinkElem_Uniq", typeof(string));
        sb.AddSimpleField("LinkInstance_Uniq", typeof(string));
        sb.AddSimpleField("Link_Name", typeof(string));
        //FieldBuilder fbString = sb.AddSimpleField("Load_Date", typeof(string));
        //FieldBuilder fbString = sb.AddSimpleField("Link_Path", typeof(string));
        FieldBuilder fbXYZ = sb.AddSimpleField("Dict_XYZ", typeof(XYZ));
        fbXYZ.SetUnitType(UnitType.UT_Length);
        sb.SetSchemaName("Ag_Schema");
        sch = sb.Finish();
        return sch;
      }
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

