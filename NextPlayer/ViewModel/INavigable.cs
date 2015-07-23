using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayer.ViewModel
{
    public interface INavigable
    {
        void Activate(object parameter, Dictionary<string, object> state);
        void Deactivate(Dictionary<string, object> state);
        void BackButonPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e);
    }
}
