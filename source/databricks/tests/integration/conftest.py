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

import os
import pyodbc
import pytest
import subprocess


@pytest.fixture(scope="session")
def databricks_path() -> str:
    """
    Returns the source/databricks folder path.
    Please note that this only works if current folder haven't been changed prior using us.chdir().
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the integration tests folder.
    """
    return os.path.dirname(os.path.realpath(__file__)) + "/../.."


@pytest.fixture(scope="session")
def source_path() -> str:
    """
    Returns the source/databricks folder path.
    Please note that this only works if current folder haven't been changed prior using us.chdir().
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the integration tests folder.
    """
    return os.path.dirname(os.path.realpath(__file__)) + "/../../.."


sql_server = "sql-server"
master_data_database = "master-data"
master_data_connection_string = f"Server={sql_server};Database={master_data_database};Trusted_Connection=True;"


@pytest.fixture(scope="session")
def master_data_database(source_path):
    # Create database if not exists
    conn = pyodbc.connect('DRIVER={SQL Server Native Client 11.0};'
                          'Server=sql-server;'
                          'Database=master;'
                          'Trusted_Connection=yes;')
    cursor = conn.cursor()
    cursor.execute(f"""
IF NOT EXISTS(SELECT name FROM sys.databases WHERE name = '{master_data_database}')
CREATE DATABASE {master_data_database}
""")

    # Build db migration program
    subprocess.call([
        "dotnet",
        "build",
        f"{source_path}/IntegrationEventListener/Energinet.DataHub.Aggregations.DatabaseMigration/Energinet.DataHub.Aggregations.DatabaseMigration.csproj"
    ])

    # Run db migrations
    subprocess.call([
        "dotnet",
        f"{source_path}/IntegrationEventListener/Energinet.DataHub.Aggregations.DatabaseMigration/bin/Debug/Energinet.DataHub.Aggregations.DatabaseMigration.dll",
        master_data_connection_string
    ])
