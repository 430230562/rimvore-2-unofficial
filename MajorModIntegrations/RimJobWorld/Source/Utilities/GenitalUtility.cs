using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RV2_RJW
{
    public static class GenitalUtility
    {
        private static GenitalAccess genitalAccess;
        public static GenitalAccess GenitalAccess
        {
            get
            {
                if(genitalAccess == null)
                {
                    genitalAccess = new GenitalAccess();
                }
                return genitalAccess;
            }
        }
    }
}
