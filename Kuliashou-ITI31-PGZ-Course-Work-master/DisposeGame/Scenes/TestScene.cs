using AmazingUILibrary;
using AmazingUILibrary.Backgrounds;
using AmazingUILibrary.Containers;
using AmazingUILibrary.Drawing;
using AmazingUILibrary.Elements;
using DisposeGame.Scripts;
using GameEngine.Animation;
using GameEngine.Game;
using GameEngine.Graphics;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Sound;

namespace DisposeGame.Scenes
{
    public class TestScene : Scene
    {
        private Game3DObject _go;

        private UIMultiElementsContainer _ui;
        private UISequentialContainer _panel;
        private UIText _text1;
        private UIText _text2;
        private UIText _text3;

        private SharpAudioVoice _voice;
        private SharpAudioVoice _voice2;

        protected override void InitializeObjects(Loader loader, SharpAudioDevice audioDevice)
        {
            string file = @"Models\T_Rex.fbx";

            _go = loader.LoadGameObjectFromFile(file, Vector3.Zero, new Vector3(0, MathUtil.PiOverTwo, 0));
            Game3DObject g1 = loader.LoadGameObjectFromFile(file, new Vector3(1, 1, 0), Vector3.Zero);
            Game3DObject g2 = loader.LoadGameObjectFromFile(file, new Vector3(-1, 1, 0), Vector3.Zero);
            Game3DObject g3 = loader.LoadGameObjectFromFile(file, Vector3.UnitZ * 2, Vector3.Zero);
            g1.AddScript(new DinoClickScript(_panel));
            g2.AddScript(new DinoClickScript(_panel));
            g3.AddScript(new DinoClickScript(_panel));
            _go.AddChild(g1);
            _go.AddChild(g2);
            _go.AddChild(g3);
            _go.AddScript(new DinoMovementScript());
            AddGameObject(_go);
            AddGameObject(loader.LoadGameObjectFromFile(@"Models\dsa.fbx", Vector3.UnitX * 5, Vector3.Zero));

            _voice = new SharpAudioVoice(audioDevice, @"Sounds\again.wav");
            _voice2 = new SharpAudioVoice(audioDevice, @"Sounds\again.wav");
        }

        protected override Renderer.IlluminationProperties CreateIllumination()
        {
            Renderer.IlluminationProperties illumination = base.CreateIllumination();
            Renderer.LightSource lightSource = new Renderer.LightSource();
            lightSource.lightSourceType = Renderer.LightSourceType.Directional;
            lightSource.color = Vector3.One;
            lightSource.direction = Vector3.Normalize(new Vector3(0.5f, -2.0f, 1.0f));
            illumination[0] = lightSource;
            return illumination;
        }

        protected override UIElement InitializeUI(Loader loader, DrawingContext context, int screenWidth, int screenHeight)
        {
            string textFormat = "testSceneTextFormat";
            string whiteBrush = "testSceneWhiteBrush";
            string text1background = "testSceneText1background";
            string text2background = "testSceneText2background";
            string text3background = "testSceneText3background";
            string panelBitmap = "testScenePanelBitmap";
            //string buttonlBitmap = "panelBitmap";

            //context.NewBitmap(buttonlBitmap, loader.LoadBitmapFromFile(@"Textures\button.png"));
            context.NewNinePartsBitmap(panelBitmap, loader.LoadBitmapFromFile(@"Textures\button.png"), 15, 121, 15, 46);
            context.NewSolidBrush(whiteBrush, new RawColor4(1f, 1f, 1f, 1f));
            context.NewSolidBrush(text1background, new RawColor4(0.6f, 0.1f, 0.2f, 1f));
            context.NewSolidBrush(text2background, new RawColor4(0.2f, 0.4f, 0.8f, 1f));
            context.NewSolidBrush(text3background, new RawColor4(0.5f, 0.4f, 0.3f, 1f));
            context.NewTextFormat(textFormat);

            _ui = new UIMultiElementsContainer(Vector2.Zero, new Vector2(screenWidth, screenHeight));

            _panel = new UISequentialContainer(Vector2.Zero, new Vector2(200, 200))
            {
                MainAxis = UISequentialContainer.Alignment.Center,
                CrossAxis = UISequentialContainer.Alignment.Center,
                //Background = new TextureBackground(panelBitmap)
                Background = new NinePartsTextureBackground(panelBitmap)
            };
            _ui.Add(_panel);

            _text1 = new UIText("X", new Vector2(120, 16), textFormat, whiteBrush)
            {
                Background = new ColorBackground(text1background)
            };
            _text2 = new UIText("Y", new Vector2(120, 16), textFormat, whiteBrush)
            {
                Background = new ColorBackground(text2background)
            };
            _text3 = new UIText("Z", new Vector2(120, 16), textFormat, whiteBrush)
            {
                Background = new ColorBackground(text3background)
            };
            UIElement text4 = new UIText("Click Me!", new Vector2(120, 16), textFormat, whiteBrush);
            UIElement m = new UIMarginContainer(_text1, 2f);

            UIButton button = new UIButton(text4) { ReleasedBackground = new ColorBackground(text1background), PressedBackground = new ColorBackground(text2background) };
            button.OnClicked += () =>
            {
                //_panel.IsVisible = false;

                //_voice2.Stop();
                //_voice2.Play();

                //Game.ChangeScene(PreviousScene);

                MainMenuScene menu = new MainMenuScene();
                Game.ChangeScene(menu);
            };

            //_panel.Add(new UIPanel(new Vector2(10, 10), new Vector2(10, 10), new Vector4(0.8f, 0.2f, 0.9f, 1f)));
            _panel.Add(m);
            _panel.Add(_text2);
            _panel.Add(_text3);
            _panel.Add(button);

            _text1.OnClicked += () =>
            {
                Transition transition = new SmoothTransition(_go.Rotation.X, _go.Rotation.X + MathUtil.TwoPi, 2);
                transition.Process += value => _go.SetRotationX(value);
                transition.Process += value => _go.SetRotationY(value);
                transition.Process += value => _go.SetRotationZ(value);
            };
            _text2.OnClicked += () =>
            {
                Transition transition = new SmoothTransition(_go.Rotation.Y, _go.Rotation.Y + MathUtil.TwoPi, 2);
                transition.Process += value => _go.SetRotationY(value);

                //_voice.Stop();
                //_voice.Play();

                _voice2.Stop();
                _voice2.Play();

            };
            _text3.OnClicked += () =>
            {
                Transition transition = new SmoothTransition(_go.Rotation.Z, _go.Rotation.Z + MathUtil.TwoPi, 2);
                transition.Process += value => _go.SetRotationZ(value);

                _voice.Play();
                //_voice2.Play();
            };
            _text1.IsClickable = true;
            _text2.IsClickable = true;
            _text3.IsClickable = true;

            return _ui;
        }

        protected override Camera CreateCamera()
        {
            Camera camera = new Camera(new Vector3(0.0f, 0, -10.0f));
            camera.AddScript(new CameraMovementScript(30f));
            AddGameObject(camera);
            return camera;
        }
    }
}
