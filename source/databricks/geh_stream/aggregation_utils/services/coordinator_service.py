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

import requests
import gzip
from geh_stream.monitoring import Telemetry
import datetime


class CoordinatorService:


    def __init__(self, args):
        self.coordinator_url = args.result_url
        self.result_id = args.result_id
        self.process_type = args.process_type
        self.start_time = args.beginning_date_time
        self.end_time = args.end_date_time
        self.telemetry_client = Telemetry.create_telemetry_client(args.telemetry_instrumentation_key)

    def send_result_to_coordinator(self, result):
        TIMESTRING = "%Y-%m-%d %H:%M:%S"

        try:
            bytes = result.encode()
            headers = {'result-id': self.result_id,
                       'process-type': self.process_type,
                       'start-time': self.start_time,
                       'end-time': self.end_time,
                       'Content-Type': 'application/json',
                       'Content-Encoding': 'gzip'}

            request_body = gzip.compress(bytes)
            now = datetime.datetime.now()
            print("Just about to post " + str(len(request_body)) + " bytes at time " + now.strftime(TIMESTRING))
            response = requests.post(self.coordinator_url, data=request_body, headers=headers)
            now = datetime.datetime.now()
            print("We have posted the result and time is now " + now.strftime(TIMESTRING))
            if response.status_code != requests.codes['ok']:
                error = "Could not communicate with coordinator due to " + response.reason
                print(error)
                print(response.text)
                now = datetime.datetime.now()
                print(now.strftime(TIMESTRING))
                raise Exception(error)
        except Exception:
            self.telemetry_client.track_exception(Exception)
            print(Exception)
            raise Exception
        self.telemetry_client.flush()
