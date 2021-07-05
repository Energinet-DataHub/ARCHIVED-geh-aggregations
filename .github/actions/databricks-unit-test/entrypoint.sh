#!/bin/sh -l

cd ./source/databricks
python setup.py install
pytest tests/
