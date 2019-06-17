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

  public class LinkEquipmentSS : IExternalCommand
  {
    public SchemaMethods sm;
    private string ElemUniq;
    Dictionary<string, string> param = new Dictionary<string, string>
    {
      ["Нормально отк/закр."] = "ce22f60b-9ae0-4c79-a624-873f39099510",
      ["Наименование"] = "b4cfdcbd-5668-4572-bcd6-3d504043bd65",
      ["Дата выгрузки"] = "2e2e42ce-3e29-4fac-a314-d6d5574ac27b",
      ["Задание СС"] = "d30b8343-4a2d-4457-9137-e34e511d7233",
      ["Новый"] = "bf28d2b7-3b97-4f90-8c2a-590a92a654c6",
      ["Номер"] = "90afe9a6-6c85-4aab-a765-f8743d986dc0",
      ["Связь"] = "41a4f06f-583e-4743-ac62-6a5b17db8cb4",
      ["Этаж"] = "4857fa3b-e80e-4167-9b66-f40cd5992680",
      ["Имя Системы"] = "303f67e6-3fd6-469b-9356-dccb116a3277",
      ["OUT"] = "478914c0-6c06-4dd6-8c41-fa1122140e87",
      ["IN"] = "cf610632-14a9-4c8d-84ae-79053ba99593",
      ["Питание"]= "b502ddde-99e5-4495-b855-e784100376d9"
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
          string famname1 = "Задание для СС";
          FilteredElementCollector collfams = collector.OfClass(typeof(Family));
          Family fam1 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname1)) as Family;
          if (fam1 == null)
          {
            var DiagRes = FamErrorMsg("Не загружено семейство:\n " + famname1 + "\n\n Загрузить ?");
            if (DiagRes == true)
            {
              using (Transaction t = new Transaction(doc, "Загрузить семейство"))
              {
                t.Start();
                string path1 = @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT\02-БИБЛИОТЕКА\10-Семейства\70-Слаботочные системы (СС)\Оборудование\Задание для СС.rfa";
                if (fam1 == null) { doc.LoadFamily(path1, out fam1); }
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
          ISelectionFilter selectionFilter = new LinkPickFilter(doc);
          refElemLinked = uidoc.Selection.PickObject(obt, selectionFilter, "Выберите связь");
          RevitLinkInstance linkInstance = doc.GetElement(refElemLinked.ElementId) as RevitLinkInstance;
          Document docLinked = linkInstance.GetLinkDocument();
          var checkLinkInst = false;
          var LinkUniq = linkInstance.UniqueId; //UniqId экземпляра связи
          var LinkName = docLinked.Title;  //Имя связи
          var LinkPath = docLinked.PathName; //Путь к связи
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
          var elems = CatsElems.Where(f => f.get_Parameter(new Guid(param["Задание СС"])) != null && f.get_Parameter(new Guid(param["Задание СС"])).AsInteger() == 1);
          InfoMsg("Связь: " + LinkName + "\n" + "Количество элементов в связи: " + elems.Count().ToString()); // MessageBox
          ISet<ElementId> elementSet1 = fam1.GetFamilySymbolIds();
          FamilySymbol type1 = doc.GetElement(elementSet1.First()) as FamilySymbol;
          sm = new SchemaMethods(SchemaGuid, "Ag_Schema"); //создание схемы ExStorage
          sch = sm.Schema;
          FilteredElementCollector MEcollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType();
          var targetElems = MEcollector.Where(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname1 && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Element_UniqueId) == i.UniqueId && (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Link_Name) == LinkName); // коллектор по UniqId элемента и имени связи для 1 ТП
          if (targetElems.Count() != 0) { checkLinkInst = true; }  //проверка новый ли это элемент, если новый то пишем в параметр
          using (Transaction t = new Transaction(doc, "Размещение элементов"))
          {
            t.Start();
            type1.Activate();
            var countTarget = 0; //количество размещаемых элементов
            foreach (Element origElement in elems) //перебираем элементы из связи
            {
              LocationPoint pPoint = origElement.Location as LocationPoint;
              XYZ coords = pPoint.Point;  //координаты экземпляра в связи
              var FamSymbol = (origElement as FamilyInstance).Symbol; // FamilySymbol элементов связи
              var linkElemUniq = origElement.UniqueId;
              var famname = origElement.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString(); //имя семейства в связи
              var typename = famname + "_/_" + origElement.Name; typename = typename.Replace("[", "("); typename = typename.Replace("]", ")");// имя семейства + имя типоразмера (заменяем скобки []) 
              var target = targetElems.Where(i => (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Linked_Element_UniqueId) == linkElemUniq); //коллектор по совпадающим UniqId (1 ТП)
              if (target.Count() != 0) { continue; } // проверка по UniqID в связи и в проекте
              FamilySymbol Newtype = Util.GetFamilySymbolByName(doc, typename) as FamilySymbol ?? CreateNewType(type1, typename);//проверка есть ли типоразмер в проекте если нет создаем
              var targetElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
              countTarget++;
              if (checkLinkInst != false) { targetElement.get_Parameter(new Guid(param["Новый"])).Set(1); } //проверка новый ли элемент, если новый то пишем в параметр
              ElemUniq = targetElement.UniqueId;
              SetValueToFields(targetElement, ElemUniq, linkElemUniq, LinkUniq, LinkName, LinkPath, typename, coords, sch); //запись параметров в ExStorage                                
              SetParameters(origElement, targetElement, LinkName);//запись параметров в Instance 
              NameSystemParameter(origElement,targetElement);
              Ozk(origElement, targetElement); //уго озк кду
              GSymbol(typename, "Электрооборудование", "Шкаф", origElement, targetElement);
              GSymbol(typename, "КСК", "УГО_КСК", targetElement);
              GSymbol(typename, "ПК", "УГО_ПК)", targetElement);
              GSymbol(typename, "СОУЭ", "УГО_СОУЭ", targetElement);
              GSymbol(typename, "СПЖ", "УГО_СПЖ", targetElement);
              GSymbol(typename, "ЦПИ", "УГО_ЦПИ", targetElement);
              GSymbol(typename, "Щит_автоматики", "УГО_ЩУ",targetElement);
            }
            t.Commit();
            if (countTarget == 0)
            {
              WinForms.MessageBox.Show("Нет элементов для размещения!", "Предупреждение", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
              break;
            }
            InfoMsg("Элементов размещено в проекте: " + countTarget);
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
    private void GSymbol(string type, string check, string param1, FamilyInstance target)
    {
      if (type.Contains(check))
      {
        target.Symbol.LookupParameter(param1).Set(1);
        target.Symbol.LookupParameter("УГО_ТП").Set(0);
      }
    }
    private void GSymbol(string type, string check, string check2, Element orig, FamilyInstance target)
    {
      if (type.Contains(check) && type.Contains(check2))
      {
        if (orig.LookupParameter("Щит аварийного освещения") != null && orig.LookupParameter("Щит аварийного освещения").AsInteger() == 1)
        {
          target.Symbol.LookupParameter("УГО_ЩАО").Set(1);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
        if (orig.LookupParameter("Щит управления двигателями") != null && orig.LookupParameter("Щит управления двигателями").AsInteger() == 1)
        {
          target.Symbol.LookupParameter("УГО_ЩУ").Set(1);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
        if (orig.LookupParameter("Щит силовой распределительный") != null && orig.LookupParameter("Щит силовой распределительный").AsInteger() == 1)
        {
          target.Symbol.LookupParameter("УГО_ЩР").Set(1);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
      }
    }
    private void SetParameters(Element orig, FamilyInstance target, string Linkname)
    {
      var FamSymbol = (orig as FamilyInstance).Symbol;
      target.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).Set(FamSymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
      target.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(orig.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
      target.get_Parameter(new Guid(param["Наименование"])).Set(orig.Name); //название типа в параметр "наименование"
      target.get_Parameter(new Guid(param["Дата выгрузки"])).Set(DateTime.Now.ToShortDateString()); //дата 
      target.get_Parameter(new Guid(param["Связь"])).Set(Linkname);
      target.get_Parameter(new Guid(param["Этаж"])).Set(orig.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsValueString());
      if (orig.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_FEED_PARAM) != null && orig.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_FEED_PARAM).AsString() != "")
      {
        target.get_Parameter(new Guid(param["Питание"])).Set(orig.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_FEED_PARAM).AsString());
      }
      SetParameterToInstance(param["OUT"], orig, target);
      SetParameterToType(param["OUT"], orig, target);
      SetParameterToInstance(param["IN"], orig, target);
      SetParameterToType(param["IN"], orig, target);
    }
    private void SetParameterToInstance(string guid, Element orig, FamilyInstance target)
    {
      if (orig.get_Parameter(new Guid(guid)) != null)
      {
        StorageType storageType = orig.get_Parameter(new Guid(guid)).StorageType;
        switch (storageType)
        {
          case StorageType.Double:
            target.get_Parameter(new Guid(guid)).Set(orig.get_Parameter(new Guid(guid)).AsDouble());
            break;
          case StorageType.String:
            target.get_Parameter(new Guid(guid)).Set(orig.get_Parameter(new Guid(guid)).AsString());
            break;
          case StorageType.Integer:
            target.get_Parameter(new Guid(guid)).Set(orig.get_Parameter(new Guid(guid)).AsInteger());
            break;
        }
      }
    }
    private void SetParameterToType(string guid, Element orig, FamilyInstance target)
    {
      var FamSymbol = (orig as FamilyInstance).Symbol;
      if (FamSymbol.get_Parameter(new Guid(guid)) != null)
      {
        StorageType storageType = orig.get_Parameter(new Guid(guid)).StorageType;
        switch (storageType)
        {
          case StorageType.Double:
            target.get_Parameter(new Guid(guid)).Set(FamSymbol.get_Parameter(new Guid(guid)).AsDouble());
            break;
          case StorageType.String:
            target.get_Parameter(new Guid(guid)).Set(FamSymbol.get_Parameter(new Guid(guid)).AsString());
            break;
          case StorageType.Integer:
            target.get_Parameter(new Guid(guid)).Set(FamSymbol.get_Parameter(new Guid(guid)).AsInteger());
            break;
        }
      }
    }
    public void SetValueToFields(Element e, string ElemUniq, string linkElemUniq, string LinkUniq, string LinkName, string LinkPath, string typename, XYZ coords, Schema sch)
    {
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Linked_Element_UniqueId, linkElemUniq);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Element_UniqueId, ElemUniq);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Link_Instance_UniqueId, LinkUniq);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Link_Name, LinkName);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Link_Path, LinkPath);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Linked_FamilyName, typename);
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Element_UniqueId, e.UniqueId.ToString());
      sm.setValueToEntity(e, "Dict_String", (int)Keys.Load_Date, DateTime.Now.ToShortDateString());
      sm.setValueToEntity(e, "Dict_XYZ", (int)Keys.Linked_Elem_Coords, coords);
    }
    private void Ozk(Element orig, FamilyInstance target)
    {
      if (orig.get_Parameter(new Guid(param["Нормально отк/закр."])) != null) //УГО для клапанов ОЗК
      {
        var origPar = orig.get_Parameter(new Guid(param["Нормально отк/закр."])).AsString();
        target.get_Parameter(new Guid(param["Нормально отк/закр."])).Set(origPar);
        if (origPar == "НЗ")
        {
          target.Symbol.LookupParameter("УГО_НЗ").Set(1); //для НЗ
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
        if (origPar == "НО")
        {
          target.Symbol.LookupParameter("УГО_НО").Set(1); //для НО
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
      }
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
    private void NameSystemParameter(Element origElement, Element targetElement)
    {
      if (origElement.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM) != null)
      {
        var origParam = origElement.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
        targetElement.get_Parameter(new Guid(param["Имя Системы"])).Set(origParam);
      }
      if (origElement.get_Parameter(new Guid(param["Имя Системы"])) != null)
      {
        var origParam = origElement.get_Parameter(new Guid(param["Имя Системы"])).AsString();
        targetElement.get_Parameter(new Guid(param["Имя Системы"])).Set(origParam);
      }
    }
    public static FamilySymbol CreateNewType(FamilySymbol Type, string Typename)
    {
      FamilySymbol newtype = Type.Duplicate(Typename) as FamilySymbol;
      return newtype;
    }
  }
}
