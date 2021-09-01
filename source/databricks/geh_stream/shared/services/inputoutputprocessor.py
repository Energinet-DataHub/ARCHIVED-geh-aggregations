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


from geh_stream.codelists import Colname, DateFormat
from geh_stream.shared.services import CoordinatorService, StorageAccountService
from pyspark.sql.functions import col, date_format
from pyspark.sql import DataFrame


class InputOutputProcessor:

    def __init__(self, args):
        self.coordinator_service = CoordinatorService(args)
        self.snapshot_base = f"{args.persist_source_dataframe_location}/{args.result_id}"
        self.data_storage_container_name = args.data_storage_container_name
        self.data_storage_account_name = args.data_storage_account_name

    def do_post_processing(self, process_type, result_id, result_url, results):

        result_base = f"Results/{process_type}/{result_id}"

        for key, dataframe, in results.items():
            path = f"{result_base}/{key}"
            result_path = StorageAccountService.get_storage_account_full_path(self.data_storage_container_name, self.data_storage_account_name, path)

            if dataframe is not None:
                dataframe \
                    .coalesce(1) \
                    .write \
                    .option("compression", "gzip") \
                    .format('json').save(result_path)

                self.coordinator_service.notify_coordinator(result_url, path)

    def store_basis_data(self, snapshot_url, snapshot_data):

        for key, dataframe in snapshot_data.items():
            path = f"{self.snapshot_base}/{key}"
            snapshot_path = StorageAccountService.get_storage_account_full_path(self.data_storage_container_name, self.data_storage_account_name, path)

            if dataframe is not None:
                dataframe \
                    .write \
                    .format("delta") \
                    .option("compression", "snappy") \
                    .save(snapshot_path)

                self.coordinator_service.notify_snapshot_coordinator(snapshot_url, self.snapshot_base)

    def load_basis_data(self, spark, key) -> DataFrame:
        path = f"{self.snapshot_base}/{key}"
        snapshot_path = StorageAccountService.get_storage_account_full_path(self.data_storage_container_name, self.data_storage_account_name, path)

        df = spark \
            .read \
            .format("delta") \
            .load(snapshot_path)
        return df
