#!/bin/sh -l

cd ./source/databricks/tests/
#Build wheel
python setup.py install
# python coverage-threshold install
pip install coverage-threshold
coverage run --branch -m pytest .
# Create data for threshold evaluation
coverage json
# Create human reader friendly HTML report
coverage html
coverage-threshold --line-coverage-min 50