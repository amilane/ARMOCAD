using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Autodesk.Revit.DB;


namespace ARMOCAD
{
  public class TagItem : INotifyPropertyChanged
  {
    private ElementId modelId;
    private ElementId draftId;
    private string modelTag;
    private string draftTag;
    private Brush color;
    private string itemName;
    private bool connectError;


    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName]string prop = "")
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(prop));
      if (prop == "ChangeModelTag" |
          prop == "ChangeDraftTag")
      {
        if (DraftId == null || DraftId.IntegerValue == -1 || DraftTag == null)
        {
          Color = Brushes.White;
        }
        else
        {
          if (ModelTag != DraftTag)
          {
            Color = Brushes.Salmon;
          }
          else
          {
            Color = Brushes.LightGreen;
          }
        }

      }

      if (prop == "ConnectError")
      {
        if (ConnectError)
        {
          Color = Brushes.HotPink;
          ItemName = $"{ItemName} - {DraftId?.ToString()}";
        }

      }
    }

    public ElementId ModelId {
      get {
        return modelId;
      }
      set {
        modelId = value;
        OnPropertyChanged("ChangeModelId");
      }
    }

    public ElementId DraftId {
      get {
        return draftId;
      }
      set {
        draftId = value;
        OnPropertyChanged("ChangeDraftId");
      }

    }
    public string ModelTag {
      get {
        return modelTag;
      }
      set {
        modelTag = value;

        if (!string.IsNullOrWhiteSpace(modelTag))
        {
          ItemName = modelTag;
        }
        else
        {
          ItemName = ModelId.ToString();
        }


        OnPropertyChanged("ChangeModelTag");
      }
    }

    public string DraftTag {
      get {
        return draftTag;
      }
      set {
        draftTag = value;
        OnPropertyChanged("ChangeDraftTag");
      }
    }

    public Brush Color {
      get {
        return color;
      }
      set {
        color = value;
        OnPropertyChanged("ChangeColor");
      }

    }

    public string ItemName {
      get {
        return itemName;
      }
      set { itemName = value; }
    }

    public bool ConnectError {
      get { return connectError; }
      set {
        connectError = value;
        OnPropertyChanged("ConnectError");
      }
    }
  }
}

