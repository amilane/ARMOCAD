using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ARMOCAD
{
  public class TBViewModel
  {
    private TagItem selectedItem;
    private ObservableCollection<TagItem> tagItems;
    private TBModel revitModel;
    private string projectName;

    // Constructor
    public TBViewModel()
    {

    }

    internal TBModel RevitModel {
      get {
        return revitModel;
      }

      set {
        revitModel = value;
      }
    }

    public TagItem SelectedItem {
      get {
        return selectedItem;
      }

      set {
        selectedItem = value;
      }
    }

    public string ProjectName {
      get {
        return RevitModel.ProjectName;
      }

      set {
        projectName = value;
      }
    }

    public ObservableCollection<TagItem> TagItems {
      get {
        return RevitModel.TagItems;
      }

    }

    // The action function for RetrieveParametersValuesCommand
    private void ConnectButtonAction()
    {

    }

    //  Commands
    // This will be used by the button in the WPF window.
    public ICommand ConnectButtonCommand {
      get {
        return new DelegateCommand(ConnectButtonAction);
      }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName]string prop = "")
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

  }
}
