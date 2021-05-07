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
````bash
./debugz.sh
````

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

If you are met by a error message related to `bad interpreter` it's because the bash shell expects `LF` as line ending according to [this article](https://ztirom.at/2016/01/resolving-binbashm-bad-interpreter-when-writing-a-shellscript-on-windows-with-vs-code-and-run-it-on-linux/)
