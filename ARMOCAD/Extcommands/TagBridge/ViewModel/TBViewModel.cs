using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public class TBViewModel : INotifyPropertyChanged
  {
    private TagItem selectedItem;
    private ObservableCollection<TagItem> tagItems;
    private TBModel revitModel;
    private string projectName;
    public ExternalEvent connectEvent;




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
        return RevitModel?.ProjectName;
      }

      set {
        projectName = value;
      }
    }


    public ObservableCollection<TagItem> TagItems {
      get {
        return RevitModel?.TagItems;
      }
      set { tagItems = value; }

    }

    private TagItem newTag;
    public TagItem NewTag {
      get {
        newTag = RevitModel.NewTag;
        return newTag;
      }

      set {
        newTag = value;
      }
    }

    // The action function for ConnectButtonAction
    /// <summary>
    /// Если в списке уже есть tagItem с таким ТЭГом, то заменяем его, если нет - добавляем в список
    /// </summary>
    private void ConnectButtonAction()
    {
      RevitModel.getTwoElements();
      connectEvent.Raise();

      if (NewTag != null)
      {
        if (TagItems.Any(i => i.ModelId == NewTag.ModelId))
        {
          TagItem t = TagItems.Where(i => i.ModelId == NewTag.ModelId).First();
          int idx = TagItems.IndexOf(t);
          TagItems[idx] = NewTag;
        }
        else
        {
          TagItems.Add(NewTag);
        }

      }
      else
      {
        TagItem t = new TagItem();
        t.ModelTag = "NewTag is Null";

        TagItems.Add(t);
      }

      
      


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
