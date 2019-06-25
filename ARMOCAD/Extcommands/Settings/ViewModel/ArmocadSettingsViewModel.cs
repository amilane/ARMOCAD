using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public string Path { get; set; }
    public Configs Configs { get; set; }



    #region Точки Доступа --> Оборудование
    public List<string> FamilySymbolsNames {
      get { return RevitModel?.FamilySymbolsNames; }
    }

    // Значения из файла json
    private string power;
    public string Power {
      get {
        power = RevitModel?.Power;
        return power;
      }
      set { power = value; }
    }

    private string videoPanel;
    public string VideoPanel {
      get {
        videoPanel = RevitModel?.VideoPanel;
        return videoPanel;
      }
      set { videoPanel = value; }
    }

    private string doorCloser;
    public string DoorCloser {
      get {
        doorCloser = RevitModel?.DoorCloser;
        return doorCloser;
      }
      set { doorCloser = value; }
    }

    private string lockMagnet;
    public string LockMagnet {
      get {
        lockMagnet = RevitModel?.LockMagnet;
        return lockMagnet;
      }
      set { lockMagnet = value; }
    }

    private string lockMechanical;
    public string LockMechanical {
      get {
        lockMechanical = RevitModel?.LockMechanical;
        return lockMechanical;
      }
      set { lockMechanical = value; }
    }

    private string latchMechanical;
    public string LatchMechanical {
      get {
        latchMechanical = RevitModel?.LatchMechanical;
        return latchMechanical;
      }
      set { latchMechanical = value; }
    }

    private string exitButton;
    public string ExitButton {
      get {
        exitButton = RevitModel?.ExitButton;
        return exitButton;
      }
      set { exitButton = value; }
    }

    private string inputControl;
    public string InputControl {
      get {
        inputControl = RevitModel?.InputControl;
        return inputControl;
      }
      set { inputControl = value; }
    }

    private string siren;
    public string Siren {
      get {
        siren = RevitModel?.Siren;
        return siren;
      }
      set { siren = value; }
    }

    private string monitor;
    public string Monitor {
      get {
        monitor = RevitModel?.Monitor;
        return monitor;
      }
      set { monitor = value; }
    }

    private string readerIn;
    public string ReaderIn {
      get {
        readerIn = RevitModel?.ReaderIn;
        return readerIn;
      }
      set { readerIn = value; }
    }

    private string readerOut;
    public string ReaderOut {
      get {
        readerOut = RevitModel?.ReaderOut;
        return readerOut;
      }
      set { readerOut = value; }
    }

    private string turnstile;
    public string Turnstile {
      get {
        turnstile = RevitModel?.Turnstile;
        return turnstile;
      }
      set { turnstile = value; }
    }

    private string doorUnlocking;
    public string DoorUnlocking {
      get {
        doorUnlocking = RevitModel?.DoorUnlocking;
        return doorUnlocking;
      }
      set { doorUnlocking = value; }
    }

    // Списки для ItemSource ComboBox с первым значением из json
    public List<string> Fsn1 {
      get => new List<string> { Power }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn2 {
      get => new List<string> { VideoPanel }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn3 {
      get => new List<string> { DoorCloser }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn4 {
      get => new List<string> { LockMagnet }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn5 {
      get => new List<string> { LockMechanical }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn6 {
      get => new List<string> { LatchMechanical }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn7 {
      get => new List<string> { ExitButton }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn8 {
      get => new List<string> { InputControl }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn9 {
      get => new List<string> { Siren }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn10 {
      get => new List<string> { Monitor }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn11 {
      get => new List<string> { ReaderIn }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn12 {
      get => new List<string> { ReaderOut }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn13 {
      get => new List<string> { Turnstile }.Union(FamilySymbolsNames).ToList();
    }
    public List<string> Fsn14 {
      get => new List<string> { DoorUnlocking }.Union(FamilySymbolsNames).ToList();
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
      // Десериализация класса Config, перезапись параметров, сериализация и сохранение файла
      Configs = JsonConvert.DeserializeObject<Configs>(File.ReadAllText(Path));

      // Перезапись параметров
      Configs.SkudTd.Power = PowerUser;
      Configs.SkudTd.VideoPanel = VideoPanelUser;
      Configs.SkudTd.DoorCloser = DoorCloserUser;
      Configs.SkudTd.LockMagnet = LockMagnetUser;
      Configs.SkudTd.LockMechanical = LockMechanicalUser;
      Configs.SkudTd.LatchMechanical = LatchMechanicalUser;
      Configs.SkudTd.ExitButton = ExitButtonUser;
      Configs.SkudTd.InputControl = InputControlUser;
      Configs.SkudTd.Siren = SirenUser;
      Configs.SkudTd.Monitor = MonitorUser;
      Configs.SkudTd.ReaderIn = ReaderInUser;
      Configs.SkudTd.ReaderOut = ReaderOutUser;
      Configs.SkudTd.Turnstile = TurnstileUser;
      Configs.SkudTd.DoorUnlocking = DoorUnlockingUser;

      // Сериализация и сохранение настрок
      File.WriteAllText(Path, JsonConvert.SerializeObject(Configs));


    }

    public ICommand SavePlaceEquipBtnCmd {
      get {
        return new DelegateCommand(SavePlaceEquipBtnAct);
      }
    }

  }
}
