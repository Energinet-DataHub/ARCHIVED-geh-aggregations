# Introduction 

The instances able to run the aggregations are created with infrastructure as code (Terraform). The code for this can be found in
[./build](./build).
This IaC is triggered by github and the following describes how to get started with provisioning your own infrastructure.

(TBD) Link the general description of how Terraform and IaC works

(TBD) Info about the shared resources and the role of the keyvault

(TBD) Info about environments

## Azure requirements

You will need a service principal with sufficient credentials that is going to be used by github to setup the infrastructure on your subscription

## Preparing secrets in github

The Terraform relies on secrets living in github.
You can find the secrets in Settings/Secrets and you will need:

- TENANT_ID (The tennant ID of your azure subscription)
- SUBSCRIPTION_ID (The azure subscription )
- SPN_SECRET (The secrect of your Service principal)
- SPN_OBJECT_ID (The object id of your Service principal)
- SPN_ID (The id of your Service principal)
- SHAREDRESOURCES_RESOURCE_GROUP_NAME (The resource group of the shared keyvault)
- SHAREDRESOURCES_KEYVAULT_NAME (the name of the shared keyvault)

## Github workflows

The aggregation domain has the following github workflows and they should be run in the order listed below:

- Infrastructure CD
- Build and Publish Wheel File
- Databricks Aggregation Job Infrastructure Deploy
- Coordinator CD

There are some supporting workflows as well that does linting on the code on PR creation

## Configuring sizes of instances

You are able to scale your instances, if you for example if you need to scale your aggregations, by setting this up in terraform.

---

- [Azure Coordinator function terraform](..\build\infrastructure\azfun-coordinator.tf)

Here you can set your plan in *azfun_coordinator_plan*

```JSON
  sku                 = {
    tier  = "Free"
    size  = "F1"
  }
```

---

- [Databricks clusters](..\databricks_aggregation_cluster\main.tf)

Here you can set the autoscaling of the clusters that do the aggregations *aggregation_autoscaling*

```JSON
  autoscale {
    min_workers = 2
    max_workers = 8
  }
```
