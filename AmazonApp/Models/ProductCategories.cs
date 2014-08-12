/*
 *
 * Version:
 *     1
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AmazonApp.Models
{
    public static class ProductCategories
    {
        public static String[] Categories = new String[] 
        {
            "All", 
            "Apparel", 
            "Appliances", 
            "ArtsAndCrafts", 
            "Automotive", 
            "Baby", 
            "Beauty", 
            "Blended", 
            "Books", 
            "Classical", 
            "Collectibles", 
            "DigitalMusic", 
            "DVD", 
            "Electronics", 
            "ForeignBooks", 
            "Garden", 
            "GourmetFood", 
            "Grocery", 
            "HealthPersonalCare", 
            "Hobbies", 
            "Home", 
            "HomeGarden", 
            "HomeImprovement", 
            "Home & Kitchen", 
            "Industrial", 
            "Jewelry", 
            "KindleStore", 
            "Kitchen", 
            "LawnAndGarden", 
            "Lighting", 
            "Luggage", 
            "Magazines", 
            "Marketplace", 
            "Miscellaneous", 
            "MobileApps", 
            "MP3Downloads", 
            "Music", 
            "MusicalInstruments", 
            "MusicTracks", 
            "OfficeProducts", 
            "OutdoorLiving", 
            "Outlet", 
            "PCHardware", 
            "PetSupplies", 
            "Photo", 
            "Shoes", 
            "Software", 
            "SoftwareVideoGames", 
            "SportingGoods", 
            "Tools", 
            "Toys", 
            "UnboxVideo", 
            "VHS", 
            "Video", 
            "VideoGames", 
            "Watches", 
            "Wireless", 
            "WirelessAccessories"
        };

        public static String ResolveCategory(int index)
        {
            index = Math.Min(index, Categories.Length);
            index = Math.Max(index, 0);
            return Categories[index];
        }
    }
}