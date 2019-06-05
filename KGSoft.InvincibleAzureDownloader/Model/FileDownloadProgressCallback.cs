using System;
using System.Collections.Generic;
using System.Text;

namespace KGSoft.InvincibleAzureDownloader.Model
{
    public class FileDownloadProgressCallback : EventArgs
    {
        private long _current;
        private long _of;
        private int _precision;

        public FileDownloadProgressCallback(long current, long of, int precision = 3)
        {
            _current = current;
            _of = of;
            _precision = precision;
        }
        public double CompletionPercentage
        {
            get
            {
                return Math.Round(((double)_current / (double)_of) * 100D, _precision);
            }
        }
    }
}
