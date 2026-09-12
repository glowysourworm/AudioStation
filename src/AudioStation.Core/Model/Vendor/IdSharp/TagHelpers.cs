namespace AudioStation.Core.Model.Vendor.IdSharp
{
    public static class TagHelpers
    {

        /// <summary>
        /// Parses [TRK] or [TPOS] tag value which should have the format "#/#" (position / total)
        /// </summary>
        /// <returns>Success indicator</returns>
        public static bool ID3V2Parse_TRK_or_TPOS(string tagValue, out int position, out int total)
        {
            if (string.IsNullOrWhiteSpace(tagValue))
                throw new ArgumentException("Invalid tag value");

            position = -1;
            total = -1;

            var pieces = tagValue.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (pieces.Length != 2)
                return false;

            if (!int.TryParse(pieces[0], out position))
                return false;

            if (!int.TryParse(pieces[1], out total))
                return false;

            return true;
        }
    }
}
