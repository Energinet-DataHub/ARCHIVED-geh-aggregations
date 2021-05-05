﻿using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf
{
    public class AggregatedExchangeNeighbourResultMessageToDtoMapper : ProtobufOutboundMapper<AggregatedExchangeNeighbourResultMessage>
    {
        protected override IMessage Convert(AggregatedExchangeNeighbourResultMessage obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new Document()
            {
                Content = System.Text.Json.JsonSerializer.Serialize(obj),

                // TODO use noda time
                EffectuationDate = Timestamp.FromDateTime(DateTime.UtcNow),
                Recipient = $"khs {DateTime.Now:HHmm dd MMMM}",
                Type = "Neighbor Exchange doc",
                Version = "1",
            };
        }
    }
}
