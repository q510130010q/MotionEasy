using System.Globalization;
using System.IO;
using System.Text;

namespace GsnMotionEasy.Model
{
    /// <summary>
    /// 五轴标定采样点。每个点 5 个值：X, Y, Z, P(主轴角度), S(从轴角度)。
    /// </summary>
    public class CalibrationPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double P { get; set; }
        public double S { get; set; }

        public CalibrationPoint() { }

        public CalibrationPoint(double x, double y, double z, double p, double s)
        {
            X = x; Y = y; Z = z; P = p; S = s;
        }

        public double[] ToArray() => new[] { X, Y, Z, P, S };

        /// <summary>从 "X,Y,Z,P,S" 或 "X Y Z P S" 格式的字符串解析</summary>
        public static CalibrationPoint Parse(string line)
        {
            var parts = line.Split(new[] { ' ', ',', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            return new CalibrationPoint(
                double.Parse(parts[0], CultureInfo.InvariantCulture),
                double.Parse(parts[1], CultureInfo.InvariantCulture),
                double.Parse(parts[2], CultureInfo.InvariantCulture),
                double.Parse(parts[3], CultureInfo.InvariantCulture),
                double.Parse(parts[4], CultureInfo.InvariantCulture));
        }

        public override string ToString() =>
            $"{X.ToString("F6", CultureInfo.InvariantCulture)}, {Y.ToString("F6", CultureInfo.InvariantCulture)}, {Z.ToString("F6", CultureInfo.InvariantCulture)}, {P.ToString("F6", CultureInfo.InvariantCulture)}, {S.ToString("F6", CultureInfo.InvariantCulture)}";
    }
}
