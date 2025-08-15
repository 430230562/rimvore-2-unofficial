using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class AlternativeMainTabIcon : DefModExtension
    {
        Texture2D icon;
        public Texture2D Icon
        {
            get
            {
                if(icon == null)
                    icon = ContentFinder<Texture2D>.Get(iconPath);
                return icon;
            }
        }

        string iconPath;
    }
}
