using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ARMOCAD
{
  class ArmocadSettingsViewModel
  {

    private ArmocadSettingsModel revitModel;
    internal ArmocadSettingsModel RevitModel {
      get {return revitModel;}
      set {revitModel = value;}
    }




    /// <summary>
    /// кнопка сохранения настроек для "ТД -> Оборудование"
    /// </summary>
    private void SavePlaceEquipBtnAct()
    {

    }

    public ICommand SavePlaceEquipBtnCmd {
      get {
        return new DelegateCommand(SavePlaceEquipBtnAct);
      }
    }

  }
}
