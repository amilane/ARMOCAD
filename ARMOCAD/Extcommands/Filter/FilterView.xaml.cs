using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ComboBox = System.Windows.Controls.ComboBox;


namespace ARMOCAD
{
  /// <summary>
  /// Логика взаимодействия для FilterControl.xaml
  /// </summary>
  public partial class FilterView : IDisposable
  {
    public UIDocument UIDOC = null;
    public Document DOC = null;
    public FilteredElementCollector Collector;

    public FilterView(Document doc)
    {
      ObservableCollection<Node> items = TreeViewData.treeViewData(doc);
      InitializeComponent();

      treeView.ItemsSource = items;
    }


    public void Dispose()
    {
      this.Close();
    }



    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      CheckBox currentCheckBox = (CheckBox)sender;
      CheckBoxId.checkBoxId = currentCheckBox.Uid;
    }

    private void AndMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      orCheckBox.IsChecked = false;
    }
    private void OrMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      andCheckBox.IsChecked = false;
    }

    private IList<ElementFilter> CollectFilters(ComboBox[,] comboBoxs)
    {
      IList<ElementFilter> filters = new List<ElementFilter>();
      for (int i = 0; i < 4; i++)
      {
        string parameter = comboBoxs[i, 0]?.SelectedValue?.ToString();
        string operat = comboBoxs[i, 1]?.SelectedValue?.ToString();

        if (parameter != null && operat != null)
        {
          ElementFilter f = CreateParameterFilter.createParameterFilter(
            DOC,
            (ParameterData)comboBoxs[i, 0].SelectedValue,
            comboBoxs[i, 1].SelectedValue.ToString(),
            comboBoxs[i, 2].Text);

          if (f != null)
          {
            filters.Add(f);
          }
        }
      }
      return filters;
    }




    private void CollectParameters_Click(object sender, RoutedEventArgs e)
    {
      ObservableCollection<Node> items = (ObservableCollection<Node>)treeView.ItemsSource;
      FilteredElementCollector collector = CollectorFromTreeView.collectorFromTreeView(DOC, items);
      List<ParameterData> parameters = GetParamsFromSelectedElements.getParamsFromSelectedElements(collector);

      cbParameter1.ItemsSource = parameters;
      cbParameter2.ItemsSource = parameters;
      cbParameter3.ItemsSource = parameters;
      cbParameter4.ItemsSource = parameters;

      Collector = collector;

      cbOperation1.Text = "";
      cbOperation2.Text = "";
      cbOperation3.Text = "";
      cbOperation4.Text = "";

      cbValue1.Text = "";
      cbValue2.Text = "";
      cbValue3.Text = "";
      cbValue4.Text = "";

    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {

      ComboBox[,] comboBoxs =
      {
        {cbParameter1, cbOperation1, cbValue1},
        {cbParameter2, cbOperation2, cbValue2},
        {cbParameter3, cbOperation3, cbValue3},
        {cbParameter4, cbOperation4, cbValue4}
      };

      IList<ElementFilter> filters = CollectFilters(comboBoxs);

      ICollection<ElementId> ids;
      if (andCheckBox.IsChecked == true)
      {
        LogicalAndFilter filter = new LogicalAndFilter(filters);
        ids = CollectorFromTreeView.collectorFromTreeView(DOC,
          (ObservableCollection<Node>)treeView.ItemsSource).WherePasses(filter).ToElementIds();
      }
      else
      {
        LogicalOrFilter filter = new LogicalOrFilter(filters);
        ids = CollectorFromTreeView.collectorFromTreeView(DOC,
          (ObservableCollection<Node>)treeView.ItemsSource).WherePasses(filter).ToElementIds();
      }

      if (ids.Count > 0)
      {
        textElementsCount.Text = ids.Count.ToString();
        UIDOC.Selection.SetElementIds(ids);
        UIDOC.ShowElements(ids);
      }
      else
      {
        textElementsCount.Text = "0";
      }

    }

    private void cbParameter1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ParameterData pd = (ParameterData)cbParameter1.SelectedValue;
      if (pd != null)
      {
        cbOperation1.ItemsSource = ListOfOperands.listOfOperands(pd.StorageType);
      }
    }

    private void cbParameter2_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ParameterData pd = (ParameterData)cbParameter2.SelectedValue;
      if (pd != null)
      {
        cbOperation2.ItemsSource = ListOfOperands.listOfOperands(pd.StorageType);
      }
    }

    private void cbParameter3_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ParameterData pd = (ParameterData)cbParameter3.SelectedValue;
      if (pd != null)
      {
        cbOperation3.ItemsSource = ListOfOperands.listOfOperands(pd.StorageType);
      }
    }

    private void cbParameter4_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ParameterData pd = (ParameterData)cbParameter4.SelectedValue;
      if (pd != null)
      {
        cbOperation4.ItemsSource = ListOfOperands.listOfOperands(pd.StorageType);
      }
    }

    private void cbValue1_OnDropDownOpened(object sender, EventArgs e)
    {
      cbValue1.ItemsSource = ValuesFromParameter.valuesFromParameter(cbParameter1.Text, Collector);
    }
    private void cbValue2_OnDropDownOpened(object sender, EventArgs e)
    {
      cbValue2.ItemsSource = ValuesFromParameter.valuesFromParameter(cbParameter2.Text, Collector);
    }
    private void cbValue3_OnDropDownOpened(object sender, EventArgs e)
    {
      cbValue3.ItemsSource = ValuesFromParameter.valuesFromParameter(cbParameter3.Text, Collector);
    }
    private void cbValue4_OnDropDownOpened(object sender, EventArgs e)
    {
      cbValue4.ItemsSource = ValuesFromParameter.valuesFromParameter(cbParameter4.Text, Collector);
    }

  }
}

