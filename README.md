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

These are the business processes maintained by this domain:

| Processes |
| ----------- |
| [Balance settlement](./docs/business-processes/balance-settlement) |
| [Wholesale settlement](./docs/business-processes/wholesale-settlement) |
| [Request of aggregated time series](./docs/business-processes/request-of-aggregated-time-series) |
| [Request of aggregated tariffs](./docs/business-processes/request-of-aggregated-tariffs) |
| [Request of historic data](./docs/business-processes/request-of-historic-data) |

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
