using System;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ARMOCAD
{
  public class SchemaMethods
  {

    public string GuidValue { get; set; }
    public string SchemaName { get; set; }
    public string SchemaDocDescription { get; set; }
    public Schema Schema { get; set; }

    #region constructor

    public SchemaMethods(string guidValue, string schemaName, string schemaDocDescription)
    {
      GuidValue = guidValue;
      SchemaName = schemaName;
      SchemaDocDescription = schemaDocDescription;

      if (SchemaExist())
      {
        Schema = GetSchema();
      }
      else
      {
        Schema = CreateSchema();
      }
      
    }

    #endregion constructor




    #region methods


    /// <summary>
    /// возвращает значение поля схемы (поле - словарь <int, value>)
    /// </summary>
    public object getSchemaDictValue<T>(Element e, string fieldName, int key)
    {
      object result = null;
      IDictionary<int, T> dict;

      var ent = e.GetEntity(Schema);
      if (ent.Schema != null)
      {
        dict = ent.Get<IDictionary<int, T>>(Schema.GetField(fieldName));
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
    public void setValueToEntity<T>(Element e, string fieldName, int key, T value)
    {
      Entity entity;
      IDictionary<int, T> dict = null;
      Field field = Schema.GetField(fieldName);

      entity = e.GetEntity(Schema);

      if (entity.Schema == null)
      {
        entity = new Entity(Schema);
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
    public bool SchemaExist()
    {
      bool result = false;
      if (GetSchema() != null)
      {
        result = true;
      }

      return result;
    }

    /// <summary>
    /// Возвращает схему по имени
    /// </summary>
    public Schema GetSchema()
    {
      Schema schema = null;
      IList<Schema> schemas = Schema.ListSchemas();
      if (schemas != null && schemas.Count > 0)
      {
        // get schema
        foreach (Schema s in schemas)
        {
          if (s.SchemaName == SchemaName)
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
    public Schema CreateSchema()
    {
      Guid schemaGuid = new Guid(GuidValue);

      SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

      // set read access
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);

      // set write access
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);

      // set schema name
      schemaBuilder.SetSchemaName(SchemaName);

      // set documentation
      schemaBuilder.SetDocumentation(SchemaDocDescription);

      // create a field to store the bool value
      FieldBuilder fbString = schemaBuilder.AddMapField("Dict_String", typeof(int), typeof(string));
      FieldBuilder fbInt = schemaBuilder.AddMapField("Dict_Int", typeof(int), typeof(int));
      FieldBuilder fbDouble = schemaBuilder.AddMapField("Dict_Double", typeof(int), typeof(double));
      FieldBuilder fbElemId = schemaBuilder.AddMapField("Dict_ElemId", typeof(int), typeof(ElementId));
      FieldBuilder fbXYZ = schemaBuilder.AddSimpleField("Dict_XYZ", typeof(XYZ));

      // register the schema
      Schema schema = schemaBuilder.Finish();

      return schema;
    }



    #endregion methods






  }
}
