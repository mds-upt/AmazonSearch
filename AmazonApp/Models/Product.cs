// Product class
using System;

[Serializable]
public class Product
{
    public string Title;
    public string Price;
    public string ImageURL;
    public string PageURL;

    public Product(String title, String price, String imageURL, String pageURL)
    {
        this.Title = title;
        this.Price = price;
        this.ImageURL = imageURL;
        this.PageURL = pageURL;
    }
}