# Aggregations

## Intro

The aggregation domain is in charge of doing calculations on the time series sent to Green Energy Hub and executing the balance and wholesale settlement process.

The main calculations the domain is responsible to process are consumption, production, exchange between grid areas and the current grid loss within a grid area.  
All calculations return a result for grid area, balance responsible and energy supplier.

The times series sent to Green Energy Hub is processed and enriched in the [time series domain](https://github.com/Energinet-DataHub/geh-timeseries) before they can be picked up by the aggregation domain.

The calculated results are packaged and forwarded to the legimate recipients such as:

| Recipients |
| ----------- |
| Grid company  |
| Balance supplier |
| Energy supplier |

These are the business processes maintained by this domain:

| Processes |
| ----------- |
| [Submission of calculated energy time series](./docs/business-processes/submission-of-calculated-energy-time-series.md) |
| [Aggregation of wholesale services](./docs/business-processes/aggregation-of-whole-sale-services.md) |
| [Request for aggregated subscriptions or fees](./docs/business-processes/request-for-aggregated-subscriptions-or-fees.md) |
| [Request for aggregated tariffs](./docs/business-processes/request-for-aggregated-tariffs.md) |
| [Request for calculated energy time series](./docs/business-processes/request-for-calculated-energy-time-series.md) |
| [Rrequest for metered data](./docs/business-processes/request-for-metered-data.md) |
| [Request for historical data](./docs/business-processes/request-for-historical-data.md) |

## Architecture

![design](./docs/images/architecture.png)

## Dataflow between domains

## How do we do aggregations?

### Coordinator function

### Databricks workspace

### Databricks cluster

### Python code

### Dataframe results

## Input into the aggregation domain

### Delta lake (market evaluation points)

### Eventhub input (TBD)

## Output from the aggregation domain

### Format of the message

### Talking to the postoffice eventhub endpoint

## Getting started

### Setting up infrastructure

The instances able to run the aggregations are created with infrastructure as code (Terraform). The code for this can be found in
[./build](./build).
This IaC is triggered by github and the following describes how to get started with provisioning your own infrastructure.

(TBD) Link the general description of how Terraform and IaC works

(TBD) Info about the shared resources and the role of the keyvault

(TBD) Info about environments

### [Read more on aggregation infrastructure](./docs/setting-up-infrastructure.md)

## Test

Link to test.md

### Generating test data

### How can you generate test data in your delta lake

## Triggering aggregations via coordinator

## Viewing results of aggregations
