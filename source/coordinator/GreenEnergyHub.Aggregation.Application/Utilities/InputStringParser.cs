namespace GreenEnergyHub.Aggregation.Application.Utilities
{
    /// <summary>
    /// this class takes care of parsing the input path coming from the databricks job
    /// </summary>
    public static class InputStringParser
    {
        public static string ParseJobPath(string path)
        {
            // source is like {0}/{1}/{2}.json.snappy
            var folderSplit = path.Split('/')[2];
            var pathSplit = folderSplit.Split('.')[0];
            return pathSplit;
        }
    }
}
