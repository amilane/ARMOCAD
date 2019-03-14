using System;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace ARMOCAD
{
  public class Node : INotifyPropertyChanged
  {
    public Node()
    {
      this.id = Guid.NewGuid().ToString();
    }

    private ObservableCollection<Node> children = new ObservableCollection<Node>();
    private ObservableCollection<Node> parent = new ObservableCollection<Node>();
    private string text;
    private string id;
    private bool? isChecked = false;



    public ObservableCollection<Node> Children {
      get { return this.children; }
    }

    public ObservableCollection<Node> Parent {
      get { return this.parent; }
    }

    public bool? IsChecked {
      get { return this.isChecked; }
      set {
        this.isChecked = value;
        RaisePropertyChanged("IsChecked");
      }
    }

    public string Text {
      get { return this.text; }
      set {
        this.text = value;

      }
    }

    public string Id {
      get { return this.id; }
      set {
        this.id = value;
      }
    }

    // обработка изменения состояния контрола CheckBox
    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string propertyName)
    {
      if (this.PropertyChanged != null)
        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      if (propertyName == "IsChecked")
      {
        if (this.Id == CheckBoxId.checkBoxId && this.Parent.Count == 0 && this.Children.Count != 0)
        {
          CheckChildNodes(this.Children, this.IsChecked);
        }
        if (this.Id == CheckBoxId.checkBoxId && this.Parent.Count > 0 && this.Children.Count > 0)
        {
          CheckChildAndParent(this.Parent, this.Children, this.IsChecked);
        }
        if (this.Id == CheckBoxId.checkBoxId && this.Parent.Count > 0 && this.Children.Count == 0)
        {
          CheckParentNodes(this.Parent);
        }
      }

    }


    // элемент является вершиной дерева и не имеет родительских элементов
    private void CheckChildAndParent(ObservableCollection<Node> itemsParent, ObservableCollection<Node> itemsChild, bool? isChecked)
    {
      CheckChildNodes(itemsChild, isChecked);
      CheckParentNodes(itemsParent);
    }

    //элемент имеет и родительские элементы и дочерние
    private void CheckChildNodes(ObservableCollection<Node> itemsChild, bool? isChecked)
    {
      foreach (Node item in itemsChild)
      {
        item.IsChecked = isChecked;
        if (item.Children.Count != 0) CheckChildNodes(item.Children, isChecked);
      }
    }

    //элемент находится в самом низу дерева и не имеет дочерних элементов
    private void CheckParentNodes(ObservableCollection<Node> itemsParent)
    {
      int countCheck = 0;
      bool isNull = false;
      foreach (Node paren in itemsParent)
      {
        foreach (Node child in paren.Children)
        {
          if (child.IsChecked == true || child.IsChecked == null)
          {
            countCheck++;
            if (child.IsChecked == null)
              isNull = true;
          }
        }
        if (countCheck != paren.Children.Count && countCheck != 0) paren.IsChecked = null;
        else if (countCheck == 0) paren.IsChecked = false;
        else if (countCheck == paren.Children.Count && isNull) paren.IsChecked = null;
        else if (countCheck == paren.Children.Count && !isNull) paren.IsChecked = true;
        if (paren.Parent.Count != 0) CheckParentNodes(paren.Parent);
      }
    }
  }

  public struct CheckBoxId
  {
    public static string checkBoxId;
  }

}
