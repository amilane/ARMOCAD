using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System.Windows.Media;

namespace ARMOCAD
{
  /// <summary>
  /// Логика взаимодействия для TagBridgeWPF.xaml
  /// </summary>
  public partial class TagBridgeWPF : Window, IDisposable
  {
    public UIDocument uidoc;
    public Document doc;

    Schema schema = null;

    ExternalEventMy hi_event;
    ExternalEvent hiEvent;

    public ObservableCollection<TagItem> tagItems;

    public TagBridgeWPF(Document doc)
    {
      if (SchemaExist("TagBridgeSchema"))
      {
        schema = GetSchema("TagBridgeSchema");
      }

      if (schema == null)
      {
        schema = CreateSchema();
      }

      tagItems = TagListData.tagListData(doc, schema);








      hi_event = new ExternalEventMy();
      hi_event.act = ConnectButton;
      hi_event.transactionName = "HI!";
      hiEvent = ExternalEvent.Create(hi_event);









      InitializeComponent();

      tagListBox.ItemsSource = tagItems;
      //this.tagListBox.ItemTemplate = ContentTemplate.

    }


    public void Dispose()
    {
      this.Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      hiEvent.Raise();
    }



    public void ConnectButton()
    {
      ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

      if (selectedIds.Count == 2 &&
          selectedIds.Any(i => doc.GetElement(i).Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents) &&
          selectedIds.Any(i => doc.GetElement(i).Category.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents))
      {
        var eModelId = selectedIds.Where(i => doc.GetElement(i).Category.Id.IntegerValue != (int)BuiltInCategory.OST_DetailComponents).First();
        var eDraftId = selectedIds.Where(i => doc.GetElement(i).Category.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents).First();

        var eModel = doc.GetElement(eModelId);
        var eDraft = doc.GetElement(eDraftId);

        AddSchemaEntity(schema, eModel, eDraft);

        Parameter parTagDraft = eDraft.LookupParameter("TAG");
        Parameter parTagModel = eModel.LookupParameter("TAG");

        if (parTagDraft != null && parTagModel != null)
        {
          parTagDraft.Set(parTagModel.AsString());
        }

        
        var currentTagItem = tagItems.Where(i => i.ModelId == eModelId);
        if (currentTagItem.Any())
        {
          var cti = currentTagItem.First();

          cti.DraftId = eDraftId;
        }
        //tagListBox.ItemsSource = tagItems;

      }
      else
      {
        TaskDialog.Show("Ошибка", "Выберите 1 элемент модели и 1 элемент узла на чертежном виде.");
      }


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

    public static void AddSchemaEntity(Schema schema, Element eModel, Element eDraft)
    {
      // create an entity object (object) for this schema (class)
      Entity entity = new Entity(schema);

      // get the field from schema
      Field field = schema.GetField("DraftElemFromScheme");

      // set the value for entity
      entity.Set(field, eDraft.Id);

      // store the entity on the element
      eModel.SetEntity(entity);
    }


  }
}
