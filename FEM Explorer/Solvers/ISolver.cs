using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM_Explorer
{
    public interface ISolver
    {
        Windows.UI.Xaml.Controls.Page Parent
        {
            get;
            set;
        }

        bool HasErrors
        {
            get;
            set;
        }

        void SolveModel();
    }
}
