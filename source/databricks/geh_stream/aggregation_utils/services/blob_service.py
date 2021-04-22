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
import snappy
import json


def upload_blob(args, data, blob_name):

    CONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix=core.windows.net" \
                       .format(args.input_storage_account_name, args.input_storage_account_key)
    containerName = args.input_storage_container_name
    jsonObj = data.toJSON().collect()
    jsonStr = json.dumps(jsonObj)
    snappyData = snappy.compress(jsonStr)
    blob_service_client = BlobServiceClient.from_connection_string(CONNECTIONSTRING)
    blob_client = blob_service_client.get_blob_client(container=containerName, blob=blob_name)
    try:
        blob_client.get_blob_properties()
        blob_client.delete_blob()
    except ResourceNotFoundError:
        pass
    blob_client.upload_blob(snappyData)
