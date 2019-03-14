using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class ListOfOperands
  {
    public static List<string> listOfOperands(StorageType storageType)
    {
      List<string> operands = new List<string>();

      switch (storageType)
      {
        case StorageType.String:
          operands = new List<string> {
            "равно",
            "не равно",
            "содержит",
            "не содержит",
            "начинается с",
            "не начинается с",
            "заканчивается на",
            "не заканчивается на"
          };
          break;
        case StorageType.Double:
          operands = new List<string>
          {
            "равно",
            "не равно",
            "больше",
            "больше или равно",
            "меньше",
            "меньше или равно"
          };
          break;
        case StorageType.Integer:
          operands = new List<string>
          {
            "равно",
            "не равно"
          };
          break;
        case StorageType.ElementId:
          operands = new List<string>
          {
            "равно",
            "не равно",
            "выше",
            "ровно или выше",
            "ниже",
            "ровно или ниже"
          };
          break;
      }

      return operands;
    }
  }
}
