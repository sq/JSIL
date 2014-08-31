using System;

public static class Program {
    class MyClass {
        public void InvokeEvent () {
            if (this.Event != null) {
                this.Event();
            }
        }

        public event Action Event;
    }

    public static void Main (string[] args) {
        var theEvent = typeof(MyClass).GetEvents()[0];
        Console.WriteLine(theEvent);

        var instance = new MyClass();
        theEvent.AddEventHandler(instance, new Action(delegate {
                Console.WriteLine("Event fired");
            }));

        instance.InvokeEvent();
    }
}