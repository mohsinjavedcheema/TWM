using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Twm.Chart.Annotations;
using Twm.Core.Helpers;
using Twm.DB.DAL.Repositories.Options;
using Twm.Model.Model;

namespace Twm.Core.Classes
{   
    [DataContract]
    public class GridColumnCustomization : INotifyPropertyChanged, ICloneable
    {

        [DataMember]
        public ObservableCollection<GridColumnInfo> Columns { get; set; }

        public string Name { get; set; }

        public event EventHandler ColumnsCollectionChanged;


        public ViewOption ViewOption { get; set; }

        public GridColumnCustomization()
        {
            Columns = new ObservableCollection<GridColumnInfo>();
        }


        public void UpdateProperty(string name)
        {
            OnPropertyChanged(name);
        }

        public void ReloadColumns()
        {
            ColumnsCollectionChanged?.Invoke(this, new EventArgs());
            OnPropertyChanged("Columns");
        }


        public async void SaveLayout()
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new ViewOptionRepository(context);

                if (ViewOption == null)
                {
                    ViewOption = new ViewOption()
                    {
                        Code = Name,
                        Data = JsonHelper.ToJson(this)
                    };
                    await repository.Add(ViewOption);
                }
                else
                {
                    ViewOption.Data = JsonHelper.ToJson(this);
                    await repository.Update(ViewOption);

                }
                await repository.CompleteAsync();
            }
        }





        public object Clone()
        {
            GridColumnCustomization gcc = (GridColumnCustomization)MemberwiseClone();
            if (ViewOption != null)
                gcc.ViewOption = (ViewOption)ViewOption.Clone();
            gcc.Columns = new ObservableCollection<GridColumnInfo>();
            if (Columns != null)
                foreach (var column in Columns)
                {
                    gcc.Columns.Add((GridColumnInfo)column.Clone());
                }
            return gcc;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}