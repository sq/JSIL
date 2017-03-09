using System;
using System.Collections.ObjectModel;

public static class Program
{
    public static void Main(string[] args)
    {


        var list = new ObservableCollection<string> { "zero", "one", "two", "three" };
        list.CollectionChanged += list_CollectionChanged;

        Console.WriteLine(list.IndexOf("two"));
        Console.WriteLine(list.IndexOf("zero"));
        Console.WriteLine(list.IndexOf("three"));

        list.Insert(0, "newOne");
        Console.WriteLine(list.IndexOf("three"));
        Console.WriteLine(list.IndexOf("newOne"));

        list.Insert(3, "anotherNewOne");
        Console.WriteLine(list.IndexOf("three"));
        Console.WriteLine(list.IndexOf("anotherNewOne"));
        Console.WriteLine(list.IndexOf("one"));

        list.Add("five");

        list.Move(list.IndexOf("five"), list.IndexOf("one"));

        list.Remove("five");

        list.RemoveAt(list.IndexOf("two"));

        list.Clear();
    }

    static void list_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        string oldItems = "", newItems = "";
        if (e.OldItems != null)
            foreach (var item in e.OldItems)
            {
                oldItems += item + ",";
            }
        if (e.NewItems != null)
            foreach (var item in e.NewItems)
            {
                newItems += item + ",";
            }
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            Console.WriteLine("Add:" + e.OldStartingIndex + " " + oldItems + " =>" + e.NewStartingIndex + " " + newItems);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
        {
            Console.WriteLine("Move:" + e.OldStartingIndex + " " + oldItems + " =>" + e.NewStartingIndex + " " + newItems);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            Console.WriteLine("Remove:" + e.OldStartingIndex + " " + oldItems + " =>" + e.NewStartingIndex + " " + newItems);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
        {
            Console.WriteLine("Replace:" + e.OldStartingIndex + " " + oldItems + " =>" + e.NewStartingIndex + " " + newItems);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        {
            Console.WriteLine("Reset:" + e.OldStartingIndex + " " + oldItems + " =>" + e.NewStartingIndex + " " + newItems);
        }

        string str = "ObservableCollection:";
        foreach (var item in ((ObservableCollection<string>)sender))
        {
            str += item + ",";
        }
        Console.WriteLine(str);
    }

}