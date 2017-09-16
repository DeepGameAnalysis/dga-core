using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DGA.Utils
{
    interface IClosableViewModel
    {
        event EventHandler CloseWindowEvent;
    }
}
