using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  /// <summary>
  /// Логика взаимодействия для TagBridgeWPF.xaml
  /// </summary>
  public partial class TBView : Window, IDisposable
  {
    public UIDocument UIDOC;
    public Document DOC;

    public TBView(UIDocument uidoc)
    {
      UIDOC = uidoc;
      DOC = uidoc.Document;
      InitializeComponent();
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var l = sender as ListBox;
      var i = l.SelectedItem as TagItem;
      ICollection<ElementId> ids = new List<ElementId>();

      ElementId mdlId = i.ModelId;
      ElementId drId = i.DraftId;

      if(mdlId != null && mdlId.IntegerValue != -1 && DOC.GetElement(mdlId) != null) { ids.Add(mdlId);}
      if (drId != null && drId.IntegerValue != -1 && DOC.GetElement(drId) != null) { ids.Add(drId); }

      if (ids.Count != 0)
      {
        UIDOC.Selection.SetElementIds(ids);
        UIDOC.ShowElements(ids);
      }
      
    }
  }
}
