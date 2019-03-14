using Autodesk.Revit.DB;

namespace ARMOCAD
{
  class ParameterData
  {
    public StorageType StorageType;
    public ElementId Id;
    public string ParameterName;

    public override string ToString()
    {
      return ParameterName;
    }
  }
}
