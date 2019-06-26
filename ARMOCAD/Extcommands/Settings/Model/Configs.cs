namespace ARMOCAD
{
  class Configs
  {
    public SkudTdEquipment SkudTd = new SkudTdEquipment();
    //public OtherConfigs oth = new OtherConfigs();

  }

  public struct SkudTdEquipment
  {
    public string Power { get; set; }
    public string VideoPanel { get; set; }
    public string DoorCloser { get; set; }
    public string LockMagnet { get; set; }
    public string LockMechanical { get; set; }
    public string LatchMechanical { get; set; }
    public string ExitButton { get; set; }
    public string InputControl { get; set; }
    public string Siren { get; set; }
    public string Monitor { get; set; }
    public string ReaderIn { get; set; }
    public string ReaderOut { get; set; }
    public string Turnstile { get; set; }
    public string DoorUnlocking { get; set; }

  }

  //public struct OtherConfigs
  //{
  //  public string kek1 { get; set; }
  //  public string kek2 { get; set; }
  //}

}
