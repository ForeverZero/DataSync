using System;

namespace DataSynchronizor.Model
{
    public class DsCtl
    {
        public string ComputerName { get; set; }
        public DateTime Time { get; set; }
        public string Hash { get; set; }
    }

    public enum DsCtlCompareResult
    {
        Earlier,
        Later,
        Equal,
        Conflict
    }
}