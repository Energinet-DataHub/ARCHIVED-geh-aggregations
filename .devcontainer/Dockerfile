FROM jupyter/pyspark-notebook:spark-3.1.2

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

USER root

# This replaces the default spark configuration in the docker image with the ones defined in the sibling file
COPY spark-defaults.conf $SPARK_HOME/conf/spark-defaults.conf

# Install ODBC drivers for pyodbc
# See https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-ver15
RUN apt-get update && apt-get install -y gnupg && apt-get install -y curl
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list
RUN apt-get update
ENV ACCEPT_EULA=Y
RUN apt-get install -y msodbcsql18 unixodbc-dev

# Install spark packages with mamba 
RUN mamba install --quiet --yes --satisfied-skip-solve \
    'pyarrow=2.0.*' 'rope=0.18.*' 'pytest=7.1.*' 'autopep8=1.5.*' 'configargparse=1.2.3'  \
    'coverage=6.3.*' 'azure-storage-blob=12.8.*' 'pytest-mock=3.7.*'

# Install python packages used in pyspark development 
RUN pip --no-cache-dir install pyspelling azure-eventhub coverage-threshold ptvsd azure-servicebus \
    protodf delta-spark pytest-asyncio flake8 black dataclasses-json pyodbc

# Below will make everything in the directory owned by the group ${NB_GID}
RUN fix-permissions "${CONDA_DIR}"

# Set misc environment variables required for properly run spark 
# note the amount of memory used on the driver is adjusted here
ENV PATH=$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH \
    PYTHONPATH="${SPARK_HOME}/python:${SPARK_HOME}/python/lib/py4j-0.10.9-src.zip" \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"
