using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Linq.QueryGeneration
{
    public sealed class ParameterAggregator
    {
        private readonly List<NamedParameter> _parameters = new List<NamedParameter>();

        public NamedParameter AddNamedParameter(object value)
        {
            var parameter = new NamedParameter
            {
                Value = value,
                Name = string.Concat("p", _parameters.Count + 1)
            };
            _parameters.Add(parameter);
            return parameter;
        }

        public NamedParameter[] GetNamedParameters()
        {
            return _parameters.ToArray();
        }
    }
}
