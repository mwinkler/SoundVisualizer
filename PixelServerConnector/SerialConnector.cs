using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelServerConnector
{
    public class SerialConnector
    {
        private SerialPort _port;

        public SerialConnector(string port, int baud)
        {
            _port = new SerialPort(port, baud);
            
        }

        public void Open()
        {
            _port.Open();
        }

        public void SetColor(byte r, byte g, byte b, byte w)
        {
            if (!_port.IsOpen)
                return;

            _port.Write(new[] { r, g, b, w }, 0, 4);
        }

        public void Close()
        {
            if (!_port.IsOpen)
                return;

            _port.Close();
        }
    }
}
