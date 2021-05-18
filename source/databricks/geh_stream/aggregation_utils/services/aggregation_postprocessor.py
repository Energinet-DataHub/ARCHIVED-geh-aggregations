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


class PostProcessor:

    def __init__(self, args):
        self.coordinator_service = CoordinatorService(args)
        self.blob_service = BlobService(args)


    def do_post_processing(self, args, results, now_path_string):

        result_path = "Results"

        for key, value in results.items():
            path = "{0}/{1}/{2}.json.gz".format(result_path, now_path_string, key)
            self.blob_service.upload_blob(value, path)
            self.coordinator_service.notify_coordinator(path)

    def store_basis_data(self, args, filtered, now_path_string):

        if args.persist_source_dataframe:
            snapshot_path = "abfss://{0}@{1}.dfs.core.windows.net/{2}/{3}".format(args.input_storage_container_name, args.input_storage_account_name, args.persist_source_dataframe_location, now_path_string)
            print("We are snapshotting " + str(filtered.count()) + " dataframes to " + snapshot_path)
            filtered.write.option("compression", "snappy").save(snapshot_path)
            self.coordinator_service.notify_snapshot_coordinator(snapshot_path)
