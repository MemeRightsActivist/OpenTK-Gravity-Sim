using System;


internal class Program
{
    public static void Main(string[] args)
    {
        using (Game game = new Game(2000, 2000, "LearnOpenTK"))
        {
            game.Run();
            Console.WriteLine(game.UpdateFrequency);



        }
    }
}

