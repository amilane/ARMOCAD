using System;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ARMOCAD
{
  public class SchemaMethods
  {
    public static Schema schema;

    #region constructor

    public SchemaMethods(string guidValue)
    {
      private 
    }


    #endregion




    #region methods


    /// <summary>
    /// возвращает значение поля схемы (поле - словарь <int, value>)
    /// </summary>
    public static object getSchemaDictValue<T>(Element e, string fieldName, int key)
    {
      object result = null;
      IDictionary<int, T> dict;

      var ent = e.GetEntity(schema);
      if (ent.Schema != null)
      {
        dict = ent.Get<IDictionary<int, T>>(schema.GetField(fieldName));
        if (dict != null && dict.ContainsKey(key))
        {
          result = dict[key];
        }
      }

      return result;
    }

    /// <summary>
    /// записывает значение в поле схемы,
    /// создает новый entity и вешает его на элемент или редактирует существующий entity элемента
    /// </summary>
    public static void setValueToEntity<T>(Element e, string fieldName, int key, T value)
    {
      Entity entity;
      IDictionary<int, T> dict = null;
      Field field = schema.GetField(fieldName);

      entity = e.GetEntity(schema);

      if (entity.Schema == null)
      {
        entity = new Entity(schema);
        dict = new Dictionary<int, T> { [key] = value };
      }
      else
      {
        dict = entity.Get<IDictionary<int, T>>(field);
        if (dict != null)
        {
          if (dict.ContainsKey(key))
          {
            dict[key] = value;
          }
          else
          {
            dict.Add(key, value);
          }
        }

      }
      if (value.GetType() == typeof(double) | value.GetType() == typeof(XYZ))
      {
        entity.Set(field, dict, DisplayUnitType.DUT_DECIMAL_FEET);
      }
      else
      {
        entity.Set(field, dict);
      }

      e.SetEntity(entity);

    }

    /// <summary>
    /// Проверяет наличие схемы по имени
    /// </summary>
    public static bool SchemaExist(string schemaName)
    {
      bool result = false;
      if (GetSchema(schemaName) != null)
      {
        result = true;
      }

      return result;
    }

    /// <summary>
    /// Возвращает схему по имени
    /// </summary>
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

    /// <summary>
    /// Создает схему
    /// </summary>
    /// <returns></returns>
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



    #endregion methods






  }
}
