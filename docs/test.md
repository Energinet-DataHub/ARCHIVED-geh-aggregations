# Test

Read about general QA that applies to the entire Green Energy Hub [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/qa.md).

The aggregation domain has [Databricks](https://databricks.com/) jobs and libraries implemented in Python. Currently, the aggregation domain has a test suite of [pytest](https://pytest.org/) unit tests. Information about and instructions on how to execute these tests are outlined [here](./source/databricks/readme.md).

## Generating test data

The time series test data is created using the [databricks workbook](./source/databricks/test_data_creation/data_creator.py).

The creaton of testdata is based on [this file](./source/databricks/test_data_creation/test_data_csv.csv) generated from the current danish DataHub system. The testdata file consists of the following data properties:

| Data properties | Description |
| ----------- | ----------- |
| GridArea |  |
| Supplier | Energy supplier |
| Type_Of_MP | Type of market evaluation point eg. production, consumption, exchange |
| Physical_Status | Status of market evaluation point eg. new, connected, disconnected |  
| Settlement_Method |  |
| Reading_Occurrence | Resolution eg. 1 hour, 15 minutes etc. |
| Count |  |
| FromGridArea |  |
| ToGridArea |  |
| BRP | Balance responsible party |

## How can you generate test data in your delta lake

The [databricks workbook](./source/databricks/test_data_creation/data_creator.py) can be used to generate the amount of data needed and is currently configured to create 24 hours of time series data for more than 50 grid areas.

The generated time series data are setup to be stored in a delta lake wherefrom the [aggregation job](./source/databricks/aggregation-jobs/aggregation_trigger.py) fetches the data to run an aggregation upon.
