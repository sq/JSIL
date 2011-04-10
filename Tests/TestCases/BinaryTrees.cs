/* The Computer Language Benchmarks Game
   http://shootout.alioth.debian.org/ 

   contributed by Marek Safar  
*/

using System;

public static class Program {
    const int minDepth = 4;

    public static void Main (String[] args) {
        int n = 0;
        if (args.Length > 0) n = Int32.Parse(args[0]);

        int maxDepth = Math.Max(minDepth + 2, n);
        int stretchDepth = maxDepth + 1;

        int check = (TreeNode.bottomUpTree(0, stretchDepth)).itemCheck();
        Console.WriteLine("stretch tree of depth {0}\t check: {1}", stretchDepth, check);

        TreeNode longLivedTree = TreeNode.bottomUpTree(0, maxDepth);

        for (int depth = minDepth; depth <= maxDepth; depth += 2) {
            int iterations = 1 << (maxDepth - depth + minDepth);

            check = 0;
            for (int i = 1; i <= iterations; i++) {
                check += (TreeNode.bottomUpTree(i, depth)).itemCheck();
                check += (TreeNode.bottomUpTree(-i, depth)).itemCheck();
            }

            Console.WriteLine("{0}\t trees of depth {1}\t check: {2}",
               iterations * 2, depth, check);
        }

        Console.WriteLine("long lived tree of depth {0}\t check: {1}",
           maxDepth, longLivedTree.itemCheck());
    }


    struct TreeNode {
        class Next {
            public TreeNode left, right;
        }

        private Next next;
        private int item;

        internal static TreeNode bottomUpTree (int item, int depth) {
            if (depth > 0) {
                return new TreeNode(
                    new Next {
                        left = bottomUpTree(2 * item - 1, depth - 1),
                        right = bottomUpTree(2 * item, depth - 1)
                    }, item
                );
            } else {
                return new TreeNode(null, item);
            }
        }

        TreeNode (Next next, int item) {
            this.next = next;
            this.item = item;
        }

        internal int itemCheck () {
            // if necessary deallocate here
            if (next == null) return item;
            else return item + next.left.itemCheck() - next.right.itemCheck();
        }
    }
}