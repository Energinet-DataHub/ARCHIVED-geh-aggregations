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
from datetime import datetime
from geh_stream.shared.period import parse_period, Period
import pytest
from dateutil import tz
from dateutil.parser._parser import ParserError


@pytest.mark.parametrize(
    "beginning_date_time,end_date_time,expected_beginning_date_time,expected_end_date_time",
    [("2020-01-01T00:00:00+0000", "2022-02-02T00:00:00+0000", datetime(2020, 1, 1, tzinfo=tz.tzutc()), datetime(2022, 2, 2, tzinfo=tz.tzutc())),
     ("2021-09-05T22:00:00Z", "2022-09-05T22:00:00Z", datetime(2021, 9, 5, 22, tzinfo=tz.tzutc()), datetime(2022, 9, 5, 22, tzinfo=tz.tzutc()))])
def test_period(
    beginning_date_time,
    end_date_time,
    expected_beginning_date_time,
    expected_end_date_time
):
    actual = parse_period(beginning_date_time, end_date_time)

    assert actual.from_date == expected_beginning_date_time
    assert actual.to_date == expected_end_date_time


@pytest.mark.parametrize("date_time", ["", "a", " ", "2021-19-15T22:00:00Z"])
def test_throws_exception_when_nonvalid_datetime_string(date_time):
    with pytest.raises(ParserError):
        parse_period(date_time, date_time)


def test_throws_exception_when_NoneType_datetime():
    with pytest.raises(TypeError):
        parse_period(None, None)
