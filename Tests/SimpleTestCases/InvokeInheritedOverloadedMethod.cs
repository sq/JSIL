using System;
using System.Collections.Generic;
using JSIL;
using JSIL.Meta;

public class BatchRemovalCollection<T> : List<T> {
    private List<T> pendingRemovals;
    private List<T> pendingAdditions;

    public BatchRemovalCollection () {
        pendingRemovals = new List<T>();
        pendingAdditions = new List<T>();
    }

    public void QueuePendingRemoval (T item) {
        pendingRemovals.Add(item);
    }

    public void QueuePendingAddition (T item) {
        pendingAdditions.Add(item);
    }

    public void ApplyPendingRemovals () {
        for (int i = 0; i < pendingRemovals.Count; i++) {
            Remove(pendingRemovals[i]);
        }
        pendingRemovals.Clear();
    }

    public void ApplyPendingSpawns () {
        for (int i = 0; i < pendingAdditions.Count; i++) {
            Add(pendingAdditions[i]);
        }
        pendingAdditions.Clear();
    }
}

public static class Program {
    public static void Main () {
        BatchRemovalCollection<string> x = new BatchRemovalCollection<string>();
    }
}