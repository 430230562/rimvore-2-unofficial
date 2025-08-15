using UnityEngine;
using Verse;

namespace RimVore2
{
    [StaticConstructorOnStartup]
    public static class UITextures
    {

        public static readonly Texture2D CheckOnTexture = ContentFinder<Texture2D>.Get("UI/Icons/CheckOn");
        public static readonly Texture2D CheckOffTexture = ContentFinder<Texture2D>.Get("UI/Icons/CheckOff");
        public static readonly Texture2D CopyTexture = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");
        public static readonly Texture2D HelpButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/help");
        public static readonly Texture2D ResetButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/reset");
        public static readonly Texture2D AddButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/add");
        public static readonly Texture2D RemoveButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/remove");
        public static readonly Texture2D EditButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/edit");
        public static readonly Texture2D DisableButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/disable");
        public static readonly Texture2D MoveUpButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/moveUp");
        public static readonly Texture2D MoveDownButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/moveDown");
        public static readonly Texture2D HiddenButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/hidden");
        public static readonly Texture2D InvalidButtonTexture = Resources.Load<Texture2D>("Textures/UI/Widgets/Warning"); //ContentFinder<Texture2D>.Get("UI/Icons/invalid");
        public static readonly Texture2D TemporaryButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/temporary");
        public static readonly Texture2D BlankButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/blank");

        public static readonly Texture2D SkullButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/skull");
        public static readonly Texture2D HeartButtonTexture = ContentFinder<Texture2D>.Get("UI/Icons/heart");

        public static readonly Texture2D InfoButton = TexButton.Info;
        public static readonly Texture2D EjectButton = TexButton.Banish;
        public static readonly Texture2D DropButton = TexButton.Drop;
        public static readonly Texture2D CollapseButton = TexButton.Collapse;
        public static readonly Texture2D RevealButton = TexButton.Reveal;
        public static readonly Texture2D ManualPassButton = TexButton.Play;
        public static readonly Texture2D IsStrugglingButton = ContentFinder<Texture2D>.Get("UI/Icons/Struggle");
        public static readonly Texture2D NotStrugglingButton = ContentFinder<Texture2D>.Get("UI/Icons/NoStruggle");

        public static readonly Texture2D GrappleAttacker = ContentFinder<Texture2D>.Get("Combat/grappleAttacker");
        public static readonly Texture2D GrappleDefender = ContentFinder<Texture2D>.Get("Combat/grappleDefender");

        public static readonly Texture2D ToggleGrappleOn = ContentFinder<Texture2D>.Get("Widget/grappleToggledOn");
        public static readonly Texture2D ToggleGrappleOff = ContentFinder<Texture2D>.Get("Widget/grappleToggledOff");

        public static readonly Texture2D PredatorIcon = ContentFinder<Texture2D>.Get("UI/Icons/predator");
        public static readonly Texture2D PreyIcon = ContentFinder<Texture2D>.Get("UI/Icons/stomach");

    }
}