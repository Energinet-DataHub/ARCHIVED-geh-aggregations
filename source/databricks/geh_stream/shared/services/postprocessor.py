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
from geh_stream.shared.services import CoordinatorService
from pyspark.sql.functions import col, date_format


class PostProcessor:

    def __init__(self, args):
        self.coordinator_service = CoordinatorService(args)

    def do_post_processing(self, args, results, now_path_string):

        result_base = "Results"

        for key, value in results.items():
            path = f"{result_base}/{now_path_string}/{key}"
            result_path = f"abfss://{args.data_storage_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{path}"
            stringFormatedTimeDf = value.withColumn("time_start", date_format(col(Colname.time_window_start), DateFormat.iso_8601)) \
                .withColumn("time_end", date_format(col(Colname.time_window_end), DateFormat.iso_8601)) \
                .drop(Colname.time_window)
            stringFormatedTimeDf \
                .coalesce(1) \
                .write \
                .option("compression", "gzip") \
                .format('json').save(result_path)
            # self.coordinator_service.notify_coordinator(path) # TODO

    def store_basis_data(self, args, snapshot_data):
        snapshot_base = f"{args.persist_source_dataframe_location}/{args.result_id}"

        for key, value in snapshot_data.items():
            path = f"{snapshot_base}/{key}"
            snapshot_path = f"abfss://{args.data_storage_container_name}@{args.data_storage_account_name}.dfs.core.windows.net/{path}"

            value \
                .write \
                .option("compression", "snappy") \
                .save(snapshot_path)

        self.coordinator_service.notify_snapshot_coordinator(snapshot_base)
