using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAnalyer
{
    public class DataSet
    {
        private int _queueSize;
        private FixedSizedQueue<float> _raw;
        private float _current;

        public DataSet(int queueSize)
        {
            _queueSize = queueSize;
            _raw = new FixedSizedQueue<float>(queueSize);
        }
        //public float Max => _raw.Any() ? _raw.Max() : 0;
        public float Max { get; set; }
        //public float Min => _raw.Any() ? _raw.Min() : 0;
        public float Min { get; set; }
        public float Current
        {
            get { return _current; }
            set
            {
                _current = value;

                Max = Math.Max(Max, value);
                Min = Math.Min(Min, value);

                _raw.Enqueue(value);
            }
        }
        public float Normalized
        {
            get
            {
                return (1 / (Max - Min)) * (Current - Min);
            }
        }
    }
}
