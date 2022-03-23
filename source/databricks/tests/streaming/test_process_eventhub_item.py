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
import shutil

from pyspark.sql import DataFrame, SparkSession
from geh_stream.event_dispatch.meteringpoint_dispatcher import meteringpoint_master_data_path, meteringpoint_dispatcher
from geh_stream.schemas import metering_point_schema
from geh_stream.schemas.integration_event_schema import integration_event_schema
from geh_stream.streaming_utils.events_data_lake_listener import incomming_event_handler

integration_events = [
        ("03c4fbb1-40e9-4063-8fa9-0af77c791354",
         "2022-03-22T10:59:45Z",
         "MeteringPointCreated",
         "MeteringPoint",
         "{\"metering_point_id\":\"574581420043388211\",\"metering_point_type\":\"E17\",\"grid_area\":\"112\",\"settlement_method\":\"E02\",\"metering_method\":\"D02\",\"resolution\":\"PT1H\",\"product\":\"8716867000030\",\"connection_state\":\"D03\",\"unit\":\"KWH\",\"effective_date\":\"2021-09-25T22:00:00Z\",\"Transaction\":{\"mRID\":\"72565eede2244c1c81265180b6da213e\"}}"),
        ("0a3fd770-2d8f-407d-8bac-2a01e4f08f1d",
         "2022-03-22T09:54:51Z",
         "MeteringPointCreated",
         "MeteringPoint",
         "{\"metering_point_id\":\"571655161401744159\",\"metering_point_type\":\"E17\",\"grid_area\":\"112\",\"settlement_method\":\"E02\",\"metering_method\":\"D02\",\"resolution\":\"PT1H\",\"product\":\"8716867000030\",\"connection_state\":\"D03\",\"unit\":\"KWH\",\"effective_date\":\"2021-09-25T22:00:00Z\",\"Transaction\":{\"mRID\":\"b1b9f3f01b6c42379b22d6bab3296a11\"}}"),
]


def test_incomming_event(spark: SparkSession):
    # we'll write locally so lets define a root path for that
    root = "/tmp/meteringpointtest"

    # handle and remove any previous data
    try:
        shutil.rmtree(root)
    except OSError as e:
        print("ignored: %s : %s" % (root, e.strerror))

    meteringpoint_dispatcher.set_master_data_root_path(root)

    # Create integration events dataframe based on our test data
    integration_events_df = spark.createDataFrame(integration_events, schema=integration_event_schema)

    # call the incomming_event_handler similar to the foreach batch
    incomming_event_handler(integration_events_df, 1)

    # load the generated meteringpoints
    mps = spark.read.format("delta").load(meteringpoint_master_data_path())

    # there should be two meteringpoints
    assert mps.count() == 2

    # make sure they look ok
    mpstypes = [f.dataType for f in mps.schema.fields]
    schematypes = [f.dataType for f in metering_point_schema.fields]

    for index in range(len(mpstypes)):
        assert mpstypes[index] == schematypes[index]
