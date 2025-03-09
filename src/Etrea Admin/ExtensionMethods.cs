using System.Text;

namespace Etrea_Admin
{
    internal static class ExtensionMethods
    {
        internal static string ConvertToString(this string[] lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string l in lines)
            {
                sb.AppendLine(l);
            }
            return sb.ToString();
        }
    }
}
