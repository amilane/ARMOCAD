using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public partial class ChangeUnitWPF : IDisposable
  {
    public Document DOC = null;
    public Dictionary<string, List<string>> DataDict;
    public ChangeUnitWPF()
    {
      InitializeComponent();
    }

    public void Dispose()
    {
      this.Close();
    }


    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
      string Unit = BuildingNameBox.SelectedValue.ToString();
      List<string> UnitData = (List<string>)BuildingNameBox.SelectedValue;
      ProjectInfo projInfo = DOC.ProjectInformation;

      //Параметры из Сведений о проекте
      Parameter parUnit = projInfo.LookupParameter("EL_Unit");
      Parameter parTagCode1 = projInfo.LookupParameter("TagCode1");
      Parameter parStageBuild = projInfo.LookupParameter("Этап строительства Амурского ГПЗ");
      Parameter parStageWork = projInfo.LookupParameter("Этап работ");
      Parameter parComplex = projInfo.LookupParameter("Эксплуатационный комплекс");
      Parameter parInstallation = projInfo.LookupParameter("Установка");
      Parameter parTitleObject = projInfo.LookupParameter("Титульный объект");
      Parameter parTitle = projInfo.LookupParameter("Титул");
      Parameter parNameObjectRu = projInfo.LookupParameter("Название объекта RU");
      Parameter parNameObjectEn = projInfo.LookupParameter("Название объекта EN");


      // Работа с номерами листов
      IEnumerable<Element> allSheets = new FilteredElementCollector(DOC).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> filterSheets = from i in allSheets
                                          where i.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString().StartsWith("0055-CPC-ARM")
                                          select i;

      string codeOfListNumber = string.Format("{0}.{1}.{2}.{3}.{4}{5}", UnitData[1], UnitData[2], UnitData[3], UnitData[4], UnitData[5], UnitData[6]);

      //Работа с TAG 
      IEnumerable<Element> terminals = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DuctTerminal).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> ductAccessory = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DuctAccessory).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> pipeAccessory = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_PipeAccessory).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> equipment = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> detail = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DetailComponents).WhereElementIsNotElementType().ToElements();

      IEnumerable<Element> ElementsGroup1 = terminals.Union(ductAccessory).Union(pipeAccessory).Union(equipment).Union(detail);

      IEnumerable<Element> ducts = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> duflexDucts = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_FlexDuctCurves).WhereElementIsNotElementType().ToElements();
      IEnumerable<Element> ductFittings = new FilteredElementCollector(DOC).OfCategory(BuiltInCategory.OST_DuctFitting).WhereElementIsNotElementType().ToElements();

      IEnumerable<Element> ElementsGroup2 = ducts.Union(duflexDucts).Union(ductFittings);


      using (Transaction t = new Transaction(DOC, "Change Unit"))
      {
        t.Start();

        parUnit.Set(Unit);
        parTagCode1.Set(UnitData[0]);
        parStageBuild.Set(UnitData[1]);
        parStageWork.Set(UnitData[2]);
        parComplex.Set(UnitData[3]);
        parInstallation.Set(UnitData[4]);
        parTitleObject.Set(UnitData[5]);
        parTitle.Set(UnitData[6]);
        parNameObjectRu.Set(UnitData[7]);
        parNameObjectEn.Set(UnitData[8]);

        foreach (Element i in filterSheets)
        {
          Parameter parSheetNumber = i.get_Parameter(BuiltInParameter.SHEET_NUMBER);
          string sheetNumberValue = parSheetNumber.AsString();
          string[] sheetNumberSplit = sheetNumberValue.Split('-');
          sheetNumberSplit[3] = codeOfListNumber;
          string newNumberSheet = string.Join("-", sheetNumberSplit);
          parSheetNumber.Set(newNumberSheet);
        }

        foreach (var i in ElementsGroup1)
        {
          Parameter parTag = i.LookupParameter("TAG");
          string tagValue = parTag.AsString();
          if (tagValue != null && tagValue != "")
          {
            string[] tagSplit = tagValue.Split('-');
            tagSplit[0] = UnitData[5];
            tagSplit[1] = UnitData[4];
            string newTag = string.Join("-", tagSplit);
            parTag.Set(newTag);
          }
        }

        foreach (var i in ElementsGroup2)
        {
          Parameter parTag = i.LookupParameter("TAG");
          string tagValue = parTag.AsString();
          if (tagValue != null && tagValue != "")
          {
            string[] tagSplit = tagValue.Split('/');
            tagSplit[0] = UnitData[0];
            string newTag = string.Join("/", tagSplit);
            parTag.Set(newTag);
          }
        }
        t.Commit();
        TaskDialog.Show("Готово", "ОК");
      }
    }
  }
}

