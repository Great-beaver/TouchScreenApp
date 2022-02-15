using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace TouchScreenApp
{
    public class Reader
    {
        private readonly SerialPort _serialPort;
        private Thread _readerThread;
        private readonly byte[] _buffer = new byte[8];

        private bool _running  = false;

        public event EventHandler<TouchEventArgs> Touched;

        public Reader(string portName, int baudRate, bool dtrEnable, bool rtsEnable)
        {
            _serialPort = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DtrEnable = dtrEnable,
                RtsEnable = rtsEnable
            };

        }        

        private void Read()
        {
            try
            {
                while (_running)
                {
                    if (_serialPort.BytesToRead <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    
                    _serialPort.ReadTo("\x55\x54");

                    _buffer[0] = (byte) _serialPort.ReadByte();
                    _buffer[1] = (byte) _serialPort.ReadByte();
                    _buffer[2] = (byte) _serialPort.ReadByte();
                    _buffer[3] = (byte) _serialPort.ReadByte();
                    _buffer[4] = (byte) _serialPort.ReadByte();
                    _buffer[5] = (byte) _serialPort.ReadByte();
                    _buffer[6] = (byte) _serialPort.ReadByte();
                    _buffer[7] = (byte) _serialPort.ReadByte();

                    var touch = Touch.Create(_buffer);

                    if (touch != null)
                    {
                        OnTouched(new TouchEventArgs {Touch = touch});
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                Stop();
                MessageBox.Show($"Проблема с COM портом, остановите программу проверьте порт и запустите снова.\n{e.Message}");
            }
            catch (Exception e)
            {
                Stop();
                MessageBox.Show($"Неизвестная ошибка. Сфотографируйте данную ошибку, остановите программу и запустите снова.\n" +
                                $"Полученный пакет: {_buffer[0]} {_buffer[1]} {_buffer[2]} {_buffer[3]} {_buffer[4]} {_buffer[5]} {_buffer[6]} {_buffer[7]}\n" +
                                $"Ошибка: {e.Message}");
            }
        }

        public void Start()
        {
            _readerThread = new Thread(Read) {IsBackground = true};

            if (_running) return;
            
            _serialPort.Open();

            _running = true;
            _readerThread.Start();
        }

        public void Stop()
        {
            try
            {
                _running = false;
                _serialPort.Close();
            }
            catch (Exception ignored)
            {
                // ignored
            }
        }

        public class TouchEventArgs : EventArgs
        {
            public Touch Touch { get; set; }
        }

        protected virtual void OnTouched(TouchEventArgs e)
        {
            Touched?.Invoke(this, e);
        }
    }
}