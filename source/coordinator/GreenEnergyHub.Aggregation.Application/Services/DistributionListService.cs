// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GreenEnergyHub.Aggregation.Application.Services;
using Microsoft.Extensions.Logging;

public class DistributionListService : IDistributionListService
{
    private const string MockedDataJson = @"[
  {
    ""GRID_AREA_CODE"": 911,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000706686"",
    ""RecipientPartyID_mRID"": ""'5790000706686""
  },
  {
    ""GRID_AREA_CODE"": 992,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'4033872000010"",
    ""RecipientPartyID_mRID"": ""'4033872000010""
  },
  {
    ""GRID_AREA_CODE"": 990,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'4260024590017"",
    ""RecipientPartyID_mRID"": ""'4260024590017""
  },
  {
    ""GRID_AREA_CODE"": 312,
    ""DELEGATIONS"": ""'5790000409266"",
    ""ORGANISATION_ID"": ""'5790000375318"",
    ""RecipientPartyID_mRID"": ""'5790000409266""
  },
  {
    ""GRID_AREA_CODE"": 991,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'7331507000006"",
    ""RecipientPartyID_mRID"": ""'7331507000006""
  },
  {
    ""GRID_AREA_CODE"": 950,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 960,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 951,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 952,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 962,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 953,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 3,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 7,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 961,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 371,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001095376"",
    ""RecipientPartyID_mRID"": ""'5790001095376""
  },
  {
    ""GRID_AREA_CODE"": 740,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000705184"",
    ""RecipientPartyID_mRID"": ""'5790000705184""
  },
  {
    ""GRID_AREA_CODE"": 854,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001088231"",
    ""RecipientPartyID_mRID"": ""'5790001088231""
  },
  {
    ""GRID_AREA_CODE"": 853,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001088460"",
    ""RecipientPartyID_mRID"": ""'5790001088460""
  },
  {
    ""GRID_AREA_CODE"": 860,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001089375"",
    ""RecipientPartyID_mRID"": ""'5790001089375""
  },
  {
    ""GRID_AREA_CODE"": 533,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000392551"",
    ""RecipientPartyID_mRID"": ""'5790000392551""
  },
  {
    ""GRID_AREA_CODE"": 16,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790002502699"",
    ""RecipientPartyID_mRID"": ""'5790002502699""
  },
  {
    ""GRID_AREA_CODE"": 152,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000681372"",
    ""RecipientPartyID_mRID"": ""'5790000681372""
  },
  {
    ""GRID_AREA_CODE"": 244,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000392261"",
    ""RecipientPartyID_mRID"": ""'5790000392261""
  },
  {
    ""GRID_AREA_CODE"": 347,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000395620"",
    ""RecipientPartyID_mRID"": ""'5790000395620""
  },
  {
    ""GRID_AREA_CODE"": 398,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001095413"",
    ""RecipientPartyID_mRID"": ""'5790001095413""
  },
  {
    ""GRID_AREA_CODE"": 396,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001095444"",
    ""RecipientPartyID_mRID"": ""'5790001095444""
  },
  {
    ""GRID_AREA_CODE"": 370,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001095451"",
    ""RecipientPartyID_mRID"": ""'5790001095451""
  },
  {
    ""GRID_AREA_CODE"": 154,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001100520"",
    ""RecipientPartyID_mRID"": ""'5790001100520""
  },
  {
    ""GRID_AREA_CODE"": 85,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001103460"",
    ""RecipientPartyID_mRID"": ""'5790001103460""
  },
  {
    ""GRID_AREA_CODE"": 233,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000610099"",
    ""RecipientPartyID_mRID"": ""'5790000610099""
  },
  {
    ""GRID_AREA_CODE"": 385,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000610822"",
    ""RecipientPartyID_mRID"": ""'5790000610822""
  },
  {
    ""GRID_AREA_CODE"": 381,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000610839"",
    ""RecipientPartyID_mRID"": ""'5790000610839""
  },
  {
    ""GRID_AREA_CODE"": 344,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000611003"",
    ""RecipientPartyID_mRID"": ""'5790000611003""
  },
  {
    ""GRID_AREA_CODE"": 42,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000681075"",
    ""RecipientPartyID_mRID"": ""'5790000681075""
  },
  {
    ""GRID_AREA_CODE"": 341,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000681105"",
    ""RecipientPartyID_mRID"": ""'5790000681105""
  },
  {
    ""GRID_AREA_CODE"": 348,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000681327"",
    ""RecipientPartyID_mRID"": ""'5790000681327""
  },
  {
    ""GRID_AREA_CODE"": 331,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000681358"",
    ""RecipientPartyID_mRID"": ""'5790000681358""
  },
  {
    ""GRID_AREA_CODE"": 342,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000682102"",
    ""RecipientPartyID_mRID"": ""'5790000682102""
  },
  {
    ""GRID_AREA_CODE"": 245,
    ""DELEGATIONS"": ""'5790002305245"",
    ""ORGANISATION_ID"": ""'5790000683345"",
    ""RecipientPartyID_mRID"": ""'5790002305245""
  },
  {
    ""GRID_AREA_CODE"": 151,
    ""DELEGATIONS"": ""'5790002305245"",
    ""ORGANISATION_ID"": ""'5790000704842"",
    ""RecipientPartyID_mRID"": ""'5790002305245""
  },
  {
    ""GRID_AREA_CODE"": 791,
    ""DELEGATIONS"": ""'5790000267958"",
    ""ORGANISATION_ID"": ""'5790000705689"",
    ""RecipientPartyID_mRID"": ""'5790000267958""
  },
  {
    ""GRID_AREA_CODE"": 384,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000706419"",
    ""RecipientPartyID_mRID"": ""'5790000706419""
  },
  {
    ""GRID_AREA_CODE"": 757,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000836239"",
    ""RecipientPartyID_mRID"": ""'5790000836239""
  },
  {
    ""GRID_AREA_CODE"": 531,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000836727"",
    ""RecipientPartyID_mRID"": ""'5790000836727""
  },
  {
    ""GRID_AREA_CODE"": 532,
    ""DELEGATIONS"": ""'5790001102364"",
    ""ORGANISATION_ID"": ""'5790001088217"",
    ""RecipientPartyID_mRID"": ""'5790001102364""
  },
  {
    ""GRID_AREA_CODE"": 357,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001088309"",
    ""RecipientPartyID_mRID"": ""'5790001088309""
  },
  {
    ""GRID_AREA_CODE"": 131,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001089030"",
    ""RecipientPartyID_mRID"": ""'5790001089030""
  },
  {
    ""GRID_AREA_CODE"": 351,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001090111"",
    ""RecipientPartyID_mRID"": ""'5790001090111""
  },
  {
    ""GRID_AREA_CODE"": 141,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001090166"",
    ""RecipientPartyID_mRID"": ""'5790001090166""
  },
  {
    ""GRID_AREA_CODE"": 84,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001095239"",
    ""RecipientPartyID_mRID"": ""'5790001095239""
  },
  {
    ""GRID_AREA_CODE"": 51,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001095277"",
    ""RecipientPartyID_mRID"": ""'5790001095277""
  },
  {
    ""GRID_AREA_CODE"": 755,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000836703"",
    ""RecipientPartyID_mRID"": ""'5790000836703""
  },
  {
    ""GRID_AREA_CODE"": 543,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000610976"",
    ""RecipientPartyID_mRID"": ""'5790000610976""
  },
  {
    ""GRID_AREA_CODE"": 512,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001087975"",
    ""RecipientPartyID_mRID"": ""'5790001087975""
  },
  {
    ""GRID_AREA_CODE"": 584,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790001089023"",
    ""RecipientPartyID_mRID"": ""'5790001089023""
  },
  {
    ""GRID_AREA_CODE"": 31,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000610877"",
    ""RecipientPartyID_mRID"": ""'5790000610877""
  },
  {
    ""GRID_AREA_CODE"": 954,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 955,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 963,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'5790000432752"",
    ""RecipientPartyID_mRID"": ""'5790000432752""
  },
  {
    ""GRID_AREA_CODE"": 993,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'7080000923168"",
    ""RecipientPartyID_mRID"": ""'7080000923168""
  },
  {
    ""GRID_AREA_CODE"": 996,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'7331507000006"",
    ""RecipientPartyID_mRID"": ""'7331507000006""
  },
  {
    ""GRID_AREA_CODE"": 997,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'7359991140008"",
    ""RecipientPartyID_mRID"": ""'7359991140008""
  },
  {
    ""GRID_AREA_CODE"": 994,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'8716867999976"",
    ""RecipientPartyID_mRID"": ""'8716867999976""
  },
  {
    """": 995,
    ""DELEGATIONS"": ""NULL"",
    ""ORGANISATION_ID"": ""'9910891000005"",
    ""RecipientPartyID_mRID"": ""'9910891000005""
  }
]";

    private readonly ILogger<DistributionListService> _logger;

    private readonly IEnumerable<DistributionItem> _listOfDistributionItems;

    public DistributionListService(ILogger<DistributionListService> logger)
    {
        _logger = logger;
        _listOfDistributionItems = JsonSerializer.Deserialize<IEnumerable<DistributionItem>>(MockedDataJson);
    }

    public string GetDistributionItem(string gridAreaCode)
    {
        var item = _listOfDistributionItems.SingleOrDefault(d => d.GridAreaCode.ToString().Equals(gridAreaCode));
        if (item == null)
        {
           _logger.LogInformation("Could not find gridAreaCode in DistributionListService {gridArea}", gridAreaCode);
        }

        return !string.Equals(item.Delegations, "NULL") ? item.Delegations : item.RecipientPartyId;
    }
}
