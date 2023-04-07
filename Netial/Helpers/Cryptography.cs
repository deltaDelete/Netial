using System.Text;
using System.Security.Cryptography;

namespace Netial.Helpers; 

public class Cryptography {
    public static string Sha256Hash(string input) {
        using var sha = SHA256.Create();
        // ComputeHash - returns byte array  
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        // Convert byte array to a string   
        StringBuilder builder = new StringBuilder();  
        for (int i = 0; i < bytes.Length; i++)  
        {  
            builder.Append(bytes[i].ToString("x2"));  
        }  
        return builder.ToString(); 
    }
}