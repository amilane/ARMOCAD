using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IronPython.Modules;
using ComboBox = System.Windows.Forms.ComboBox;
using Form = System.Windows.Forms.Form;

namespace ARMOCAD
{
  public class PlaceEquip
  {
    public Result res;
    public string linkN;
    public PlaceEquip(Document _doc,string _linkN)
    {
      linkN = _linkN;
      doc = _doc;
      Run();
    }

    public Document doc;
    public SchemaMethods sm;
    private string ElemUniq;
    public bool WSCheck;
    private string linkName = String.Empty;

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
      ["Тип подключения"] = "d512be5c-4315-4b86-aad1-74e7648760ef"
    };
    public Result Run()
    {
      Schema sch = null;
      string SchemaGuid = "ce827518-2247-4eda-b76d-c7dfb4681f3c";

      FilteredElementCollector collector = new FilteredElementCollector(doc);
      string famname1 = "Задание для СС";
      FilteredElementCollector collfams = collector.OfClass(typeof(Family));

      Family fam1 = collfams.FirstOrDefault<Element>(e => e.Name.Equals(famname1)) as Family;
      if (fam1 == null)
      {
        string path1 =
          @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT\02-БИБЛИОТЕКА\10-Семейства\70-Слаботочные системы (СС)\Оборудование\Задание для СС.rfa";
        var tdRes = TDFamLoad(path1, famname1);
        if (tdRes == TaskDialogResult.Yes)
        {
          using (Transaction t = new Transaction(doc, "Загрузить семейство"))
          {
            t.Start();

            if (fam1 == null)
            {
              doc.LoadFamily(path1, out fam1);
            }

            t.Commit();
          }
        }

        if (tdRes == TaskDialogResult.No)
        {
          return Result.Failed;
        }

        if (tdRes == TaskDialogResult.Close)
        {
          return Result.Failed;
        }
      }

      IList<Element> links = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks)
        .WhereElementIsNotElementType().ToElements();
      if (linkN == null)
      {
        linkName = SelectLink(links);
      }
      else
      {
        linkName = linkN;
      }
      var link = links.Where(i => i.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString() == linkName);
      RevitLinkInstance linkInstance = link.First() as RevitLinkInstance;
      Document docLinked = linkInstance.GetLinkDocument();
      var checkLinkInst = false;
      var LinkUniq = linkInstance.UniqueId; //UniqId экземпляра связи
      var LinkName = docLinked.Title; //Имя связи
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

      IEnumerable<Element> elems = CatsElems.Where(f => f.get_Parameter(new Guid(param["Задание СС"])) != null && f.get_Parameter(new Guid(param["Задание СС"])).AsInteger() == 1);

      ISet<ElementId> elementSet1 = fam1.GetFamilySymbolIds();
      FamilySymbol type1 = doc.GetElement(elementSet1.First()) as FamilySymbol;
      sm = new SchemaMethods(SchemaGuid, "Ag_Schema"); //создание схемы ExStorage
      sch = sm.Schema;
      FilteredElementCollector MEcollector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType();
      var targetElems = MEcollector.Where(i =>
        i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == famname1 &&
        (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Element_UniqueId) == i.UniqueId &&
        (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Link_Name) == LinkName); // коллектор по UniqId элемента и имени связи 
      if (targetElems.Count() != 0)
      {
        checkLinkInst = true;
      } //проверка новый ли это элемент, если новый то пишем в параметр

      var Res = TDInfo(elems, LinkName);
      if (Res == TaskDialogResult.Cancel)
      {
        return Result.Failed;
      }
      if (WSCheck == true)
      {
        checkLinkInst = false;
      }
      using (Transaction t = new Transaction(doc, "Размещение элементов"))
      {
        t.Start();
        type1.Activate();

        var countTarget = 0; //количество размещаемых элементов
        foreach (Element origElement in elems) //перебираем элементы из связи
        {
          LocationPoint pPoint = origElement.Location as LocationPoint;
          XYZ coords = pPoint.Point; //координаты экземпляра в связи
          double rotation = pPoint.Rotation;
          var FamSymbol = (origElement as FamilyInstance).Symbol; // FamilySymbol элементов связи
          var linkElemUniq = origElement.UniqueId;
          var famname = origElement.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString(); //имя семейства в связи
          var typename = famname + "_/_" + origElement.Name;typename = typename.Replace("[", "(");typename = typename.Replace("]", ")"); // имя семейства + имя типоразмера (заменяем скобки []) 
          if (origElement.LookupParameter("Щит аварийного освещения") != null &&origElement.LookupParameter("Щит аварийного освещения").AsInteger() == 1)
          {
            typename = typename + "/Щит аварийного освещения";
          }
          if (origElement.LookupParameter("Щит управления двигателями") != null &&origElement.LookupParameter("Щит управления двигателями").AsInteger() == 1)
          {
            typename = typename + "/Щит управления двигателями";
          }
          if (origElement.LookupParameter("Щит силовой распределительный") != null &&origElement.LookupParameter("Щит силовой распределительный").AsInteger() == 1)
          {
            typename = typename + "/Щит силовой распределительный";
          }
          var target = targetElems.Where(i => (string)sm.getSchemaDictValue<string>(i, "Dict_String", (int)Keys.Linked_Element_UniqueId) == linkElemUniq); //коллектор по совпадающим UniqId (1 ТП)
          if (target.Count() != 0)
          {
            continue;
          } // проверка по UniqID в связи и в проекте

          FamilySymbol Newtype = Util.GetFamilySymbolByName(doc, typename) as FamilySymbol ?? CreateNewType(type1, typename); //проверка есть ли типоразмер в проекте если нет создаем
          //var targetElement = doc.Create.NewFamilyInstance(coords, Newtype, StructuralType.NonStructural);
          XYZ point2 = new XYZ(coords.X, coords.Y, coords.Z + 100);
          Line axis = Line.CreateBound(coords, point2);
          FamilyInstance ins = origElement as FamilyInstance;
          var tr = ins.GetTotalTransform();
          var basX1 = tr.BasisX.X;
          var basX2 = tr.BasisX.Y;
          var basX3 = tr.BasisX.Z;
          var basY1 = tr.BasisY.X;
          var basY2 = tr.BasisY.Y;
          var basY3 = tr.BasisY.Z;
          var basZ1 = tr.BasisZ.X;
          var basZ2 = tr.BasisZ.Y;
          var basZ3 = tr.BasisZ.Z;
          XYZ liner = new XYZ(basX1, basX2, 0);
          if (basZ1 != 0 || basZ2 != 0)
          {
            liner = new XYZ(basZ1, basZ2, 0);
          }
          var targetElement = doc.Create.NewFamilyInstance(coords,Newtype,liner,origElement,StructuralType.NonStructural);
          countTarget++;
          if (checkLinkInst != false)
          {
            targetElement.get_Parameter(new Guid(param["Новый"])).Set(1);
          } //проверка новый ли элемент, если новый то пишем в параметр

          ElemUniq = targetElement.UniqueId;
          SetValueToFields(targetElement, ElemUniq, linkElemUniq, LinkUniq, LinkName, LinkPath, typename, coords,sch); //запись параметров в ExStorage                                
          SetParameters(origElement, targetElement, LinkName); //запись параметров в Instance 
          NameSystemParameter(origElement, targetElement);
          Ozk(origElement, targetElement); //уго озк кду
          GSymbol(typename, "Электрооборудование", "Шкаф", origElement, targetElement);
          GSymbol(typename, "КСК", "УГО_КСК", targetElement);
          GSymbol(typename, "ШПК", "УГО_ПК", targetElement);
          GSymbol(typename, "СОУЭ", "УГО_СОУЭ", targetElement);
          GSymbol(typename, "СПЖ", "УГО_СПЖ", targetElement);
          GSymbol(typename, "ЦПИ", "УГО_ЦПИ", targetElement);
          GSymbol(typename, "Щит_автоматики", "УГО_ЩУ", targetElement);
          GSymbol(typename, "РП", "УГО_СПЖ", targetElement);
        }

        t.Commit();
        if (countTarget == 0)
        {
          TaskDialog tdPr = new TaskDialog("Предупреждение");
          tdPr.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
          tdPr.TitleAutoPrefix = false;
          tdPr.AllowCancellation = true;
          tdPr.MainInstruction = "Нет элементов для размещения!";
          tdPr.CommonButtons = TaskDialogCommonButtons.Close;
          tdPr.DefaultButton = TaskDialogResult.Close;
          TaskDialogResult tdRes = tdPr.Show();
          return Result.Succeeded;
        }
        else
        {
          TaskDialog end = new TaskDialog("Информация");
          end.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
          end.MainInstruction = "Элементов размещено в проекте: " + countTarget;
          end.TitleAutoPrefix = false;
          end.Show();
          return Result.Succeeded;
        }
      }
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
        if (orig.LookupParameter("Щит аварийного освещения") != null &&
            orig.LookupParameter("Щит аварийного освещения").AsInteger() == 1)
        {
          target.Symbol.LookupParameter("УГО_ЩАО").Set(1);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }

        if (orig.LookupParameter("Щит управления двигателями") != null &&
            orig.LookupParameter("Щит управления двигателями").AsInteger() == 1)
        {
          target.Symbol.LookupParameter("УГО_ЩУ").Set(1);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }

        if (orig.LookupParameter("Щит силовой распределительный") != null &&
            orig.LookupParameter("Щит силовой распределительный").AsInteger() == 1)
        {
          target.Symbol.LookupParameter("УГО_ЩР").Set(1);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
      }
    }

    private void SetParameters(Element orig, FamilyInstance target, string Linkname)
    {
      var FamSymbol = (orig as FamilyInstance).Symbol;
      target.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS)
        .Set(FamSymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString());
      target.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK)
        .Set(FamSymbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString());
      target.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
        .Set(FamSymbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString());
      target.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
        .Set(orig.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
      target.get_Parameter(BuiltInParameter.DOOR_NUMBER)
        .Set(orig.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString());
      target.get_Parameter(new Guid(param["Наименование"])).Set(orig.Name); //название типа в параметр "наименование"
      target.get_Parameter(new Guid(param["Дата выгрузки"])).Set(DateTime.Now.ToShortDateString()); //дата 
      target.get_Parameter(new Guid(param["Связь"])).Set(Linkname);
      target.get_Parameter(new Guid(param["Этаж"]))
        .Set(orig.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsValueString());
      if (orig.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_FEED_PARAM) != null &&
          orig.get_Parameter(BuiltInParameter.RBS_ELEC_PANEL_FEED_PARAM).AsString() != "")
      {
        target.get_Parameter(new Guid(param["Тип подключения"]))
          .Set(orig.get_Parameter(new Guid(param["Тип подключения"])).AsString());
      }

      SetParameterToInstance(param["OUT"], orig, target);
      SetParameterToType(param["OUT"], orig, target);
      SetParameterToInstance(param["IN"], orig, target);
      SetParameterToType(param["IN"], orig, target);
    }

    public static void SetParameterToInstance(string guid, Element orig, FamilyInstance target)
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

    public static void SetParameterToType(string guid, Element orig, FamilyInstance target)
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

    private void SetValueToFields(Element e, string ElemUniq, string linkElemUniq, string LinkUniq, string LinkName,
      string LinkPath, string typename, XYZ coords, Schema sch)
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
          target.LookupParameter("УГО_НЗ").Set(1);
          target.LookupParameter("УГО_НО").Set(0);//для НЗ
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }

        if (origPar == "НО")
        {
          target.LookupParameter("УГО_НО").Set(1); //для НО
          target.LookupParameter("УГО_НЗ").Set(0);
          target.Symbol.LookupParameter("УГО_ТП").Set(0);
        }
      }
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

    public static string SelectLink(IList<Element> links)
    {
      IList<string> Names = new List<string>();
      foreach (var l in links)
      {
        Names.Add(l.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString());
      }

      Form form1 = new Form();
      GroupBox groupbox = new GroupBox();
      ComboBox comboBox1 = new ComboBox();
      //Button button1 = new Button();
      Button button2 = new Button();
      Label label1 = new Label();
      // 
      // groupbox
      // 
      groupbox.Controls.Add(label1);
      groupbox.Controls.Add(button2);
      //groupbox.Controls.Add(button1);
      groupbox.Controls.Add(comboBox1);
      groupbox.Location = new System.Drawing.Point(12, 12);
      groupbox.Name = "groupbox";
      groupbox.Size = new System.Drawing.Size(339, 178);
      groupbox.TabIndex = 1;
      groupbox.TabStop = false;
      groupbox.Text = "Связанные файлы Revit";
      // 
      // comboBox1
      // 
      comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBox1.FormattingEnabled = true;
      comboBox1.Location = new System.Drawing.Point(30, 77);
      comboBox1.Name = "comboBox1";
      comboBox1.Size = new System.Drawing.Size(287, 21);
      comboBox1.TabIndex = 0;
      comboBox1.DataSource = Names;
      // 
      // button1
      // 
      //button1.Location = new System.Drawing.Point(146, 149);
      //button1.Text = "ОК";
      // 
      // button2
      // 
      button2.Location = new System.Drawing.Point(242, 149);
      button2.Text = "OK";
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(27, 47);
      label1.Size = new System.Drawing.Size(93, 13);
      label1.TabIndex = 3;
      label1.Text = "Выберите связь:";
      // 
      // Form1
      // 
      form1.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      form1.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      form1.ClientSize = new System.Drawing.Size(363, 202);
      form1.Controls.Add(groupbox);
      form1.Text = "Form1";
      groupbox.ResumeLayout(false);
      groupbox.PerformLayout();
      form1.ResumeLayout(false);
      form1.Text = "Размещение из связи";
      form1.FormBorderStyle = FormBorderStyle.FixedDialog;
      form1.MaximizeBox = false;
      form1.MinimizeBox = false;
      form1.ControlBox = false;
      form1.AcceptButton = button2;
      form1.CancelButton = button2;
      form1.StartPosition = FormStartPosition.CenterScreen;
      form1.ShowDialog();
      string res = comboBox1.Text;
      return res;
    }

    private TaskDialogResult TDFamLoad(string path1, string famname1)
    {
      TaskDialog tdLoad = new TaskDialog("Предупреждение");
      tdLoad.MainIcon = TaskDialogIcon.TaskDialogIconError;
      tdLoad.Title = "Предупреждение";
      tdLoad.TitleAutoPrefix = false;
      tdLoad.AllowCancellation = false;
      tdLoad.MainInstruction = "Не загружено семейство:\n " + "[" + famname1 + "]" + "\n\n Загрузить ?";
      tdLoad.FooterText = path1.Substring(44);
      tdLoad.CommonButtons = TaskDialogCommonButtons.No | TaskDialogCommonButtons.Yes;
      tdLoad.DefaultButton = TaskDialogResult.Yes;
      TaskDialogResult tdRes = tdLoad.Show();
      return tdRes;
    }

    private TaskDialogResult TDInfo(IEnumerable<Element> elements, string LinkName)
    {
      string families = string.Empty;
      TaskDialog tdinfo = new TaskDialog("Информация");
      tdinfo.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
      tdinfo.Title = "Информация";
      tdinfo.TitleAutoPrefix = false;
      tdinfo.AllowCancellation = true;
      tdinfo.MainInstruction = "Связь: " + LinkName;
      tdinfo.MainContent = "Количество элементов в связи: " + elements.Count().ToString();
      foreach (var e in elements)
      {
        var fname = e.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
        if (families.Contains(fname))
        {
          continue;
        }
        families = families + fname + "\n";
      }

      tdinfo.ExpandedContent = "Семейства: \n" + families;
      tdinfo.VerificationText = "Не отмечать новые";
      tdinfo.CommonButtons = TaskDialogCommonButtons.Cancel | TaskDialogCommonButtons.Ok;
      tdinfo.DefaultButton = TaskDialogResult.Ok;
      TaskDialogResult Res = tdinfo.Show();
      WSCheck = tdinfo.WasVerificationChecked();
      return Res;
    }
  }
}

