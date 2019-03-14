using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace ARMOCAD
{
  static class CreateParameterFilter
  {
    public static ElementFilter createParameterFilter(Document doc, ParameterData parameter, string operation, string ruleString)
    {
      ElementId parameterId = parameter.Id;
      ParameterValueProvider fvp = new ParameterValueProvider(parameterId);
      StorageType storageType = parameter.StorageType;
      FilterRule fRule = null;
      FilterInverseRule fInvRule = null;
      ElementParameterFilter filter = null;


      switch (storageType)
      {
        case StorageType.String:
        case StorageType.Integer:
          FilterStringRuleEvaluator fsre = null;
          switch (operation)
          {
            case "равно":
              fsre = new FilterStringEquals();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              filter = new ElementParameterFilter(fRule);
              break;
            case "не равно":
              fsre = new FilterStringEquals();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              fInvRule = new FilterInverseRule(fRule);
              filter = new ElementParameterFilter(fInvRule);
              break;
            case "содержит":
              fsre = new FilterStringContains();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              filter = new ElementParameterFilter(fRule);
              break;
            case "не содержит":
              fsre = new FilterStringContains();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              fInvRule = new FilterInverseRule(fRule);
              filter = new ElementParameterFilter(fInvRule);
              break;
            case "начинается с":
              fsre = new FilterStringBeginsWith();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              filter = new ElementParameterFilter(fRule);
              break;
            case "не начинается с":
              fsre = new FilterStringBeginsWith();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              fInvRule = new FilterInverseRule(fRule);
              filter = new ElementParameterFilter(fInvRule);
              break;
            case "заканчивается на":
              fsre = new FilterStringEndsWith();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              filter = new ElementParameterFilter(fRule);
              break;
            case "не заканчивается на":
              fsre = new FilterStringEndsWith();
              fRule = new FilterStringRule(fvp, fsre, ruleString, true);
              fInvRule = new FilterInverseRule(fRule);
              filter = new ElementParameterFilter(fInvRule);
              break;
          }
          break;
        case StorageType.Double:
          FilterNumericRuleEvaluator fnre = null;
          double ruleValue = Convert.ToDouble(ruleString);
          switch (operation)
          {
            case "равно":
              fnre = new FilterNumericEquals();
              fRule = new FilterDoubleRule(fvp, fnre, ruleValue, 0.0);
              filter = new ElementParameterFilter(fRule);
              break;
            case "не равно":
              fnre = new FilterNumericEquals();
              fRule = new FilterDoubleRule(fvp, fnre, ruleValue, 0.0);
              fInvRule = new FilterInverseRule(fRule);
              filter = new ElementParameterFilter(fInvRule);
              break;
            case "больше":
              fnre = new FilterNumericGreater();
              fRule = new FilterDoubleRule(fvp, fnre, ruleValue, 0.0);
              filter = new ElementParameterFilter(fRule);
              break;
            case "больше или равно":
              fnre = new FilterNumericGreaterOrEqual();
              fRule = new FilterDoubleRule(fvp, fnre, ruleValue, 0.0);
              filter = new ElementParameterFilter(fRule);
              break;
            case "меньше":
              fnre = new FilterNumericLess();
              fRule = new FilterDoubleRule(fvp, fnre, ruleValue, 0.0);
              filter = new ElementParameterFilter(fRule);
              break;
            case "меньше или равно":
              fnre = new FilterNumericLessOrEqual();
              fRule = new FilterDoubleRule(fvp, fnre, ruleValue, 0.0);
              filter = new ElementParameterFilter(fRule);
              break;
          }
          break;
        case StorageType.ElementId:

          var levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).ToElements();
          var level = levels.Where(i => i.Name == ruleString).FirstOrDefault();
          ElementId ruleId = level.Id;

          fnre = null;
          switch (operation)
          {
            case "равно":
              fnre = new FilterNumericEquals();
              fRule = new FilterElementIdRule(fvp, fnre, ruleId);
              filter = new ElementParameterFilter(fRule);
              break;
            case "не равно":
              fnre = new FilterNumericEquals();
              fRule = new FilterElementIdRule(fvp, fnre, ruleId);
              fInvRule = new FilterInverseRule(fRule);
              filter = new ElementParameterFilter(fInvRule);
              break;
            case "выше":
              fnre = new FilterNumericGreater();
              fRule = new FilterElementIdRule(fvp, fnre, ruleId);
              filter = new ElementParameterFilter(fRule);
              break;
            case "ровно или выше":
              fnre = new FilterNumericGreaterOrEqual();
              fRule = new FilterElementIdRule(fvp, fnre, ruleId);
              filter = new ElementParameterFilter(fRule);
              break;
            case "ниже":
              fnre = new FilterNumericLess();
              fRule = new FilterElementIdRule(fvp, fnre, ruleId);
              filter = new ElementParameterFilter(fRule);
              break;
            case "ровно или ниже":
              fnre = new FilterNumericLessOrEqual();
              fRule = new FilterElementIdRule(fvp, fnre, ruleId);
              filter = new ElementParameterFilter(fRule);
              break;
          }
          break;




      }
      return filter;

    }
  }
}
