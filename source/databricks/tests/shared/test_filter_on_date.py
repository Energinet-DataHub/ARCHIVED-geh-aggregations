# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from geh_stream.shared.filters import filter_on_date
from datetime import datetime
from tests.helpers.dataframe_creators.charges_creator import charges_factory
import pytest


def test_filter_on_date(spark, charges_factory):
    charges_df_1 = charges_factory(datetime(2020, 1, 4), datetime(2020, 1, 28))
    charges_df_2 = charges_factory(datetime(2020, 1, 26), datetime(2020, 1, 31))
    charges_df_3 = charges_factory(datetime(2020, 1, 20), datetime(2020, 1, 27))
    charges_df_4 = charges_factory(datetime(2020, 1, 10), datetime(2020, 1, 18))
    charges_df_5 = charges_factory(datetime(2020, 1, 5), datetime(2020, 1, 12))
    charges_df_6 = charges_factory(datetime(2020, 1, 2), datetime(2020, 1, 6))
    charges_df = charges_df_1.union(charges_df_2).union(charges_df_3).union(charges_df_4).union(charges_df_5).union(charges_df_6)
    filtered_df = filter_on_date(charges_df, datetime(2020, 1, 8), datetime(2020, 1, 24))
    filtered_df.show()
    assert True
