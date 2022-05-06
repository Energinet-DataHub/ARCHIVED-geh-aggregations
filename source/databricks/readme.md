# Python Environment

## Source Code

Source code is mainly located in folder `geh_stream`. This folder also constitutes the functionality of the `geh_stream` [wheel](https://pythonwheels.com/) package.

## Unit test with Pytest

[Pytest](https://pytest.org/) is used for unit testing.

### Testing Pyspark using fixture factories

It is quite cumbersome to unit test pyspark with data frames with a large number of columns.

In order to do this various concepts have been invented. In general you should start searching in the `conftest.py` files.

One of the probably hardest concepts to understand from reading code is the fixture factory concept. Basically it has the following form:

```python
@pytest.fixture(scope="session")
def enriched_data_factory(dependency1, dependency2):
    def factory(col1="default value",
                col2="default value",
                ...):
        result = {calculate from default values and dependencies}
        return result
    return factory
```

Then a test can depend on this fixture without any other transient dependency like especially the `SparkSession`.

By providing default values for columns also allow tests to have to specify values they care about.

### Usage python package geh_stream

#### Install on your environment

`python setup.py install`

#### Create wheel

`python setup.py sdist bdist_wheel`

#### Run tests based on local changes

`python -m pytest tests`

#### Run tests based on installed package

`pytest tests`

## Test coverage

Test coverage can be calculated by executing the script `create_coverage_report.sh`. This generates the HTML report `htmlcov/index.html`.

## Attach vs code debugger to pytest

Running the `debugz.sh` script in 'source\databricks' allows you to debug the pytests with VS code:

```bash

./debugz.sh

```

In your `launch.json` file add the following configuration:

```json

{
    "name": "Python: Attach container",
    "type": "python",
    "request": "attach",
    "port": 3000,
    "host": "localhost"
}

```

You can now launch your [VS code debugger](https://code.visualstudio.com/docs/editor/debugging#_launch-configurations) with the "Python: Attach container" configuration.

## Attach vs code debugger to Python file

You can now execute the [aggregation job](https://github.com/Energinet-DataHub/geh-aggregations/blob/main/source/databricks/aggregation-jobs/aggregation_trigger.py)
locally in vs code against your setup resources with the "Python: Current File" configuration.

In your `launch.json` file add the following configuration (the arguments below are a subset of the entire list taken from the top of [aggregation_trigger.py](https://github.com/Energinet-DataHub/geh-aggregations/blob/main/source/databricks/aggregation-jobs/aggregation_trigger.py)):

```json

{
    "name": "Python: Current File",
    "type": "python",
    "request": "launch",
    "program": "${file}",
    "console": "integratedTerminal",
    "args":[
        "--data-storage-account-name", <insert storage account name>,
        "--data-storage-account-key",<insert storage account key>,
        "--data-storage-container-name", <insert storage container name>,
        "--beginning-date-time", <insert beginning date time>,
        "--end-date-time",<insert end date time>,
        "--process-type", <insert process type>,
        "--result-url", <insert result url> ,
        "--job-id", <insert result id>,
        "--snapshot-notify-url", <insert snapshot notify url>,
        "--resolution", <insert resolution eg. 60 miutes>
    ]
}

```

Further information can be seen [here](https://code.visualstudio.com/docs/python/debugging#_initialize-configurations)
