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
      IDictionary<int, T> Dict;

      var ent = e.GetEntity(schema);
      if (ent.Schema != null)
      {
        Dict = ent.Get<IDictionary<int, T>>(schema.GetField(fieldName));
        if (Dict != null && Dict.ContainsKey(key))
        {
          result = Dict[key];
        }
      }

      return result;
    }

    //записывает значение в поле схемы,
    //создает новый entity и вешает его на элемент или редактирует существующий entity элемента
    public static void setValueToEntity<T>(Element e, string fieldName, int key, T newId)
    {
      Entity entity;
      IDictionary<int, T> dictId = null;
      Field field = schema.GetField(fieldName);

      entity = e.GetEntity(schema);

      if (entity.Schema == null)
      {
        entity = new Entity(schema);
        dictId = new Dictionary<int, T> { [key] = newId };
      }
      else
      {
        dictId = entity.Get<IDictionary<int, T>>(field);
        if (dictId != null)
        {
          if (dictId.ContainsKey(key))
          {
            dictId[key] = newId;
          }
          else
          {
            dictId.Add(key, newId);
          }
          
        }
        
      }

      entity.Set(field, dictId);
      e.SetEntity(entity);

    }



  }
}
