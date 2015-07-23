using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerDataLayer.Model
{
    public class PlaylistItem : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                    onPropertyChanged(this, "Name");
                }
            }
        }
        private int id;
        public int Id { get { return id; } }
        private bool isSmart;
        public bool IsSmart { get { return isSmart; } }

        public PlaylistItem(int id, bool issmart, string name)
        {
            this.id = id;
            this.isSmart = issmart;
            this.name = name;
        }

        public override string ToString()
        {
            if (IsSmart) return "smart|" + id.ToString();
            else return "plain|" + id.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
