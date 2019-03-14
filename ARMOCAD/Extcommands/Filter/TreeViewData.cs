using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ARMOCAD
{
  static class TreeViewData
  {
    public static ObservableCollection<Node> treeViewData(Document doc)
    {
      List<Element> selectedElements = new List<Element>();

      FilteredElementCollector collector
        = new FilteredElementCollector(doc)
          .WhereElementIsNotElementType();

      foreach (Element e in collector)
      {
        if (e.Category != null && e.Category.CategoryType == CategoryType.Model & e.Category.CanAddSubcategory)
        {
          selectedElements.Add(e);
        }

      }

      // Create data for TreeView (Category - Family - FamilyType)
      var elemsByCategories = selectedElements.OrderBy(i => i.Category.Name).GroupBy(i => i.Category.Name);
      ObservableCollection<Node> familyList = new ObservableCollection<Node>();

      foreach (var category in elemsByCategories)
      {
        Node categoryNode = new Node();

        categoryNode.Text = category.Key;

        var elemsByFamilyName = category.
          OrderBy(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString()).
          GroupBy(i => i.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString());

        foreach (var family in elemsByFamilyName)
        {
          Node familyNode = new Node();
          familyNode.Text = family.Key;
          familyNode.Parent.Add(categoryNode);
          categoryNode.Children.Add(familyNode);

          var elemsByFamilyType = family.OrderBy(i => i.Name).GroupBy(i => i.Name);
          foreach (var familyType in elemsByFamilyType)
          {
            Node familyTypeNode = new Node();
            familyTypeNode.Text = familyType.Key;
            familyTypeNode.Parent.Add(familyNode);
            familyNode.Children.Add(familyTypeNode);
          }
        }
        familyList.Add(categoryNode);
      }

      return familyList;
    }


  }
}
