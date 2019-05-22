using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ARMOCAD
{
  class viewmodelRevitBridge
  {
    private Dictionary<string, int> _dicWallType;
    private int _selectedWallType;
    private ObservableCollection<string> _listParameters;

    // Declare the Revit model class here.
    // Consequently, create a get-set variable representing this.
    private modelRevitBridge _revitModel;

    public Dictionary<string, int> DicWallType {
      get {
        return _dicWallType;
      }

      set {
        _dicWallType = value;
      }
    }

    public int SelectedWallType {
      get {
        return _selectedWallType;
      }

      set {
        _selectedWallType = value;
      }
    }

    public ObservableCollection<string> ListParameters {
      get {
        return _listParameters;
      }

      set {
        _listParameters = value;
      }
    }

    //  Commands
    // This will be used by the button in the WPF window.
    public ICommand RetrieveParametersValuesCommand {
      get {
        return new DelegateCommand(RetrieveParametersValuesAction);
      }
    }

    // The get-set variable
    internal modelRevitBridge RevitModel {
      get {
        return _revitModel;
      }

      set {
        _revitModel = value;
      }
    }

    // The action function for RetrieveParametersValuesCommand
    private void RetrieveParametersValuesAction()
    {
      if (SelectedWallType != -1)
      {
        ListParameters = new ObservableCollection<string>(RevitModel.GenerateParametersAndValues(SelectedWallType));
      }
    }

    // Constructor
    public viewmodelRevitBridge()
    {

    }
  }
}
