using System.ComponentModel;

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
        private bool isNotDefault;
        public bool IsNotDefault { get { return isNotDefault; } }

        public PlaylistItem(int id, bool issmart, string name)
        {
            this.id = id;
            this.isSmart = issmart;
            this.name = name;
            if (issmart)
            {
                this.isNotDefault = !NextPlayerDataLayer.Helpers.SmartPlaylistHelper.IsDefaultSmartPlaylist(id);
            }
            else
            {
                this.isNotDefault = true;
            }
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
