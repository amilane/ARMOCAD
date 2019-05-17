using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;

namespace ARMOCAD
{
  public class TagItem : INotifyPropertyChanged
  {
    public Element element;
    private ElementId modelId;
    private ElementId draftId;
    private string modelTag;
    private string draftTag;
    private Brush color;
    public static Schema schema;
    public static Document doc;

    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string propertyName)
    {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      if (propertyName == "ChangeDraftId")
      {

        Color = Brushes.Blue;
        MessageBox.Show("dmkdf", "sdsd");
        //if (draftId == null)
        //{
        //  color = Brushes.White;
        //}
        //else
        //{
        //  if (ModelTag != DraftTag)
        //  {
        //    color = Brushes.Salmon;
        //  }
        //  else
        //  {
        //    color = Brushes.LightGreen;
        //  }
        //}
      }
    }

    public ElementId ModelId {
      get
      {
        this.modelId = element.Id;
        return this.modelId;
      }
      set {
        
      }
    }
    public ElementId DraftId {
      get
      {
        var ent = element.GetEntity(schema);
        if (ent.Schema != null)
        {
          draftId = ent.Get<ElementId>("DraftElemFromScheme");
        }
        return draftId;
      }
      set
      {
        draftId = value;
        RaisePropertyChanged("ChangeDraftId");
      }

    }
    public string ModelTag {
      get
      {
        this.modelTag = element.LookupParameter("TAG").AsString();
        return this.modelTag;
      }
      
    }
    public string DraftTag {
      get
      {
        draftTag = doc.GetElement(draftId).LookupParameter("TAG").AsString();
        return draftTag;
      }
      

    }

    public Brush Color {
      get
      {
        //if (DraftId == null)
        //{
        //  color = Brushes.White;
        //}
        //else
        //{
        //  if (ModelTag != DraftTag)
        //  {
        //    color = Brushes.Salmon;
        //  }
        //  else
        //  {
        //    color = Brushes.LightGreen;
        //  }
        //}

        return color;
      }
      set { color = value; }

    }




  }
}

