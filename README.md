# Aggregations

## Intro

The aggregation domain is in charge of doing calculations on the time series sent to Green Energy Hub and executing the balance and wholesale settlement process.

The main calculations the domain is responsible to process are consumption, production, exchange between grid areas and the current grid loss within a grid area.  
All calculations return a result for grid area, balance responsible and energy supplier.

The times series sent to Green Energy Hub is processed and enriched in the [time series domain](https://github.com/Energinet-DataHub/geh-timeseries) before they can be picked up by the aggregation domain.

The calculated results are packaged and forwarded to the relevant receivers such as:

| Receiver |
| ----------- |
| Grid operator  |
| Balance responsible |
| Energy supplier |
| ... |

These are the business processes maintained by this domain:

| Processes |
| ----------- |
| Balance settlement |
| Wholesale settlement |
| Request of aggregated time series, aggregated tariffs and historic data |
| ... |

## Architecture

![design](./docs/images/architecture.png)

## Dataflow between domains

---

## How do we do aggregations?

The aggregations/calculations of the market evaluation points stored in the delta lake are done by databricks jobs containing
python code utilizing [pyspark](https://databricks.com/glossary/pyspark).

### Coordinator function

The coordinator has a descriptive name in the sense that it does what it says on the tin.
It allows an external entity to trigger an aggregation job via a http interface.

[Peek here to see we start and manage databricks from the coordinator](https://github.com/Energinet-DataHub/geh-aggregations/blob/d7750efc6a3c172a0ea69775fa5a157ecd4c9481/source/coordinator/GreenEnergyHub.Aggregation.Application/Coordinator/CoordinatorService.cs#L64)
Once the calculations are done the databricks jobs sends the results back to the coordinator for further processing.

### Databricks workspace

This is the instance in which the databricks cluster resides.
(TBD) When the instance is in the shared domain, describe that.

### Databricks cluster

The databricks cluster is configured via [a specific workflow](./.github/workflows/aggregation-job-infra-cd.yml) that picks up the [generated wheel file](./.github/workflows/build-publish-wheel-file.yml) containing the code for the aggregations. This wheel file is installed as a library allowing all the workers in the cluster to use that code.

### Python code

The aggregation job itself is defined by python code. The code is both compiled into a wheel file and a python file triggered by the job.
The starting point for the databricks job is in [./source/databricks/aggregation-jobs/aggregation_trigger.py](./source/databricks/aggregation-jobs/aggregation_trigger.py)
The specific aggregations in [.\source\databricks\geh_stream\aggregation_utils\aggregators](./source/databricks/geh_stream/aggregation_utils/aggregators) these are compiled into a wheel file and installed as a library on the cluster.

### Dataframe results

The results of the aggregation [dataframes](https://databricks.com/glossary/what-are-dataframes) are combined in [aggregation_trigger.py](./source/databricks/aggregation-jobs/aggregation_trigger.py) and then sent back to the coordinator as json

---

## Input into the aggregation domain

### Delta lake (market evaluation points)

### Eventhub input (TBD)

---

## Output from the aggregation domain

### Format of the message

### Talking to the postoffice eventhub endpoint

---

## Getting started

### Setting up infrastructure

The instances able to run the aggregations are created with infrastructure as code (Terraform). The code for this can be found in
[./build](./build).
This IaC is triggered by github and the following describes how to get started with provisioning your own infrastructure.

(TBD) Link the general description of how Terraform and IaC works

(TBD) Info about the shared resources and the role of the keyvault

(TBD) Info about environments

### [Read more on aggregation infrastructure](./docs/setting-up-infrastructure.md)

---

## Test

Link to test.md

### Generating test data

### How can you generate test data in your delta lake

## Triggering aggregations via coordinator

An example:

```URL

https://azfun-coordinator-aggregations-XXXXXX.azurewebsites.net/api/KickStartJob?beginTime=2013-01-01T23%3A00%3A00%2B0100&endTime=2013-01-30T23%3A00%3A00%2B0100&processType=D03
```

This will ask the coordinator to do an aggregation in the specified time frame with process type D03

## Viewing results of aggregations
