using System;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;
using TouchScreenApp.Properties;

namespace TouchScreenApp
{
    public class AppContext : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;
        private readonly CoordinateConverter _coordinateConverter;
        private readonly Reader _reader;
        private readonly int _rightClickAllowedDeviationInPixels;
        private readonly int _rightClickHoldTimeInMilliseconds;       
        private readonly System.Threading.Timer _rightClickTimer;
        private MouseOperations.MousePoint _rightClickStartPoint;

        readonly MenuItem _startMenuItem;
        readonly MenuItem _stopMenuItem;

        public AppContext()
        {
            try
            {
                _rightClickAllowedDeviationInPixels = int.Parse(ConfigurationManager.AppSettings["rightClickAllowedDeviationInPixels"]);
                _rightClickHoldTimeInMilliseconds = int.Parse(ConfigurationManager.AppSettings["rightClickHoldTimeInMilliseconds"]);

                _coordinateConverter = new CoordinateConverter(
                    int.Parse(ConfigurationManager.AppSettings["minX"]),
                    int.Parse(ConfigurationManager.AppSettings["maxX"]),
                    int.Parse(ConfigurationManager.AppSettings["minY"]),
                    int.Parse(ConfigurationManager.AppSettings["maxY"]));

                _reader = new Reader(
                    ConfigurationManager.AppSettings["portName"],
                    int.Parse(ConfigurationManager.AppSettings["baudRate"]),
                    bool.Parse(ConfigurationManager.AppSettings["dtrEnable"]),
                    bool.Parse(ConfigurationManager.AppSettings["rtsEnable"]));

                _reader.Touched += Reader_Touched;


                _rightClickTimer = new System.Threading.Timer(state =>
                {
                    if (_rightClickStartPoint.GetDistance(MouseOperations.GetCursorPosition()) > _rightClickAllowedDeviationInPixels) return;
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
                    MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.RightUp | MouseOperations.MouseEventFlags.RightDown);

                });

                _startMenuItem = new MenuItem("Start", Start) { Visible = false };
                _stopMenuItem = new MenuItem("Stop", Stop);

                trayIcon = new NotifyIcon
                {
                    Icon = Resources.AppIcon,
                    ContextMenu = new ContextMenu(new[] {
                        _startMenuItem,
                        _stopMenuItem,
                        new MenuItem("Exit", Exit)
                    }),
                    Visible = true
                };

                Start(null, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void Start(object sender, EventArgs e)
        {
            try
            {
                _stopMenuItem.Visible = true;
                _startMenuItem.Visible = false;
                _reader.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void Stop(object sender, EventArgs e)
        {
            try
            {
                _stopMenuItem.Visible = false;
                _startMenuItem.Visible = true;
                _reader.Stop();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Reader_Touched(object sender, Reader.TouchEventArgs e)
        {
            try
            {
                MouseOperations.SetCursorPosition(_coordinateConverter.ConvertToWindowsCoordinates(e.Touch.X, e.Touch.Y));

                switch (e.Touch.TouchState)
                {
                    case Touch.State.Start:
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                        _rightClickStartPoint = MouseOperations.GetCursorPosition();
                        _rightClickTimer.Change(_rightClickHoldTimeInMilliseconds, Timeout.Infinite);
                        break;
                    case Touch.State.Hold:
                        break;
                    case Touch.State.End:
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
                        _rightClickTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        break;
                    case Touch.State.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void Exit(object sender, EventArgs e)
        {
            Stop(null, null);
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}