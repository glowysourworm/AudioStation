using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Component.CDPlayer
{
    public class CDBuffer
    {
        byte[] _buffer;
        int _position = 0;

        public CDBuffer(byte[] buffer)
        {
            _buffer = buffer;
        }
        public void ReadData(object sender, CDDataReadEventArgs e)
        {
            Buffer.BlockCopy(e.Data, 0, _buffer, _position, (int)e.DataSize);
            _position += (int)e.DataSize;
        }
    }
}
