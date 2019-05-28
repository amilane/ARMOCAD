using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
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
    public ExternalEvent moveTagsEvent;




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
        tagItems = RevitModel?.TagItems;
        return tagItems;
      }
      set { tagItems = value; }

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
        RevitModel.getElementForDeletingDraftId();

        connectEvent.Raise();
        int idx;

        if (NewTag != null)
        {
          //закрашивается белым старый тэг, при переназначении элемента узла
          var tItemsWithErrorDraftId = TagItems.Where(i => i.DraftId == NewTag.DraftId);
          if (tItemsWithErrorDraftId.Count() != 0)
          {
            var tagItemWithErrorDraftId = tItemsWithErrorDraftId.First();
            idx = TagItems.IndexOf(tagItemWithErrorDraftId);

            TagItem tWithoutDraftTag = new TagItem();
            tWithoutDraftTag.ModelTag = tagItemWithErrorDraftId.ModelTag;
            tWithoutDraftTag.ModelId = tagItemWithErrorDraftId.ModelId;

            TagItems[idx] = tWithoutDraftTag;

          }
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
        else
        {
          TagItem t = new TagItem();
          t.ModelTag = "NewTag is Null";

          TagItems.Add(t);
        }
      }
      









    }
    private void MoveTagButtonAction()
    {
      moveTagsEvent.Raise();
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




    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName]string prop = "")
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

  }
}
