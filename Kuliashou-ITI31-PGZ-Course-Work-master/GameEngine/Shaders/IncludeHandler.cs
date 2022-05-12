using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;

namespace GameEngine
{
    class IncludeHandler : Include
    {
        private IDisposable _shadow = null;
        public IDisposable Shadow { get => _shadow; set => _shadow = value; }

        private Stream _stream;

        public void Close(Stream stream)
        {
            _stream.Dispose();
            _stream = null;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            _stream = new FileStream(fileName, FileMode.Open);
            return _stream;
        }
    }
}
