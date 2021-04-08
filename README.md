# Aggregations

## Intro

The aggregation domain is in charge of doing calculations upon timeseries sent in by the market actors.

The calculated results are forwarded to the relevant market actors such as:

Grid operator
Energy supplier
Balance responsible

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
