using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  class TBModel
  {
    // Just like what you do when creating a Revit command, declare the necessary variable such as below.
    private static UIApplication UIAPP = null;
    private static Application APP = null;
    private static UIDocument UIDOC = null;
    private static Document DOC = null;

    private static Schema schema = null;
    public ObservableCollection<TagItem> TagItems;
    public string ProjectName;

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
      IEnumerable<Element> ductAccessory = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DuctAccessory)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> pipeAccessory = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_PipeAccessory)
        .WhereElementIsNotElementType()
        .ToElements();
      IEnumerable<Element> equipment = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_MechanicalEquipment)
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
      schemaBuilder.SetDocumentation("Хранение ElementId элементов узлов из принципиальной схемы внутри экземпляров семейств модели");

      // create a field to store the bool value
      FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("DraftElemFromScheme", typeof(ElementId));

      // register the schema
      Schema schema = schemaBuilder.Finish();

      return schema;
    }

    // создание первоначального списка элементов
    public static ObservableCollection<TagItem> tagListData(IEnumerable<Element> elems)
    {
      ObservableCollection<TagItem> tagItems = new ObservableCollection<TagItem>();

      foreach (var e in elems)
      {
        string modelTag = e.LookupParameter("TAG")?.AsString();

        if (!string.IsNullOrWhiteSpace(modelTag))
        {
          TagItem t = new TagItem();
          t.ModelTag = modelTag;
          t.ModelId = e.Id;
          var ent = e.GetEntity(schema);
          if (ent.Schema != null)
          {
            ElementId draftId = ent.Get<ElementId>("DraftElemFromScheme");
            string draftTag = DOC.GetElement(draftId).LookupParameter("TAG")?.AsString();
            t.DraftId = draftId;
            t.DraftTag = draftTag;
            if (modelTag != draftTag)
            {
              t.Color = Brushes.Salmon;
            }
            else
            {
              t.Color = Brushes.LightGreen;
            }
          }
          else
          {
            t.Color = Brushes.White;
          }

          tagItems.Add(t);
        }
      }

      return tagItems;

    }


  }
}
