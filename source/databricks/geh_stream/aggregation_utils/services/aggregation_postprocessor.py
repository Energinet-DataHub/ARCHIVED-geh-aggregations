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


def now_path_string():
    return datetime.now().strftime('%Y-%m-%d_%H-%M-%S')


def do_post_processing(args, results):

    result_path = "Results"

    coordinator_service = CoordinatorService(args)
    blob_service = BlobService(args)

    for key, value in results.items():
        path = "{0}/{1}/{2}.json.gz".format(result_path, now_path_string(), key)
        blob_service.upload_blob(value, path)
        coordinator_service.notify_coordinator(path)


def store_basis_data(args, filtered):

    if args.persist_source_dataframe:
        coordinator_service = CoordinatorService(args)
        path = "{0}/{1}".format(args.args.persist_source_dataframe_location, now_path_string())
        filtered.write.option("compression", "snappy").save(path)
        coordinator_service.notify_coordinator(path)
