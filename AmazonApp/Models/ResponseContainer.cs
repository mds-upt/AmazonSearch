using System;
using System.Collections.Generic;

// This is sent to client script as json
[Serializable]
public class ResponseContainer
{
    public List<Product> products = new List<Product>();
    public string error;
    public int totalResults;
}