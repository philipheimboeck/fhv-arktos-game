using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Arctos.Annotations;
using Arctos.Game.Client.Model;

namespace Arctos.Game.Client
{
    public class GameArea : INotifyPropertyChanged
    {
        private TrulyObservableCollection<Area> areaList;

        public TrulyObservableCollection<Area> AreaList
        {
            get { return areaList; }
            set { areaList = value;
                OnPropertyChanged();
            }
        }

        public int Rows
        {
            get
            {
                if (AreaList != null && AreaList.Count > 0)
                {
                    return AreaList.Max(x => x.Row);
                }
                return 0;
            }
        }

        public int Columns
        {
            get
            {
                if (AreaList != null && AreaList.Count > 0)
                {
                    return AreaList.Max(x => x.Column);
                }
                return 0;
            }
        }

        public int AreaHeight { get; set; }
        public int AreaWidth { get; set; }

        public int GameWidth { get; set; }
        public int GameHeight { get; set; }

        /// <summary>
        /// GameArea Constructor
        /// </summary>
        public GameArea()
        {
            this.AreaList = new TrulyObservableCollection<Area>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
