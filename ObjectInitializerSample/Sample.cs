namespace ObjectInitializerSample
{
    public static class Sample
    {
        public static void Main()
        {
            var model = new Model
            {
                Text = "text",
                Number = 13,
            };
        }
    }

    public class Model
    {
        public string Text { get; set; }
        
        public int Number { get; set; }
        
        private int PrivateNumber { get; set; }
    }
}