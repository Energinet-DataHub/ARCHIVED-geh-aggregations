from pyspark.sql.types import StructType, StructField, StringType

charges_schema = StructType([
      StructField("id", StringType(), False),
])

charges = [
    ("001-D01-001"),
    ("001-D01-001"),
    ("001-D01-001"),
    ("001-D01-001")
]


def test_bar():
    i = 42
    pass


def test_foo(spark):
    df = spark.createDataFrame(charges, schema=charges_schema)
    df.show()