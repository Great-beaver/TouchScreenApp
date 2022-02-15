namespace TouchScreenApp
{
    public class CoordinateConverter
    {
        private readonly double _resolutionHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
        private readonly double _resolutionWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

        private readonly int _minX;
        private readonly int _minY;

        private readonly double _ratioX;
        private readonly double _ratioY;

        public CoordinateConverter(int minX, int maxX, int minY, int maxY)
        {            
            _minX = minX;
            _minY = minY;

            _ratioX = (maxX - _minX) / _resolutionWidth;
            _ratioY = (maxY - _minY) / _resolutionHeight;
        }

        public MouseOperations.MousePoint ConvertToWindowsCoordinates(int x, int y)
        {
            return new MouseOperations.MousePoint((int)((x - _minX) / _ratioX), (int)_resolutionHeight - (int)((y - _minY) / _ratioY));
        }
    }
}