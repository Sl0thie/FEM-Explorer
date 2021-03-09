using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEM_Explorer
{
    internal class SectionProfileCollection : Dictionary<string, SectionProfile>
    {
        public SectionProfileCollection() : base()
        {

        }

        private Material currentSectionProfile;
        internal Material CurrentSectionProfile
        {
            get { return currentSectionProfile; }
            set { currentSectionProfile = value; }
        }





    }
}