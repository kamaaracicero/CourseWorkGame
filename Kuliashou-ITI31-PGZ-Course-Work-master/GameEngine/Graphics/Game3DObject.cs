using System;
using System.Collections.Generic;
using GameEngine.Collisions;
using GameEngine.Components;
using GameEngine.Game;
using GameEngine.Scripts;
using SharpDX;

namespace GameEngine.Graphics
{
    public class Game3DObject : IDisposable
    {
        private Game3DObject _parent;
        private List<Game3DObject> _children = new List<Game3DObject>();
        private List<Script> _scripts = new List<Script>();
        private List<Script> _scriptsToRemove = new List<Script>();
        private List<Script> _scriptsToAdd = new List<Script>();
        private Dictionary<Type, ObjectComponent> _components = new Dictionary<Type, ObjectComponent>();
        private MeshObject _mesh;
        private ObjectCollision _collision;
        private Vector3 _position;
        private Vector3 _rotation;

        public Scene Scene { get; set; }
        public Game3DObject Parent { get => _parent; set => _parent = value; }
        public List<Game3DObject> Children { get => _children; }
        public MeshObject Mesh { get => _mesh; }
        public ObjectCollision Collision 
        { 
            get => _collision; 
            set
            {
                _collision = value;
                _collision.GameObject = this;
            }
        }

        public Vector3 Position
        {
            get
            {
                if (_parent != null)
                {
                    Matrix parentRotation = Matrix.RotationYawPitchRoll(_parent.Rotation.Z, _parent.Rotation.Y, _parent.Rotation.X);
                    Vector3 position = (Vector3)Vector3.Transform(_position, parentRotation);
                    return _parent.Position + position;
                }
                return _position;
            }
        }
        public Vector3 Rotation
        {
            get
            {
                return _parent == null ? _rotation : _parent.Rotation + _rotation;
            }
        }

        public bool IsHidden { get; set; } = false;

        public Game3DObject(Vector3 position, Vector3 rotation)
        {
            _position = position;
            _rotation = rotation;
        }

        public void AddMeshObject(MeshObject obj)
        {
            _mesh = obj;
            if (_mesh == null)
            {
                IsHidden = true;
            }
        }

        public Game3DObject AddChild(Game3DObject child)
        {
            child.Parent = this;
            _children.Add(child);
            return child;
        }

        public void AddScript(Script script)
        {
            script.GameObject = this;
            _scriptsToAdd.Add(script);
        }

        public void RemoveScript(Script script)
        {
            _scriptsToRemove.Add(script);
        }

        public void AddComponent<T>(T component) where T : ObjectComponent
        {
            _components[typeof(T)] = component;
            component.GameObject = this;
        }

        public bool HasComponent<T>() where T : ObjectComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public T GetComponent<T>() where T : ObjectComponent
        {
            Type key = typeof(T);
            if (!_components.ContainsKey(key)) return null;
            return _components[key] as T;
        }

        public void Update(float delta)
        {
            AddScripts();
            foreach (Script script in _scripts)
            {
                script.Update(delta);
            }
            foreach (Game3DObject child in _children)
            {
                child.Update(delta);
            }
            RemoveScripts();
        }

        private void AddScripts()
        {
            _scripts.AddRange(_scriptsToAdd);
            _scriptsToAdd.Clear();
        }

        private void RemoveScripts()
        {
            _scripts.RemoveAll(_ => _scriptsToRemove.Contains(_));
            _scriptsToRemove.Clear();
        }

        private void ClampAngle(ref float angle)
        {
            if (angle > MathUtil.Pi) angle -= MathUtil.TwoPi;
            else if (angle < -MathUtil.Pi) angle += MathUtil.TwoPi;
        }

        public virtual void SetRotationX(float x)
        {
            _rotation.X = x;
            ClampAngle(ref _rotation.X);
        }

        public virtual void SetRotationY(float y)
        {
            _rotation.Y = y;
            ClampAngle(ref _rotation.Y);
        }

        public virtual void SetRotationZ(float z)
        {
            _rotation.Z = z;
            ClampAngle(ref _rotation.Z);
        }

        public virtual void RotateZ(float deltaZ)
        {
            _rotation.Z += deltaZ;
            ClampAngle(ref _rotation.Z);
        }

        public virtual void RotateY(float deltaY)
        {
            _rotation.Y += deltaY;
            ClampAngle(ref _rotation.Y);
        }

        public virtual void RotateX(float deltaX)
        {
            _rotation.X += deltaX;
            ClampAngle(ref _rotation.X);
        }

        public virtual void SetPositionY(float y)
        {
            _position.Y = y;
        }

        public virtual void MoveBy(float deltaX, float deltaY, float deltaZ)
        {
            _position.X += deltaX;
            _position.Y += deltaY;
            _position.Z += deltaZ;
        }
        public virtual void MoveBy(Vector3 delta)
        {
            _position += delta;
        }

        public virtual void MoveTo(Vector3 position)
        {
            _position = position;
        }

        public Matrix GetWorldMatrix()
        {
            return Matrix.Multiply(Matrix.RotationYawPitchRoll(Rotation.Z, Rotation.Y, Rotation.X), Matrix.Translation(Position));
        }

        public Game3DObject GetCopy()
        {
            Game3DObject copy = new Game3DObject(_position, _rotation);
            copy.AddMeshObject(_mesh);
            if (Collision != null) copy.Collision = Collision.GetCopy();
            foreach (Game3DObject child in _children)
            {
                copy.AddChild(child.GetCopy());
            }
            return copy;
        }

        public void Dispose()
        {
            _mesh?.Dispose();
        }
    }
}