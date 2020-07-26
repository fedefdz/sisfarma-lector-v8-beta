using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Models
{
    public class QRCode
    {
        private static readonly int _qrLenPrefix = 3;
        private static readonly int _qrLenBarCode = 13;

        public string Prefix { get; private set; }

        public string BarCode { get; private set; }

        public string Sufix { get; private set; }

        public QRCode(string prefix, string barCode, string sufix)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            if (Prefix.Length != _qrLenPrefix)
                throw new FormatException(nameof(Prefix));

            BarCode = barCode ?? throw new ArgumentNullException(nameof(barCode));
            if (BarCode.Length != _qrLenBarCode)
                throw new FormatException(nameof(BarCode));

            Sufix = sufix ?? throw new ArgumentNullException(nameof(sufix));
        }

        public QRCode(string qr)
        {
            if (!QRCode.IsValid(qr))
                throw new FormatException(nameof(qr));

            Prefix = qr.Substring(0, _qrLenPrefix);
            BarCode = qr.Substring(_qrLenPrefix, _qrLenBarCode);
            Sufix = qr.Substring(_qrLenPrefix + _qrLenBarCode);
        }

        public override string ToString()
            => Prefix + BarCode + Sufix;

        public static bool IsValid(string qr)
        {
            var rg = new Regex(@"^\d{" + _qrLenPrefix + @"}\d{" + _qrLenBarCode + @"}[0-9|A-Z|a-z]+$");
            return rg.IsMatch(qr);
        }

        public static bool TryParse(string qrString, out QRCode qrCode)
        {
            qrCode = default(QRCode);
            if (!QRCode.IsValid(qrString))
                return false;

            try
            {
                qrCode = new QRCode(qrString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}