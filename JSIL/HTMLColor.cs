// Used when generating barrier XML

using System;

namespace JSIL.Internal {
    public static class HTMLColor {
        public const UInt16 HueUnit = 600;
        public const UInt16 HueMin = 0, HueMax = (HueUnit * 6) - 1;
        public const UInt16 SaturationMin = 0, SaturationMax = 2550;
        public const UInt16 ValueMin = 0, ValueMax = 2550;

        static byte ClampByte (int value) {
            if (value < 0)
                return 0;
            else if (value > 255)
                return 255;
            else
                return (byte)value;
        }

        public static string FromRGB (int r, int g, int b) {
            return String.Format(
                "#{0:x2}{1:x2}{2:x2}",
                r, g, b
            );
        }

        public static string FromHSV (UInt16 hue, UInt16 saturation, UInt16 value) {
            if (value <= ValueMin)
                return FromRGB(0, 0, 0);
            if ((value >= ValueMax) && (saturation <= SaturationMin))
                return FromRGB(255, 255, 255);
            if (value > ValueMax)
                value = ValueMax;
            if (saturation > SaturationMax)
                saturation = SaturationMax;

            int range = value * 255 / ValueMax;
            if (saturation <= SaturationMin)
                return FromRGB(range, range, range);

            int segment = hue / HueUnit;
            int remainder = hue - (segment * HueUnit);
            int colorRange = (saturation) * range / SaturationMax;

            int c = (SaturationMax - saturation) * range / SaturationMax;
            int b = (remainder * colorRange) / HueUnit + c;
            int rb = colorRange - b + (c * 2);
            int a = colorRange + c;

            switch (segment) {
                case 0:
                    return FromRGB(ClampByte(a), ClampByte(b), ClampByte(c));
                case 1:
                    return FromRGB(ClampByte(rb), ClampByte(a), ClampByte(c));
                case 2:
                    return FromRGB(ClampByte(c), ClampByte(a), ClampByte(b));
                case 3:
                    return FromRGB(ClampByte(c), ClampByte(rb), ClampByte(a));
                case 4:
                    return FromRGB(ClampByte(b), ClampByte(c), ClampByte(a));
                case 5:
                    return FromRGB(ClampByte(a), ClampByte(c), ClampByte(rb));
                default:
                    throw new ArgumentException("Invalid color");
            }
        }
    }
}