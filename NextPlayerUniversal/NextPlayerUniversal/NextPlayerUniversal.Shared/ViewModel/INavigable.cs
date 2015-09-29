using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUniversal.ViewModel
{
    public interface INavigable
    {
        void Activate(object parameter, Dictionary<string, object> state);
        void Deactivate(Dictionary<string, object> state);
    }
}
