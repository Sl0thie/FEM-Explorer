using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FEM_Explorer
{
    internal class MaterialCollection : Dictionary<string, Material>
    {

        public MaterialCollection() : base()
        {

        }

        private Material currentMaterial;
        internal Material CurrentMaterial
        {
            get { return currentMaterial; }
            set { currentMaterial = value; }
        }
    }
}