using System;
using System.Collections.Generic;
using System.Text;
using GreenEnergyHub.Aggregation.Domain.Types;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public class Job
    {
        public Job(ProcessType processType)
        {
            Id = Guid.NewGuid();
            Created = SystemClock.Instance.GetCurrentInstant();
            ProcessType = processType;
            State = "Job created";
        }

        public Guid Id { get; set; }

        public long DatabricksJobId { get; set; }

        public string State { get; set; }

        public Instant Created { get; }

        public string Owner { get; set; }

        public string SnapshotPath { get; set; }

        public ProcessType ProcessType { get; }

        public string ClusterId { get; set; }

        public long RunId { get; set; }
    }
}
