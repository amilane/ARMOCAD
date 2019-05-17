using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Linq;

namespace GrimshawRibbon
{
  // Create Elment Filter that passes all elements through
  public class ElementSelectionFilter : ISelectionFilter
  {
    public bool AllowElement(Element e)
    {
      return true;
    }
    public bool AllowReference(Reference r, XYZ p)
    {
      return false;
    }
  }

  [TransactionAttribute(TransactionMode.Manual)]
  public class PreventDeletion : IExternalCommand
  {
    static List<ElementId> protectedIds = new List<ElementId>();
    public const string Caption = "PreventDeletion";

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

    public static bool SchemaExist(string schemaName)
    {
      bool result = false;
      if (GetSchema(schemaName) != null)
      {
        result = true;
      }
      return result;
    }

    public static void PopulateIds(Document doc, Schema schema)
    {
      IList<Element> allSchemaElements = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType()
          .Where(x => x.GetEntity(schema) != null && x.GetEntity(schema).Schema != null)
          .ToList();

      if (allSchemaElements.Count > 0)
      {
        // write these elments ids to static list storage
        foreach (Element e in allSchemaElements)
        {
          protectedIds.Add(e.Id);
        }
      }
    }

    public static bool IsProtected(ElementId id)
    {
      return protectedIds.Contains(id);
    }

    public static Schema CreateSchema()
    {
      Guid schemaGuid = new Guid("0DC954AE-ADEF-41c1-8D38-EB5B8465D255");

      SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

      // set read access
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);

      // set write access
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);

      // set schema name
      schemaBuilder.SetSchemaName("PreventDeletionSchema");

      // set documentation
      schemaBuilder.SetDocumentation("A stored boolean value where true means that wall cannot be deleted.");

      // create a field to store the bool value
      FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField("PreventDeletionField", typeof(String));

      // register the schema
      Schema schema = schemaBuilder.Finish();

      return schema;
    }

    public static void AddSchemaEntity(Schema schema, Element e)
    {
      // create an entity object (object) for this schema (class)
      Entity entity = new Entity(schema);

      // get the field from schema
      Field fieldPreventDeletion = schema.GetField("PreventDeletionField");

      // set the value for entity
      entity.Set(fieldPreventDeletion, "Prevent");

      // store the entity on the element
      e.SetEntity(entity);
    }

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uiapp.ActiveUIDocument.Document;

      Schema schema = null;

      try
      {
        IList<Reference> refs = null;
        ElementSelectionFilter selFilter = new ElementSelectionFilter();

        // get all elements with schema
        if (SchemaExist("PreventDeletionSchema"))
        {
          schema = GetSchema("PreventDeletionSchema");
          protectedIds.Clear();
          PopulateIds(doc, schema);
        }

        // add elements with Schema to pre-selection list
        List<Reference> preSelectedElems = new List<Reference>();
        if (protectedIds.Count != 0)
        {
          foreach (ElementId id in protectedIds)
          {
            Reference elemRef = new Reference(doc.GetElement(id));
            preSelectedElems.Add(elemRef);
          }
        }

        using (Transaction trans = new Transaction(doc, "PreventDeletion"))
        {
          trans.Start();
          try
          {
            // prompt user to add to selection or remove from it
            Selection sel = uidoc.Selection;
            refs = sel.PickObjects(ObjectType.Element, selFilter, "Please pick elements to prevent from deletion.", preSelectedElems);
          }
          catch (OperationCanceledException)
          {
            return Result.Cancelled;
          }

          // get set difference between selected elements and elements that were preselected
          List<Reference> removeSchemaSet = preSelectedElems.Where(x => !refs.Any(l => x.ElementId == l.ElementId)).ToList();
          if (removeSchemaSet.Count > 0)
          {
            foreach (Reference r in removeSchemaSet)
            {
              // remove schema
              Element e = doc.GetElement(r.ElementId);
              e.DeleteEntity(schema);
            }
          }
          // get set difference between preselected elements and newly selected elements
          List<Reference> addSchemaSet = refs.Where(x => !preSelectedElems.Any(l => x.ElementId == l.ElementId)).ToList();
          if (addSchemaSet.Count > 0)
          {
            // if schema exists add it else create new one
            if (schema == null)
            {
              schema = CreateSchema();
            }
            foreach (Reference r in addSchemaSet)
            {
              // add schema entity to element
              Element e = doc.GetElement(r.ElementId);
              AddSchemaEntity(schema, e);

              // add element ids to protectedids
              protectedIds.Add(r.ElementId);
            }
          }
          trans.Commit();
        }

        // show message
        TaskDialog.Show(Caption, "There is total of " + refs.Count.ToString() + " elements protected from being deleted.");
        return Result.Succeeded;
      }
      catch (OperationCanceledException)
      {
        return Result.Cancelled;
      }
      catch (Exception ex)
      {
        message = ex.Message;
        return Result.Failed;
      }

    } // execute
  } // prevent deletion class
} // namespace
