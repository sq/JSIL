using System;
using System.Collections;
using System.Collections.Generic;

public static class Program
{
    public class PickupItem {
        string _itemRepresented = "default";

        public Glide Glide = new Glide();

        public PickupItem (string itemRepresented) {
            _itemRepresented = itemRepresented;
        }

        public override string ToString () {
            return String.Format("PickupItem(item={0}, mapposition={1})", _itemRepresented, Glide.MapPosition);
        }
    }

    public class Glide {
        public string MapPosition = "default";
    }

    public static void Main(string[] args)
    {
        var representedItem = "representedItem";
        var gridPosition = "gridPosition";

        PickupItem item = new PickupItem(representedItem);
        item.Glide.MapPosition = gridPosition;

        Console.WriteLine(item);
    }
}