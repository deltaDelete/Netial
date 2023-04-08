namespace Netial.Helpers; 

public class UriQuery {
    public static string[] SplitQuery(string query) {
        return query.Split('&');
    }

    public static Dictionary<string, string?> QueryDictionary(string query) {
        var dict = new Dictionary<string, string?>();

        if (string.IsNullOrEmpty(query))
            return dict;
        
        foreach (var s in SplitQuery(query[1..])) {
            string?[] keyValue = s.Split('=');
            if (keyValue.Length == 0) {
                continue;
            }
            if(keyValue.Length == 1) {
                dict.Add(keyValue[0], default);
                continue;
            }

            if (keyValue.Length == 2) {
                dict.Add(keyValue[0], keyValue[1]);
            }
        }

        return dict;
    }
}