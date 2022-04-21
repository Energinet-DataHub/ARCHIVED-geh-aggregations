# Fixed to specific version because of build errors
FROM jupyter/scipy-notebook:016833b15ceb

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

USER root

ARG openjdk_version="8"

ARG hadoop_version="3.3.2"

ARG spark_version="3.1.2"

ARG py4j_version="0.10.9"

ENV APACHE_SPARK_VERSION="${spark_version}" \
    HADOOP_VERSION="${hadoop_version}"

RUN apt-get -y update && \
    apt-get install --no-install-recommends -y \
    "openjdk-${openjdk_version}-jre-headless" \
    "openssh-client" "git" \
    "aspell" "aspell-en" \
    "flake8" \
    "ca-certificates-java" && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ENV JAVA_HOME=/usr/lib/jvm/java-${openjdk_version}-openjdk-amd64 \
    PATH=$JAVA_HOME/bin:$PATH

# Spark and Hadoop installation
WORKDIR /tmp

RUN wget -q $(wget -qO- https://www.apache.org/dyn/closer.cgi/hadoop/common/hadoop-${HADOOP_VERSION}/hadoop-${HADOOP_VERSION}.tar.gz\?as_json | \
    python -c "import sys, json; content=json.load(sys.stdin); print(content['preferred']+content['path_info'])") && \
    tar xzf "hadoop-${HADOOP_VERSION}.tar.gz" -C /usr/local --owner root --group root --no-same-owner && \
    rm "hadoop-${HADOOP_VERSION}.tar.gz"

RUN wget -q $(wget -qO- https://www.apache.org/dyn/closer.lua/spark/spark-${APACHE_SPARK_VERSION}/spark-${APACHE_SPARK_VERSION}-bin-without-hadoop.tgz\?as_json | \
    python -c "import sys, json; content=json.load(sys.stdin); print(content['preferred']+content['path_info'])") && \
    tar xzf "spark-${APACHE_SPARK_VERSION}-bin-without-hadoop.tgz" -C /usr/local --owner root --group root --no-same-owner && \
    rm "spark-${APACHE_SPARK_VERSION}-bin-without-hadoop.tgz"


WORKDIR /usr/local
RUN ln -s "spark-${APACHE_SPARK_VERSION}-bin-without-hadoop" spark && \
    ln -s "hadoop-${HADOOP_VERSION}" hadoop

# Configure Spark
ENV SPARK_HOME=/usr/local/spark \
    HADOOP_HOME=/usr/local/hadoop
    #PYSPARK_SUBMIT_ARGS='--packages io.delta:delta-core_2.12:1.0.0,org.apache.hadoop:hadoop-azure:3.3.2,org.apache.hadoop:hadoop-client:3.3.2 pyspark-shell'

ENV PATH=$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH \
    PYTHONPATH="${SPARK_HOME}/python:${SPARK_HOME}/python/lib/py4j-${py4j_version}-src.zip" \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"

# Install spark dependencies
RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/io/delta/delta-core_2.12/1.0.1/delta-core_2.12-1.0.1.jar
RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/org/apache/hadoop/hadoop-azure/3.3.2/hadoop-azure-3.3.2.jar
RUN wget -P $SPARK_HOME/jars https://repo1.maven.org/maven2/com/microsoft/azure/spark-mssql-connector_2.12/1.2.0/spark-mssql-connector_2.12-1.2.0.jar

USER $NB_UID

ENV PATH=$JAVA_HOME/bin:$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH
RUN echo "export SPARK_DIST_CLASSPATH=$(hadoop classpath)" >> $HOME/.bashrc

WORKDIR $HOME

# Install `sqlcmd` command line tool and ODBC drivers for pyodbc
# See https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-ver15
USER root
RUN apt-get update && apt-get install -y gnupg && apt-get install -y curl
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list
RUN apt-get update
ENV ACCEPT_EULA=Y
RUN apt-get install -y msodbcsql18 mssql-tools18 unixodbc-dev
USER $NB_UID
RUN echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
RUN source ~/.bashrc

RUN conda install --quiet --yes --satisfied-skip-solve \
    'pyarrow=2.0.*' 'rope=0.18.*' 'pytest=6.1.*' 'autopep8=1.5.*' 'configargparse=1.2.3' 'applicationinsights=0.11.9' \
    'coverage=5.3.*' 'azure-storage-blob=12.8.*' 'pytest-mock=3.5.*' \
    && \
    pip --no-cache-dir install pyspelling azure-eventhub coverage-threshold ptvsd azure-servicebus delta-spark dataclasses-json pyodbc && \
    conda clean --all -f -y && \
    fix-permissions "${CONDA_DIR}" && \
    fix-permissions "/home/${NB_USER}"

EXPOSE 3000

USER root
