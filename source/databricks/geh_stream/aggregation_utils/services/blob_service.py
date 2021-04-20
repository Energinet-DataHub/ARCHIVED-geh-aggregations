from azure.storage.blob import BlobServiceClient
from azure.core.exceptions import ResourceNotFoundError
import snappy
import json


class BlobService:

    def upload_blob(self, data, blob_name):

        jsonObj = data.toJSON().collect()
        jsonStr = json.dumps(jsonObj)
        snappyData = snappy.compress(jsonStr)
        blob_service_client = BlobServiceClient.from_connection_string("DefaultEndpointsProtocol=https;AccountName=timeseriesdatajouless;AccountKey=KIGkX7XIGilmUwADO5qT7k4AzFk8S0nl5QmfBD3+ktHxqhoVwfQUPeV7DpMxvhmTNVe/bcdJwA6PIyUHuvqKZw==;EndpointSuffix=core.windows.net")
        blob_client = blob_service_client.get_blob_client(container="messagedata", blob=blob_name)
        try:
            blob_client.get_blob_properties()
            blob_client.delete_blob()
        except ResourceNotFoundError:
            pass
        blob_client.upload_blob(snappyData)
