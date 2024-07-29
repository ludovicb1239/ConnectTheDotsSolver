namespace ConnectTheDotsSolver
{
    internal class StackExample
    {
        public static void Example()
        {

            // Creating a stack of integers
            Stack<int> stack = new Stack<int>();

            // Pushing elements onto the stack
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            Console.WriteLine("Elements in the stack:");
            foreach (int item in stack)
            {
                Console.WriteLine(item);
            }

            // Peeking at the top element
            int top = stack.Peek();
            Console.WriteLine($"Top element is: {top}");

            // Popping elements from the stack
            int popped = stack.Pop();
            Console.WriteLine($"Popped element: {popped}");

            Console.WriteLine("Elements in the stack after popping:");
            foreach (int item in stack)
            {
                Console.WriteLine(item);
            }
        }
    }
}
