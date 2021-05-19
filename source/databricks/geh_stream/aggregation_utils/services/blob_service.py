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

from azure.storage.blob import BlobServiceClient
from azure.core.exceptions import ResourceNotFoundError
from pyspark.sql.functions import col, date_format
import gzip
import json
import datetime


class BlobService:

    def __init__(self, args):
        CONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net".format(args.input_storage_account_name, args.input_storage_account_key)
        self.blob_service_client = BlobServiceClient.from_connection_string(CONNECTIONSTRING)
        self.containerName = args.input_storage_container_name


    def upload_blob(self, data, blob_name):

        stringFormatedTimeDf = data.withColumn("time_start", date_format(col("time_window.start"), "yyyy-MM-dd'T'HH:mm:ss'Z'")) \
                                   .withColumn("time_end", date_format(col("time_window.end"), "yyyy-MM-dd'T'HH:mm:ss'Z'")) \
                                   .drop("time_window")
        stringFormatedTimeDf.coalesce(1).write.format('json').option("compression", "org.apache.hadoop.io.compress.GzipCodec").save(result_path)