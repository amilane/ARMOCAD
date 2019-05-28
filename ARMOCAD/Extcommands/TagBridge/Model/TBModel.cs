using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  [Transaction(TransactionMode.Manual)]
  [Regeneration(RegenerationOption.Manual)]
  class TBModel
  {
    // Just like what you do when creating a Revit command, declare the necessary variable such as below.
    private static UIApplication UIAPP = null;
    private static Application APP = null;
    private static UIDocument UIDOC = null;
    private static Document DOC = null;

    public Schema schema = null;
    public ObservableCollection<TagItem> TagItems;
    public string ProjectName;

    public IEnumerable<Element> elems;

    private Element eModel;
    public Element EModel {
      get { return eModel; }
      set { eModel = value; }
    }

    private Element eDraft;
    public Element EDraft {
      get { return eDraft; }
      set { eDraft = value; }
    }

    private Element eModelDelDraft;
    public Element EModelDelDraft {
      get { return eModelDelDraft; }
      set { eModelDelDraft = value; }
    }


    private TagItem newTag;
    public TagItem NewTag {
      get { return newTag; }
      set { newTag = value; }
    }

    private bool isTwoElementsSelected = true;
    public bool IsTwoElementsSelected {
      get { return isTwoElementsSelected; }
      set { isTwoElementsSelected = value; }
    }


    public TBModel(UIApplication uiapp)
    {
      UIAPP = uiapp;
      APP = UIAPP.Application;
      UIDOC = UIAPP.ActiveUIDocument;
      DOC = UIDOC.Document;
      ProjectName = DOC.Title;

      if (SchemaExist("AgSchema"))
      {
        schema = GetSchema("AgSchema");
      }

      if (schema == null)
      {
        schema = CreateSchema();
      }

      SchemaMethods.schema = schema;

      IEnumerable<Element> terminal = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DuctTerminal)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> ductAccessory = new FilteredElementCollector(DOC)
        .OfCategory(BuiltInCategory.OST_DuctAccessory)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> pipeAccessory = new FilteredElementCollector(DOC)
        .OfCategory(BuiltInCategory.OST_PipeAccessory)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> equipment = new FilteredElementCollector(DOC)
        .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
        .WhereElementIsNotElementType()
        .ToElements();
      elems = terminal.Union(ductAccessory).Union(pipeAccessory).Union(equipment);

      TagItems = tagListData(elems);
    }

    public static bool SchemaExist(string schemaName)
    {
      bool result = false;
      if (GetSchema(schemaName) != null)
      {
        result = true;
      }

      return result;
    }

    public static Schema GetSchema(string schemaName)
    {
      Schema schema = null;
      IList<Schema> schemas = Schema.ListSchemas();
      if (schemas != null && schemas.Count > 0)
      {
        // get schema
        foreach (Schema s in schemas)
        {
          if (s.SchemaName == schemaName)
          {
            schema = s;
            break;
          }
        }
      }

      return schema;
    }

    public static Schema CreateSchema()
    {
      Guid schemaGuid = new Guid("ce6a412e-1e20-4ac3-a081-0a6bde126466");

      SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

      // set read access
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);

      // set write access
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);

      // set schema name
      schemaBuilder.SetSchemaName("AgSchema");

      // set documentation
      schemaBuilder.SetDocumentation(
        "Хранение ElementId элементов узлов из принципиальной схемы внутри экземпляров семейств модели");

      // create a field to store the bool value
      FieldBuilder elemIdField = schemaBuilder.AddMapField("DictElemId", typeof(Int32), typeof(ElementId));
      FieldBuilder elemStringField = schemaBuilder.AddMapField("DictString", typeof(Int32), typeof(string));
      FieldBuilder elemIntField = schemaBuilder.AddMapField("DictInt", typeof(Int32), typeof(Int32));

      // register the schema
      Schema schema = schemaBuilder.Finish();

      return schema;
    }

    // создание первоначального списка элементов
    public ObservableCollection<TagItem> tagListData(IEnumerable<Element> elems)
    {
      ObservableCollection<TagItem> tagItems = new ObservableCollection<TagItem>();

      foreach (var e in elems)
      {
        string modelTag = e.LookupParameter("TAG")?.AsString();

        TagItem t = new TagItem();
        t.ModelId = e.Id;
        t.ModelTag = modelTag;

        setDraftInfoToTagItem(t, e);

        tagItems.Add(t);
      }

      
      return tagItems;

    }

    //собирает 2 элемента, делает из них новый tagItem
    public void getTwoElements()
    {
      ICollection<ElementId> selectedIds = UIDOC.Selection.GetElementIds();

      List<Element> elems = new List<Element>();
      if (selectedIds.Count != 2)
      {

        IsTwoElementsSelected = false;
        NewTag = null;
      }
      else
      {
        foreach (var i in selectedIds)
        {
          elems.Add(DOC.GetElement(i));
        }

        if (elems.Any(i => i.Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents) &&
            elems.Any(i => i.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents))
        {
          IsTwoElementsSelected = true;

          EModel = elems.Where(i => i.Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents).First();
          EDraft = elems.Where(i => i.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents).First();

          TagItem tag = new TagItem();
          tag.ModelId = EModel.Id;
          tag.DraftId = EDraft.Id;
          string tagValue = EModel.LookupParameter("TAG").AsString();
          tag.ModelTag = tagValue;
          tag.DraftTag = tagValue;

          NewTag = tag;
        }
        else
        {
          IsTwoElementsSelected = false;
        }

      }


    }



    public void setDraftInfoToTagItem(TagItem t, Element e)
    {
      string draftTag = String.Empty;

      ElementId draftId = SchemaMethods.getSchemaDictValue<ElementId>(e, "DictElemId", 0) as ElementId;

      if (draftId != null && draftId.IntegerValue != -1)
      {
        draftTag = DOC.GetElement(draftId).LookupParameter("TAG")?.AsString();
      }

      t.DraftId = draftId;
      t.DraftTag = draftTag;
    }


    //проверка, есть ли в модели уже где-то назначенный данный EDraft, если есть, то удаляем оттуда ElementId
    //чтобы в модели не оказывалось множественного назначения одного элемента узла разным экземплярям семейств
    public void getElementForDeletingDraftId()
    {
      var x = elems.Where(e => SchemaMethods.getSchemaDictValue<ElementId>(e, "DictElemId", 0) as ElementId == EDraft.Id);

      if (x.Count() != 0)
      {
        EModelDelDraft = x.First();
      }
    }




  }
}
