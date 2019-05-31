using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public class TBViewModel : INotifyPropertyChanged
  {
    private TagItem selectedItem;
    private ObservableCollection<TagItem> tagItems;

    private string projectName;
    public ExternalEvent connectEvent;
    public ExternalEvent moveTagsEvent;




    // Constructor
    public TBViewModel()
    {

    }

    private TBModel revitModel;
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
        tagItems = RevitModel.TagItems;
        return tagItems;
      }
      
    }

    private TagItem newTag;
    public TagItem NewTag {
      get {
        newTag = RevitModel?.NewTag;
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

      if (RevitModel.IsTwoElementsSelected)
      {

        connectEvent.Raise();
        int idx;

        if (NewTag != null)
        {
          //внесение правки в существующий элемент списка или добавление нового
          if (TagItems.Any(i => i.ModelId == NewTag.ModelId))
          {
            TagItem t = TagItems.Where(i => i.ModelId == NewTag.ModelId).First();
            idx = TagItems.IndexOf(t);
            TagItems[idx] = NewTag;
          }
          else
          {
            TagItems.Add(NewTag);
          }

        }

      }

    }
    private void MoveTagButtonAction()
    {
      moveTagsEvent.Raise();
    }

    private void RefreshList()
    {
      TagItems.Clear();
      foreach (var t in RevitModel.tagListData())
      {
        TagItems.Add(t);
      }


    }

    //  Commands
    // Команда для соединения двух экземпляров семейств (модели и чертежного вида)
    public ICommand ConnectButtonCommand {
      get {
        return new DelegateCommand(ConnectButtonAction);
      }
    }

    //Команда для кнопки переноса тэгов
    public ICommand MoveTagButtonCommand {
      get {
        return new DelegateCommand(MoveTagButtonAction);
      }
    }

    // Команда для обновления списка
    public ICommand RefreshListButtonCommand {
      get {
        return new DelegateCommand(RefreshList);
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
