using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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


    private TagItem newTag;
    public TagItem NewTag {
      get { return newTag; }

      set { newTag = value; }
    }


    public TBModel(UIApplication uiapp)
    {
      UIAPP = uiapp;
      APP = UIAPP.Application;
      UIDOC = UIAPP.ActiveUIDocument;
      DOC = UIDOC.Document;
      ProjectName = DOC.Title;

      if (SchemaExist("TagBridgeSchema"))
      {
        schema = GetSchema("TagBridgeSchema");
      }

      if (schema == null)
      {
        schema = CreateSchema();
      }

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
      IEnumerable<Element> elems = terminal.Union(ductAccessory).Union(pipeAccessory).Union(equipment);

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
      Guid schemaGuid = new Guid("3d47d6ed-2bee-414e-a76e-ff9d38829bf2");

      SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

      // set read access
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);

      // set write access
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);

      // set schema name
      schemaBuilder.SetSchemaName("TagBridgeSchema");

      // set documentation
      schemaBuilder.SetDocumentation(
        "Хранение ElementId элементов узлов из принципиальной схемы внутри экземпляров семейств модели");

      // create a field to store the bool value
      FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("DraftElemFromScheme", typeof(ElementId));

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

        if (!string.IsNullOrWhiteSpace(modelTag))
        {
          TagItem t = new TagItem();
          t.ModelId = e.Id;
          t.ModelTag = modelTag;

          setDraftInfoToTagItem(t, e);

          tagItems.Add(t);
        }
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
        TaskDialog.Show("Ошибка", "Выберите 2 элемента.");
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
          TaskDialog.Show("Ошибка", "Выберите 1 элемент модели и 1 элемент узла на чертежном виде.");
        }

      }


    }


    public void checkTagList(TagItem t)
    {
      ElementId elId = t.ModelId;
      Element e = DOC.GetElement(elId);

      t.ModelTag = elId.ToString();
      //setDraftInfoToTagItem(t, e);

    }


    public void setDraftInfoToTagItem(TagItem t, Element e)
    {
      string draftTag = String.Empty;
      ElementId draftId = null;

      var ent = e.GetEntity(schema);
      if (ent.Schema != null)
      {
        draftId = ent.Get<ElementId>("DraftElemFromScheme");
        if (draftId != null && draftId.IntegerValue != -1)
        {
          draftTag = DOC.GetElement(draftId).LookupParameter("TAG")?.AsString();
        }
      }

      t.DraftId = draftId;
      t.DraftTag = draftTag;







    }




  }
}
