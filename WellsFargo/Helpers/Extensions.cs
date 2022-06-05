namespace WellsFargo.Helpers
{
    public static class Extensions
    {
        public static IEnumerable<string> GetFromCsv(this IFormFile upload)
        {
            using (var fileStream = upload.OpenReadStream())
            using (var reader = new StreamReader(fileStream))
            {
                string row;
                while (!reader.EndOfStream)
                {
                    row = reader.ReadLine();
                    yield return row;
                }
            }
        }
    }
}
