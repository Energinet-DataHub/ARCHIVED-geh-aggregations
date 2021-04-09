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

These are the business processes maintanined by this domain:

| Processes |
| ----------- |
| Balnce settlement |
| Wholesale settlement |
| Request of aggregated time series, aggregated tariffs and historic data |
| ... |

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

#### Preparing secrets in github

#### Github workflows

#### Configuring sizes of instances

##### Azure function

##### Cluster sizes

##### Order of actions to run

## Test

Link to test.md

### Generating test data

### How can you generate test data in your delta lake

## Triggering aggregations via coordinator

## Viewing results of aggregations
