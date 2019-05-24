using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public class TBViewModel: INotifyPropertyChanged
  {
    private TagItem selectedItem;
    private ObservableCollection<TagItem> tagItems;
    private TBModel revitModel;
    private string projectName;
    private Document doc;
    private Element eModel;
    private Element eDraft;
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
      get
      {
        newTag = RevitModel.NewTag;
        return newTag;
      }

      set {
        newTag = value;
      }
    }

    // The action function for ConnectButtonAction
    /// <summary>
    /// Если в списке уже есть tagItem с таким ТЭГом, то вытаскиваем и изменяем его, 
    /// </summary>
    private void ConnectButtonAction()
    {
      connectEvent.Raise();

      if (NewTag != null)
      {
        
        TagItems.Add(NewTag);
      }
      else
      {
        TagItem t = new TagItem();
        t.ModelTag = "NewTag is Null";

        TagItems.Add(t);
      }
      
      

      

      //string eModelTag = EModel.LookupParameter("TAG")?.AsString();

      //var currentTagItems = TagItems.Where(i => i.ModelId == eModelId);

      ////если в списке уже есть такой элемент, изменяем его
      //if (currentTagItems.Any())
      //{
      //  TagItem t = currentTagItems.First();
      //  t.ModelId = eModelId;
      //  t.DraftId = eDraftId;
      //  t.ModelTag = eModelTag;
      //  t.DraftTag = eModelTag;

      //}
      //else
      //{
      //  TaskDialog.Show("1", "ХЗ");
      //}
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
