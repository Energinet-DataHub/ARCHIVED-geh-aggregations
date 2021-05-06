#!/bin/sh -l

cd ./source/databricks/
python setup.py install
# python coverage-threshold install
pip install coverage
coverage run --branch -m pytest tests/
# Create data for threshold evaluation
coverage xml

bash <(curl -s https://codecov.io/bash)