resource "databricks_cluster" "aggregation_autoscaling" {
  cluster_name            = "Aggregation Autoscaling"
  spark_version           = "7.2.x-scala2.12"
  node_type_id            = "Standard_DS3_v2"
  autotermination_minutes = 20
  autoscale {
    min_workers = 2
    max_workers = 8
  }

  library {
    pypi {
      package = "configargparse==1.2.3"
    }
  }

  library {
    pypi {
      package = "applicationinsights==0.11.9"
    }
  }

  library {
    pypi {
      package = "azure-storage-blob==12.8.0"
    }
  }

  library {
    pypi {
      package = "python-snappy==0.6.0"
    }
  }  

  library {
    whl = var.wheel_file
  }
}