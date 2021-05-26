using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public class Result
    {
        public Result(string name, string path)
        {
            Name = name;
            Path = path;
            State = "Result Created";
        }

        public string State { get; set; }

        public string Name { get; }

        public string Path { get;  }
    }
}
