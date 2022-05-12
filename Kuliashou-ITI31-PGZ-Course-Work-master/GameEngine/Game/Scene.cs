using System;
using System.Collections.Generic;
using System.Linq;
using AmazingUILibrary;
using AmazingUILibrary.Containers;
using AmazingUILibrary.Drawing;
using GameEngine.Graphics;
using SharpDX;
using Sound;

namespace GameEngine.Game
{
    public class Scene : IDisposable
    {
        private Dictionary<string, Texture> _textures;
        public Dictionary<string, Texture> Textures { get => _textures; }

        private Dictionary<string, Material> _materials;
        public Dictionary<string, Material> Materials { get => _materials; }

        public Game Game { get; set; }
        public Scene PreviousScene { get; set; }

        private List<Game3DObject> _gameObjects;
        private List<Game3DObject> _gameObjectsToAdd;
        private List<Game3DObject> _gameObjectsToRemove;

        private bool _isInitialized = false;

        public Renderer.IlluminationProperties Illumination { get; private set; }
        public Camera Camera { get; private set; }
        public UIElement UI { get; private set; }
        public List<Game3DObject> GameObjects { get => _gameObjects; }

        public Scene()
        {
            _textures = new Dictionary<string, Texture>();
            _materials = new Dictionary<string, Material>();
            _gameObjects = new List<Game3DObject>();
            _gameObjectsToAdd = new List<Game3DObject>();
            _gameObjectsToRemove = new List<Game3DObject>();
        }

        public void Initialize(Loader loader, SharpAudioDevice audioDevice, DrawingContext context, int screenWidth, int screenHeight)
        {
            if (_isInitialized) return;
            UI = InitializeUI(loader, context, screenWidth, screenHeight);
            InitializeObjects(loader, audioDevice);
            Illumination = CreateIllumination();
            Camera = CreateCamera();
            _isInitialized = true;
        }

        protected virtual void InitializeObjects(Loader loader, SharpAudioDevice audioDevice)
        {
        }

        protected virtual Renderer.IlluminationProperties CreateIllumination()
        {
            Renderer.IlluminationProperties illumination = new Renderer.IlluminationProperties();
            Renderer.LightSource lightSource = new Renderer.LightSource();
            illumination.globalAmbient = new Vector3(0.02f);
            lightSource.lightSourceType = Renderer.LightSourceType.Base;
            lightSource.constantAttenuation = 0.01f;
            lightSource.color = Vector3.Zero;
            for (int i = 0; i < Renderer.MaxLights; i++)
            {
                illumination[i] = lightSource;
            }
            return illumination;
        }

        protected virtual UIElement InitializeUI(Loader loader, DrawingContext context, int screenWidth, int screenHeight)
        {
            return new UIMultiElementsContainer(Vector2.Zero, Vector2.Zero);
        }

        protected virtual Camera CreateCamera()
        {
            return new Camera(Vector3.Zero);
        }

        public void AddGameObject(Game3DObject gameObject)
        {
            SetGameObjectScene(gameObject);
            _gameObjectsToAdd.Add(gameObject);
        }

        private void SetGameObjectScene(Game3DObject gameObject)
        {
            gameObject.Scene = this;
            foreach (Game3DObject child in gameObject.Children)
            {
                SetGameObjectScene(child);
            }
        }

        public void RemoveGameObject(Game3DObject gameObject)
        {
            _gameObjectsToRemove.Add(gameObject);
            gameObject.Dispose();
        }

        public void Update(float delta)
        {
            AddGameObjects();
            foreach (Game3DObject gameObject in _gameObjects)
            {
                gameObject.Update(delta);
            }
            RemoveGameObjects();
        }

        private void AddGameObjects()
        {
            _gameObjects.AddRange(_gameObjectsToAdd);
            _gameObjectsToAdd.Clear();
        }

        private void RemoveGameObjects()
        {
            _gameObjects.RemoveAll(_ => _gameObjectsToRemove.Contains(_));
            _gameObjectsToRemove.Clear();
        }

        public void RenderGameObjects(Camera camera, Renderer renderer)
        {
            Matrix viewMatrix = camera.GetViewMatrix();
            Matrix projectionMatrix = camera.GetPojectionMatrix();

            for (int i = 0; i < _gameObjects.Count; i++)
            {
                Game3DObject gameObject = _gameObjects[i];
                RenderObject(gameObject, renderer, viewMatrix, projectionMatrix);
            }
        }

        private void RenderObject(Game3DObject obj, Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            if (!obj.IsHidden)
            {
                renderer.UpdatePerObjectConstantBuffers(obj.GetWorldMatrix(), viewMatrix, projectionMatrix, 1);
                renderer.RenderMeshObject(obj.Mesh);

                if (obj.Collision != null)
                {
                    renderer.UpdatePerObjectConstantBuffers(Matrix.Translation(obj.Position), viewMatrix, projectionMatrix, 1);
                    renderer.RenderMeshObject(obj.Collision.GetMesh());
                }
            }

            foreach (var child in obj.Children)
            {
                RenderObject(child, renderer, viewMatrix, projectionMatrix);
            }
        }

        public virtual void Dispose()
        {
            for (int i = _gameObjects.Count - 1; i >= 0; i--)
            {
                Game3DObject meshObject = _gameObjects[i];
                _gameObjects.Remove(meshObject);
                Utilities.Dispose(ref meshObject);
            }

            for (int i = _textures.Count - 1; i >= 0; i--)
            {
                string textureName = _textures.ElementAt(i).Key;
                Texture texture = _textures.ElementAt(i).Value;
                _materials.Remove(textureName);
                Utilities.Dispose(ref texture);
            }
        }
    }
}
