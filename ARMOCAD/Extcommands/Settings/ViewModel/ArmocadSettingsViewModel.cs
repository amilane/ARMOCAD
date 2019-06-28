using System;
using System.Collections.Generic;

using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json;

namespace ARMOCAD
{
  class ArmocadSettingsViewModel
  {

    private ArmocadSettingsModel revitModel;
    internal ArmocadSettingsModel RevitModel {
      get { return revitModel; }
      set { revitModel = value; }
    }
    public static string Path { get; set; }
    public static Configs Configs { get; set; }

    private static SkudTdEquipment SkudTD {
      get => Configs.SkudTd;
    }

    #region Точки Доступа --> Оборудование
    public List<string> FamilySymbolsNames {
      get { return RevitModel?.FamilySymbolsNames; }
    }

    // Значения из файла json
    private string power;
    public string Power {
      get {
        power = SkudTD.Power;
        return power;
      }
      set { power = value; }
    }

    private string videoPanel;
    public string VideoPanel {
      get {
        videoPanel = SkudTD.VideoPanel;
        return videoPanel;
      }
      set { videoPanel = value; }
    }

    private string doorCloser;
    public string DoorCloser {
      get {
        doorCloser = SkudTD.DoorCloser;
        return doorCloser;
      }
      set { doorCloser = value; }
    }

    private string lockMagnet;
    public string LockMagnet {
      get {
        lockMagnet = SkudTD.LockMagnet;
        return lockMagnet;
      }
      set { lockMagnet = value; }
    }

    private string lockMechanical;
    public string LockMechanical {
      get {
        lockMechanical = SkudTD.LockMechanical;
        return lockMechanical;
      }
      set { lockMechanical = value; }
    }

    private string latchMechanical;
    public string LatchMechanical {
      get {
        latchMechanical = SkudTD.LatchMechanical;
        return latchMechanical;
      }
      set { latchMechanical = value; }
    }

    private string exitButton;
    public string ExitButton {
      get {
        exitButton = SkudTD.ExitButton;
        return exitButton;
      }
      set { exitButton = value; }
    }

    private string inputControl;
    public string InputControl {
      get {
        inputControl = SkudTD.InputControl;
        return inputControl;
      }
      set { inputControl = value; }
    }

    private string siren;
    public string Siren {
      get {
        siren = SkudTD.Siren;
        return siren;
      }
      set { siren = value; }
    }

    private string monitor;
    public string Monitor {
      get {
        monitor = SkudTD.Monitor;
        return monitor;
      }
      set { monitor = value; }
    }

    private string readerIn;
    public string ReaderIn {
      get {
        readerIn = SkudTD.ReaderIn;
        return readerIn;
      }
      set { readerIn = value; }
    }

    private string readerOut;
    public string ReaderOut {
      get {
        readerOut = SkudTD.ReaderOut;
        return readerOut;
      }
      set { readerOut = value; }
    }

    private string turnstile;
    public string Turnstile {
      get {
        turnstile = SkudTD.Turnstile;
        return turnstile;
      }
      set { turnstile = value; }
    }

    private string doorUnlocking;
    public string DoorUnlocking {
      get {
        doorUnlocking = SkudTD.DoorUnlocking;
        return doorUnlocking;
      }
      set { doorUnlocking = value; }
    }


    // Выбранные пользователем значения
    public string PowerUser { get; set; }
    public string VideoPanelUser { get; set; }
    public string DoorCloserUser { get; set; }
    public string LockMagnetUser { get; set; }
    public string LockMechanicalUser { get; set; }
    public string LatchMechanicalUser { get; set; }
    public string ExitButtonUser { get; set; }
    public string InputControlUser { get; set; }
    public string SirenUser { get; set; }
    public string MonitorUser { get; set; }
    public string ReaderInUser { get; set; }
    public string ReaderOutUser { get; set; }
    public string TurnstileUser { get; set; }
    public string DoorUnlockingUser { get; set; }

    

    #endregion


    /// <summary>
    /// кнопка сохранения настроек для "ТД -> Оборудование"
    /// </summary>
    private void SavePlaceEquipBtnAct()
    {

      // Перезапись параметров
      if (!string.IsNullOrEmpty(PowerUser)) Configs.SkudTd.Power = PowerUser;
      if (!string.IsNullOrEmpty(VideoPanelUser)) Configs.SkudTd.VideoPanel = VideoPanelUser;
      if (!string.IsNullOrEmpty(DoorCloserUser)) Configs.SkudTd.DoorCloser = DoorCloserUser;
      if (!string.IsNullOrEmpty(LockMagnetUser)) Configs.SkudTd.LockMagnet = LockMagnetUser;
      if (!string.IsNullOrEmpty(LockMechanicalUser)) Configs.SkudTd.LockMechanical = LockMechanicalUser;
      if (!string.IsNullOrEmpty(LatchMechanicalUser)) Configs.SkudTd.LatchMechanical = LatchMechanicalUser;
      if (!string.IsNullOrEmpty(ExitButtonUser)) Configs.SkudTd.ExitButton = ExitButtonUser;
      if (!string.IsNullOrEmpty(InputControlUser)) Configs.SkudTd.InputControl = InputControlUser;
      if (!string.IsNullOrEmpty(SirenUser)) Configs.SkudTd.Siren = SirenUser;
      if (!string.IsNullOrEmpty(MonitorUser)) Configs.SkudTd.Monitor = MonitorUser;
      if (!string.IsNullOrEmpty(SirenUser)) Configs.SkudTd.Siren = SirenUser;
      if (!string.IsNullOrEmpty(ReaderInUser)) Configs.SkudTd.ReaderIn = ReaderInUser;
      if (!string.IsNullOrEmpty(ReaderOutUser)) Configs.SkudTd.ReaderOut = ReaderOutUser;
      if (!string.IsNullOrEmpty(TurnstileUser)) Configs.SkudTd.Turnstile = TurnstileUser;
      if (!string.IsNullOrEmpty(DoorUnlockingUser)) Configs.SkudTd.DoorUnlocking = DoorUnlockingUser;



      // Сериализация и сохранение настрок
      File.WriteAllText(Path, JsonConvert.SerializeObject(Configs));

      MessageBox.Show("Изменения сохранены.");


    }

    public ICommand SavePlaceEquipBtnCmd {
      get {
        return new DelegateCommand(SavePlaceEquipBtnAct);
      }
    }

    

  }
}
