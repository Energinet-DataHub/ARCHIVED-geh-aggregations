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

from geh_stream.aggregation_utils.services import CoordinatorService
from geh_stream.aggregation_utils.services import BlobService


def do_post_processing(args, results):

    nowstring = datetime.now().strftime('%Y-%m-%d_%H-%M-%S')

    result_path = "Results"

    coordinator_service = CoordinatorService(args)
    blob_service = BlobService(args)

    for key, value in results.items():
        path = "{0}/{1}/{2}.json.gz".format(result_path, nowstring, key)
        blob_service.upload_blob(value, path)
        coordinator_service.notify_coordinator(path)
