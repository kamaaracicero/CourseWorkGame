using AmazingUILibrary;
using AmazingUILibrary.Backgrounds;
using AmazingUILibrary.Containers;
using AmazingUILibrary.Drawing;
using AmazingUILibrary.Elements;
using GameEngine.Game;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace DisposeGame.Scenes
{
    public class PauseMenuScene : Scene
    {
        protected override UIElement InitializeUI(Loader loader, DrawingContext context, int screenWidth, int screenHeight)
        {
            context.NewNinePartsBitmap("glowingBorder", loader.LoadBitmapFromFile(@"Textures\GlowingBorder.png"), 21, 29, 21, 29);
            context.NewBitmap("backgroundBitmap", loader.LoadBitmapFromFile(@"Textures\mainbg.jpg"));
            context.NewSolidBrush("whiteBrush", new RawColor4(1f, 1f, 1f, 1f));
            context.NewTextFormat("textFormat", textAlignment: TextAlignment.Center, paragraphAlignment: ParagraphAlignment.Center);

            var ui = new UISequentialContainer(Vector2.Zero, new Vector2(screenWidth, screenHeight))
            {
                MainAxis = UISequentialContainer.Alignment.Center,
                CrossAxis = UISequentialContainer.Alignment.Center,
                Background = new TextureBackground("backgroundBitmap")
            };
            var menu = new UISequentialContainer(Vector2.Zero, new Vector2(150, 170))
            {
                MainAxis = UISequentialContainer.Alignment.Center,
                CrossAxis = UISequentialContainer.Alignment.Center,
                Background = new NinePartsTextureBackground("glowingBorder")
            };
            ui.Add(menu);

            var continueText = new UIText("Continue", new Vector2(120, 52), "textFormat", "whiteBrush");
            var mainMenuText = new UIText("Main Menu", new Vector2(120, 52), "textFormat", "whiteBrush");

            var continueButton = new UIButton(continueText) 
            { 
                ReleasedBackground = new NinePartsTextureBackground("glowingBorder"), 
                PressedBackground = new NinePartsTextureBackground("glowingBorder")
            };
            var mainMenuButton = new UIButton(mainMenuText)
            {
                ReleasedBackground = new NinePartsTextureBackground("glowingBorder"),
                PressedBackground = new NinePartsTextureBackground("glowingBorder")
            };

            continueButton.OnClicked += () =>
            {
                Game.ChangeScene(PreviousScene);
            };
            mainMenuButton.OnClicked += () =>
            {
                Game.ChangeScene(new MainMenuScene());
            };

            menu.Add(continueButton);
            menu.Add(mainMenuButton);

            return ui;
        }
    }
}
