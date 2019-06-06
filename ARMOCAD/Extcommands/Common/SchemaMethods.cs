using Autodesk.Revit.DB;
using System.Collections.Generic;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace ARMOCAD
{
  public static class SchemaMethods
  {
    public static Schema schema;

    //возвращает значение поля схемы (поле - словарь <int, value>)
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

    //записывает значение в поле схемы,
    //создает новый entity и вешает его на элемент или редактирует существующий entity элемента
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



  }
}
