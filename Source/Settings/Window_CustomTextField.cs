using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class Window_CustomTextField : Window
    {
        public bool Saved = false;
        public bool Cancelled = false;
        public string TextFieldContent;
        private readonly Action saveAction;
        private readonly Action cancelAction;

        public Window_CustomTextField(string textFieldContent, Action<string> saveAction, Action cancelAction)
        {
            this.TextFieldContent = textFieldContent;
            this.saveAction = () => saveAction(TextFieldContent);
            this.cancelAction = cancelAction;
        }

        public void DoWindow()
        {
            absorbInputAroundWindow = true;
            focusWhenOpened = true;
            draggable = false;
            onlyOneOfTypeAllowed = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(400, 99);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect exitButtonsRect = new Rect(inRect.x, inRect.y + inRect.height * 0.5f, inRect.width, inRect.height * 0.5f);
            if(UIUtility.DoSaveCancelButtons(exitButtonsRect, saveAction, cancelAction))
            {
                this.Close();
            }
            Rect fieldRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - exitButtonsRect.height);
            TextFieldContent = UIUtility.DoLabelledTextField(fieldRect, "RV2_Settings_Rules_PresetName".Translate(), TextFieldContent);
        }
    }
}
