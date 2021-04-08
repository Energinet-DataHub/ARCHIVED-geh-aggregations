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
