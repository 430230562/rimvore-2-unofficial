using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimVore2
{
    public class UpdateCompatibilityFix : Attribute
    {
        private string version;
        /// <summary>
        /// Used to flag special functionality that is only relevant for cross-version compatibility
        /// </summary>
        /// <param name="version">For which version this object enables compatibility</param>
        public UpdateCompatibilityFix(string version)
        {
            this.version = version;
        }

        public UpdateCompatibilityFix() { }
    }
}
