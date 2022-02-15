using System;
using System.Linq;

namespace TouchScreenApp
{
    public class Touch
    {
        private const int MagicNumber = 82;

        public enum State
        {
            Start,
            Hold,
            End,
            Unknown
        }

        public readonly short X;
        public readonly short Y;
        public readonly State TouchState;

        private Touch(byte[] packet)
        {
            switch (packet[0])
            {
                case 1:
                    TouchState = State.Start;
                    break;
                case 2:
                    TouchState = State.Hold;
                    break;
                case 4:
                    TouchState = State.End;
                    break;
                default: TouchState = State.Unknown;
                    break;
            }

            X = BitConverter.ToInt16(packet.Skip(1).Take(2).ToArray(), 0);
            Y = BitConverter.ToInt16(packet.Skip(3).Take(2).ToArray(), 0);
        }

        public static Touch Create(byte[] packet)
        {
            return !IsChecksumValid(packet) ? null : new Touch(packet);
        }

        private static int GetChecksum(byte[] packet)
        {
            var sum = packet.Take(5).Select(x => (int)x).Sum() + MagicNumber;

            if (sum <= 255) return sum;

            var quotient = sum / 255;

            return sum - 255 * quotient - quotient;
        }

        private static bool IsChecksumValid(byte[] packet)
        {
            return packet[7] == GetChecksum(packet);
        }
    }
}