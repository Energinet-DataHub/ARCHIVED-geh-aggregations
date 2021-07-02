FROM jupyter/pyspark-notebook:45bfe5a474fa

COPY spark-defaults.conf /spark-defaults.conf
COPY entrypoint.sh /entrypoint.sh

RUN conda install --quiet --yes --satisfied-skip-solve \
    'rope=0.18.*' 'pytest=6.1.*' 'pylint=2.6.*' 'autopep8=1.5.*' 'configargparse=1.2.3' 'applicationinsights=0.11.9' 'coverage=5.3.*' \
    'pytest-mock=3.5.*' \
    && \
    conda clean --all -f -y && \
    fix-permissions "${CONDA_DIR}" && \
    fix-permissions "/home/${NB_USER}"

USER root

RUN sudo ln -s `(realpath /spark-defaults.conf)` $SPARK_HOME/conf/spark-defaults.conf

ENTRYPOINT ["/entrypoint.sh"]
