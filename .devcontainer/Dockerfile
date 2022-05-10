# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

FROM databricksruntime/python:9.x
#SHELL ["/bin/bash", "-o", "pipefail", "-c"]

#USER root

# This replaces the default spark configuration in the docker image with the ones defined in the sibling file
COPY spark-defaults.conf $SPARK_HOME/conf/spark-defaults.conf

# Install ODBC drivers for pyodbc
# See https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-ver15
RUN apt-get update
RUN apt-get install -y gnupg curl g++ wget
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list
RUN apt-get update
ENV ACCEPT_EULA=Y
RUN apt-get install -y msodbcsql18 unixodbc-dev python3.8-dev 

# Install spark packages with pip 
RUN /databricks/python3/bin/pip --no-cache-dir install \
   pyarrow==2.0.* rope==0.18.* pytest==7.1.* autopep8==1.5.* ConfigArgParse==1.2.3  \
   coverage==6.3.* azure-storage-blob==12.8.* pytest-mock==3.7.*


# Install python packages used in pyspark development 
RUN /databricks/python3/bin/pip --no-cache-dir install pyspelling azure-eventhub coverage-threshold ptvsd azure-servicebus \
    delta-spark pytest-asyncio flake8 black dataclasses-json pyodbc
# Below will make everything in the directory owned by the group ${NB_GID}
#RUN fix-permissions "${CONDA_DIR}"

# HADOOP
ENV HADOOP_VERSION 3.3.0
ENV HADOOP_HOME /usr/hadoop-$HADOOP_VERSION
ENV HADOOP_CONF_DIR=$HADOOP_HOME/etc/hadoop
ENV PATH $PATH:$HADOOP_HOME/bin
RUN curl -sL --retry 3 \
  "http://archive.apache.org/dist/hadoop/common/hadoop-$HADOOP_VERSION/hadoop-$HADOOP_VERSION.tar.gz" \
  | gunzip \
  | tar -x -C /usr/ \
 && rm -rf $HADOOP_HOME/share/doc \
 && chown -R root:root $HADOOP_HOME

# SPARK
ENV SPARK_VERSION 3.1.2
ENV SPARK_PACKAGE spark-${SPARK_VERSION}-bin-without-hadoop
ENV SPARK_HOME /usr/spark-${SPARK_VERSION}
ENV SPARK_DIST_CLASSPATH="$HADOOP_HOME/etc/hadoop/*:$HADOOP_HOME/share/hadoop/common/lib/*:$HADOOP_HOME/share/hadoop/common/*:$HADOOP_HOME/share/hadoop/hdfs/*:$HADOOP_HOME/share/hadoop/hdfs/lib/*:$HADOOP_HOME/share/hadoop/hdfs/*:$HADOOP_HOME/share/hadoop/yarn/lib/*:$HADOOP_HOME/share/hadoop/yarn/*:$HADOOP_HOME/share/hadoop/mapreduce/lib/*:$HADOOP_HOME/share/hadoop/mapreduce/*:$HADOOP_HOME/share/hadoop/tools/lib/*"
ENV PATH $PATH:${SPARK_HOME}/bin
RUN curl -sL --retry 3 \
  "https://archive.apache.org/dist/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tgz" \
  | gunzip \
  | tar x -C /usr/ \
 && mv /usr/$SPARK_PACKAGE $SPARK_HOME \
 && chown -R root:root $SPARK_HOME

ENV SCALA_VERSION 2.12.10
RUN curl -fsL "https://downloads.typesafe.com/scala/$SCALA_VERSION/scala-$SCALA_VERSION.tgz" | tar xfz - -C .

# Set misc environment variables required for properly run spark 
# note the amount of memory used on the driver is adjusted here
ENV PATH=$SCALA_VERSION:$SPARK_HOME/bin:$SPARK_HOME/python:$SPARK_HOME/python/lib/py4j-0.10.9-src.zip:$HADOOP_HOME/bin:/databricks/python3/bin:$PATH \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"

# Add .NET 5 SDK (used to build and execute database migration project in integration test setup)
RUN apt-get update; \
    apt-get install -y apt-transport-https && \
    apt-get update && \
    apt-get install -y dotnet-sdk-5.0

# Add SQL Server connector for spark (used to connect to database in integration tests)
RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/com/microsoft/azure/spark-mssql-connector_2.12/1.2.0/spark-mssql-connector_2.12-1.2.0.jar
RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/com/microsoft/sqlserver/mssql-jdbc/8.4.1.jre11/mssql-jdbc-8.4.1.jre11.jar
RUN wget -P $SPARK_HOME/jars https://search.maven.org/remotecontent?filepath=io/delta/delta-core_2.12/1.2.1/delta-core_2.12-1.2.1.jar

ENTRYPOINT echo hello && sleep infinity 